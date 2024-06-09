import { WebSocketServer, WebSocket } from 'ws';
import { v4 as uuidv4 } from 'uuid';
import { StringDecoder } from 'string_decoder';
import chalk from 'chalk';
import fs from 'fs';
import { Low } from 'lowdb';
import { JSONFile } from 'lowdb/node';
import express from 'express';

const port = 8382;

// 初始化 Express 應用
const app = express();

// 設置靜態目錄
app.use('/', express.static('images'));

// 啟動 HTTP 伺服器
const server = app.listen(port, () => {
    console.log(`HTTP server is listening on port ${port}`);
});

// 使用相同的端口為 WebSocket 伺服器
const wss = new WebSocketServer({ server });

const defaultData = { images: [] };
const adapter = new JSONFile('db.json');
const db = new Low(adapter, defaultData);

async function initDB() {
    await db.read();
    if (!db.data) {
        db.data = { images: [] };
        await db.write();
    }
}

async function saveImage(room, imageName, imageData) {
    const dirPath = `./images/${room}`;
    const filePath = `${dirPath}/${imageName}.png`;

    // 確保目錄存在
    fs.mkdirSync(dirPath, { recursive: true });

    // 將圖片儲存到本地檔案系統
    fs.writeFileSync(filePath, imageData, 'base64');

    // 更新資料庫
    await db.read();
    db.data.images.push({
        room,
        imageName,
        likedBy: [],
        comments: ""
    });
    await db.write();
}

async function getImagesByRoom(room, username) {
    await db.read();
    const roomImages = db.data.images.filter(img => img.room == room);

    return roomImages.map(img => {
        const imagePath = `${room}/${img.imageName}.jpg`;
        // const imageData = fs.existsSync(imagePath) ? fs.readFileSync(imagePath).toString('base64') : null;

        const isLiked = (username) && img.likedBy.includes(username);
        const likeCount = img.likedBy.length;

        return {
            room: img.room,
            imageName: img.imageName,
            imagePath,
            isLiked,
            likeCount,
            comments: img.comments,
            type: 'image'
        };
    });
}

async function likeImage(room, imageName, playerName, isLiked) {
    await db.read();
    const image = db.data.images.find(img => img.room == room && img.imageName == imageName);
    if (isLiked == "true" && image && !image.likedBy.includes(playerName)) {
        image.likedBy.push(playerName);
        await db.write();
        updateLikeAndComments(room, imageName, image);
    }
    else if (image && image.likedBy.includes(playerName)) {
        image.likedBy = image.likedBy.filter(item => item != playerName);
        await db.write();
        updateLikeAndComments(room, imageName, image);

    }
}

async function addComment(room, imageName, playerUUID, comment) {
    await db.read();
    const image = db.data.images.find(img => img.room == room && img.imageName == imageName);
    if (image) {
        image.comments += `${playerUUID}: ${comment}\n`;
        await db.write();
        updateLikeAndComments(room, imageName, image);
    }
}

function updateLikeAndComments(room, imageName, image) {
    players.forEach(player => {
        if (player.room != room) return;
        player.ws.send(JSON.stringify({
            type: "image_update",
            imageName,
            isLiked: image.likedBy.includes(player.username),
            likeCount: image.likedBy.length,
            comments: image.comments
        }))
    })
}

initDB();

const players = new Map();

wss.on('connection', (ws) => {
    const playerUUID = uuidv4();
    let room;
    let username;

    ws.playerUUID = playerUUID;

    // 發送玩家的 UUID 給該玩家
    ws.send(JSON.stringify({ type: 'UUID', uuid: playerUUID }));

    ws.on('message', (message) => {
        const decoder = new StringDecoder('utf8');
        const buffer = Buffer.from(message);
        const msg = decoder.write(buffer);
        const type = msg.split(':')[0];
        const data = msg.substring(type.length + 1);

        switch (type) {
            case 'playerData':
                players.set(playerUUID, { room, playerData: data, username, ws });

                if (!room) return;
                const broadcastData = JSON.stringify({ type: 'playerData', data: data, uuid: playerUUID });

                wss.clients.forEach(client => {
                    if (client !== ws && client.readyState === WebSocket.OPEN && players.get(client.playerUUID)?.room == room) {
                        client.send(broadcastData);
                    }
                });
                break;
            case 'joinroom':
                [room, username] = data.split(":");
                getImagesByRoom(room, username).then(images => {
                    images.forEach(img => {
                        ws.send(JSON.stringify(img));
                    });
                });
                break;
            case 'like':
                const [likeImageName, isLiked] = data.split(':');
                likeImage(room, likeImageName, username, isLiked);
                break;
            case 'comment':
                const [commentImageName, commentText] = data.split(':');
                addComment(room, commentImageName, username, commentText);
                break;
        }
    });

    ws.on('close', () => {
        players.delete(ws.playerUUID)
        // 當玩家斷開連線時，廣播玩家的 UUID 給所有玩家
        const disconnectMessage = JSON.stringify({ type: 'disconnect', uuid: playerUUID });
        wss.clients.forEach(client => {
            if (client.readyState === WebSocket.OPEN) {
                client.send(disconnectMessage);
            }
        });
    });
});

if (true)
    setInterval(() => {

        console.clear();

        console.log(chalk.redBright('WebSocket Server port') + ' : ' + chalk.blueBright(port));
        // 顯示玩家數量
        console.log(chalk.green(`玩家數量: ${wss.clients.size}`));

        console.log();

        // 顯示每個玩家
        Array.from(players.entries())
            .reduce((acc, item) => {
                if (!item[1].room) item[1].room = "menu";
                if (!acc.has(item[1].room)) {
                    acc.set(item[1].room, []);
                }
                item[1].uuid = item[0];
                acc.get(item[1].room).push(item[1]);
                return acc;
            }, new Map())
            .forEach((players, room) => {
                console.log(chalk.blueBright(`Room: ${room}`));
                players.forEach(player => {
                    console.log(chalk.yellow(player.username));
                    console.log(chalk.redBright(player.uuid));
                    console.log(player.playerData);
                    // const playerData = JSON.parse(player.playerData);
                    // console.log('Animator State Data:', playerData.animatorStateData);
                    // console.log('Transform Data:', playerData.transformData);
                });
                console.log();
            });
    }, 500);

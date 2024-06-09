const WebSocketServer = require('ws').WebSocketServer;
const WebSocket = require('ws');
const { v4: uuidv4 } = require('uuid');
const { StringDecoder } = require('string_decoder');
const fs = require('fs');
const express = require('express');

const { port } = require("./config.json");

// 初始化 Express 應用
const app = express();

// 設置靜態目錄
app.use('/images', express.static('images'));

// 啟動 HTTP 伺服器
const server = app.listen(port, () => {
    console.log("WebSocket and ImageServer are running on port " + port);
    console.log("Realtime Player Data: http://localhost:" + port + "/realtime");
    console.log("(crtl + left click to open)");
});

// 使用相同的端口為 WebSocket 伺服器
const wss = new WebSocketServer({ server });

const dbFilePath = 'db.json';

function readDatabase() {
    if (fs.existsSync(dbFilePath)) {
        const data = fs.readFileSync(dbFilePath, 'utf-8');
        return JSON.parse(data);
    } else {
        return { images: [] };
    }
}

function writeDatabase(data) {
    fs.writeFileSync(dbFilePath, JSON.stringify(data, null, 2));
}

const db = readDatabase();

async function saveImage(room, imageName, imageData) {
    const dirPath = `./images/${room}`;
    const filePath = `${dirPath}/${imageName}.jpg`;

    // 確保目錄存在
    fs.mkdirSync(dirPath, { recursive: true });

    // 將圖片儲存到本地檔案系統
    fs.writeFileSync(filePath, imageData, 'base64');

    // 更新資料庫
    db.images.push({
        room,
        imageName,
        likedBy: [],
        comments: ""
    });
    writeDatabase(db);
}

async function getImagesByRoom(room, username) {
    const roomImages = db.images.filter(img => img.room == room);

    return roomImages.map(img => {
        const imagePath = `images/${room}/${img.imageName}.jpg`;
        const isLiked = (username) && img.likedBy.includes(username);
        const likeCount = img.likedBy.length;

        return {
            room: img.room,
            imageName: img.imageName,
            imagePath,
            isLiked,
            likeCount,
            comments: img.comments,
            likedBy: img.likedBy,
            type: 'image'
        };
    });
}

async function likeImage(room, imageName, playerName, isLiked) {
    const image = db.images.find(img => img.room == room && img.imageName == imageName);
    if (isLiked == "true" && image && !image.likedBy.includes(playerName)) {
        image.likedBy.push(playerName);
        writeDatabase(db);
        updateLikeAndComments(room, imageName, image);
    } else if (image && image.likedBy.includes(playerName)) {
        image.likedBy = image.likedBy.filter(item => item != playerName);
        writeDatabase(db);
        updateLikeAndComments(room, imageName, image);
    }
}

async function addComment(room, imageName, playerUUID, comment) {
    const image = db.images.find(img => img.room == room && img.imageName == imageName);
    if (image) {
        image.comments += `${playerUUID}: ${comment}\n`;
        writeDatabase(db);
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
        }));
    });
}

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
        players.delete(ws.playerUUID);
        // 當玩家斷開連線時，廣播玩家的 UUID 給所有玩家
        const disconnectMessage = JSON.stringify({ type: 'disconnect', uuid: playerUUID });
        wss.clients.forEach(client => {
            if (client.readyState === WebSocket.OPEN) {
                client.send(disconnectMessage);
            }
        });
    });
});

// 即時數據頁面
app.get('/realtime', (req, res) => {
    res.send(`
        <!DOCTYPE html>
        <html>
        <head>
            <title>Realtime Player Data</title>
            <link href="https://maxcdn.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet">
            <style>
                body { font-family: Arial, sans-serif; margin: 20px; }
                .room { margin-bottom: 20px; }
                .room-header { font-weight: bold; font-size: 1.2em; }
                .player-card, .image-card { margin: 10px 0; }
                .image-card img { max-width: 300px; }
            </style>
        </head>
        <body>
            <h1>Realtime Player Data</h1>
            <div id="data" class="container"></div>
            <script>
                const fetchData = async () => {
                    const response = await fetch('/api/realtime-data');
                    const data = await response.json();
                    const dataDiv = document.getElementById('data');
                    dataDiv.innerHTML = '';
                    for (const [room, players] of Object.entries(data.players)) {
                        const roomDiv = document.createElement('div');
                        roomDiv.classList.add('room');
                        const roomHeader = document.createElement('div');
                        roomHeader.classList.add('room-header');
                        roomHeader.textContent = \`Room: \${room}\`;
                        roomDiv.appendChild(roomHeader);

                        players.forEach(player => {
                            const playerCard = document.createElement('div');
                            playerCard.classList.add('card', 'player-card');
                            playerCard.innerHTML = \`
                                <div class="card-body">
                                    <h5 class="card-title">Username: \${player.username}</h5>
                                    <p class="card-text">UUID: \${player.uuid}</p>
                                    <p class="card-text">Player Data: \${player.playerData}</p>
                                </div>
                            \`;
                            roomDiv.appendChild(playerCard);
                        });

                        if (data.images[room]) {
                            data.images[room].forEach(image => {
                                const imageCard = document.createElement('div');
                                imageCard.classList.add('card', 'image-card');
                                imageCard.innerHTML = \`
                                    <div class="card-body">
                                        <h5 class="card-title">Image: \${image.imageName}</h5>
                                        <img src="\${image.imagePath}" class="card-img-top" alt="\${image.imageName}">
                                        <p class="card-text">Liked By: \${image.likedBy.join(', ')}</p>
                                        <p class="card-text">Comments: \${image.comments.replace(/\\n/g, '<br>')}</p>
                                    </div>
                                \`;
                                roomDiv.appendChild(imageCard);
                            });
                        }

                        dataDiv.appendChild(roomDiv);
                    }
                };

                setInterval(fetchData, 1000);
                fetchData();
            </script>
        </body>
        </html>
    `);
});

// 提供即時數據的API
app.get('/api/realtime-data', async (req, res) => {
    const playerData = Array.from(players.entries())
        .reduce((acc, item) => {
            if (!item[1].room) item[1].room = "menu";
            if (!acc.has(item[1].room)) {
                acc.set(item[1].room, []);
            }
            item[1].uuid = item[0];
            acc.get(item[1].room).push(item[1]);
            return acc;
        }, new Map());

    const formattedData = { players: {}, images: {} };
    playerData.forEach((players, room) => {
        formattedData.players[room] = players.map(player => ({
            username: player.username,
            uuid: player.uuid,
            playerData: player.playerData
        }));
    });

    db.images.forEach(img => {
        if (!formattedData.images[img.room]) {
            formattedData.images[img.room] = [];
        }
        formattedData.images[img.room].push({
            imageName: img.imageName,
            imagePath: `/images/${img.room}/${img.imageName}.jpg`,
            likedBy: img.likedBy,
            comments: img.comments
        });
    });

    res.json(formattedData);
});

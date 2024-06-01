using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class WebSocketManager : MonoBehaviour
{
    public Boolean wss;
    public String server_ip;
    public int port;
    public WebSocket ws;
    public int executionsPerSecond = 20;
    private float interval;
    private float timer;
    private string address;

    public GameObject player;
    public GameObject otherPlayerPrefab;
    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    private readonly Queue<Action> actions = new Queue<Action>();
    private readonly object lockObject = new object();


    void Start()

    {
        interval = 1f / executionsPerSecond;
        timer = 0f;

        address = (wss ? "wss" : "ws") + "://" + server_ip + ":" + port;

        Debug.Log(address);

        ws = new WebSocket(address);

        ws.OnMessage += OnMessage;
        ws.OnOpen += OnOpen;
        ws.OnClose += OnClose;
        ws.OnError += OnError;

        ws.Connect();
    }

    void Update()
    {
        ExecuteActions();

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer -= interval;
            WebSocketAction();
        }
    }

    private void ExecuteActions()
    {
        lock (lockObject)
        {
            while (actions.Count > 0)
            {
                var action = actions.Dequeue();
                action.Invoke();
            }
        }
    }

    void WebSocketAction()
    {
        ws.Send(GameObjectSerializer.SerializeGameObject(player));
    }

    void OnMessage(object sender, MessageEventArgs e)
    {
        var messageData = JsonUtility.FromJson<MessageData>(e.Data);

        try
        {

            lock (lockObject)
            {
                actions.Enqueue(() =>
                {
                    if (messageData.type == "message")
                    {
                        if (!players.ContainsKey(messageData.uuid))
                        {

                            // 創建新的玩家物件
                            GameObject newPlayer = Instantiate(otherPlayerPrefab);
                            players[messageData.uuid] = newPlayer;
                        }

                        // 更新玩家物件
                        GameObject player = players[messageData.uuid];
                        GameObjectSerializer.DeserializeGameObject(player, messageData.data);
                    }
                    else if (messageData.type == "disconnect")
                    {
                        if (players.ContainsKey(messageData.uuid))
                        {
                            // 刪除玩家物件
                            Destroy(players[messageData.uuid]);
                            players.Remove(messageData.uuid);
                        }
                    }
                });
            }
        }
        catch (Exception err)
        {
            Debug.LogError(err);
        }
    }

    void OnOpen(object sender, System.EventArgs e)
    {
        Debug.Log("WebSocket connection opened : " + address);
    }

    void OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log("WebSocket connection closed: " + e.Reason);
    }

    void OnError(object sender, ErrorEventArgs e)
    {
        Debug.LogError("WebSocket error: ");
        Debug.LogError(e);
    }

    [System.Serializable]
    public class MessageData
    {
        public string type;
        public string uuid;
        public string data;
    }
}

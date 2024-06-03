using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using UnityEngine.SceneManagement;

public class WebSocketManager : MonoBehaviour
{
    public WebSocket ws;
    public int executionsPerSecond = 20;
    private float interval;
    private float timer;
    private string address;
    private bool connected = false;

    public GameObject player;
    public Camera vThirdPersonCamera;
    public GameObject otherPlayerPrefab;

    // UI
    public Canvas controlPromptsUI;
    public Canvas menuUI;
    public Button connectBtn;
    public Button[] roomBtns;
    public TMP_InputField addressInput;
    public TMP_InputField portInput;
    public TMP_Text logDisplay;
    public GameObject eventSystem;


    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    private readonly Queue<Action> actions = new Queue<Action>();
    private readonly object lockObject = new object();


    void Start()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        foreach (var roomBtn in roomBtns)
        {
            roomBtn.onClick.AddListener(()=>OnRoomBtnClick(roomBtn.name));
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(player);
        DontDestroyOnLoad(vThirdPersonCamera);
        DontDestroyOnLoad(controlPromptsUI);
        DontDestroyOnLoad(menuUI);
        DontDestroyOnLoad(eventSystem);

        interval = 1f / executionsPerSecond;
        timer = 0f;

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ws.Send("joinroom:" + scene.name);
    }

    void OnConnectBtnClick()
    {

        address = "ws://" + addressInput.text + ":" + portInput.text;

        Debug.Log(address);

        if (ws != null && ws.IsAlive)
        {
            ws.Close();
        }

        ws = new WebSocket(address);

        ws.OnMessage += OnMessage;
        ws.OnOpen += OnOpen;
        ws.OnClose += OnClose;
        ws.OnError += OnError;

        ws.Connect();
    }

    void OnRoomBtnClick(string room)
    {
        menuUI.enabled = false;
        SceneManager.LoadScene(room);
    }

    void Update()
    {
        if (!connected) return;

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
        ws.Send("playerData:" + GameObjectSerializer.SerializeGameObject(player));
    }

    void OnMessage(object sender, MessageEventArgs e)
    {
        var messageData = JsonUtility.FromJson<MessageData>(e.Data);
        Debug.Log(messageData);

        try
        {

            lock (lockObject)
            {
                actions.Enqueue(() =>
                {
                    if (messageData.type == "playerData")
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
            logDisplay.SetText(err.Message);
        }
    }

    void OnOpen(object sender, EventArgs e)
    {
        Debug.Log("WebSocket connection opened : " + address);
        logDisplay.SetText("WebSocket connection opened : " + address);
        connected = true;
    }

    void OnClose(object sender, CloseEventArgs e)
    {
        lock (lockObject)
        {
            actions.Enqueue(() =>
            {
                menuUI.enabled = true;
                menuUI.gameObject.SetActive(true);
            });
        }

        Debug.Log("WebSocket connection closed: " + e.Reason);
        logDisplay.SetText("WebSocket connection closed: " + e.Reason);

        connected = false;

    }

    void OnError(object sender, ErrorEventArgs e)
    {
        Debug.LogError("WebSocket error: " + e);
        logDisplay.SetText("WebSocket error: " + e);
    }

    [System.Serializable]
    public class MessageData
    {
        public string type;
        public string uuid;
        public string data;
    }
}

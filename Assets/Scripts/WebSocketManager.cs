using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;

public class WebSocketManager : MonoBehaviour
{
    public WebSocket ws;
    public int executionsPerSecond = 20;
    private float interval;
    private float timer;
    private string webSocket_address;
    private string imageServer_address;
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
    public TMP_InputField imageServerURL;
    public GameObject eventSystem;

    public TMP_InputField usernameInput;

    public GameObject imageViewer;

    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    private readonly Queue<Action> actions = new Queue<Action>();
    private readonly object lockObject = new object();


    void Start()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        foreach (var roomBtn in roomBtns)
        {
            roomBtn.onClick.AddListener(() => OnRoomBtnClick(roomBtn.name));
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(player);
        DontDestroyOnLoad(vThirdPersonCamera);
        DontDestroyOnLoad(controlPromptsUI);
        DontDestroyOnLoad(menuUI);
        DontDestroyOnLoad(eventSystem);
        DontDestroyOnLoad(imageViewer);

        interval = 1f / executionsPerSecond;
        timer = 0f;

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(usernameInput.text);
        ws.Send("joinroom:" + scene.name + ":" + usernameInput.text);
    }

    void OnConnectBtnClick()
    {

        webSocket_address = "ws://" + addressInput.text + ":" + portInput.text;

        Debug.Log(webSocket_address);

        if (ws != null && ws.IsAlive)
        {
            ws.Close();
        }

        ws = new WebSocket(webSocket_address);

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

    public void ExecuteInMainThread(Action action)
    {
        lock (lockObject)
        {
            actions.Enqueue(action);
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
        ExecuteInMainThread(() =>
        {
            switch (messageData.type)
            {
                case "playerData":

                    if (!players.ContainsKey(messageData.uuid))
                    {
                        // 創建新的玩家物件
                        GameObject newPlayer = Instantiate(otherPlayerPrefab);
                        players[messageData.uuid] = newPlayer;
                    }
                    // 更新玩家物件
                    GameObject player = players[messageData.uuid];
                    GameObjectSerializer.DeserializeGameObject(player, messageData.data);

                    break;
                case "disconnect":
                    if (players.ContainsKey(messageData.uuid))
                    {
                        // 刪除玩家物件
                        Destroy(players[messageData.uuid]);
                        players.Remove(messageData.uuid);
                    }
                    break;
                case "image":
                    Debug.Log("Loading: " + messageData.imageName);

                    GameObject targetObject = GameObject.Find(messageData.imageName);
                    if (targetObject == null)
                    {
                        Debug.LogError("Image Plane not found!");
                        return;
                    }


                    targetObject.GetComponent<PlaneClickDetector>().UpdateImage(messageData.isLiked, messageData.likeCount, messageData.comments);

                    StartCoroutine(GetImageAndModify(targetObject, (imageServerURL.text.EndsWith("/") ? imageServerURL.text : imageServerURL.text + "/") + messageData.imagePath));
                    // ModifyDrawingImage(targetObject, messageData.imageData);
                    break;
                case "image_update":
                    Debug.Log("Update: " + messageData.imageName);
                    GameObject targetUpdateObject = GameObject.Find(messageData.imageName);

                    targetUpdateObject.GetComponent<PlaneClickDetector>().UpdateImage(messageData.isLiked, messageData.likeCount, messageData.comments);

                    break;
            }
        });
    }

    IEnumerator GetImageAndModify(GameObject targetObject, string url)
    {
        Debug.Log(url);

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            ModifyDrawingImage(targetObject, texture);
        }
    }
    void OnOpen(object sender, EventArgs e)
    {
        Debug.Log("WebSocket connection opened : " + webSocket_address);
        logDisplay.SetText("WebSocket connection opened : " + webSocket_address);
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

    void ModifyDrawingImage(GameObject targetObject, Texture2D texture)
    {
        targetObject.GetComponent<PlaneClickDetector>().texture2D = texture;

        Renderer renderer = targetObject.GetComponent<Renderer>();

        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"))
            {
                mainTexture = texture
            };
            renderer.material = material;
        }
        else
        {
            Debug.LogError("Renderer not found on target object.");
        }

    }


    [System.Serializable]
    public class MessageData
    {
        public string type;
        public string uuid;
        public string data;
        public string imageName;
        public string imagePath;
        public bool isLiked;
        public int likeCount;
        public string comments;
    }
}

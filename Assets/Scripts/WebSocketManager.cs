using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;
using System.Threading.Tasks;
using UnityEditor;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Unity.VisualScripting.Dependencies.NCalc;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine.Events;
using Invector.vCharacterController;

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
    public Button scanBtn;
    public Button[] roomBtns;
    public TMP_InputField addressInput;
    public TMP_InputField portInput;
    public TMP_InputField imageServerURL;
    public GameObject eventSystem;

    public TMP_InputField usernameInput;

    public GameObject imageViewer;
    public Button menuBtn;
    public Button menuCloseBtn;
    public CanvasGroup connectionInfoPage;
    public Button connectionInfoBtn;
    public Button connectionInfoCloseBtn;
    public TMP_Text statusDisplay;

    public RawImageClickHandler perspectiveBtn;
    public RawImageClickHandler strafeBtn;

    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    private readonly Queue<Action> actions = new Queue<Action>();
    private readonly object lockObject = new object();

    private UdpClient udpClient;
    private IPEndPoint broadcastEndPoint;
    private const int BroadcastPort = 8382;
    private bool receivingUdpResponses = true;

    void Start()
    {
        udpClient = new UdpClient
        {
            EnableBroadcast = true,
        };
        broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, BroadcastPort);

        SendBroadcastMessage();
        ListenForUdpResponses();

        connectBtn.onClick.AddListener(OnConnectBtnClick);
        scanBtn.onClick.AddListener(OnScanBtnClick);

        menuBtn.onClick.AddListener(() => OpenMenu());
        menuCloseBtn.onClick.AddListener(() => CloseMenu());

        connectionInfoBtn.onClick.AddListener(ConnectionInfoOpen);
        connectionInfoCloseBtn.onClick.AddListener(ConnectionInfoClose);

        foreach (var roomBtn in roomBtns)
        {
            roomBtn.onClick.AddListener(() => LoadScene(roomBtn.name));
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

    void ConnectionInfoOpen()
    {
        connectionInfoPage.alpha = 1;
        connectionInfoPage.interactable = true;
        connectionInfoPage.blocksRaycasts = true;
    }

    void ConnectionInfoClose()
    {
        connectionInfoPage.alpha = 0;
        connectionInfoPage.interactable = false;
        connectionInfoPage.blocksRaycasts = false;
    }

    void OpenMenu()
    {
        ConnectionInfoClose();
        menuUI.enabled = true;
        usernameInput.interactable = true;
    }

    void CloseMenu()
    {
        usernameInput.interactable = false;
        menuUI.enabled = false;
    }

    private void SendBroadcastMessage()
    {
        string message = "Discover WebSocket Server";
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, broadcastEndPoint);
        Debug.Log("進行UDP廣播");
        statusDisplay.text = "正在尋找伺服器";
    }

    private async void ListenForUdpResponses()
    {
        Debug.Log("開始等待回應");
        Task timeoutTask = Task.Delay(10000); // 10 秒超時

        try
        {
            while (receivingUdpResponses)
            {
                var receiveTask = udpClient.ReceiveAsync();
                var completedTask = await Task.WhenAny(receiveTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    statusDisplay.text = "找不到伺服器";
                    Debug.Log("超過10秒未獲得回應");
                    receivingUdpResponses = false;
                    break;
                }

                UdpReceiveResult result = receiveTask.Result;
                string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                Debug.Log($"收到來自 {result.RemoteEndPoint} 的 UDP 訊息 : {receivedMessage}");

                if (receivedMessage.Contains("WebSocket server is here"))
                {
                    addressInput.text = result.RemoteEndPoint.Address.ToString();
                    portInput.text = result.RemoteEndPoint.Port.ToString();
                    Debug.Log($"已確定 {result.RemoteEndPoint} 為WebSocket伺服器位址");

                    statusDisplay.text = "確定伺服器位址，開始連線";
                    OnConnectBtnClick();
                    receivingUdpResponses = false;

                    Debug.Log("已將udpClient關閉");
                    udpClient.Close();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("接收UDP訊息發生錯誤: " + ex.Message);
        }
        finally
        {
            Debug.Log("不再等待回應");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("上傳使用者名稱: " + usernameInput.text);
        ws.Send("joinroom:" + scene.name + ":" + usernameInput.text);
    }
    void OnScanBtnClick()
    {

        if (udpClient.Client == null)
        {
            udpClient = new UdpClient();
        }

        broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, BroadcastPort);

        SendBroadcastMessage();

        if (!receivingUdpResponses)
        {
            receivingUdpResponses = true;
            ListenForUdpResponses();
        }
    }

    void OnConnectBtnClick()
    {
        webSocket_address = "ws://" + addressInput.text + ":" + portInput.text;

        statusDisplay.text = "嘗試連線WebSocket";
        Debug.Log("嘗試連線WebSocket: " + webSocket_address);

        if (ws != null && ws.IsAlive)
        {
            ws.OnClose -= OnClose;
            ws.Close();
        }

        ws = new WebSocket(webSocket_address)
        {
            WaitTime = TimeSpan.FromMilliseconds(2000)
        };

        ws.OnMessage += OnMessage;
        ws.OnOpen += OnOpen;
        ws.OnClose += OnClose;
        ws.OnError += OnError;

        ws.Connect();
    }

    public void LoadScene(string room)
    {
        player.transform.position = Vector3.zero;
        CloseMenu();
        SceneManager.LoadScene(room);
        menuCloseBtn.gameObject.SetActive(true);
    }

    void Update()
    {
        if (connected)
        {
            timer += Time.deltaTime;

            if (timer >= interval)
            {
                timer -= interval;
                WebSocketAction();
            }
        }

        ExecuteActions();

        if (perspectiveBtn.PressDown())
        {
            vThirdPersonCamera c = Camera.main.GetComponent<vThirdPersonCamera>();
            if (perspectiveBtn.switchValue)
            {
                c.defaultDistance = 0f;

                int layerMask = LayerMask.NameToLayer("Player");
                Camera.main.cullingMask &= ~(1 << layerMask);

                player.GetComponent<vThirdPersonController>().isStrafing = true;
            }
            else
            {
                c.defaultDistance = 2.5f;

                StartCoroutine(WaitAndExecute(0.25f, () =>
                {
                    int layerMask = LayerMask.NameToLayer("Player");
                    Camera.main.cullingMask |= 1 << layerMask;

                    player.GetComponent<vThirdPersonController>().isStrafing = strafeBtn.switchValue;
                }));
            }
        }
    }

    IEnumerator WaitAndExecute(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
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
                    if (messageData.room != SceneManager.GetActiveScene().name) break;

                    GameObject player;
                    if (players.TryGetValue(messageData.uuid, out player) && player != null)
                    {
                        GameObjectSerializer.DeserializeGameObject(player, messageData.data);
                    }
                    else
                    {
                        player = Instantiate(otherPlayerPrefab);
                        players[messageData.uuid] = player;

                        GameObjectSerializer.DeserializeGameObject(player, messageData.data);
                    }
                    break;

                case "disconnect":
                    if (players.ContainsKey(messageData.uuid))
                    {
                        GameObject disconnectedPlayer = players[messageData.uuid];
                        if (disconnectedPlayer != null)
                        {
                            Destroy(disconnectedPlayer);
                        }
                        players.Remove(messageData.uuid);
                    }
                    break;

                case "image":
                    Debug.Log("開始從ImageServer獲取圖片: " + messageData.imageName);

                    GameObject targetObject = GameObject.Find(messageData.imageName);
                    if (targetObject == null)
                    {
                        Debug.LogError("(image)找不到物件: " + messageData.imageName);
                        return;
                    }

                    targetObject.GetComponent<PlaneClickDetector>().UpdateImage(messageData.isLiked, messageData.likeCount, messageData.comments);
                    imageServer_address = imageServerURL.text;

                    if (imageServer_address == "")
                        imageServer_address = addressInput.text + ":" + portInput.text;

                    if (!imageServer_address.StartsWith("http://") && !imageServer_address.StartsWith("https://"))
                        imageServer_address = "http://" + imageServer_address;

                    if (!imageServer_address.EndsWith("/"))
                        imageServer_address += "/";

                    StartCoroutine(GetImageAndModify(targetObject, imageServer_address + messageData.imagePath));
                    break;

                case "image_update":
                    Debug.Log("準備更新圖片資料: " + messageData.imageName);
                    GameObject targetUpdateObject = GameObject.Find(messageData.imageName);

                    if (targetUpdateObject != null)
                    {
                        PlaneClickDetector planeClickDetector = targetUpdateObject.GetComponent<PlaneClickDetector>();
                        if (planeClickDetector != null)
                        {
                            planeClickDetector.UpdateImage(messageData.isLiked, messageData.likeCount, messageData.comments);
                        }
                        else
                        {
                            _3DObjectClickDetector objectClickDetector = targetUpdateObject.GetComponent<_3DObjectClickDetector>();
                            if (objectClickDetector != null)
                            {
                                objectClickDetector.Update3DObject(messageData.isLiked, messageData.likeCount, messageData.comments);
                            }
                            else
                            {
                                Debug.LogError("(image_update)找不到 PlaneClickDetector 或 _3DObjectClickDetector: " + messageData.imageName);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("(image_update)找不到物件: " + messageData.imageName);
                    }
                    break;
            }
        });
    }

    IEnumerator GetImageAndModify(GameObject targetObject, string url)
    {
        Debug.Log("Downloading: " + url);

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
        statusDisplay.text = "連線成功";
        Debug.Log("WebSocket 連線成功: " + webSocket_address);
        connected = true;
    }

    void OnClose(object sender, CloseEventArgs e)
    {
        statusDisplay.text = "連線已關閉";

        Debug.Log("WebSocket 連線關閉: " + e.Reason);

        connected = false;

        ExecuteInMainThread(() =>
        {
            menuUI.enabled = true;
        });
    }

    void OnError(object sender, ErrorEventArgs e)
    {
        statusDisplay.text = "發生錯誤 (但連線未關閉)";
        Debug.LogError("WebSocket 錯誤: " + e);
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

    void OnDestroy()
    {
        // 程式結束時關閉 WebSocket
        if (ws != null)
        {
            ws.Close();
            ws = null;
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
        public string room;
    }
}

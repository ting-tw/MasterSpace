
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    [Header("UI")]
    public Canvas menuUI;
    [Header("Pages")]
    public CanvasGroup connectionInfoPage;
    [Header("Main Page")]
    public TMP_InputField usernameInput;
    public TMP_Text statusDisplay;
    public Button scanBtn;
    public Button connectionInfoBtn;
    public TMP_Text touchToggleDisplay;
    public Button touchToggleBtn;
    [Header("Touch Toggle")]
    public RawImageClickHandler jumpBtn;
    public GameObject joystick;
    private bool touch = true;
    public Button[] roomBtns;
    [Header("Connection Info Page")]
    public TMP_InputField addressInput;
    public TMP_InputField portInput;
    public Button connectBtn;
    public TMP_InputField imageServerURL;
    public Button connectionInfoCloseBtn;
    [Header("Menu Buttons")]
    public Button menuBtn;
    public Button menuCloseBtn;
    [Header("Others")]
    public WebSocketManager webSocketManager;

    void Start()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            TouchToggle();
        }

        connectBtn.onClick.AddListener(webSocketManager.Connect);
        scanBtn.onClick.AddListener(webSocketManager.OnScanBtnClick);

        touchToggleBtn.onClick.AddListener(TouchToggle);

        menuBtn.onClick.AddListener(OpenMenu);
        menuCloseBtn.onClick.AddListener(CloseMenu);

        connectionInfoBtn.onClick.AddListener(ConnectionInfoOpen);
        connectionInfoCloseBtn.onClick.AddListener(ConnectionInfoClose);

        foreach (var roomBtn in roomBtns)
        {
            roomBtn.onClick.AddListener(() => webSocketManager.LoadScene(roomBtn.name));
        }
    }

    public void ConnectionInfoOpen()
    {
        connectionInfoPage.alpha = 1;
        connectionInfoPage.interactable = true;
        connectionInfoPage.blocksRaycasts = true;
    }

    public void ConnectionInfoClose()
    {
        connectionInfoPage.alpha = 0;
        connectionInfoPage.interactable = false;
        connectionInfoPage.blocksRaycasts = false;
    }

    public void OpenMenu()
    {
        ConnectionInfoClose();
        menuUI.enabled = true;
        usernameInput.interactable = true;
    }

    public void CloseMenu()
    {
        usernameInput.interactable = false;
        menuUI.enabled = false;
    }

    public void UpdateStatus(string status)
    {
        statusDisplay.text = status;
    }

    public void SetServerAddress(string address, string port)
    {
        addressInput.text = address;
        portInput.text = port;
    }

    public string GetUsername()
    {
        return usernameInput.text;
    }

    public string GetImageServerURL()
    {
        return imageServerURL.text;
    }

    public string GetServerAddress()
    {
        return addressInput.text + ":" + portInput.text;
    }

    public void ShowMenu()
    {
        menuUI.enabled = true;
    }

    private void TouchToggle()
    {
        touch = !touch;

        jumpBtn.rawImage.enabled = touch;
        joystick.SetActive(touch);

        touchToggleDisplay.text = touch ? "開啟" : "關閉";
    }
}

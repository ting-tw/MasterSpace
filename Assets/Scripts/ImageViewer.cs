using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageViewer : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    [Header("WebSocket")]
    public WebSocketManager webSocketManager;

    [Header("內部元件")]
    public Canvas canvas;
    public Image image;
    public Button LikeBtn;
    public TMP_Text likeCount;
    public TMP_Text imageTitle;
    public TMP_InputField commentInput;
    public Button submitBtn;
    public TMP_Text comments;
    public Button closeBtn;
    public RectTransform panel;
    public Canvas controlPromptsUI;
    public RawImage _3DViewRawImage;
    public GameObject[] _3DViewObjects;
    public GameObject _3DViewObject;
    public Camera _3DViewCamera;
    [Header("Zoom頁面")]
    public GameObject zoomPage;
    public Image zoomImage;
    public RawImage zoom3DViewRawImage;


    private bool isDragging = false;

    void Start()
    {
        canvas = gameObject.GetComponent<Canvas>();
        LikeBtn.onClick.AddListener(OnLikeBtnClick);

        closeBtn.onClick.AddListener(Close);
        submitBtn.onClick.AddListener(OnSubmitBtnClick);

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(_3DViewCamera);

        foreach (var obj in _3DViewObjects)
        {
            DontDestroyOnLoad(obj);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragging)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(image.rectTransform, eventData.position, eventData.pressEventCamera) ||
             RectTransformUtility.RectangleContainsScreenPoint(_3DViewRawImage.rectTransform, eventData.position, eventData.pressEventCamera))
            {
                OpenZoomPage();
            }
        }
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
    }


    public void OpenZoomPage()
    {
        zoomPage.gameObject.SetActive(true);

        if (image.isActiveAndEnabled)
        {
            zoomImage.sprite = image.sprite;
            RectTransform imageRectTransform = image.GetComponent<RectTransform>();
            RectTransform zoomImageRectTransform = zoomImage.GetComponent<RectTransform>();

            zoomImageRectTransform.sizeDelta = imageRectTransform.sizeDelta * 1.2f;
            zoomImageRectTransform.localScale = imageRectTransform.localScale;
            zoomImageRectTransform.rotation = imageRectTransform.rotation;

            zoom3DViewRawImage.gameObject.SetActive(false);
            zoomImage.gameObject.SetActive(true);
        }
        else
        {
            zoom3DViewRawImage.GetComponent<RectTransform>().localScale = new Vector2(1.2f, 1.2f);
            zoom3DViewRawImage.gameObject.SetActive(true);
            zoomImage.gameObject.SetActive(false);
        }
    }

    public void UpdateImageViewer(Texture2D newTexture, string newTitle, bool isLiked, int newLikeCount, string newComments, bool portrait)
    {
        RectTransform rectTransform = image.GetComponent<RectTransform>();

        rectTransform.sizeDelta = portrait ? new Vector2(500, 800) : new Vector2(800, 500);
        rectTransform.localScale = portrait ? new Vector3(1, 1, 1) : new Vector3(0.625f, 1.6f, 1);
        rectTransform.rotation = portrait ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, 90f);

        _3DViewRawImage.gameObject.SetActive(false);
        image.gameObject.SetActive(true);

        if (image != null && newTexture != null)
        {
            Sprite newSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f));
            image.sprite = newSprite;
        }

        UpdateImageViewer(newTitle, isLiked, newLikeCount, newComments);

        if (gameObject.GetComponent<ScrollRect>() != null)
        {
            gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        }

        commentInput.text = "";
    }

    public void UpdateImageViewer(string newTitle, bool isLiked, int newLikeCount, string newComments)
    {
        if (LikeBtn != null)
        {
            LikeBtn.GetComponent<Image>().color = isLiked ? Color.white : Color.black;
        }

        if (likeCount != null)
        {
            likeCount.text = newLikeCount.ToString();
        }

        if (imageTitle != null)
        {
            imageTitle.text = newTitle;
        }

        if (comments != null)
        {
            comments.text = newComments;
            LayoutRebuilder.ForceRebuildLayoutImmediate(comments.GetComponent<RectTransform>());
        }

        Canvas.ForceUpdateCanvases();
    }
    public void UpdateImageViewer(GameObject newObject, string newTitle, bool isLiked, int newLikeCount, string newComments)
    {
        _3DViewObject = newObject;

        foreach (var obj in _3DViewObjects)
        {
            if (obj == _3DViewObject)
            {
                obj.SetActive(true);
            }
            else
            {
                obj.SetActive(false);
            }
        }
        _3DViewRawImage.gameObject.SetActive(true);
        image.gameObject.SetActive(false);

        UpdateImageViewer(newTitle, isLiked, newLikeCount, newComments);

        if (gameObject.GetComponent<ScrollRect>() != null)
        {
            gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        }

        commentInput.text = "";
    }

    public void Open()
    {
        Canvas.ForceUpdateCanvases();
        canvas.enabled = true;
        zoomPage.gameObject.SetActive(false);
        controlPromptsUI.enabled = false;
    }

    void OnLikeBtnClick()
    {
        webSocketManager.ws.Send(
            "like:" + imageTitle.text +
                (LikeBtn.GetComponent<Image>().color.Equals(Color.white) ? ":false" : ":true")
        );
    }

    public void Close()
    {
        if (zoomPage.activeInHierarchy)
        {
            zoomPage.SetActive(false);
        }
        else
        {
            canvas.enabled = false;
            controlPromptsUI.enabled = true;
        }
    }
    public void CloseAll()
    {
        zoomPage.SetActive(false);
        canvas.enabled = false;
        controlPromptsUI.enabled = true;
    }

    void OnSubmitBtnClick()
    {
        webSocketManager.ws.Send(
            "comment:" + imageTitle.text + ":" + commentInput.text
        );

        commentInput.text = "";
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageViewer : MonoBehaviour
{
    public WebSocketManager webSocketManager;
    public Image image;
    public Button LikeBtn;
    public TMP_Text likeCount;
    public TMP_Text imageTitle;
    public TMP_InputField commentInput;
    public Button submitBtn;
    public TMP_Text comments;
    public Button closeBtn;
    public Canvas canvas;
    public RectTransform panel;
    public Canvas controlPromptsUI;

    void Start()
    {
        canvas = gameObject.GetComponent<Canvas>();
        LikeBtn.onClick.AddListener(OnLikeBtnClick);

        closeBtn.onClick.AddListener(OnCloseBtnClick);
        submitBtn.onClick.AddListener(OnSubmitBtnClick);
    }

    public void UpdateImageViewer(Texture2D newTexture, string newTitle, bool isLiked, int newLikeCount, string newComments)
    {
        if (image != null && newTexture != null)
        {
            Sprite newSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f));
            image.sprite = newSprite;
        }

        canvas.enabled = true;

        UpdateImageViewer(newTitle, isLiked, newLikeCount, newComments);

        Canvas.ForceUpdateCanvases();

        gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;

        canvas.enabled = true;

        controlPromptsUI.enabled = false;
    }

    public void UpdateImageViewer(string newTitle, bool isLiked, int newLikeCount, string newComments)
    {
        if (LikeBtn != null)
        {
            if (isLiked)
            {
                LikeBtn.GetComponent<Image>().color = Color.white;
            }
            else
            {
                LikeBtn.GetComponent<Image>().color = Color.black;
            }
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
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(comments.GetComponent<RectTransform>());
    }

    void OnLikeBtnClick()
    {
        webSocketManager.ws.Send(
            "like:" + imageTitle.text +
                (LikeBtn.GetComponent<Image>().color.Equals(Color.white) ? ":false" : ":true")
        );
    }

    void OnCloseBtnClick()
    {
        webSocketManager.ExecuteInMainThread(() =>
        {
            canvas.enabled = false;
            controlPromptsUI.enabled = true;
        });
    }

    void OnSubmitBtnClick()
    {
        webSocketManager.ws.Send(
            "comment:" + imageTitle.text + ":" + commentInput.text
        );
    }
}

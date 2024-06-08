using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;

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

    public async Task UpdateImageViewerAsync(string imageUrl, string newTitle, bool isLiked, int newLikeCount, string newComments)
    {
        Texture2D newTexture = await DownloadImageAsync(imageUrl);
        if (newTexture != null)
        {
            UpdateImageViewer(newTexture, newTitle, isLiked, newLikeCount, newComments);
        }
        else
        {
            UpdateImageViewer(newTitle, isLiked, newLikeCount, newComments);
        }
    }

    private async Task<Texture2D> DownloadImageAsync(string imageUrl)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                return ((DownloadHandlerTexture)request.downloadHandler).texture;
            }
            else
            {
                Debug.LogError("Image download failed: " + request.error);
                return null;
            }
        }
    }

    public void UpdateImageViewer(Texture2D newTexture, string newTitle, bool isLiked, int newLikeCount, string newComments)
    {
        // 批量更新 UI 元素
        canvas.enabled = false;

        if (image != null && newTexture != null)
        {
            Sprite newSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f));
            image.sprite = newSprite;
        }

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
        }

        canvas.enabled = true;
        Canvas.ForceUpdateCanvases();

        if (gameObject.GetComponent<ScrollRect>() != null)
        {
            gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        }

        controlPromptsUI.enabled = false;
    }

    public void UpdateImageViewer(string newTitle, bool isLiked, int newLikeCount, string newComments)
    {
        // 批量更新 UI 元素
        canvas.enabled = false;

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

        canvas.enabled = true;
        Canvas.ForceUpdateCanvases();

        if (gameObject.GetComponent<ScrollRect>() != null)
        {
            gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        }

        controlPromptsUI.enabled = false;
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

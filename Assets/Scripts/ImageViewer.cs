using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
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

    void Start()
    {
        canvas = gameObject.GetComponent<Canvas>();
        LikeBtn.onClick.AddListener(OnLikeBtnClick);
    }

    public void UpdateImageViewer(Texture2D newTexture, string newTitle, bool isLiked, int newLikeCount, string newComments)
    {
        // 修改圖片
        if (image != null && newTexture != null)
        {
            Sprite newSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f));
            image.sprite = newSprite;
        }

        // 修改按鈕顏色
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

        // 修改喜歡數量
        if (likeCount != null)
        {
            likeCount.text = newLikeCount.ToString();
        }

        // 修改圖片標題
        if (imageTitle != null)
        {
            imageTitle.text = newTitle;
        }

        // 修改評論
        if (comments != null)
        {
            comments.text = newComments;
        }

        canvas.enabled = true;
    }

    void OnLikeBtnClick()
    {
        webSocketManager.ws.Send(
            "like:" + imageTitle.text +
                (LikeBtn.GetComponent<Image>().color.Equals(Color.white) ? ":false" : ":true")
        );
    }

}

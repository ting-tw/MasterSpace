using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageViewer : MonoBehaviour
{
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
    }

    public void UpdateImageViewer(Texture2D newTexture, bool isLiked, int newLikeCount, string newTitle, string newComments)
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
            ColorBlock colors = LikeBtn.colors;
            if (isLiked)
            {
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.white;
                colors.pressedColor = Color.gray;
            }
            else
            {
                colors.normalColor = Color.black;
                colors.highlightedColor = Color.black;
                colors.pressedColor = Color.gray;
            }
            LikeBtn.colors = colors;
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

}

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaneClickDetector : MonoBehaviour
{
    private bool isDragging = false;
    private bool isPointerDown = false;
    private float pointerDownTime;
    private const float clickThreshold = 0.2f; // Maximum time for a click (in seconds)
    private const float dragThreshold = 10.0f; // Minimum distance for a drag (in pixels)
    ImageViewer imageViewer;
    public Canvas menuUI;
    public Texture2D texture2D;
    public bool isLiked = false;
    public int likeCount = 0;
    public string comments = "";
    public bool isPortrait;


    void Start()
    {
        imageViewer = GameObject.Find("ImageViewer").GetComponent<ImageViewer>();
        menuUI = GameObject.Find("Menu UI").GetComponent<Canvas>();
    }

    void Update()
    {
        // Handle mouse input
        if (Input.GetMouseButtonDown(0))
        {
            OnPointerDown();
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnPointerUp();
        }

        if (Input.GetMouseButton(0))
        {
            OnPointerMove(Input.mousePosition);
        }

        // Handle touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                OnPointerDown();
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                OnPointerUp();
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                OnPointerMove(touch.position);
            }
        }
    }

    void OnPointerDown()
    {
        isPointerDown = true;
        pointerDownTime = Time.time;
        isDragging = false;
    }

    void OnPointerUp()
    {
        if (isPointerDown && !isDragging)
        {
            float heldTime = Time.time - pointerDownTime;
            if (heldTime <= clickThreshold)
            {
                Vector2 screenPoint = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(screenPoint);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform == transform)
                    {
                        if (IsPointerOverUIObject(screenPoint)) return;
                        OnPlaneClicked();
                    }
                }
            }
        }
        isPointerDown = false;
    }

    void OnPointerMove(Vector2 currentPosition)
    {
        if (isPointerDown)
        {
            float distance = Vector2.Distance(currentPosition, Input.mousePosition);
            if (distance > dragThreshold)
            {
                isDragging = true;
            }
        }
    }
    private bool IsPointerOverUIObject(Vector2 v2)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = v2;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }


    void OnPlaneClicked()
    {
        if (imageViewer.canvas.enabled) return;
        if (menuUI.enabled) return;

        imageViewer.UpdateImageViewer(texture2D, gameObject.name, isLiked, likeCount, comments, isPortrait);
        imageViewer.Open();
    }

    public void UpdateImage(bool isLiked, int likeCount, string comments)
    {
        this.isLiked = isLiked;
        this.likeCount = likeCount;
        this.comments = comments;
        if (imageViewer.imageTitle.text == gameObject.name && imageViewer.enabled)
        {
            imageViewer.UpdateImageViewer(gameObject.name, isLiked, likeCount, comments);
        }
    }
}

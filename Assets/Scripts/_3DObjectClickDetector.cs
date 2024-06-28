
using UnityEngine;

public class _3DObjectClickDetector : MonoBehaviour
{
    private bool isDragging = false;
    private bool isPointerDown = false;
    private float pointerDownTime;
    private const float clickThreshold = 0.2f; // Maximum time for a click (in seconds)
    private const float dragThreshold = 10.0f; // Minimum distance for a drag (in pixels)
    private Vector2 pointerDownPosition;

    public Canvas menuUI;
    ImageViewer imageViewer;
    public bool isLiked = false;
    public int likeCount = 0;
    public string comments = "";

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
            OnPointerDown(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnPointerUp(Input.mousePosition);
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
                OnPointerDown(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                OnPointerUp(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                OnPointerMove(touch.position);
            }
        }
    }

    void OnPointerDown(Vector2 screenPosition)
    {
        isPointerDown = true;
        pointerDownTime = Time.time;
        pointerDownPosition = screenPosition;
        isDragging = false;
    }

    void OnPointerUp(Vector2 screenPosition)
    {
        if (isPointerDown && !isDragging)
        {
            float heldTime = Time.time - pointerDownTime;
            if (heldTime <= clickThreshold)
            {
                Ray ray = Camera.main.ScreenPointToRay(screenPosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform == transform)
                    {
                        OnObjectClicked();
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
            float distance = Vector2.Distance(currentPosition, pointerDownPosition);
            if (distance > dragThreshold)
            {
                isDragging = true;
            }
        }
    }

    public void Update3DObject(bool isLiked, int likeCount, string comments)
    {
        this.isLiked = isLiked;
        this.likeCount = likeCount;
        this.comments = comments;

        if (imageViewer.imageTitle.text == gameObject.name)
        {
            imageViewer.UpdateImageViewer(gameObject.name, isLiked, likeCount, comments);
        }
    }

    void OnObjectClicked()
    {
        if (imageViewer.canvas.enabled) return;
        if (menuUI.enabled) return;
        foreach (GameObject obj in imageViewer._3DViewObjects)
        {
            if (obj.name == "3DView " + gameObject.name)
            {
                imageViewer.UpdateImageViewer(obj, gameObject.name, isLiked, likeCount, comments);
                imageViewer.Open();
            }
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class Rotate3DView : MonoBehaviour
{
    public ImageViewer imageViewer;
    public GameObject targetPanel;

    const float MouseRotationSpeed = 1000f;
    const float rotationSpeed = 0.01f;

    private bool isDraggingFromPanel = false;
    private bool isPointerDown = false;
    private float pointerDownTime = 0f;
    private const float clickThreshold = 0.2f; // 點擊與拖曳的時間閾值

    void Start()
    {
        // 為Panel添加EventTrigger並設置PointerDown事件
        EventTrigger trigger = targetPanel.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        entry.callback.AddListener((eventData) => { OnPointerDown((PointerEventData)eventData); });
        trigger.triggers.Add(entry);

        EventTrigger.Entry upEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        upEntry.callback.AddListener((eventData) => { OnPointerUp((PointerEventData)eventData); });
        trigger.triggers.Add(upEntry);
    }

    void Update()
    {
        if (imageViewer._3DViewObject == null) return;

        if (isDraggingFromPanel)
        {
            HandleTouchInput();
            HandleMouseInput();
        }

        // 檢查直接點擊的情況
        if (isPointerDown && (Time.time - pointerDownTime) > clickThreshold)
        {
            isDraggingFromPanel = true;
        }
    }

    void OnPointerDown(PointerEventData eventData)
    {
        // 確認觸摸或滑鼠按下在Panel內
        if (RectTransformUtility.RectangleContainsScreenPoint(targetPanel.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera))
        {
            isPointerDown = true;
            pointerDownTime = Time.time;
        }
    }

    void OnPointerUp(PointerEventData eventData)
    {
        if (isPointerDown && !isDraggingFromPanel)
        {
            // 處理直接點擊事件
            HandleDirectClick();
        }
        isPointerDown = false;
        isDraggingFromPanel = false;
    }

    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 deltaPosition = touch.deltaPosition;
                RotateObject(deltaPosition);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDraggingFromPanel = false;
            }
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButton(0)) // 當按住滑鼠左鍵時
        {
            float rotationX = Input.GetAxis("Mouse X") * MouseRotationSpeed;
            float rotationY = Input.GetAxis("Mouse Y") * MouseRotationSpeed;

            RotateObject(new Vector2(rotationX, rotationY));
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDraggingFromPanel = false;
        }
    }

    void RotateObject(Vector2 deltaPosition)
    {
        float rotationX = deltaPosition.y * rotationSpeed;
        float rotationY = -deltaPosition.x * rotationSpeed;

        // 根據攝影機的位置計算旋轉軸
        Vector3 rotationAxisX = imageViewer._3DViewCamera.transform.right;
        Vector3 rotationAxisY = imageViewer._3DViewCamera.transform.up;

        // 旋轉目標物體
        imageViewer._3DViewObject.transform.RotateAround(imageViewer._3DViewObject.transform.position, rotationAxisX, rotationX);
        imageViewer._3DViewObject.transform.RotateAround(imageViewer._3DViewObject.transform.position, rotationAxisY, rotationY);
    }

    void HandleDirectClick()
    {
        imageViewer.OpenZoomPage();
    }
}

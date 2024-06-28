using UnityEngine;
using UnityEngine.EventSystems;

public class Rotate3DView : MonoBehaviour
{
    public ImageViewer imageViewer;
    public GameObject targetPanel;

    const float MouseRotationSpeed = 10f;
    const float TouchRotationSpeed = 0.3f;

    public bool isDraggingFromPanel = false;
    public bool isPointerDown = false;
    public Vector2 pointerDownPosition;

    void Start()
    {
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

        if (isPointerDown)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            HandleMouseInput();
#else
            HandleTouchInput();
#endif
        }
    }

    void OnPointerDown(PointerEventData eventData)
    {
        // 確認觸摸或滑鼠按下在Panel內
        if (RectTransformUtility.RectangleContainsScreenPoint(targetPanel.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera))
        {
            isPointerDown = true;
            pointerDownPosition = eventData.position;
        }
    }

    void OnPointerUp(PointerEventData eventData)
    {
        if (pointerDownPosition == eventData.position) {
            imageViewer.OpenZoomPage();
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
                Vector2 deltaPosition = touch.deltaPosition * TouchRotationSpeed;
                RotateObject(deltaPosition);
                isDraggingFromPanel = true;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDraggingFromPanel = false;
            }
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            float rotationX = Input.GetAxis("Mouse X") * MouseRotationSpeed;
            float rotationY = Input.GetAxis("Mouse Y") * MouseRotationSpeed;

            RotateObject(new Vector2(rotationX, rotationY));
            isDraggingFromPanel = true;
        }
    }

    void RotateObject(Vector2 deltaPosition)
    {
        float rotationX = deltaPosition.y;
        float rotationY = -deltaPosition.x;

        // 根據攝影機的位置計算旋轉軸
        Vector3 rotationAxisX = imageViewer._3DViewCamera.transform.right;
        Vector3 rotationAxisY = imageViewer._3DViewCamera.transform.up;

        // 旋轉目標物體
        imageViewer._3DViewObject.transform.RotateAround(imageViewer._3DViewObject.transform.position, rotationAxisX, rotationX);
        imageViewer._3DViewObject.transform.RotateAround(imageViewer._3DViewObject.transform.position, rotationAxisY, rotationY);
    }

    // void HandleDirectClick()
    // {
    //     imageViewer.OpenZoomPage();
    // }
}

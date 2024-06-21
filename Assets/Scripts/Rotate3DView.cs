using UnityEngine;
using UnityEngine.EventSystems;

public class Rotate3DView : MonoBehaviour
{
    public ImageViewer imageViewer;
    public GameObject targetPanel;

    private bool isDraggingFromPanel = false;

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
    }

    void Update()
    {
        if (imageViewer._3DViewObject == null) return;

        if (isDraggingFromPanel)
        {
            HandleTouchInput();
            HandleMouseInput();
        }
    }

    void OnPointerDown(PointerEventData eventData)
    {
        // 確認觸摸或滑鼠按下在Panel內
        if (RectTransformUtility.RectangleContainsScreenPoint(targetPanel.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera))
        {
            isDraggingFromPanel = true;
        }
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
            float rotationSpeed = 200f;
            float rotationX = Input.GetAxis("Mouse X") * rotationSpeed;
            float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed;

            RotateObject(new Vector2(rotationX, rotationY));
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDraggingFromPanel = false;
        }
    }

    void RotateObject(Vector2 deltaPosition)
    {
        float rotationSpeed = 0.05f; // 控制旋轉速度的變量
        float rotationX = deltaPosition.y * rotationSpeed;
        float rotationY = -deltaPosition.x * rotationSpeed;

        // 根據攝影機的位置計算旋轉軸
        Vector3 rotationAxisX = imageViewer._3DViewCamera.transform.right;
        Vector3 rotationAxisY = imageViewer._3DViewCamera.transform.up;

        // 旋轉目標物體
        imageViewer._3DViewObject.transform.Rotate(rotationAxisX, rotationX, Space.World);
        imageViewer._3DViewObject.transform.Rotate(rotationAxisY, rotationY, Space.World);
    }
}

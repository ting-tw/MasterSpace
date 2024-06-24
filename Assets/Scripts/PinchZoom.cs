using UnityEngine;
using UnityEngine.UI;

public class PinchZoom : MonoBehaviour
{
    public float zoomSpeed = 0.1f; // 縮放速度
    private Vector2 initialTouchPosition1;
    private Vector2 initialTouchPosition2;
    private float initialDistance;
    private Vector3 initialScale;

    void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                initialTouchPosition1 = touch1.position;
                initialTouchPosition2 = touch2.position;
                initialDistance = Vector2.Distance(initialTouchPosition1, initialTouchPosition2);
                initialScale = transform.localScale;
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                Vector2 currentTouchPosition1 = touch1.position;
                Vector2 currentTouchPosition2 = touch2.position;
                float currentDistance = Vector2.Distance(currentTouchPosition1, currentTouchPosition2);

                float scaleFactor = currentDistance / initialDistance;
                transform.localScale = initialScale * scaleFactor;
            }
        }
    }
}

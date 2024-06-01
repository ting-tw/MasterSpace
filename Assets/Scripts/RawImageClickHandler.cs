using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RawImageClickHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool pressdown = false;
    private bool pressup = false;
    public bool PressDown()
    {
        if (pressdown)
        {
            pressdown = false;
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool PressUp()
    {
        if (pressup)
        {
            pressup = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    RawImage rawImage;
    void Start()
    {
        rawImage = GetComponent<RawImage>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == rawImage.gameObject)
        {
            pressdown = true;
            pressup = false;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressdown = false;
        pressup = true;

    }
}

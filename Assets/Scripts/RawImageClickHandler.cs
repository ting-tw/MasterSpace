using Invector.vCharacterController;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RawImageClickHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool isSwitch;
    public KeyCode keyboardInput;

    [HideInInspector]
    public bool switchValue = false;
    private bool pressdown = false;
    private bool pressup = false;
    public RawImage rawImage;
    public bool PressDown()
    {
        if (pressdown)
        {
            if (isSwitch)
            {
                switchValue = !switchValue;
                rawImage.color = switchValue ? Color.gray : Color.white;
            }
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

    void Start()
    {
        rawImage = GetComponent<RawImage>();
    }
    void Update()
    {
        if (Input.GetKeyDown(keyboardInput))
        {
            OnKeyDown();
        }
        if (Input.GetKeyUp(keyboardInput))
        {
            OnKeyUp();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == rawImage.gameObject)
        {
            OnKeyDown();
        }
    }

    public void OnKeyDown()
    {
        pressdown = true;
        pressup = false;
        if (!isSwitch)
            rawImage.color = Color.gray;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        OnKeyUp();
    }
    public void OnKeyUp()
    {
        pressdown = false;
        pressup = true;
        if (!isSwitch)
            rawImage.color = Color.white;
    }
}

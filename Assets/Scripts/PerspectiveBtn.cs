using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;

public class PerspectiveBtn : MonoBehaviour
{
    public RawImageClickHandler perspectiveBtn;
    public RawImageClickHandler strafeBtn;
    public vThirdPersonController controller;

    void Update()
    {
        if (perspectiveBtn.PressDown())
        {
            vThirdPersonCamera c = Camera.main.GetComponent<vThirdPersonCamera>();
            if (perspectiveBtn.switchValue)
            {
                c.defaultDistance = 0f;

                int layerMask = LayerMask.NameToLayer("Player");
                Camera.main.cullingMask &= ~(1 << layerMask);

                controller.isStrafing = true;
            }
            else
            {
                c.defaultDistance = 2.5f;

                StartCoroutine(WaitAndExecute(0.25f, () =>
                {
                    int layerMask = LayerMask.NameToLayer("Player");
                    Camera.main.cullingMask |= 1 << layerMask;

                    controller.isStrafing = strafeBtn.switchValue;
                }));
            }
        }
    }

    IEnumerator WaitAndExecute(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();

    }
}

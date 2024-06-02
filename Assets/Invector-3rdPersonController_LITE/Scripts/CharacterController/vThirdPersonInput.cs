using UnityEngine;

namespace Invector.vCharacterController
{
    public class vThirdPersonInput : MonoBehaviour
    {
        #region Variables       

        [Header("Controller Input")]
        public string horizontalInput = "Horizontal";
        public string verticallInput = "Vertical";
        public KeyCode jumpInput = KeyCode.Space;
        public KeyCode strafeInput = KeyCode.Tab;
        public KeyCode sprintInput = KeyCode.LeftShift;

        [Header("Camera Input")]
        public string rotateCameraXInput = "Mouse X";
        public string rotateCameraYInput = "Mouse Y";

        [HideInInspector] public vThirdPersonController cc;
        [HideInInspector] public vThirdPersonCamera tpCamera;
        [HideInInspector] public Camera cameraMain;

        #endregion

        [SerializeField]
        private FixedJoystick _joystick;
        public GameObject SprintBtn;
        public GameObject JumpBtn;
        public GameObject StrafeBtn;
        private RawImageClickHandler sprintClickHandler;
        private RawImageClickHandler jumpClickHandler;
        private RawImageClickHandler strafeClickHandler;

        public float rotationSpeed; // 旋轉速度



        protected virtual void Start()
        {
            sprintClickHandler = SprintBtn.GetComponent<RawImageClickHandler>();
            jumpClickHandler = JumpBtn.GetComponent<RawImageClickHandler>();
            strafeClickHandler = StrafeBtn.GetComponent<RawImageClickHandler>();

            InitilizeController();
            InitializeTpCamera();
        }

        protected virtual void FixedUpdate()
        {
            cc.UpdateMotor();               // updates the ThirdPersonMotor methods
            cc.ControlLocomotionType();     // handle the controller locomotion type and movespeed
            cc.ControlRotationType();       // handle the controller rotation type
        }

        protected virtual void Update()
        {
            InputHandle();                  // update the input methods
            cc.UpdateAnimator();            // updates the Animator Parameters
        }

        public virtual void OnAnimatorMove()
        {
            cc.ControlAnimatorRootMotion(); // handle root motion animations 
        }

        #region Basic Locomotion Inputs

        protected virtual void InitilizeController()
        {
            cc = GetComponent<vThirdPersonController>();

            if (cc != null)
                cc.Init();
        }

        protected virtual void InitializeTpCamera()
        {
            if (tpCamera == null)
            {
                tpCamera = FindObjectOfType<vThirdPersonCamera>();
                if (tpCamera == null)
                    return;
                if (tpCamera)
                {
                    tpCamera.SetMainTarget(this.transform);
                    tpCamera.Init();
                }
            }
        }

        protected virtual void InputHandle()
        {
            MoveInput();
            CameraInput();
            SprintInput();
            StrafeInput();
            JumpInput();
        }

        public virtual void MoveInput()
        {
            cc.input.x = Mathf.Abs(_joystick.Horizontal) >= Mathf.Abs(Input.GetAxis(horizontalInput)) ?
             _joystick.Horizontal : Input.GetAxis(horizontalInput);
            cc.input.z = Mathf.Abs(_joystick.Vertical) >= Mathf.Abs(Input.GetAxis(verticallInput)) ?
            _joystick.Vertical : Input.GetAxis(verticallInput);
        }

        protected virtual void CameraInput()
        {
            if (!cameraMain)
            {
                if (!Camera.main) Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
                else
                {
                    cameraMain = Camera.main;
                    cc.rotateTarget = cameraMain.transform;
                }
            }

            if (cameraMain)
            {
                cc.UpdateMoveDirection(cameraMain.transform);
            }

            if (tpCamera == null)
                return;

            float Y = 0;
            float X = 0;
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    Vector2 touchPosition = touch.position;

                    // 檢查觸摸位置是否在螢幕右半邊
                    if (touchPosition.x > Screen.width / 2)
                    {
                        // 根據觸摸事件類型執行不同的操作
                        if (touch.phase == TouchPhase.Moved)
                        {
                            // 根據觸摸移動距離旋轉物體
                            Y = touch.deltaPosition.y * rotationSpeed * Time.deltaTime;
                            X = touch.deltaPosition.x * rotationSpeed * Time.deltaTime;
                        }
                    }
                }
            }

            // develop (
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                Y = Input.GetAxis(rotateCameraYInput) * 1.4f;
                X = Input.GetAxis(rotateCameraXInput) * 1.4f;
            }
            // ) develop

            tpCamera.RotateCamera(X, Y);
        }

        protected virtual void StrafeInput()
        {
            if (strafeClickHandler.PressDown() || Input.GetKeyDown(strafeInput))
                cc.Strafe();
        }

        protected virtual void SprintInput()
        {
            if (sprintClickHandler.PressDown() || Input.GetKeyDown(sprintInput))
                cc.Sprint(true);
            else if (sprintClickHandler.PressUp() || Input.GetKeyUp(sprintInput))
                cc.Sprint(false);
        }

        /// <summary>
        /// Conditions to trigger the Jump animation & behavior
        /// </summary>
        /// <returns></returns>
        protected virtual bool JumpConditions()
        {
            return cc.isGrounded && cc.GroundAngle() < cc.slopeLimit && !cc.isJumping && !cc.stopMove;
        }

        /// <summary>
        /// Input to trigger the Jump 
        /// </summary>
        protected virtual void JumpInput()
        {
            // if (Input.GetKeyDown(jumpInput) && JumpConditions())
            if ((jumpClickHandler.PressDown() || Input.GetKeyDown(jumpInput)) && JumpConditions())
                cc.Jump();
        }

        #endregion
    }
}
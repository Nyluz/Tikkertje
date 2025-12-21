using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class InputScript : MonoBehaviour
    {
        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        [Header("Input Action References")]
        public InputActionAsset inputActions;
        private InputAction lookAction;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction slapAction;
        private InputAction sprintAction;

        [Header("Raw Input Values")]
        public Vector2 lookInput;
        public Vector2 moveInput;
        public bool jumpInput;
        public bool slapInput;
        public float sprintInput;

        [Header("Actions")]
        public bool look;
        public bool move;
        public bool jump;
        public bool slap;
        public bool sprint;

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void Awake()
        {
            lookAction = inputActions.FindAction("Look");
            moveAction = inputActions.FindAction("Move");
            jumpAction = inputActions.FindAction("Jump");
            slapAction = inputActions.FindAction("Slap");
            sprintAction = inputActions.FindAction("Sprint");
        }

        private void LateUpdate()
        {
            // Raw inputs
            lookInput = lookAction.ReadValue<Vector2>();
            moveInput = moveAction.ReadValue<Vector2>();
            jumpInput = jumpAction.ReadValue<float>() != 0f;
            slapInput = slapAction.ReadValue<float>() != 0f;
            sprintInput = sprintAction.ReadValue<float>();

            // Player actions
            look = lookInput != Vector2.zero;
            move = moveInput != Vector2.zero;
            jump = jumpAction.WasPressedThisFrame();
            slap = slapAction.WasPressedThisFrame();
            sprint = sprintAction.IsPressed();
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

}
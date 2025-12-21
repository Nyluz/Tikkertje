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
        public InputActionReference lookAction;
        public InputActionReference moveAction;
        public InputActionReference jumpAction;
        public InputActionReference slapAction;
        public InputActionReference sprintAction;

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

        private void LateUpdate()
        {
            // Raw inputs
            lookInput = lookAction.action.ReadValue<Vector2>();
            moveInput = moveAction.action.ReadValue<Vector2>();
            jumpInput = jumpAction.action.ReadValue<float>() != 0f;
            slapInput = slapAction.action.ReadValue<float>() != 0f;
            sprintInput = sprintAction.action.ReadValue<float>();

            // Player actions
            look = lookInput != Vector2.zero;
            move = moveInput != Vector2.zero;
            jump = jumpAction.action.WasPressedThisFrame();
            slap = slapAction.action.WasPressedThisFrame();
            sprint = sprintAction.action.IsPressed();


            if (jump)
                print("button!");
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

}
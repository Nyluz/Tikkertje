using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class InputScript : MonoBehaviour
    {
        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput playerInput;

        private InputAction lookAction;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction slapAction;
        private InputAction sprintAction;
#endif

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
#if ENABLE_INPUT_SYSTEM
            playerInput = GetComponent<PlayerInput>();

            // IMPORTANT: use the per-player actions instance from PlayerInput
            var actions = playerInput.actions;

            lookAction = actions.FindAction("Look", true);
            moveAction = actions.FindAction("Move", true);
            jumpAction = actions.FindAction("Jump", true);
            slapAction = actions.FindAction("Slap", true);
            sprintAction = actions.FindAction("Sprint", true);
#endif
        }

        private void Start()
        {
            var pi = GetComponent<PlayerInput>();
            foreach (var d in pi.devices)
                Debug.Log($"Player {pi.playerIndex} device: {d.displayName}");
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            lookInput = lookAction.ReadValue<Vector2>();
            moveInput = moveAction.ReadValue<Vector2>();
            jumpInput = jumpAction.ReadValue<float>() != 0f;
            slapInput = slapAction.ReadValue<float>() != 0f;
            sprintInput = sprintAction.ReadValue<float>();

            look = lookInput != Vector2.zero;
            move = moveInput != Vector2.zero;
            jump = jumpAction.WasPressedThisFrame();
            slap = slapAction.WasPressedThisFrame();
            sprint = sprintAction.IsPressed();
#endif
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}

using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;




#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;
        [Tooltip("Rotation speed of the character")]
        public float RotationSpeed = 1.0f;
        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.1f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;
        [SerializeField] private bool isJumping;
        [SerializeField] private bool isFalling;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;
        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.5f;
        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 90.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -90.0f;

        private CinemachineCamera currentCamera;
        [SerializeField] private List<CinemachineCamera> cameras = new List<CinemachineCamera>();

        // player
        private float _speed;
        [SerializeField] private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private CharacterController controller;
        private Animator animator;
        private StarterAssetsInputs _input;

        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference moveAction;

        private RagdollScript ragdollScript;
        private HitTarget hitTargetScript;

        [SerializeField] private Transform characterModel;
        [SerializeField] private Transform head;

        [SerializeField] private float headRotationClamp_min;
        [SerializeField] private float headRotationClamp_max;

        private float currentVelocity_x;
        private float currentVelocity_y;

        [SerializeField] private bool bodyDetached;
        [SerializeField] private bool standingUp;

        [SerializeField] private float _animationBlendSpeed;
        [SerializeField] private GameObject[] firstPersonHideModels;

        public float PovSensitivity = 1.5f;

        public CinemachineOrbitalFollow orbitalFollow;

        [SerializeField] private bool manualSwitchCamera;
        private Coroutine disableModelsRoutine;

        [SerializeField] float maxRagdollFallVelocity = -5f;
        [SerializeField] float ragdollFallingVelocityMulitplier = 10f;


        CinemachineOrbitalFollow orbital;

        private enum Modes
        {
            firstPerson,
            thirdPerson
        }

        private Modes previousMode;
        private Modes mode()
        {
            if (ragdollScript.characterState == RagdollScript.CharacterState.Idle)
                return Modes.firstPerson;
            else
                return Modes.thirdPerson;
        }

        private bool hitTargetEnabled()
        {
            if (currentCamera == cameras[0])
                return true;
            else return false;
        }

        private bool hasInput()
        {
            if (moveAction.action.ReadValue<Vector2>().magnitude != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            ragdollScript = GetComponentInChildren<RagdollScript>();
            hitTargetScript = GetComponent<HitTarget>();
            controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

            orbital = cameras[1].GetComponent<CinemachineOrbitalFollow>();
        }

        private void Start()
        {
            SwitchCamera(cameras[0]);

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            Application.targetFrameRate = 60;
        }

        private void Update()
        {
            hitTargetScript.enabled = hitTargetEnabled();

            animator.SetBool("PlayerInput", hasInput());

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                //ToggleCamera();
                //StartCoroutine(RecenterRoutine(orbitalFollow));
            }
        }

        private void FixedUpdate()
        {
            if (ragdollScript.characterState != RagdollScript.CharacterState.Idle)
                return;

            JumpAndGravity();
            GroundedCheck();
            Move();

        }

        private void LateUpdate()
        {
            if (mode() == Modes.firstPerson)
            {
                FPSMode();

                if (previousMode == Modes.thirdPerson && !manualSwitchCamera)
                    SwitchCamera(cameras[0]);
            }
            else if (mode() == Modes.thirdPerson)
            {
                RagdollMode();

                if (previousMode == Modes.firstPerson && !manualSwitchCamera)
                    SwitchCamera(cameras[1]);
            }

            previousMode = mode();
        }

        [SerializeField] float horizontalSpeed = 180f;
        [SerializeField] float verticalSpeed = 120f;
        [SerializeField] float minVertical = -30f;
        [SerializeField] float maxVertical = 60f;

        private void RagdollMode()
        {
            // When player enters ragdoll mode
            if (ragdollScript.characterState == RagdollScript.CharacterState.Ragdoll && !bodyDetached)
            {
                bodyDetached = true;
            }

            if (bodyDetached)
            {
                // When player gets out of ragdoll mode
                if (ragdollScript.characterState == RagdollScript.CharacterState.ResettingBones)
                {
                    bodyDetached = false;
                    standingUp = true;
                }
            }

            Vector2 look = lookAction.action.ReadValue<Vector2>();

            // Horizontal orbit (yaw)
            orbital.HorizontalAxis.Value +=
                look.x * horizontalSpeed * Time.deltaTime;

            // Vertical orbit (pitch)
            orbital.VerticalAxis.Value -=
                look.y * verticalSpeed * Time.deltaTime;

            orbital.VerticalAxis.Value =
                Mathf.Clamp(orbital.VerticalAxis.Value, minVertical, maxVertical);

        }

        [SerializeField] float yawSpeed = 10f;
        [SerializeField] float pitchSpeed = 10f;
        [SerializeField] float minPitch = -60f;
        [SerializeField] float maxPitch = 60f;

        float pitch;

        private void FPSMode()
        {
            //// Rotate the player left and right
            //Vector2 look = lookAction.action.ReadValue<Vector2>();
            //float deltaTimeMultiplier = Mouse.current != null ? 1f : Time.deltaTime;
            //float rotation = look.x * RotationSpeed * deltaTimeMultiplier;
            //transform.Rotate(Vector3.up * rotation);

            Vector2 look = lookAction.action.ReadValue<Vector2>();

            // Yaw (player) – mouse is fine without dt
            transform.Rotate(Vector3.up * look.x * yawSpeed * Time.deltaTime);

            // Pitch (camera) – MUST be time-scaled
            pitch -= look.y * pitchSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            cameras[0].transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            // Rotate head with camera
            float x = currentCamera.transform.localEulerAngles.x;
            if (x > 180f) x -= 360f;
            x = Mathf.Clamp(x, headRotationClamp_min, headRotationClamp_max);
            Vector3 euler = head.localEulerAngles;
            euler.x = x;
            head.localEulerAngles = euler;

            // When player is done standing up
            if (ragdollScript.characterState == RagdollScript.CharacterState.Idle && standingUp)
            {
                standingUp = false;

                // Set Player position to ragdoll position
                Vector3 ragdollPos = ragdollScript.hipsBone.position;

                controller.enabled = false;
                transform.position = ragdollPos;
                controller.enabled = true;

                characterModel.transform.localPosition = Vector3.zero;
                characterModel.transform.localRotation = Quaternion.identity;
            }
        }

        private void SwitchCamera(CinemachineCamera newCamera)
        {
            foreach (var camera in cameras)
            {
                camera.Priority = 0;
            }
            newCamera.Priority = 10;
            currentCamera = newCamera;

            // If third person
            if (currentCamera == cameras[1])
            {
                // Reset third person camera's position
                StartCoroutine(RecenterRoutine(orbitalFollow));

                // Turn models on again
                if (disableModelsRoutine != null)
                    StopCoroutine(disableModelsRoutine);
                disableModelsRoutine = StartCoroutine(SwitchModelLayersAfterDelay(0f, 3));

            }
            // If first person
            else if (currentCamera == cameras[0])
            {
                // Turn models off
                if (disableModelsRoutine != null)
                    StopCoroutine(disableModelsRoutine);
                disableModelsRoutine = StartCoroutine(SwitchModelLayersAfterDelay(1f, 10));
            }
        }

        IEnumerator SwitchModelLayersAfterDelay(float delay, int layer)
        {
            yield return new WaitForSeconds(delay);

            foreach (var model in firstPersonHideModels)
            {
                model.layer = layer;
            }
        }


        private void ToggleCamera()
        {
            if (currentCamera == cameras[0])
                SwitchCamera(cameras[1]);
            else if (currentCamera == cameras[1])
                SwitchCamera(cameras[0]);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                // move
                inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
            }

            // move the player
            controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            Vector3 localVelocity = transform.InverseTransformDirection(controller.velocity);

            currentVelocity_x = Mathf.Lerp(currentVelocity_x, localVelocity.x, _animationBlendSpeed * Time.deltaTime);
            currentVelocity_y = Mathf.Lerp(currentVelocity_y, localVelocity.z, _animationBlendSpeed * Time.deltaTime);

            animator.SetFloat(Animator.StringToHash("X_Velocity"), currentVelocity_x);
            animator.SetFloat(Animator.StringToHash("Y_Velocity"), currentVelocity_y);
        }

        private void JumpAndGravity()
        {
            float fallingVelocity = 0;

            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    fallingVelocity = _verticalVelocity;
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    isJumping = true;
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }

                if (isFalling)
                {
                    isFalling = false;
                    if ((fallingVelocity + 2.5f) < maxRagdollFallVelocity)
                    {
                        ragdollScript.TriggerRagdoll(Vector3.up * fallingVelocity * ragdollFallingVelocityMulitplier, characterModel.transform.position);
                    }
                    else
                    {
                        float stunTime = 0.2f;
                        if (fallingVelocity < 7f)
                            stunTime = 0.5f;

                        StartCoroutine(MoveZeroForSeconds(stunTime));
                    }
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                if (isJumping && _fallTimeoutDelta < 0)
                {
                    isJumping = false;
                }

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }

                isFalling = true;

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }

            animator.SetBool("IsJumping", isJumping);
            animator.SetBool("IsFalling", isFalling);
            animator.SetBool("IsGrounded", Grounded);
        }

        IEnumerator MoveZeroForSeconds(float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                controller.Move(Vector3.zero);
                t += Time.deltaTime;
                yield return null;
            }
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        IEnumerator RecenterRoutine(CinemachineOrbitalFollow orbital)
        {
            RecenterAxis(ref orbital.HorizontalAxis);
            RecenterAxis(ref orbital.VerticalAxis);
            RecenterAxis(ref orbital.RadialAxis);

            yield return null; // wait 1 frame

            DisableRecentering(ref orbital.HorizontalAxis);
            DisableRecentering(ref orbital.VerticalAxis);
            DisableRecentering(ref orbital.RadialAxis);

        }

        private void RecenterAxis(ref InputAxis axis)
        {
            var inputAxis = axis;

            inputAxis.Center = 0f;
            inputAxis.Recentering.Enabled = true;
            inputAxis.Recentering.Wait = 0;
            inputAxis.Recentering.Time = 0f;

            axis = inputAxis;
        }

        private void DisableRecentering(ref InputAxis axis)
        {
            var recentering = axis.Recentering;
            recentering.Enabled = false;
            axis.Recentering = recentering;
        }

    }
}
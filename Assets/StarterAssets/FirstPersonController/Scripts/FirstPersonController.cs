using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]

    public class FirstPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        [SerializeField] private float MoveSpeed = 2.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        [SerializeField] private float SprintSpeed = 6.0f;
        [Tooltip("Acceleration and deceleration")]
        [SerializeField] private float SpeedChangeRate = 10.0f;
        [Tooltip("The height the player can jump")]
        [SerializeField] private float JumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        [SerializeField] private float Gravity = -15.0f;
        [SerializeField] private float _terminalVelocity = 53.0f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        [SerializeField] private float FallTimeout = 0.30f;
        [SerializeField] private LayerMask GroundLayers;
        [SerializeField] private float GroundedOffset = -0.14f;
        [SerializeField] private float GroundedRadius = 0.5f;

        [Header("Ragdoll")]
        [SerializeField] private float maxRagdollFallVelocity = -5f;
        [SerializeField] private float minStunFallVelocity = -8f;
        [SerializeField] private float ragdollFallingVelocityMulitplier = 10f;

        [Header("3rd Person Camera")]
        [SerializeField] private float horizontalSpeed = 180f;
        [SerializeField] private float verticalSpeed = 120f;
        [SerializeField] private float minVertical = -30f;
        [SerializeField] private float maxVertical = 60f;

        [Header("First Person Camera")]
        [SerializeField] private float yawSpeed = 10f;
        [SerializeField] private float pitchSpeed = 10f;
        [SerializeField] private float minPitch = -60f;
        [SerializeField] private float maxPitch = 60f;
        [Space(10)]

        [SerializeField] private float headRotationClamp_min;
        [SerializeField] private float headRotationClamp_max;
        [SerializeField] private float _animationBlendSpeed;

        [Header("Dependencies")]
        [SerializeField] private List<CinemachineCamera> cameras = new List<CinemachineCamera>();
        [SerializeField] private GameObject[] firstPersonHideModels;
        [SerializeField] private Transform characterModel;
        [SerializeField] private Transform head;

        [Header("State")]
        [SerializeField] private float currentVelocity_x;
        [SerializeField] private float currentVelocity_y;
        [SerializeField] private float _verticalVelocity;
        [SerializeField] private bool Grounded = true;
        [SerializeField] private bool isJumping;
        [SerializeField] private bool isFalling;
        [SerializeField] private bool bodyDetached;
        [SerializeField] private bool standingUp;
        [SerializeField] private float speed;
        [SerializeField] private CinemachineCamera currentCamera;

        private Coroutine disableModelsRoutine;
        private CinemachineOrbitalFollow orbitalFollow;
        private RagdollScript ragdollScript;
        private HitTarget hitTargetScript;
        private CharacterController controller;
        private Animator animator;
        private InputScript input;
        private PlayerStats stats;

        private float _fallTimeoutDelta;
        private float pitch;

        private enum Modes
        {
            firstPerson,
            thirdPerson
        }

        private Modes previousMode;

        private Modes mode()
        {
            if (ragdollScript.state == RagdollScript.State.Idle)
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
            if (input.moveInput.magnitude != 0)
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
            input = GetComponent<InputScript>();
            orbitalFollow = cameras[1].GetComponent<CinemachineOrbitalFollow>();
            stats = GetComponent<PlayerStats>();
        }

        private void Start()
        {
            SwitchCamera(cameras[0]);
            _fallTimeoutDelta = FallTimeout;

            Application.targetFrameRate = 60;
        }

        private void Update()
        {
            hitTargetScript.enabled = hitTargetEnabled();
            animator.SetBool("PlayerInput", hasInput());
        }

        private void FixedUpdate()
        {
            if (ragdollScript.state != RagdollScript.State.Idle)
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

                if (previousMode == Modes.thirdPerson)
                    SwitchCamera(cameras[0]);
            }
            else if (mode() == Modes.thirdPerson)
            {
                RagdollMode();

                if (previousMode == Modes.firstPerson)
                    SwitchCamera(cameras[1]);
            }

            previousMode = mode();
        }

        private void FPSMode()
        {
            RotateFPSCamera();

            // When player is done standing up
            if (ragdollScript.state == RagdollScript.State.Idle && standingUp)
            {
                standingUp = false;

                // Set Player position to ragdoll position
                Vector3 ragdollPos = ragdollScript.hipsBone.position;

                controller.enabled = false;
                transform.position = ragdollPos - new Vector3(0, .75f, 0);
                controller.enabled = true;

                characterModel.transform.localPosition = Vector3.zero;
                characterModel.transform.localRotation = Quaternion.identity;
            }
        }

        private void RotateFPSCamera()
        {
            // Rotate player
            Vector2 look = input.lookInput;
            transform.Rotate(Vector3.up * look.x * yawSpeed * Time.deltaTime);
            // Rotate camera
            pitch -= look.y * pitchSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            cameras[0].transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            // Rotate character head with camera angle
            float x = currentCamera.transform.localEulerAngles.x;
            if (x > 180f) x -= 360f;
            x = Mathf.Clamp(x, headRotationClamp_min, headRotationClamp_max);
            Vector3 euler = head.localEulerAngles;
            euler.x = x;
            head.localEulerAngles = euler;
        }

        private void RagdollMode()
        {
            RotateThirdPersonCamera();

            // When player enters ragdoll mode
            if (ragdollScript.state == RagdollScript.State.Ragdoll && !bodyDetached)
            {
                bodyDetached = true;
            }

            if (bodyDetached)
            {
                // When player gets out of ragdoll mode
                if (ragdollScript.state == RagdollScript.State.ResettingBones)
                {
                    bodyDetached = false;
                    standingUp = true;
                }
            }
        }

        private void RotateThirdPersonCamera()
        {
            Vector2 look = input.lookInput;
            // Horizontal orbit (yaw)
            orbitalFollow.HorizontalAxis.Value +=
                look.x * horizontalSpeed * Time.deltaTime;

            // Vertical orbit (pitch)
            orbitalFollow.VerticalAxis.Value -=
                look.y * verticalSpeed * Time.deltaTime;

            orbitalFollow.VerticalAxis.Value =
                Mathf.Clamp(orbitalFollow.VerticalAxis.Value, minVertical, maxVertical);
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

        private void Move()
        {
            if (!input.sprint)
                stats.GainStamina();

            if (input.sprint && stats.currentStamina > 0)
                stats.DrainStamina();

            Vector2 moveInput = input.moveInput;

            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = input.sprint && stats.currentStamina > 0 ? SprintSpeed : MoveSpeed;

            // if there is no input, set the target speed to 0
            if (moveInput == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = moveInput.magnitude;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                speed = Mathf.Round(speed * 1000f) / 1000f;
            }
            else
            {
                speed = targetSpeed * inputMagnitude;
            }

            // normalise input direction
            Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (moveInput != Vector2.zero)
            {
                // move
                inputDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            }

            // Move the player
            controller.Move(inputDirection.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // Animator
            Vector3 localVelocity = transform.InverseTransformDirection(controller.velocity);
            currentVelocity_x = Mathf.Lerp(currentVelocity_x, localVelocity.x, _animationBlendSpeed * Time.deltaTime);
            currentVelocity_y = Mathf.Lerp(currentVelocity_y, localVelocity.z, _animationBlendSpeed * Time.deltaTime);

            animator.SetFloat(Animator.StringToHash("X_Velocity"), currentVelocity_x);
            animator.SetFloat(Animator.StringToHash("Y_Velocity"), currentVelocity_y);

            float t = Mathf.InverseLerp(MoveSpeed, SprintSpeed, speed);
            float animSpeed = Mathf.Lerp(1f, 1.33f, t);
            animator.SetFloat("MoveSpeed", animSpeed);

        }

        private void JumpAndGravity()
        {
            float fallingVelocity = 0;

            if (Grounded)
            {
                isJumping = false;

                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    fallingVelocity = _verticalVelocity;
                    _verticalVelocity = -2f;
                }

                // Jump
                if (input.jump)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    isJumping = true;
                    print("Jump!");
                }

                if (isFalling)
                {
                    isFalling = false;

                    if ((fallingVelocity) < maxRagdollFallVelocity)
                    {
                        ragdollScript.TriggerRagdoll(Vector3.up * fallingVelocity * ragdollFallingVelocityMulitplier, characterModel.transform.position);
                    }
                    else
                    {
                        float stunTime = 0.1f;
                        if (fallingVelocity < -8f)
                        {
                            float t = Mathf.InverseLerp(minStunFallVelocity, maxRagdollFallVelocity, fallingVelocity);
                            stunTime = Mathf.Lerp(0.1f, 1.5f, t);
                        }
                        StartCoroutine(MoveZeroForSeconds(stunTime));
                    }
                }

            }
            else
            {
                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    isJumping = false;
                    isFalling = true;
                }

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
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class HitTarget : MonoBehaviour
{
    [SerializeField]
    private CharacterController characterController;
    private PlayerUI playerUI;
    private InputScript input;
    private PlayerInput playerInput;

    [SerializeField]
    public float slapForce;

    [SerializeField]
    private float velocityMultiplier = 5f;

    [SerializeField] private Sprite crosshairTexture;
    [SerializeField] private Sprite handTexture;

    private Camera player_camera;

    [SerializeField] private float tagDistance;
    [SerializeField] private float slapVelocity;

    [Header("Status")]
    [SerializeField] private float velocity;
    [SerializeField] private bool tagAbility;


    void Awake()
    {
        player_camera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        input = GetComponent<InputScript>();
        playerUI = GetComponentInChildren<PlayerUI>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        velocity = characterController.velocity.magnitude;
        float distance = 0f;

        int layers = LayerMask.GetMask("PlayerCollider");

        Ray ray = new Ray(player_camera.transform.position, player_camera.transform.forward);

        Debug.DrawRay(
            player_camera.transform.position,
            player_camera.transform.forward * 10f,
            Color.red
        );

        tagAbility = GameModeManager.Instance.players[playerInput.playerIndex].tagAbility;

        if (tagAbility)
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, layers))
            {
                CharacterController controller = hitInfo.collider.GetComponentInChildren<CharacterController>();
                if (controller != null)
                {
                    // Get target player index
                    int targetPlayerIndex = hitInfo.transform.gameObject.GetComponent<PlayerInput>().playerIndex;

                    // Check for player without tag ability
                    if (GameModeManager.Instance.players[targetPlayerIndex].tagAbility)
                        return;

                    distance = Vector3.Distance(player_camera.transform.position, controller.transform.position);
                    if (distance < tagDistance)
                    {
                        playerUI.SetCrosshair(handTexture, 128);
                        if (input.slap)
                        {
                            input.slap = false;
                            RagdollScript ragdoll = controller.GetComponentInChildren<RagdollScript>();
                            Vector3 forceDirection = ragdoll.transform.position - player_camera.transform.position;
                            forceDirection.y = 1;
                            forceDirection.Normalize();

                            slapVelocity = velocity * velocityMultiplier;
                            Vector3 force = forceDirection * (slapForce + slapVelocity);

                            ragdoll.TriggerRagdoll(force, hitInfo.point);

                            SoundManager.PlaySound(transform, "event:/Slap");
                            GameModeManager.Instance.SlapAction(playerInput.playerIndex, targetPlayerIndex);
                        }
                        return;
                    }
                }
            }
        }

        // Bomb game mode
        if (GameModeManager.Instance.gameMode == GameMode.Bomb)
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
            {
                if (hitInfo.collider.gameObject.tag == "Bomb")
                {
                    distance = Vector3.Distance(player_camera.transform.position, hitInfo.collider.transform.position);
                    if (distance < tagDistance)
                    {
                        playerUI.SetCrosshair(handTexture, 128);

                        if (input.slap)
                        {
                            input.slap = false;

                            GameModeManager.Instance.ObtainBomb(playerInput.playerIndex);
                            GameModeManager.Instance.playTimeLeft = GameModeManager.minuteToSeconds(GameModeManager.Instance.fuseTime);
                            SoundManager.PlaySound(transform, "event:/Slap");
                            hitInfo.collider.gameObject.SetActive(false);
                        }
                    }
                    return;
                }
            }
            playerUI.SetCrosshair(crosshairTexture, 16);
        }
    }
}

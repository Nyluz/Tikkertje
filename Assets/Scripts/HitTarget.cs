using StarterAssets;
using UnityEngine;

public class HitTarget : MonoBehaviour
{
    [SerializeField]
    private CharacterController characterController;
    private PlayerUI playerUI;
    private InputScript input;

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
    [SerializeField]
    private float velocity;

    void Awake()
    {
        player_camera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        input = GetComponent<InputScript>();
        playerUI = GetComponentInChildren<PlayerUI>();
    }

    void Update()
    {
        velocity = characterController.velocity.magnitude;


        float distance = 0f;

        Ray ray = new Ray(player_camera.transform.position, player_camera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            RagdollScript ragdoll = hitInfo.collider.GetComponentInParent<RagdollScript>();
            if (ragdoll != null)
            {
                distance = Vector3.Distance(player_camera.transform.position, ragdoll.transform.position);

                if (distance < tagDistance && ragdoll.tag == "Sheep")
                {
                    playerUI.SetCrosshair(handTexture, 64);
                    if (input.slap)
                    {
                        Vector3 forceDirection = ragdoll.transform.position - player_camera.transform.position;
                        forceDirection.y = 1;
                        forceDirection.Normalize();

                        slapVelocity = velocity * velocityMultiplier;
                        Vector3 force = forceDirection * (slapForce + slapVelocity);

                        ragdoll.TriggerRagdoll(force, hitInfo.point);
                    }
                    return;
                }
            }
        }

        playerUI.SetCrosshair(crosshairTexture, 16);
    }
}

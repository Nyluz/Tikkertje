using StarterAssets;
using UnityEngine;

public class HitTarget : MonoBehaviour
{
    [SerializeField]
    private CharacterController characterController;
    private InputScript input;

    [SerializeField]
    public float slapForce;

    [SerializeField]
    private float velocityMultiplier = 5f;

    [SerializeField]
    private Texture2D crosshairTexture;
    [SerializeField]
    private Texture2D handTexture;
    private float crosshairSize;

    private Texture2D currentCrosshair;
    private Camera player_camera;

    [SerializeField] private float tagDistance;
    [SerializeField] private float slapVelocity;

    [Header("Status")]
    [SerializeField]
    private float velocity;

    void OnGUI()
    {
        float size = crosshairSize;
        float x = (Screen.width - size) / 2f;
        float y = (Screen.height - size) / 2f;

        GUI.DrawTexture(new Rect(x, y, size, size), currentCrosshair);
    }

    void Awake()
    {
        player_camera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        input = GetComponent<InputScript>();
        currentCrosshair = crosshairTexture;
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
                    currentCrosshair = handTexture;
                    crosshairSize = 64f;
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
        currentCrosshair = crosshairTexture;
        crosshairSize = 16f;
    }
}

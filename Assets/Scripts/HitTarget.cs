using UnityEngine;
using UnityEngine.InputSystem;

public class HitTarget : MonoBehaviour
{
    [SerializeField]
    private CharacterController characterController;

    [SerializeField]
    public float slapForce;

    [SerializeField]
    private float velocityMultiplier = 5f;

    [SerializeField]
    private Texture2D crosshairTexture;

    private Camera m_camera;

    [Header("Status")]
    [SerializeField]
    private float velocity;

    void OnGUI()
    {
        float size = 32;
        float x = (Screen.width - size) / 2f;
        float y = (Screen.height - size) / 2f;

        GUI.DrawTexture(new Rect(x, y, size, size), crosshairTexture);
    }

    void Awake()
    {
        m_camera = GetComponent<Camera>();
        characterController = transform.parent.parent.GetComponent<CharacterController>();
    }

    void Update()
    {
        velocity = characterController.velocity.magnitude;
        float slapVelocity = velocity * velocityMultiplier;

        if (Mouse.current.leftButton.isPressed)
        {
            Ray ray = new Ray(m_camera.transform.position, m_camera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Ragdoll ragdoll = hitInfo.collider.GetComponentInParent<Ragdoll>();
                if (ragdoll != null)
                {
                    Vector3 forceDirection = ragdoll.transform.position - m_camera.transform.position;
                    forceDirection.y = 1;
                    forceDirection.Normalize();

                    Vector3 force = forceDirection * (slapForce + slapVelocity);

                    ragdoll.TriggerRagdoll(force, hitInfo.point);
                }
            }
        }
    }
}

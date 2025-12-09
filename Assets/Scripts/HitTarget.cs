using UnityEngine;
using UnityEngine.InputSystem;

public class HitTarget : MonoBehaviour
{
    [SerializeField]
    private CharacterController characterController;

    private Camera m_camera;

    public float maximumForce;

    private float velocity;

    [SerializeField]
    private float velocityMultiplier = 4f;

    public Texture2D crosshairTexture;

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
    }

    void Update()
    {
        velocity = characterController.velocity.magnitude * 5f;

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

                    Vector3 force = forceDirection * (maximumForce + velocity);

                    ragdoll.TriggerRagdoll(force, hitInfo.point);
                }
            }
        }
    }
}

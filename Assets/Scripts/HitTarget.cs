using UnityEngine;
using UnityEngine.InputSystem;

public class HitTarget : MonoBehaviour
{
    private Camera m_camera;
    public float maximumForce;

    void Awake()
    {
        m_camera = GetComponent<Camera>();
    }

    void Update()
    {
        if (Mouse.current.leftButton.isPressed == true)
        {
            Ray ray = m_camera.ScreenPointToRay(Pointer.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Ragdoll ragdoll = hitInfo.collider.GetComponentInParent<Ragdoll>();
                if (ragdoll != null)
                {
                    Vector3 forceDirection = ragdoll.transform.position - m_camera.transform.position;
                    forceDirection.y = 1;
                    forceDirection.Normalize();
                    Vector3 force = forceDirection * maximumForce;

                    ragdoll.TriggerRagdoll(force, hitInfo.point);

                }
            }
        }
    }
}

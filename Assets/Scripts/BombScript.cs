using UnityEngine;
using UnityEngine.InputSystem;

public class BombScript : MonoBehaviour
{
    public RagdollScript ragdoll;

    public float explosionRadius = 5f;
    public float explosionForce = 800f;
    public float upwardModifier = 1f;
    public LayerMask affectedLayers;

    public GameObject particle;

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Explode();
        }
    }

    public void Explode()
    {
        ragdoll.TriggerRagdollBomb();

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            explosionRadius
        );

        foreach (var hit in hits)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null) continue;

            rb.AddExplosionForce(
                explosionForce,
                transform.position,
                explosionRadius,
                upwardModifier,
                ForceMode.Impulse
            );
        }

        GameObject explosion = Instantiate(particle, transform.position, Quaternion.identity);
        SoundManager.PlaySound(transform, "event:/Explosion");
    }

}

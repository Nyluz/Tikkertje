using UnityEngine;

public class ImpactSound : MonoBehaviour
{
    public float impactThreshold = 5f;
    public float impactCooldown = 0.2f;

    private float nextAllowedTime = 0f;

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time < nextAllowedTime)
            return;

        float impactForce = collision.impulse.magnitude / Time.fixedDeltaTime;

        if (impactForce > impactThreshold)
        {
            SoundManager.Instance.PlayImpactSound(transform.position);
            nextAllowedTime = Time.time + impactCooldown;
        }
    }
}

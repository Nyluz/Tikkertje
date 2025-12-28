using UnityEngine;

public class FootstepSoundPlayer : MonoBehaviour
{
    public AudioClip[] clips;
    private Animator animator;
    private float lastFootstep;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        var footstep = animator.GetFloat("Footstep");

        if (lastFootstep > 0 && footstep < 0 || lastFootstep < 0 && footstep > 0)
        {
            var randomClip = clips[Random.Range(0, clips.Length - 1)];
            AudioSource.PlayClipAtPoint(randomClip, transform.position);
        }

        lastFootstep = footstep;
    }
}

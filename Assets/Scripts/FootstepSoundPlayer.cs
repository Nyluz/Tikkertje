using UnityEngine;

public class FootstepSoundPlayer : MonoBehaviour
{
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
            SoundManager.PlaySound(transform, "event:/Footsteps");
        }

        lastFootstep = footstep;
    }
}

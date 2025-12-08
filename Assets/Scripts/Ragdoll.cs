using System.Linq;
using UnityEngine;
public class Ragdoll : MonoBehaviour
{
    private enum CharacterState
    {
        Idle,
        Ragdoll
    }

    private Rigidbody[] ragdollRigidbodies;
    private Animator animator;
    private CharacterState characterState = CharacterState.Idle;

    void Awake()
    {
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();
        DisableRagdoll();
    }


    void Update()
    {
        switch (characterState)
        {
            case CharacterState.Idle:
                IdleBehaviour();
                break;
            case CharacterState.Ragdoll:
                RagdollBehaviour();
                break;

        }
    }

    public void TriggerRagdoll(Vector3 force, Vector3 hitpoint)
    {
        EnableRagdoll();

        Rigidbody hitRigidbody = ragdollRigidbodies.OrderBy(rigidbody => Vector3.Distance(rigidbody.position, hitpoint)).FirstOrDefault();
        hitRigidbody.AddForceAtPosition(force, hitpoint, ForceMode.Impulse);
        characterState = CharacterState.Ragdoll;
    }

    private void DisableRagdoll()
    {
        foreach (var rigidbody in ragdollRigidbodies)
        {
            rigidbody.isKinematic = true;
        }

        animator.enabled = true;
    }

    private void EnableRagdoll()
    {
        foreach (var rigidbody in ragdollRigidbodies)
        {
            rigidbody.isKinematic = false;
        }

        animator.enabled = false;
    }

    private void IdleBehaviour()
    {

    }

    private void RagdollBehaviour()
    {

    }
}

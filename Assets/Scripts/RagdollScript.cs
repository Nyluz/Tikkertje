using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
public class RagdollScript : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    private class BoneTransform
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
    }

    public enum State
    {
        Idle,
        Ragdoll,
        StandingUp,
        ResettingBones
    }

    [SerializeField]
    private string faceUpstandUpStateName;

    [SerializeField]
    private string faceDownstandUpStateName;

    [SerializeField]
    private string faceUpStandUpClipName;

    [SerializeField]
    private string faceDownStandUpClipName;

    [SerializeField]
    private float timeToResetBones;

    [SerializeField] private Rigidbody[] ragdollRigidbodies;
    private Animator animator;
    public State state = State.Idle;
    [SerializeField] private float timeToWakeUp;
    private float fallTimer;
    private float laydownTimer;
    [SerializeField] private float standUpVelocity = 0.01f;

    [HideInInspector] public Transform hipsBone;

    private BoneTransform[] faceUpStandUpBoneTransforms;
    private BoneTransform[] faceDownStandUpBoneTransforms;

    private BoneTransform[] ragdollBoneTransforms;
    [SerializeField] private Transform[] bones;
    private float elapsedResetBonesTime;

    private bool isFacingUp;

    [SerializeField] Vector3 forceDirection;
    [SerializeField] float forceMagnitude;


    void Awake()
    {
        characterController = GetComponentInParent<CharacterController>();
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();

        hipsBone = animator.GetBoneTransform(HumanBodyBones.Hips);
        bones = hipsBone.GetComponentsInChildren<Transform>();

        faceUpStandUpBoneTransforms = new BoneTransform[bones.Length];
        faceDownStandUpBoneTransforms = new BoneTransform[bones.Length];

        ragdollBoneTransforms = new BoneTransform[bones.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            faceUpStandUpBoneTransforms[i] = new BoneTransform();
            faceDownStandUpBoneTransforms[i] = new BoneTransform();

            ragdollBoneTransforms[i] = new BoneTransform();
        }

        PopulateAnimationStartBoneTransforms(faceUpStandUpClipName, faceUpStandUpBoneTransforms);
        PopulateAnimationStartBoneTransforms(faceDownStandUpClipName, faceDownStandUpBoneTransforms);

        DisableRagdoll();
    }


    void Update()
    {
        switch (state)
        {
            case State.Idle:
                break;
            case State.Ragdoll:
                RagdollBehaviour();
                break;
            case State.StandingUp:
                StandingUpBehaviour();
                break;
            case State.ResettingBones:
                ResetBonesBehaviour();
                break;
        }

        if (Keyboard.current.enterKey.IsPressed())
        {
            if (state == State.Idle)
            {
                Vector3 force = forceDirection * forceMagnitude;
                TriggerRagdoll(force, bones[0].transform.position);
            }
        }
    }

    public void TriggerRagdoll(Vector3 force, Vector3 hitpoint)
    {
        EnableRagdoll();

        Rigidbody hitRigidbody = ragdollRigidbodies.OrderBy(rigidbody => Vector3.Distance(rigidbody.position, hitpoint)).FirstOrDefault();
        hitRigidbody.AddForceAtPosition(force, hitpoint, ForceMode.Impulse);
        state = State.Ragdoll;
    }

    private void DisableRagdoll()
    {
        foreach (var rigidbody in ragdollRigidbodies)
        {
            rigidbody.isKinematic = true;
        }

        animator.Rebind();
        animator.Update(0f);

        animator.enabled = true;

        if (characterController)
            characterController.excludeLayers = 0;
    }

    private void EnableRagdoll()
    {
        foreach (var rigidbody in ragdollRigidbodies)
        {
            rigidbody.isKinematic = false;
        }

        animator.enabled = false;

        if (characterController)
            characterController.excludeLayers = ~0;
    }

    private void StandingUpBehaviour()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(GetStandUpStateName()) == false)
        {
            state = State.Idle;
        }
    }

    private void ResetBonesBehaviour()
    {
        elapsedResetBonesTime += Time.deltaTime;
        float elapsedPercentage = elapsedResetBonesTime / timeToResetBones;

        BoneTransform[] standUpBoneTransforms = GetStandUpBonesTransforms();

        for (int i = 0; i < bones.Length; i++)
        {
            bones[i].localPosition = Vector3.Lerp(
                ragdollBoneTransforms[i].Position,
                standUpBoneTransforms[i].Position,
                elapsedPercentage);

            bones[i].localRotation = Quaternion.Lerp(
                ragdollBoneTransforms[i].Rotation,
                standUpBoneTransforms[i].Rotation, elapsedPercentage);
        }

        if (elapsedPercentage >= 1)
        {
            state = State.StandingUp;
            DisableRagdoll();
            animator.Play(GetStandUpStateName(), 0, 0f);
        }
    }

    private void RagdollBehaviour()
    {
        fallTimer += Time.deltaTime;

        if (fallTimer >= timeToWakeUp)
        {

            if (ragdollRigidbodies[0].linearVelocity.sqrMagnitude < standUpVelocity)
            {
                laydownTimer += Time.deltaTime;

                if (laydownTimer >= timeToWakeUp)
                {
                    fallTimer = 0;
                    laydownTimer = 0;

                    isFacingUp = hipsBone.forward.y > 0f;

                    AlignPositionToHips();
                    AlignRotationToHips();

                    PopulateBoneTransforms(ragdollBoneTransforms);

                    state = State.ResettingBones;
                    elapsedResetBonesTime = 0f;

                }
            }
        }
    }

    private void AlignRotationToHips()
    {
        Vector3 originalHipsPosition = hipsBone.position;
        Quaternion originalHipsRotation = hipsBone.rotation;
        Vector3 desiredDirection = hipsBone.up;
        if (isFacingUp)
        {
            desiredDirection *= -1;
        }

        desiredDirection.y = 0;
        desiredDirection.Normalize();

        Quaternion fromToRotation = Quaternion.FromToRotation(transform.forward, desiredDirection);
        transform.rotation *= fromToRotation;

        hipsBone.position = originalHipsPosition;
        hipsBone.rotation = originalHipsRotation;
    }

    private void AlignPositionToHips()
    {
        Vector3 originalHipsPosition = hipsBone.position;
        transform.position = hipsBone.position;

        Vector3 positionOffset = GetStandUpBonesTransforms()[0].Position;
        positionOffset.y = 0;
        positionOffset = transform.rotation * positionOffset;
        transform.position -= positionOffset;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo))
        {
            transform.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);
        }

        hipsBone.position = originalHipsPosition;
    }

    private void PopulateBoneTransforms(BoneTransform[] boneTransforms)
    {
        for (int i = 0; i < bones.Length; i++)
        {
            boneTransforms[i].Position = bones[i].localPosition;
            boneTransforms[i].Rotation = bones[i].localRotation;
        }
    }

    private void PopulateAnimationStartBoneTransforms(string clipName, BoneTransform[] boneTransforms)
    {
        Vector3 positionBeforeSampling = transform.position;
        Quaternion rotationBeforeSampling = transform.rotation;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                clip.SampleAnimation(gameObject, 0);
                PopulateBoneTransforms(boneTransforms);
                break;
            }
        }

        transform.position = positionBeforeSampling;
        transform.rotation = rotationBeforeSampling;
    }

    private string GetStandUpStateName()
    {
        return isFacingUp ? faceUpstandUpStateName : faceDownstandUpStateName;
    }

    private BoneTransform[] GetStandUpBonesTransforms()
    {
        return isFacingUp ? faceUpStandUpBoneTransforms : faceDownStandUpBoneTransforms;
    }
}

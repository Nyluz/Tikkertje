using System.Linq;
using UnityEngine;
public class Ragdoll : MonoBehaviour
{
    private class BoneTransform
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
    }

    private enum CharacterState
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

    private Rigidbody[] ragdollRigidbodies;
    private Animator animator;
    private CharacterState characterState = CharacterState.Idle;
    [SerializeField]
    private float timeToWakeUp;
    private float timer;

    private Transform hipsBone;

    private BoneTransform[] faceUpStandUpBoneTransforms;
    private BoneTransform[] faceDownStandUpBoneTransforms;

    private BoneTransform[] ragdollBoneTransforms;
    private Transform[] bones;
    private float elapsedResetBonesTime;

    private bool isFacingUp;

    void Awake()
    {
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
        switch (characterState)
        {
            case CharacterState.Idle:
                IdleBehaviour();
                break;
            case CharacterState.Ragdoll:
                RagdollBehaviour();
                break;
            case CharacterState.StandingUp:
                StandingUpBehaviour();
                break;
            case CharacterState.ResettingBones:
                ResetBonesBehaviour();
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

    private void StandingUpBehaviour()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(GetStandUpStateName()) == false)
        {
            characterState = CharacterState.Idle;
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
            characterState = CharacterState.StandingUp;
            DisableRagdoll();
            animator.Play(GetStandUpStateName(), 0, 0f);
        }
    }

    private void RagdollBehaviour()
    {
        timer += Time.deltaTime;
        if (timer >= timeToWakeUp)
        {
            timer = 0;
            isFacingUp = hipsBone.forward.y > 0f;
            AlignPositionToHips();
            AlignRotationToHips();

            PopulateBoneTransforms(ragdollBoneTransforms);

            characterState = CharacterState.ResettingBones;
            elapsedResetBonesTime = 0;
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

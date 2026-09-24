using TheWard.Combat;
using UnityEngine;

namespace TheWard.Character
{
    [RequireComponent(typeof(Animator)), DefaultExecutionOrder(200)]
    public sealed class FirstPersonArms : MonoBehaviour
    {
        private const int ActionLayer = 1;
        private const int LeftFingerLayer = 2;
        private const int RightFingerLayer = 3;
        private const float BlendInDuration = 0.06f;
        private const float BlendOutDuration = 0.12f;

        [SerializeField] private WeaponController weapons;
        [SerializeField] private Transform view;
        [SerializeField] private Vector3 actionOffset = new Vector3(0f, 0.07f, -0.45f);
        [Header("Grip Timing")]
        [SerializeField, Range(0f, 0.4f)] private float gripHoldFraction = 0.2f;
        [SerializeField, Range(0.6f, 1f)] private float gripReturnFraction = 0.75f;
        [SerializeField, Range(0.01f, 0.2f)] private float gripBlendFraction = 0.1f;
        private Vector3 readyRigPosition;

        private static readonly int HoldingGun = Animator.StringToHash("HoldingGun");
        private static readonly int CancelAction = Animator.StringToHash("CancelAction");
        private static readonly int ActionSpeed = Animator.StringToHash("ActionSpeed");
        private Animator animator;
        private Animator bodyAnimator;
        private Transform rightHand, leftHand;
        private Weapon actionWeapon;
        private AnimationClip actionClip;
        private int actionTrigger;
        private bool contactSent;
        private float actionDuration, actionWeight, actionProgress;
        private Vector3 readyRightPosition, readyLeftPosition;
        private Quaternion readyRightRotation, readyLeftRotation;
        private Quaternion wristToGripRotation, leftWristToGripRotation;
        private Vector3 wristToGripPosition;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            readyRigPosition = transform.localPosition;
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            PlayerBodyAnimator body = weapons != null ? weapons.GetComponentInChildren<PlayerBodyAnimator>() : null;
            if (body != null) bodyAnimator = body.GetComponent<Animator>();
        }

        public void PlayAction(Weapon weapon, AnimationClip clip, float duration)
        {
            if (clip == null) return;
            actionWeapon = weapon;
            actionClip = clip;
            actionTrigger = Animator.StringToHash(clip.name);
            contactSent = false;
            actionProgress = 0f;
            actionDuration = duration;
            wristToGripRotation = Quaternion.Inverse(rightHand.rotation) * weapon.RightHandGrip.rotation;
            leftWristToGripRotation = Quaternion.Inverse(leftHand.rotation) * weapon.LeftHandGrip.rotation;
            wristToGripPosition = rightHand.InverseTransformPoint(weapon.RightHandGrip.position);
            readyRightPosition = view.InverseTransformPoint(weapon.RightHandGrip.position);
            readyRightRotation = Quaternion.Inverse(view.rotation) * weapon.RightHandGrip.rotation;
            readyLeftPosition = view.InverseTransformPoint(weapon.LeftHandGrip.position);
            readyLeftRotation = Quaternion.Inverse(view.rotation) * weapon.LeftHandGrip.rotation;
            animator.SetFloat(ActionSpeed, clip.length / duration);
            animator.ResetTrigger(CancelAction);
            animator.SetTrigger(actionTrigger);
            if (bodyAnimator != null)
            {
                bodyAnimator.SetFloat(ActionSpeed, clip.length / duration);
                bodyAnimator.ResetTrigger(CancelAction);
                bodyAnimator.SetTrigger(actionTrigger);
            }
        }

        public void StopAction(Weapon weapon, bool cancelled)
        {
            if (actionWeapon != weapon) return;
            actionWeapon = null;
            actionClip = null;
            animator.ResetTrigger(actionTrigger);
            if (cancelled) animator.SetTrigger(CancelAction);
            if (bodyAnimator != null)
            {
                bodyAnimator.ResetTrigger(actionTrigger);
                if (cancelled) bodyAnimator.SetTrigger(CancelAction);
            }
            ApplyActionWeight(0f);
        }

        // These are called by events on the imported action clips.
        public void ActionContact(AnimationEvent animationEvent)
        {
            if (actionWeapon == null || contactSent || animationEvent.animatorClipInfo.clip != actionClip) return;
            contactSent = true;
            actionWeapon.OnAnimationContact();
        }

        public void ActionComplete(AnimationEvent animationEvent)
        {
            if (actionWeapon == null || animationEvent.animatorClipInfo.clip != actionClip) return;
            actionWeapon.OnAnimationComplete();
        }

        private void Update()
        {
            animator.SetBool(HoldingGun, weapons != null && weapons.ActiveWeapon is GunWeapon);
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(ActionLayer);
            float elapsed = state.shortNameHash == actionTrigger ? state.normalizedTime * actionDuration : 0f;
            actionProgress = actionDuration > 0f ? Mathf.Clamp01(elapsed / actionDuration) : 0f;
            float weight = actionWeapon == null ? 0f : Mathf.SmoothStep(0f, 1f,
                Mathf.Min(elapsed / BlendInDuration, (actionDuration - elapsed) / BlendOutDuration));
            ApplyActionWeight(weight);
        }

        private void ApplyActionWeight(float weight)
        {
            actionWeight = weight;
            transform.localPosition = readyRigPosition + actionOffset * actionWeight;
            // A zero-weight layer stops delivering events. Keep it active until completion.
            animator.SetLayerWeight(ActionLayer, actionWeapon != null ? Mathf.Max(0.001f, actionWeight) : 0f);
            UpdateGripWeights();
            if (bodyAnimator != null) bodyAnimator.SetLayerWeight(ActionLayer, actionWeight);
        }

        private void UpdateGripWeights()
        {
            // Keep melee fingers wrapped throughout the swing. Reloads and throws
            // release later and regain their grip before the action finishes.
            float releaseWeight = 0f;
            if (actionWeapon is GunWeapon || actionWeapon is GrenadeWeapon)
            {
                float blend = Mathf.Max(0.01f, gripBlendFraction);
                float release = Mathf.SmoothStep(0f, 1f,
                    (actionProgress - gripHoldFraction) / blend);
                float returnToGrip = Mathf.SmoothStep(0f, 1f,
                    (actionProgress - gripReturnFraction) / blend);
                releaseWeight = release * (1f - returnToGrip);
            }
            animator.SetLayerWeight(LeftFingerLayer, 1f - releaseWeight);
            // The throwing hand must hold the grenade until the release event.
            animator.SetLayerWeight(RightFingerLayer,
                actionWeapon is GrenadeWeapon && contactSent ? 1f - releaseWeight : 1f);
        }

        private void LateUpdate()
        {
            if (actionWeapon == null) return;
            if (actionWeapon is MeleeWeapon melee && melee.UsesForwardSwing) return;
            // During actions the clip drives the wrist and the weapon follows it.
            // IK uses the saved ready pose, never this moving grip, to avoid feedback.
            Transform grip = actionWeapon.RightHandGrip;
            Transform held = actionWeapon.transform;
            held.rotation = rightHand.rotation * wristToGripRotation * Quaternion.Inverse(grip.rotation) * held.rotation;
            held.position += rightHand.TransformPoint(wristToGripPosition) - grip.position;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            Weapon weapon = weapons != null ? weapons.ActiveWeapon : null;
            UpdateElbow(AvatarIKHint.RightElbow, new Vector3(0.5f, -0.65f, 0.2f));
            UpdateElbow(AvatarIKHint.LeftElbow, new Vector3(-0.5f, -0.65f, 0.2f));
            if (actionWeapon != null)
            {
                if (actionWeapon is MeleeWeapon melee && melee.UsesForwardSwing)
                {
                    // Sample after Animator evaluation so the visible contact matches
                    // the clip event rather than the previous frame's Update time.
                    AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(ActionLayer);
                    float progress = state.shortNameHash == actionTrigger ? Mathf.Clamp01(state.normalizedTime) : 0f;
                    melee.ApplyForwardSwing(view, progress);
                    UpdateHand(AvatarIKGoal.RightHand, melee.RightHandGrip);
                    UpdateHand(AvatarIKGoal.LeftHand, melee.LeftHandGrip);
                    return;
                }
                FitAnimatedHand(AvatarIKGoal.RightHand, rightHand, wristToGripRotation,
                    readyRightPosition, readyRightRotation);
                FitAnimatedHand(AvatarIKGoal.LeftHand, leftHand, leftWristToGripRotation,
                    readyLeftPosition, readyLeftRotation);
                return;
            }
            UpdateHand(AvatarIKGoal.RightHand, weapon != null ? weapon.RightHandGrip : null);
            UpdateHand(AvatarIKGoal.LeftHand, weapon != null ? weapon.LeftHandGrip : null);
        }

        private void FitAnimatedHand(AvatarIKGoal goal, Transform hand, Quaternion gripRotation,
            Vector3 readyPosition, Quaternion readyRotation)
        {
            // Preserve the clip's swing, but keep the wrist in front of the lens.
            Vector3 position = view.InverseTransformPoint(hand.position);
            position.z = Mathf.Max(0.42f, position.z);
            position.y = Mathf.Min(0.05f, position.y);
            SetHand(goal, view.TransformPoint(Vector3.Lerp(readyPosition, position, actionWeight)),
                Quaternion.Slerp(view.rotation * readyRotation, hand.rotation * gripRotation, actionWeight));
        }

        private void UpdateElbow(AvatarIKHint elbow, Vector3 offset)
        {
            animator.SetIKHintPositionWeight(elbow, 1f - actionWeight);
            animator.SetIKHintPosition(elbow, view.TransformPoint(offset));
        }

        private void UpdateHand(AvatarIKGoal hand, Transform grip)
        {
            if (grip != null) SetHand(hand, grip.position, grip.rotation);
            else
            {
                animator.SetIKPositionWeight(hand, 0f);
                animator.SetIKRotationWeight(hand, 0f);
            }
        }

        private void SetHand(AvatarIKGoal hand, Vector3 position, Quaternion rotation)
        {
            animator.SetIKPositionWeight(hand, 1f);
            animator.SetIKRotationWeight(hand, 1f);
            animator.SetIKPosition(hand, position);
            animator.SetIKRotation(hand, rotation);
        }
    }
}

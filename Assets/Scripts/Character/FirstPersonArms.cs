using TheWard.Combat;
using UnityEngine;

namespace TheWard.Character
{
    [RequireComponent(typeof(Animator)), DefaultExecutionOrder(200)]
    public sealed class FirstPersonArms : MonoBehaviour
    {
        [SerializeField] private WeaponController weapons;
        [SerializeField] private Transform view;
        [SerializeField] private Vector3 actionOffset = new Vector3(0f, 0.07f, -0.45f);
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
        private float actionDuration, actionWeight;
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
            PlayerBodyAnimator body = GetComponentInParent<WeaponController>()
                .GetComponentInChildren<PlayerBodyAnimator>();
            if (body != null) bodyAnimator = body.GetComponent<Animator>();
        }

        public void PlayAction(Weapon weapon, AnimationClip clip, float duration)
        {
            if (clip == null) return;
            actionWeapon = weapon;
            actionClip = clip;
            actionTrigger = Animator.StringToHash(clip.name);
            contactSent = false;
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
            actionWeight = 0f;
            transform.localPosition = readyRigPosition;
            animator.SetLayerWeight(1, 0f);
            animator.SetLayerWeight(2, 1f);
            if (bodyAnimator != null) bodyAnimator.SetLayerWeight(1, 0f);
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
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(1);
            float elapsed = state.shortNameHash == actionTrigger ? state.normalizedTime * actionDuration : 0f;
            actionWeight = actionWeapon == null ? 0f : Mathf.SmoothStep(0f, 1f,
                Mathf.Min(elapsed / 0.06f, (actionDuration - elapsed) / 0.12f));
            transform.localPosition = readyRigPosition + actionOffset * actionWeight;
            // A zero-weight layer stops delivering events. Keep it active until completion.
            animator.SetLayerWeight(1, actionWeapon != null ? Mathf.Max(0.001f, actionWeight) : 0f);
            animator.SetLayerWeight(2, 1f - actionWeight);
            if (bodyAnimator != null) bodyAnimator.SetLayerWeight(1, actionWeight);
        }

        private void LateUpdate()
        {
            if (actionWeapon == null) return;
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
                Quaternion.Slerp(view.rotation * readyRotation, hand.rotation * gripRotation, actionWeight), 1f);
        }

        private void UpdateElbow(AvatarIKHint elbow, Vector3 offset)
        {
            animator.SetIKHintPositionWeight(elbow, 1f - actionWeight);
            animator.SetIKHintPosition(elbow, view.TransformPoint(offset));
        }

        private void UpdateHand(AvatarIKGoal hand, Transform grip)
        {
            if (grip != null) SetHand(hand, grip.position, grip.rotation, 1f);
            else
            {
                animator.SetIKPositionWeight(hand, 0f);
                animator.SetIKRotationWeight(hand, 0f);
            }
        }

        private void SetHand(AvatarIKGoal hand, Vector3 position, Quaternion rotation, float weight)
        {
            animator.SetIKPositionWeight(hand, weight);
            animator.SetIKRotationWeight(hand, weight);
            animator.SetIKPosition(hand, position);
            animator.SetIKRotation(hand, rotation);
        }
    }
}

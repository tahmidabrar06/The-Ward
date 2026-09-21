using TheWard.Combat;
using UnityEngine;

namespace TheWard.Character
{
    [RequireComponent(typeof(Animator))]
    public sealed class FirstPersonArms : MonoBehaviour
    {
        [SerializeField] private WeaponController weapons;
        [SerializeField] private Transform view;

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            Weapon weapon = weapons != null ? weapons.ActiveWeapon : null;
            if (view != null)
            {
                UpdateElbow(AvatarIKHint.RightElbow, new Vector3(0.5f, -0.65f, 0.2f));
                UpdateElbow(AvatarIKHint.LeftElbow, new Vector3(-0.5f, -0.65f, 0.2f));
            }
            UpdateHand(AvatarIKGoal.RightHand, weapon != null ? weapon.RightHandGrip : null);
            UpdateHand(AvatarIKGoal.LeftHand, weapon != null ? weapon.LeftHandGrip : null);
        }

        private void UpdateElbow(AvatarIKHint elbow, Vector3 offset)
        {
            animator.SetIKHintPositionWeight(elbow, 1f);
            animator.SetIKHintPosition(elbow, view.TransformPoint(offset));
        }

        private void UpdateHand(AvatarIKGoal hand, Transform grip)
        {
            float weight = grip != null ? 1f : 0f;
            animator.SetIKPositionWeight(hand, weight);
            animator.SetIKRotationWeight(hand, weight);
            if (grip == null) return;

            animator.SetIKPosition(hand, grip.position);
            animator.SetIKRotation(hand, grip.rotation);
        }
    }
}

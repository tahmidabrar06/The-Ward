using TheWard.Character;
using UnityEngine;

namespace TheWard.Combat
{
    // The controller and hand rig only need these shared references and actions.
    public abstract class Weapon : MonoBehaviour
    {
        [SerializeField] private string displayName = "Weapon";
        [SerializeField] protected Transform rightHandGrip;
        [SerializeField] protected Transform leftHandGrip;
        [SerializeField] protected Transform attackOrigin;

        protected Vector3 startingPosition;
        protected Quaternion startingRotation;
        protected bool isAnimating;
        private FirstPersonArms arms;

        public string DisplayName => displayName;
        public Transform RightHandGrip => rightHandGrip;
        public Transform LeftHandGrip => leftHandGrip;
        public Camera AimCamera { get; private set; }
        public bool IsAnimating => isAnimating;
        protected Transform Origin => attackOrigin != null ? attackOrigin : transform;

        protected virtual void Awake()
        {
            AimCamera = attackOrigin != null
                ? attackOrigin.GetComponent<Camera>()
                : GetComponentInParent<Camera>();
            if (AimCamera != null) arms = AimCamera.GetComponentInChildren<FirstPersonArms>();
            startingPosition = transform.localPosition;
            startingRotation = transform.localRotation;
        }

        public abstract void HandleInput(bool pressed, bool held, bool released);
        public virtual void Reload() { }

        public Ray GetAimRay()
        {
            return AimCamera != null
                ? AimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))
                : new Ray(Origin.position, Origin.forward);
        }

        protected bool PlayAnimation(AnimationClip clip, float duration)
        {
            if (arms == null || clip == null)
            {
                Debug.LogWarning($"Assign an animation and first-person arm rig for {displayName}.", this);
                return false;
            }
            isAnimating = true;
            arms.PlayAction(this, clip, duration);
            return true;
        }

        public virtual void OnAnimationContact() { }
        public virtual void OnAnimationComplete() => FinishAnimation(false);

        protected void FinishAnimation(bool cancelled = true)
        {
            arms?.StopAction(this, cancelled);
            isAnimating = false;
            ResetWeaponTransform();
        }

        protected void ResetWeaponTransform()
        {
            transform.localPosition = startingPosition;
            transform.localRotation = startingRotation;
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            FinishAnimation();
        }
    }
}

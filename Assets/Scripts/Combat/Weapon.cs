using System.Collections;
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

        protected IEnumerator MoveWeapon(Vector3 position, Quaternion rotation, float duration)
        {
            Vector3 fromPosition = transform.localPosition;
            Quaternion fromRotation = transform.localRotation;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.localPosition = Vector3.Lerp(fromPosition, position, t);
                transform.localRotation = Quaternion.Slerp(fromRotation, rotation, t);
                yield return null;
            }
            transform.SetLocalPositionAndRotation(position, rotation);
        }

        protected void ResetWeaponTransform()
        {
            transform.localPosition = startingPosition;
            transform.localRotation = startingRotation;
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            isAnimating = false;
            ResetWeaponTransform();
        }
    }
}

using UnityEngine;

namespace TheWard.Character
{
    [RequireComponent(typeof(Animator))]
    public sealed class PlayerBodyAnimator : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private Animator animator;
        private CharacterController player;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            player = GetComponentInParent<CharacterController>();
        }

        private void Update()
        {
            Vector3 velocity = player.velocity;
            velocity.y = 0f;
            animator.SetFloat(Speed, velocity.magnitude, 0.12f, Time.deltaTime);
        }
    }
}

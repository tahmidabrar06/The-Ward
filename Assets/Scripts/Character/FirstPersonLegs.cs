using UnityEngine;

namespace TheWard.Character
{
    [RequireComponent(typeof(Animator))]
    public sealed class FirstPersonLegs : MonoBehaviour
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
            float speed = player.isGrounded ? velocity.magnitude : 0f;
            animator.SetFloat(Speed, speed, 0.12f, Time.deltaTime);
        }
    }
}

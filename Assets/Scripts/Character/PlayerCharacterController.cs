using UnityEngine;
using UnityEngine.InputSystem;

namespace TheWard.Character
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerCharacterController : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField, Min(0f)] private float walkSpeed = 4f;
        [SerializeField, Min(0f)] private float sprintSpeed = 7f;
        [SerializeField, Min(0f)] private float jumpHeight = 1.2f;
        [SerializeField, Min(0)] private int extraAirJumps = 1;
        [SerializeField] private float gravity = -20f;

        private static readonly int MovingHash = Animator.StringToHash("Moving");
        private static readonly int RunningHash = Animator.StringToHash("Running");
        private static readonly int MeleeEquippedHash = Animator.StringToHash("MeleeEquipped");
        private static readonly int GunEquippedHash = Animator.StringToHash("GunEquipped");
        private static readonly int BatStrikeHash = Animator.StringToHash("BatStrike");
        private static readonly int FireRifleHash = Animator.StringToHash("FireRifle");
        private static readonly int ReloadGunHash = Animator.StringToHash("ReloadGun");
        private static readonly int ThrowGrenadeHash = Animator.StringToHash("ThrowGrenade");
        private static readonly int SlashSwordHash = Animator.StringToHash("SlashSword");
        private static readonly int WalkingStateHash = Animator.StringToHash("Base Layer.Walking");
        private static readonly int RunningStateHash = Animator.StringToHash("Base Layer.Running");
        private static readonly int ShootingStateHash = Animator.StringToHash("Combat.Shooting Gun");
        private static readonly int FiringRifleStateHash = Animator.StringToHash("Combat.Firing Rifle");

        private CharacterController characterController;
        private Animator characterAnimator;
        private float verticalVelocity;
        private int airJumpsRemaining;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            characterAnimator = GetComponentInChildren<Animator>();
            if (characterAnimator != null)
            {
                // CharacterController moves the player; animation only moves the body.
                characterAnimator.applyRootMotion = false;
            }
        }

        // Bat and sword share the same idle/run poses. Guns keep their normal poses.
        public void SetMeleeLocomotion(bool meleeEquipped)
        {
            if (characterAnimator != null)
                characterAnimator.SetBool(MeleeEquippedHash, meleeEquipped);
        }

        public void SetGunEquipped(bool gunEquipped)
        {
            if (characterAnimator != null)
                characterAnimator.SetBool(GunEquippedHash, gunEquipped);
        }

        private void OnEnable()
        {
            EnableAction(moveAction);
            EnableAction(jumpAction);
            EnableAction(sprintAction);
        }

        private void OnDisable()
        {
            DisableAction(moveAction);
            DisableAction(jumpAction);
            DisableAction(sprintAction);
        }

        private void Update()
        {
            Vector2 input = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
            Vector3 horizontalMovement = transform.right * input.x + transform.forward * input.y;
            horizontalMovement = Vector3.ClampMagnitude(horizontalMovement, 1f);
            bool sprinting = sprintAction != null && sprintAction.action.IsPressed();
            float speed = sprinting ? sprintSpeed : walkSpeed;

            if (characterController.isGrounded)
            {
                // A small downward speed keeps the controller touching the floor.
                verticalVelocity = -2f;
                airJumpsRemaining = extraAirJumps;
            }

            if (jumpAction != null && jumpAction.action.WasPressedThisFrame() &&
                (characterController.isGrounded || airJumpsRemaining > 0))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (!characterController.isGrounded)
                {
                    airJumpsRemaining--;
                }
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 movement = horizontalMovement * speed;
            movement.y = verticalVelocity;
            characterController.Move(movement * Time.deltaTime);

            UpdateAnimation(horizontalMovement.sqrMagnitude > 0.01f, sprinting);
        }

        public void PlayAttackAnimation()
        {
            if (characterAnimator == null)
            {
                return;
            }

            characterAnimator.SetTrigger(BatStrikeHash);
        }

        public void PlayGunShot(bool automatic)
        {
            if (characterAnimator == null) return;

            // A pistol restarts its recoil on each click. An automatic gun lets
            // one recoil finish rather than restarting it for every bullet.
            if (!automatic)
            {
                characterAnimator.CrossFadeInFixedTime(ShootingStateHash, 0.12f, 1, 0f);
                return;
            }

            AnimatorStateInfo state = characterAnimator.GetCurrentAnimatorStateInfo(1);
            bool startingShot = characterAnimator.IsInTransition(1) &&
                characterAnimator.GetNextAnimatorStateInfo(1).fullPathHash == FiringRifleStateHash;
            if (state.fullPathHash != FiringRifleStateHash && !startingShot)
            {
                characterAnimator.SetTrigger(FireRifleHash);
            }
        }

        public void PlayGunReload()
        {
            if (characterAnimator != null) characterAnimator.SetTrigger(ReloadGunHash);
        }

        public void PlayGrenadeThrow()
        {
            if (characterAnimator != null) characterAnimator.SetTrigger(ThrowGrenadeHash);
        }

        public void PlaySwordSlash()
        {
            if (characterAnimator != null) characterAnimator.SetTrigger(SlashSwordHash);
        }

        private void UpdateAnimation(bool moving, bool sprinting)
        {
            if (characterAnimator == null)
            {
                return;
            }

            characterAnimator.SetBool(MovingHash, moving);
            characterAnimator.SetBool(RunningHash, moving && sprinting);

            AnimatorStateInfo state = characterAnimator.GetCurrentAnimatorStateInfo(0);
            if ((state.fullPathHash == WalkingStateHash || state.fullPathHash == RunningStateHash) &&
                state.normalizedTime >= 1f && !characterAnimator.IsInTransition(0))
            {
                // Mixamo clips may import as one-shot; restart locomotion if needed.
                characterAnimator.Play(state.fullPathHash, 0, 0f);
            }
        }

        private static void EnableAction(InputActionReference actionReference)
        {
            actionReference?.action.Enable();
        }

        private static void DisableAction(InputActionReference actionReference)
        {
            actionReference?.action.Disable();
        }
    }
}

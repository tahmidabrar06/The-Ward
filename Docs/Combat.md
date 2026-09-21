# Weapons and player movement

Open `Assets/Scenes/TestCharWeapons.unity`. Use 1–5 to switch weapons, left click to attack, and R to reload. Hold and release left click to throw a grenade. Move with WASD and hold Shift to sprint.

## Weapon animations

The weapons play the imported humanoid clips, not scripted transform swings:

| Weapon | Clip | Duration | Contact/release |
| --- | --- | --- | --- |
| Pistol | Reload.fbx | 1.65 s | Ammo added at completion |
| Machine gun | Reload.fbx | 1.9 s | Ammo added at completion |
| Baseball bat | Baseball Strike.fbx | 0.55 s | Damage at 0.18 s |
| Sword | Great Sword Slash.fbx | 0.5 s | Damage at 0.19 s |
| Grenade | Grenade Throw.fbx | 0.8 s | Release at 0.45 s |

The FBX import settings trim the long setup and trailing motion. Adjust the assigned clip, duration, and contact/release delay on the weapon's component. If a clip is renamed, rename its state in both animation controllers too.

`FirstPersonArms.cs` plays the clip, blends from the holding pose, and attaches the weapon to the animated wrist. It keeps the wrist's IK rotation offset so the handle stays in the palm. The action framing keeps hands in front of the lens. The right-hand finger pose keeps the grip closed; the left fingers can follow the imported action.

The arms controller has a static ready pose, an Action layer, and separate left/right finger layers. There is no player idle motion or Rifle Idle asset. `ReadyPose.anim` is a frozen pose needed to hold the rig between actions. Walking remains for the visible legs.

## Combat scripts

- `GunWeapon.cs`: center-crosshair shooting, ammunition, recoil and clip-based reloads. Firing runs after the camera updates. Reloading blocks firing; switching cancels the reload without adding ammo.
- `MeleeWeapon.cs`: plays the assigned swing and damages the nearest visible enemy at the contact time. The full reach includes close enemies. Walls block hits and multiple colliders do not multiply damage.
- `GrenadeWeapon.cs`: charge, clip playback and timed release. Switching before release cancels the throw. `UnifiedGrenadeProjectile.cs` handles the existing explosion behavior.
- `WeaponController.cs`: input and weapon selection.
- `Weapon.cs`: shared grip/camera references, animation playback requests and cancellation.

For holding poses, adjust `Right Hand Grip` and `Left Hand Grip` under each weapon. One-handed weapons use a left-hand rest target under the camera. Keep these targets outside the animated hand hierarchy to avoid an IK feedback loop.

## Body and movement

`PlayerCharacterController.cs` uses one CharacterController for horizontal walking and sprinting. There is no jump, gravity, or player Rigidbody. Place the player at the intended floor height.

`PlayerBodyAnimator.cs` reads horizontal movement speed. `Full Body Shadow` retains the complete body and head with Shadows Only rendering; `Visible Torso and Legs` shares its skeleton. The body's upper half plays the same action clip for the shadow while the legs keep their movement animation. First-person arms do not cast a second arm shadow.

`PlayerLookController` follows the head's Eye Anchor in LateUpdate and keeps rotation under mouse control. Keep the camera/arm rig beside `Player Body`, rather than nesting one humanoid Animator under the other. There is still one camera.

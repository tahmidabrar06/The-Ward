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

The FBX import settings trim the long setup and trailing motion. Set the playback duration on the weapon's component. Contact and release timing come from Animation Events in the FBX's Animation tab, so changing playback speed also changes when the hit happens. The times above are approximate at the configured durations.

The bat, sword and grenade clips call `ActionContact` at 32.7%, 38% and 56.25% respectively. All four clips call `ActionComplete` at 99%, just before the final frame. Reload adds ammunition at that event. There are no separate hit, release or reload timers.

`FirstPersonArms.cs` sets the clip's named Animator trigger, blends from the holding pose using animation progress, and attaches the weapon to the animated wrist. It keeps the wrist's IK rotation offset so the handle stays in the palm. The action framing keeps hands in front of the lens. The right-hand finger pose keeps the grip closed; the left fingers can follow the imported action.

Open the **Action** layer in either controller to see the four action transitions. Each trigger has the same name as its clip; `ActionSpeed` controls playback speed. Actions return to Ready at Exit Time 1. `CancelAction` returns immediately when switching weapons. If a clip is renamed, update its state and trigger in both controllers.

Only the arms Animator sends gameplay events. The body Animator has `fireEvents` disabled so its matching shadow animation cannot deal damage or spawn a second grenade. Events from a cancelled action are ignored; contact is accepted once per action. The arms action layer retains a small nonzero weight until completion, because Unity suppresses events on a layer with zero weight.

The arms controller has a static ready pose, an Action layer, and separate left/right finger layers. There is no player idle motion or Rifle Idle asset. `ReadyPose.anim` is a frozen pose needed to hold the rig between actions. Walking remains for the visible legs.

## Combat scripts

- `GunWeapon.cs`: center-crosshair shooting, ammunition, recoil and clip-based reloads. Firing runs after the camera updates. Reloading blocks firing; switching cancels the reload without adding ammo.
- `MeleeWeapon.cs`: plays the assigned swing and damages the nearest visible enemy at the contact event. The full reach includes close enemies. Walls block hits and multiple colliders do not multiply damage.
- `GrenadeWeapon.cs`: charge, clip playback and release at the contact event. Switching before release cancels the throw. `UnifiedGrenadeProjectile.cs` handles the existing explosion behavior.
- `WeaponController.cs`: input and weapon selection.
- `Weapon.cs`: shared grip/camera references, animation playback requests and cancellation.

For holding poses, adjust `Right Hand Grip` and `Left Hand Grip` under each weapon. One-handed weapons use a left-hand rest target under the camera. Keep these targets outside the animated hand hierarchy to avoid an IK feedback loop.

## Body and movement

`PlayerCharacterController.cs` uses one CharacterController for horizontal walking and sprinting. There is no jump, gravity, or player Rigidbody. Place the player at the intended floor height.

`PlayerBodyAnimator.cs` reads horizontal movement speed. `Full Body Shadow` retains the complete body and head with Shadows Only rendering; `Visible Torso and Legs` shares its skeleton. The body's upper half plays the same action clip for the shadow while the legs keep their movement animation. First-person arms do not cast a second arm shadow.

`PlayerLookController` follows the head's Eye Anchor in LateUpdate and keeps rotation under mouse control. Keep the camera/arm rig beside `Player Body`, rather than nesting one humanoid Animator under the other. There is still one camera.

## Connection to the animation lecture

These are the techniques from **8. Animation.pdf**, with slide numbers for review:

| Slides | Technique | Where to find it |
| --- | --- | --- |
| 12, 18–24 | Humanoid rigs, Avatar retargeting and baked root motion | The four action FBX import settings. The CharacterController drives movement; player Animators do not apply root motion. |
| 14–17 | State machines, transitions and script parameters | Both controllers' Action layers; `FirstPersonArms` sets triggers and `ActionSpeed`. |
| 28–29 | A 1D blend tree driven by Speed | `PlayerBody.controller` blends the static ready pose into walking and faster walking. `PlayerBodyAnimator` supplies horizontal speed. |
| 33–36 | Layers and Avatar Masks | The body uses `UpperBodyActions`; arms use `FirstPersonActions` and finger masks. Leg movement continues during actions. |
| 38–40 | Animation Events calling script methods | Imported clips call `FirstPersonArms.ActionContact` / `ActionComplete`, which notify the active weapon. |
| 41 | Inverse kinematics | `FirstPersonArms.OnAnimatorIK` aligns hands with grip targets and fits action poses in front of the camera. |

To follow one action, select Baseball Strike in the FBX importer and inspect its two Events. Then open the arms controller's Action layer and follow the Baseball Strike trigger into the swing state and its exit back to Ready. `MeleeWeapon.OnAnimationContact` is where the swing applies damage.

The lecture's idle, jump and larger directional blend-tree examples are examples of the techniques, rather than additional features required by this project's first-person controls. The player keeps the requested four action clips and walking legs.

Unity's reference for editing imported clip events: [Add events to animation clips](https://docs.unity.com/en-us/engine/6000.3/manual/animation-section/animation-mecanim/animation-clips/animation-events-on-imported-clips).

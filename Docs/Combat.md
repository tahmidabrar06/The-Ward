# Combat setup

Open `Assets/Scenes/Floor1.unity`. Use WASD to move, Shift to sprint, E to pick up a weapon, 1–2 or the mouse wheel to switch inventory slots, left click to attack, and R to reload. Hold and release left click to throw a grenade.

## Ammunition and cooldowns

The pistol holds 12 rounds with 192 in reserve; the AK74 holds 30 with 480 in reserve. Both player prefabs and their world pickup prefabs use these values. Duplicate gun pickups refill the magazine and reserve through `RefillAmmo`; scene transitions retain the current ammunition.

Grenades are unlimited in the current inventory system. Throws have a 2-second cooldown from the start of the throw animation, including the 0.8-second animation. Switching away and back does not reset this cooldown.

Both guns reference `Assets/Visual Effects/Combat/ShotTracer.mat`, which uses the URP particle unlit shader with additive blending. Keep this material assigned on each gun so the shader is included in builds. Tracers run from the muzzle to the hitscan endpoint and disappear after 0.12 seconds.

## Animations

Kept reloads, the two melee swings, grenade throw, and walking for the legs. `ReadyPose.anim` holds the hands still between actions, so there’s no idle movement.

| Weapon | Clip | Duration | Hit/release |
| --- | --- | --- | --- |
| Pistol | Reload.fbx | 1.65 s | Ammo added near the end |
| Machine gun | Reload.fbx | 1.9 s | Ammo added near the end |
| Bat | Baseball Strike.fbx | 0.55 s | About 0.18 s |
| Sword | Great Sword Slash.fbx | 0.5 s | About 0.19 s |
| Grenade | Grenade Throw.fbx | 0.8 s | About 0.45 s |

Change the duration on the weapon component. To adjust hit or release timing, move `ActionContact` in the FBX’s animation events. `ActionComplete` finishes the action and adds ammo for reloads. Switching weapons cancels the current action.

The bat in `TestCharWeapons` enables `Use Forward Swing`: its handle winds up on the right, drives the barrel toward the crosshair, follows through left, and returns to rest. The right hand follows the handle with IK; the left stays at its rest target. The imported clip still drives body motion and gameplay events. Keep `Swing Contact Fraction` aligned with the normalized `ActionContact` event time (currently 0.32727274). This option is off for the sword.

`FirstPersonArms.cs` handles hand placement and starts the Animator transitions. Adjust `Right Hand Grip` and `Left Hand Grip` to change how a weapon is held. Keep these targets outside the animated hand bones.

Grip poses loop while holding a weapon. Melee keeps the fingers closed for the whole swing. Reloads and throws hold the left-hand grip for the first 20% of the action, blend into the clip's finger pose over 10%, then start re-gripping at 75%. Tune `Grip Hold Fraction`, `Grip Return Fraction`, and `Grip Blend Fraction` on the FirstPersonArms component. The right hand keeps its grip until the grenade's release event. These settings change finger posing without changing attack, reload, or projectile timing.

Only the arms Animator sends gameplay events, so the body’s matching animation won’t cause a second hit. If you rename a clip, update its state and trigger in both controllers too.

## Scripts

- `GunWeapon.cs`: Shooting, ammo, recoil, and reloads.
- `MeleeWeapon.cs`: Swings and hits the nearest enemy within reach. Walls block hits.
- `GrenadeWeapon.cs`: Charging and throwing. `UnifiedGrenadeProjectile.cs` handles explosions.
- `WeaponController.cs`: Input and weapon switching.
- `Weapon.cs`: Shared camera, grip, and animation setup.

## Player setup

Movement uses one CharacterController for walking and sprinting. There’s no jump or gravity, so place the player at floor height.

The full body and head still cast a shadow, and the torso and legs are visible when looking down. The camera follows the eye anchor. Keep the camera and arm rig beside `Player Body` in the hierarchy.

The setup follows the lecture’s Animator transitions, blend tree, avatar masks, animation events, and hand IK. Masks let the upper-body actions play while the legs keep walking.

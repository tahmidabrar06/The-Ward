# Weapons and player movement

Open `Assets/Scenes/TestCharWeapons.unity` to try the five weapons. Use 1–5 to switch, left click to attack, and R to reload. Hold and release left click to throw a grenade. Move with WASD and hold Shift to sprint.

Each weapon has its own component:

- `GunWeapon.cs`: shooting, ammunition, recoil and reloads for the pistol and machine gun. Shots follow the center dot without random spread. Firing runs after the camera's head-follow update so aiming matches the rendered view.
- `MeleeWeapon.cs`: bat and sword swings. `Hit()` selects the nearest visible enemy within reach; `Swing()` calls it a quarter of the way through the animation (about 0.1 seconds for the bat and 0.06 for the sword), then recovers. Change damage, range, hit radius and swing duration in the weapon's Inspector.
- `GrenadeWeapon.cs`: charging and throwing. `UnifiedGrenadeProjectile.cs` still handles the existing impact/explosion behavior.
- `WeaponController.cs`: input and weapon selection only.
- `Weapon.cs`: shared hand/camera references and helpers for moving the held model. It contains no hit detection or damage rules.

Melee checks the full reach from the camera, so close enemies can be hit. A wall blocks the hit, multiple colliders do not multiply damage, and switching weapons cancels a swing before contact.

`PlayerCharacterController.cs` handles horizontal walking and sprinting with a CharacterController. There is no jump action, gravity, or player Rigidbody. Place the player at the intended floor height; movement does not make the player fall onto a lower floor.

The hand poses and animation assets are separate from this cleanup. `FirstPersonArms.cs` follows the grip transforms on the selected weapon. `PlayerBodyAnimator.cs` reads horizontal movement speed without requiring gravity or a grounded check.

For hand placement, adjust `Right Hand Grip` and `Left Hand Grip` under each weapon. One-handed weapons use a left-hand rest target under the camera. Keep those targets outside the animated hand hierarchy to avoid an IK feedback loop. The Grip layer uses two static finger poses: a trigger-finger pose for guns and a closed grip for melee/grenades.

The player keeps a complete body and head in `Full Body Shadow`, using Shadows Only rendering. `Visible Torso and Legs` shares that skeleton and supplies the local body view. The separate first-person arms do not cast a second set of arm shadows.

`PlayerLookController` follows the head's `Eye Anchor` position in LateUpdate and applies mouse look in world space. Keep the camera/arm rig beside `Player Body` in the hierarchy: nesting the arm Animator under the body's head causes Unity to bind the body Animator to the wrong skeleton. There is still one player camera.

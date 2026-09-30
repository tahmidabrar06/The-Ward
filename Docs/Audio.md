# Gameplay audio

Audio is wired into the existing player, weapon, grenade, and zombie scripts. No scene or prefab audio assignments are required. `Assets/Resources/GameAudioBank.asset` references the original files under `Assets/SoundFX`; keep their `.meta` files when committing or moving them.

## Selected sounds

| Action | Clip / set under `Assets/SoundFX` |
| --- | --- |
| Player walking and crouching | `Footsteps - Essentials/Footsteps_Tile/Footsteps_Tile_Walk` |
| Player running | `Footsteps - Essentials/Footsteps_Tile/Footsteps_Tile_Run` |
| Pistol shot | `FreeWeaponSFX_v1.1/AssaultRifle/Gunshots/sfx_wpn_ar_gunshot_01.wav` |
| Gun reload (all profiles) | `FreeWeaponSFX_v1.1/AssaultRifle/Reload/sfx_wpn_ar_reload_03_magIn.wav` |
| Grenade launch | `FreeWeaponSFX_v1.1/GrenadeLauncher/sfx_wpn_gl_fire.wav` |
| Grenade explosion | `FreeWeaponSFX_v1.1/GrenadeLauncher/sfx_wpn_gl_explosion.wav` |
| Machine gun | AssaultRifle gunshot variants |
| Gun selection | Corresponding handgun / assault-rifle equip foley |
| Sword / bat | Zombie Massacre swing and flesh / blunt impact variants |
| Zombies | ZombieHorror idle, footsteps, body fall; Zombie Massacre 16-bit Impact Pain Hurt clips; Zombie Massacre attack voices: Short, Medium, Intimidating |
| Player damage / zombie eating attack | `TerrorByte Games/Zombie Massacre SFX Pack/16bit/Flesh Blood and Gore/zombie_massacre_sfx_flesh_blood_gore_stereo_01.wav` |
| Music | `HS_MP` Exploration, MidTension, and Tension loops |

No jumping behavior or jump audio was added.

Each zombie keeps an assigned attack voice set. Automatic assignment cycles through Short (7 clips), Medium (13), and Intimidating (7) as zombies spawn; attacks vary within that set without immediate repeats. Set the Zombie component’s **Attack Voice** in the Inspector to choose a specific set for a prefab. All 27 attack clips come from the requested 16-bit Vocal/Attack folder.

The bank exposes per-cue volume, effects volume, and music volume in the Inspector. Gun sound profiles default to handgun for press-once weapons and assault rifle for automatic weapons; an explicit profile can override this. The pistol intentionally uses the requested assault-rifle clip.

Footsteps follow actual grounded movement, with a faster cadence when sprinting and lower volume while crouching. Nearby suspicious or chasing zombies crossfade the music; tension releases after a short delay. Music loops use streaming import settings. Spatial zombie and explosion sounds fade out at 28 metres. A scene-local pool limits simultaneous effects to 32, pauses with `Time.timeScale`, and lets explosion audio finish after the grenade is destroyed. Switching away from a gun cancels its reload sound.

## Audio scripts

- `GameAudioBank` holds the clips and volumes shown in the Inspector.
- `GameAudio` handles sound effects through cached `AudioSource` components and `PlayOneShot`. It also stops a weapon or zombie's sounds when needed.
- `MusicController` plays the three music loops and fades between them. Its sources are separate from the effects, so combat sounds do not interrupt the music.

The player starts the audio setup automatically in Floor1, Floor2, and Floor3. Both player controllers (`PlayerCharacterController` and the older `FirstPerson`) register with the audio system, including when a floor is opened directly or loaded through an elevator. During Play mode, expand **Game Audio → Music** in the Hierarchy to inspect the music sources. Existing scenes and prefabs need no new assignments.

This follows the component and playback approach in the [Unity music example](https://discussions.unity.com/t/how-to-insert-music-into-a-scene-and-have-it-play-on-the-click-of-a-button/582425) and Unity's [AudioSource documentation](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/audiosource).

## Play-mode verification

Open `Floor1CharTest` and wait for Unity to finish importing and compiling:

1. Walk, sprint, crouch, stop, and push against a wall. Only grounded movement should produce Tile footsteps, with quieter crouching and faster running.
2. Fire the pistol and machine gun. Confirm the pistol uses the selected clip and shots require ammunition.
3. Reload a partly empty gun: hear the selected magazine-in clip once. A full magazine or no reserve ammunition should stay silent. Switching weapons cancels reload audio.
4. Throw a grenade: hear the launch clip on release and one spatial explosion at impact. The explosion tail should continue after the grenade disappears.
5. Swing the sword and bat, with and without a target. Impacts should sound only on a successful hit.
6. Approach zombies: check spatial idle, movement, attack, hurt, and death sounds. Music should rise around threats and settle after leaving them.
7. Pause via `Time.timeScale = 0`, resume, and reload the scene. Audio should pause/resume without duplicate music managers.

Compilation and asset-link checks can run outside Unity; audible balance and scene playback require this play-mode pass.

To check floor music, open Floor2 alone and press Play, then repeat with Floor3. Each should fade into Exploration automatically. Also check an elevator transition between floors; the previous scene’s audio should stop and the destination floor’s music should begin.

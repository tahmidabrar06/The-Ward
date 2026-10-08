using System;
using UnityEngine;

namespace TheWard.Audio
{
    // User levels are separate from the bank's per-clip mix and survive scene changes.
    public static class AudioSettings
    {
        public static float Master { get; private set; } = 1f;
        public static float Sfx { get; private set; } = 1f;
        public static float Music { get; private set; } = 0.2f;
        public static float Cinematics { get; private set; } = 0.6f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Load()
        {
            SetVolumes(PlayerPrefs.GetFloat("MasterVolume", 1f),
                PlayerPrefs.GetFloat("SfxVolume", 1f), PlayerPrefs.GetFloat("MusicVolume", 0.2f),
                PlayerPrefs.GetFloat("CinematicVolume", 0.6f));
        }

        public static void SetVolumes(float master, float sfx, float music, float cinematics)
        {
            Master = Mathf.Clamp01(master);
            Sfx = Mathf.Clamp01(sfx);
            Music = Mathf.Clamp01(music);
            Cinematics = Mathf.Clamp01(cinematics);
            AudioListener.volume = Master;
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat("MasterVolume", Master);
            PlayerPrefs.SetFloat("SfxVolume", Sfx);
            PlayerPrefs.SetFloat("MusicVolume", Music);
            PlayerPrefs.SetFloat("CinematicVolume", Cinematics);
        }
    }

    // Unity saves these enum values as numbers. Append new cues instead of reordering them.
    public enum AudioCue
    {
        HandgunShot, HandgunEquip, HandgunReload,
        RifleShot, RifleEquip, RifleReload,
        ShotgunShot, ShotgunEquip, ShotgunReload,
        GrenadeThrow, Explosion, SwordSwing, BatSwing, SwordHit, BatHit,
        ZombieIdle, ZombieAttack, ZombieHurt, ZombieDeath, ZombieFootstep, PlayerHurt
    }

    // Explicit voice choices map to the bank order: Short, Medium, then Intimidating.
    public enum ZombieAttackVoice { Automatic, Short, Medium, Intimidating }

    public enum FootstepAction { Walk, Run }

    // Edit Assets/Resources/GameAudioBank.asset to change clips or volumes for all floors.
    [CreateAssetMenu(menuName = "The Ward/Audio Bank")]
    public class GameAudioBank : ScriptableObject
    {
        [Serializable]
        public class Sound
        {
            public AudioCue cue;
            [Range(0f, 1f)] public float volume = 0.7f;
            public AudioClip[] clips;
            // Reload foley is ordered, rather than picked as random variations.
            public bool sequential;
            // Playback history is runtime state, not a setting saved into the asset.
            [NonSerialized] private int lastIndex = -1;

            // Pick a variation, avoiding the last clip when there is more than one option.
            public AudioClip Next()
            {
                if (clips == null || clips.Length == 0) return null;
                int index = UnityEngine.Random.Range(0, clips.Length);
                if (clips.Length > 1 && index == lastIndex)
                {
                    index = (index + 1) % clips.Length;
                }
                lastIndex = index;
                return clips[index];
            }
        }

        [Serializable]
        public class Steps
        {
            public AudioClip[] walk;
            public AudioClip[] run;
        }

        [Serializable]
        public class AttackVoice
        {
            public string name;
            public AudioClip[] clips;
        }

        // Each zombie keeps one set; the remaining cues share the general sound list.
        public AttackVoice[] zombieAttackVoices;
        public Sound[] sounds;
        public Steps footsteps;
        public AudioClip exploration;
        public AudioClip suspicion;
        public AudioClip combat;
        public AudioClip waveDefense;
        [Range(0f, 1f)] public float musicVolume = 0.18f;
        [Range(0f, 1f)] public float effectsVolume = 0.8f;

        public Sound Find(AudioCue cue)
        {
            if (sounds == null) return null;
            foreach (Sound sound in sounds)
            {
                if (sound != null && sound.cue == cue) return sound;
            }
            return null;
        }
    }
}

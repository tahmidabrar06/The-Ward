using TheWard.Combat;
using UnityEngine;

namespace TheWard.Audio
{
    // Three looping tracks share the mix: exploration, suspicion, and combat.
    public class MusicController : MonoBehaviour
    {
        [SerializeField] private AudioSource explorationSource;
        [SerializeField] private AudioSource suspicionSource;
        [SerializeField] private AudioSource combatSource;
        [SerializeField] private float fadeSpeed = 0.12f;

        private GameAudioBank bank;
        private Transform player;
        private Health playerHealth;
        private float suspicionUntil;
        private float combatUntil;
        private bool paused;

        // GameAudio calls Setup before registering the player or starting playback.
        public void Setup(GameAudioBank audioBank)
        {
            bank = audioBank;
            explorationSource = CreateSource("Exploration", bank.exploration);
            suspicionSource = CreateSource("Suspicion", bank.suspicion);
            combatSource = CreateSource("Combat", bank.combat);
        }

        private AudioSource CreateSource(string name, AudioClip clip)
        {
            GameObject musicObject = new GameObject(name);
            musicObject.transform.SetParent(transform);

            AudioSource source = musicObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            // Music is 2D: its volume should not depend on where the player stands.
            source.spatialBlend = 0f;
            source.volume = 0f;
            source.clip = clip;
            return source;
        }

        public void SetPlayer(Transform playerTransform, Health health)
        {
            player = playerTransform;
            playerHealth = health;
            PlayMusic();
        }

        // Keep all loops running and fade their volumes, rather than restarting on each threat.
        public void PlayMusic()
        {
            PlaySource(explorationSource);
            PlaySource(suspicionSource);
            PlaySource(combatSource);
        }

        private void PlaySource(AudioSource source)
        {
            if (source != null && source.clip != null && !source.isPlaying)
            {
                source.Play();
            }
        }

        // Hold tension briefly after the last report to avoid abrupt changes at detection boundaries.
        public void ReportThreat(bool chasing)
        {
            if (chasing)
            {
                combatUntil = Time.time + 4f;
            }
            else
            {
                suspicionUntil = Time.time + 3f;
            }
        }

        public void PauseMusic()
        {
            if (explorationSource == null) return;
            explorationSource.Pause();
            suspicionSource.Pause();
            combatSource.Pause();
        }

        public void ResumeMusic()
        {
            if (explorationSource == null) return;
            explorationSource.UnPause();
            suspicionSource.UnPause();
            combatSource.UnPause();
        }

        private void Update()
        {
            if (bank == null) return;

            // Pause preserves each track's playback position until the game resumes.
            bool gamePaused = Time.timeScale <= 0f;
            if (gamePaused != paused)
            {
                paused = gamePaused;
                if (paused) PauseMusic();
                else ResumeMusic();
            }
            if (paused) return;

            // Combat takes priority over suspicion; exploration returns when both timers expire.
            AudioSource activeTrack = explorationSource;
            if (Time.time < combatUntil)
            {
                activeTrack = combatSource;
            }
            else if (Time.time < suspicionUntil)
            {
                activeTrack = suspicionSource;
            }

            // Older floor players may not have Health. They still need background music.
            bool playerAlive = player != null && (playerHealth == null || playerHealth.IsAlive);
            if (!playerAlive) activeTrack = null;

            FadeSource(explorationSource, activeTrack);
            FadeSource(suspicionSource, activeTrack);
            FadeSource(combatSource, activeTrack);
        }

        // Fade the selected track up and the others down over several frames.
        private void FadeSource(AudioSource source, AudioSource activeTrack)
        {
            float targetVolume = 0f;
            if (source == activeTrack) targetVolume = bank.musicVolume;
            source.volume = Mathf.MoveTowards(source.volume, targetVolume, fadeSpeed * Time.deltaTime);
        }
    }
}

using TheWard.Combat;
using UnityEngine;

namespace TheWard.Audio
{
    public class MusicController : MonoBehaviour
    {
        [SerializeField] private AudioSource explorationSource;
        [SerializeField] private AudioSource suspicionSource;
        [SerializeField] private AudioSource combatSource;
        [SerializeField] private AudioSource waveDefenseSource;
        [SerializeField] private float fadeSpeed = 0.12f;

        [SerializeField] private float explorationLevel, suspicionLevel, combatLevel, waveDefenseLevel;
        [SerializeField] private bool waveDefenseActive;
        [SerializeField] private GameAudioBank bank;
        [SerializeField] private Transform player;
        [SerializeField] private Health playerHealth;
        [SerializeField] private float suspicionUntil;
        [SerializeField] private float combatUntil;
        [SerializeField] private bool paused;

        public void Setup(GameAudioBank audioBank)
        {
            bank = audioBank;
            explorationSource = CreateSource("Exploration", bank.exploration);
            suspicionSource = CreateSource("Suspicion", bank.suspicion);
            combatSource = CreateSource("Combat", bank.combat);
            waveDefenseSource = CreateSource("Wave Defense", bank.waveDefense);
            if (player != null) PlayMusic();
        }

        private void OnEnable()
        {
            // A newly introduced source also needs creating after a Play-mode script reload.
            if (bank != null) Setup(bank);
        }

        private AudioSource CreateSource(string name, AudioClip clip)
        {
            Transform child = transform.Find(name);
            if (child == null)
            {
                child = new GameObject(name).transform;
                child.SetParent(transform);
            }
            AudioSource source = child.GetComponent<AudioSource>();
            if (source != null) return source;
            source = child.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
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
            PlaySource(waveDefenseSource);
        }

        private void PlaySource(AudioSource source)
        {
            if (!paused && Time.timeScale > 0f && source != null && source.clip != null && !source.isPlaying)
            {
                source.Play();
            }
        }

        public void SetWaveDefense(bool active)
        {
            if (waveDefenseActive == active) return;
            waveDefenseActive = active;
            if (active)
            {
                // Begin the defense cue at its opening, rather than mid-loop.
                waveDefenseSource.Stop();
                waveDefenseSource.time = 0f;
                PlaySource(waveDefenseSource);
                if (paused || Time.timeScale <= 0f) waveDefenseSource.Pause();
            }
            else
            {
                suspicionUntil = 0f;
                combatUntil = 0f;
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
            waveDefenseSource.Pause();
        }

        public void ResumeMusic()
        {
            if (explorationSource == null) return;
            explorationSource.UnPause();
            suspicionSource.UnPause();
            combatSource.UnPause();
            waveDefenseSource.UnPause();
        }

        private void Update()
        {
            if (bank == null) return;
            if (waveDefenseSource == null) Setup(bank);

            // Pause preserves each track's playback position until the game resumes.
            bool gamePaused = Time.timeScale <= 0f;
            if (gamePaused != paused)
            {
                paused = gamePaused;
                if (paused) PauseMusic();
                else ResumeMusic();
            }
            AudioSource activeTrack = explorationSource;
            if (waveDefenseActive && waveDefenseSource.clip != null)
            {
                activeTrack = waveDefenseSource;
            }
            else if (Time.time < combatUntil)
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

            UpdateTrack(explorationSource, activeTrack, ref explorationLevel);
            UpdateTrack(suspicionSource, activeTrack, ref suspicionLevel);
            UpdateTrack(combatSource, activeTrack, ref combatLevel);
            UpdateTrack(waveDefenseSource, activeTrack, ref waveDefenseLevel);
        }

        private void UpdateTrack(AudioSource source, AudioSource activeTrack, ref float level)
        {
            if (source == null) return;
            if (!paused)
            {
                float target = source == activeTrack ? bank.musicVolume : 0f;
                level = Mathf.MoveTowards(level, target, fadeSpeed * Time.deltaTime);
            }
            source.volume = level * AudioSettings.Music;
        }
    }
}

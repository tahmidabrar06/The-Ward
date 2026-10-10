using System.Collections;
using TheWard.Combat;
using UnityEngine;

namespace TheWard.Audio
{
    public class GameAudio : MonoBehaviour
    {
        private const int SoundSourceCount = 32;
        private static GameAudio instance;
        private float lastSfxVolume = -1f;
        private float lastCinematicVolume = -1f;
        [SerializeField] private GameAudioBank bank;
        [SerializeField] private AudioSource[] soundSources = new AudioSource[SoundSourceCount];
        [SerializeField] private Object[] sourceOwners = new Object[SoundSourceCount];
        [SerializeField] private float[] sourceLevels = new float[SoundSourceCount];
        [SerializeField] private bool[] cinematicSources = new bool[SoundSourceCount];
        [SerializeField] private MusicController musicController;
        [SerializeField] private Transform player;
        [SerializeField] private bool paused;
        [SerializeField] private int nextZombieVoice;

        // Clear the cached instance when entering Play mode, even with domain reload disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
        }

        // Resources keeps the bank available in builds without wiring it into every scene.
        private static GameAudio GetInstance()
        {
            if (instance != null) return instance;

            // Static fields reset on script reload, while scene objects remain alive.
            GameAudio existing = FindFirstObjectByType<GameAudio>();
            if (existing != null)
            {
                instance = existing;
                existing.SetupAudio();
                return instance;
            }
            if (Resources.Load<GameAudioBank>("GameAudioBank") == null) return null;
            GameObject audioObject = new GameObject("Game Audio");
            return audioObject.AddComponent<GameAudio>();
        }

        private void OnEnable()
        {
            if (instance != null && instance != this)
            {
                enabled = false;
                Destroy(gameObject);
                return;
            }
            instance = this;
            SetupAudio();
        }

        private void SetupAudio()
        {
            if (bank == null) bank = Resources.Load<GameAudioBank>("GameAudioBank");
            if (bank == null) { enabled = false; return; }
            for (int i = 0; i < soundSources.Length; i++)
            {
                Transform child = transform.Find("Sound Effect " + (i + 1));
                if (child == null)
                {
                    child = new GameObject("Sound Effect " + (i + 1)).transform;
                    child.SetParent(transform);
                }
                AudioSource source = child.GetComponent<AudioSource>();
                if (source == null) source = child.gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = 2f;
                source.maxDistance = 28f;
                source.dopplerLevel = 0f;
                source.priority = 64;
                soundSources[i] = source;
            }

            Transform musicTransform = transform.Find("Music");
            if (musicTransform == null)
            {
                musicTransform = new GameObject("Music").transform;
                musicTransform.SetParent(transform);
            }
            musicController = musicTransform.GetComponent<MusicController>();
            if (musicController == null) musicController = musicTransform.gameObject.AddComponent<MusicController>();
            musicController.Setup(bank);
        }

        public static void RegisterPlayer(Transform transform, Health health)
        {
            GameAudio audio = GetInstance();
            if (audio == null) return;
            audio.player = transform;
            audio.musicController.SetPlayer(transform, health);
        }

        public static void SetWaveDefense(bool active)
        {
            GameAudio audio = active ? GetInstance() : instance;
            if (audio != null) audio.musicController.SetWaveDefense(active);
        }

        public static void ReportThreat(Vector3 position, bool attackingOrChasing)
        {
            if (instance == null || instance.player == null ||
                (position - instance.player.position).sqrMagnitude > 18f * 18f) return;
            instance.musicController.ReportThreat(attackingOrChasing);
        }

        // Spatial sounds fade with distance. An owner lets a weapon or zombie cancel its sounds.
        public static void Play(AudioCue cue, Vector3 position, bool spatial = false,
            float volume = 1f, Object owner = null)
        {
            if (Time.timeScale <= 0f) return;
            GameAudio audio = GetInstance();
            if (audio == null) return;
            GameAudioBank.Sound sound = audio.bank.Find(cue);
            if (sound == null) return;
            AudioClip clip = sound.Next();
            audio.PlayClip(clip, position, spatial, volume * sound.volume, owner);
        }

        // Run on the weapon so disabling it cancels pending reload stages too.
        public static IEnumerator PlayReload(AudioCue cue, Transform weapon, float duration, Object owner)
        {
            GameAudio audio = GetInstance();
            if (audio == null) yield break;
            GameAudioBank.Sound sound = audio.bank.Find(cue);
            if (sound == null) yield break;
            if (!sound.sequential)
            {
                Play(cue, weapon.position, owner: owner);
                yield break;
            }
            if (sound.clips == null || sound.clips.Length == 0) yield break;

            // Leave the final part of the animation for the charging sound's tail.
            float interval = Mathf.Max(0.1f, duration) / sound.clips.Length;
            for (int i = 0; i < sound.clips.Length; i++)
            {
                if (i > 0) yield return new WaitForSeconds(interval);
                if (audio == null || weapon == null) yield break;
                audio.PlayClip(sound.clips[i], weapon.position, false, sound.volume, owner);
            }
        }

        public static int AssignZombieVoice(ZombieAttackVoice preference)
        {
            GameAudio audio = GetInstance();
            if (audio == null || audio.bank.zombieAttackVoices == null) return -1;
            int count = audio.bank.zombieAttackVoices.Length;
            if (count == 0) return -1;
            if (preference != ZombieAttackVoice.Automatic)
            {
                return Mathf.Clamp((int)preference - 1, 0, count - 1);
            }

            // Keep one voice set per zombie, cycling through the sets as they spawn.
            int voice = audio.nextZombieVoice;
            audio.nextZombieVoice = (voice + 1) % count;
            return voice;
        }

        // The zombie stores its previous clip, so repeats are avoided separately for each zombie.
        public static void PlayZombieAttack(int voice, ref int previousClip, Vector3 position, Object owner)
        {
            if (Time.timeScale <= 0f) return;
            GameAudio audio = GetInstance();
            if (audio == null) return;
            GameAudioBank.AttackVoice[] sets = audio.bank.zombieAttackVoices;
            if (sets == null || voice < 0 || voice >= sets.Length)
            {
                Play(AudioCue.ZombieAttack, position, true, owner: owner);
                return;
            }
            AudioClip[] clips = sets[voice].clips;
            if (clips == null || clips.Length == 0) return;
            int index = Random.Range(0, clips.Length);
            if (clips.Length > 1 && index == previousClip)
            {
                index = (index + Random.Range(1, clips.Length)) % clips.Length;
            }
            previousClip = index;
            float volume = 0.32f;
            GameAudioBank.Sound attackSound = audio.bank.Find(AudioCue.ZombieAttack);
            if (attackSound != null) volume = attackSound.volume;
            audio.PlayClip(clips[index], position, true, volume, owner);
        }

        // Cancel only this object's sounds, leaving other enemies and effects playing.
        public static void StopOwned(Object owner)
        {
            if (instance == null || owner == null) return;
            for (int i = 0; i < SoundSourceCount; i++)
            {
                if (instance.sourceOwners[i] != owner) continue;
                instance.soundSources[i].Stop();
                instance.sourceOwners[i] = null;
            }
        }

        public static void Step(FootstepAction action, Vector3 position, float volume)
        {
            if (Time.timeScale <= 0f) return;
            GameAudio audio = GetInstance();
            if (audio == null) return;
            GameAudioBank.Steps steps = audio.bank.footsteps;
            if (steps == null) return;
            AudioClip[] clips = action == FootstepAction.Run ? steps.run : steps.walk;
            if (clips == null || clips.Length == 0) clips = steps.walk;
            if (clips == null || clips.Length == 0) return;
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            audio.PlayClip(clip, position, false, volume, null);
        }

        // Story sequences can use this path to respect the independent cinematic level.
        public static void PlayCinematic(AudioClip clip, Vector3 position, Object owner = null)
        {
            if (Time.timeScale <= 0f) return;
            GameAudio audio = GetInstance();
            if (audio != null) audio.PlayClip(clip, position, false, 1f, owner, cinematic: true);
        }

        private void PlayClip(AudioClip clip, Vector3 position, bool spatial, float volume, Object owner,
            bool cinematic = false)
        {
            if (clip == null || paused) return;
            // Sounds stay on these sources so an explosion can finish after its grenade is destroyed.
            if (spatial && player != null && (position - player.position).sqrMagnitude > 28f * 28f) return;
            for (int i = 0; i < soundSources.Length; i++)
            {
                AudioSource source = soundSources[i];
                // Never steal an active source. If all are busy, this sound is skipped.
                if (source.isPlaying) continue;
                sourceOwners[i] = owner;
                source.transform.position = position;
                source.spatialBlend = spatial ? 1f : 0f;
                sourceLevels[i] = Mathf.Clamp01(volume * (cinematic ? 1f : bank.effectsVolume));
                cinematicSources[i] = cinematic;
                source.volume = sourceLevels[i] * (cinematic ? AudioSettings.Cinematics : AudioSettings.Sfx);
                source.PlayOneShot(clip);
                return;
            }
        }

        private void Update()
        {
            if (lastSfxVolume != AudioSettings.Sfx || lastCinematicVolume != AudioSettings.Cinematics)
            {
                lastSfxVolume = AudioSettings.Sfx;
                lastCinematicVolume = AudioSettings.Cinematics;
                for (int i = 0; i < soundSources.Length; i++)
                    soundSources[i].volume = sourceLevels[i] *
                        (cinematicSources[i] ? lastCinematicVolume : lastSfxVolume);
            }

            // Audio does not follow timeScale automatically; pause and resume sources explicitly.
            bool gamePaused = Time.timeScale <= 0f;
            if (gamePaused == paused) return;
            paused = gamePaused;

            foreach (AudioSource source in soundSources)
            {
                if (paused)
                {
                    source.Pause();
                }
                else
                {
                    source.UnPause();
                }
            }
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}

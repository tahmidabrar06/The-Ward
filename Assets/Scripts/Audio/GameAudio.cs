using TheWard.Combat;
using UnityEngine;

namespace TheWard.Audio
{
    // Shared sound playback for the current scene. Player startup creates it on demand.
    public class GameAudio : MonoBehaviour
    {
        // Reuse a fixed number of sources instead of creating an object for every sound.
        private const int SoundSourceCount = 32;
        private static GameAudio instance;
        [SerializeField] private GameAudioBank bank;
        private readonly AudioSource[] soundSources = new AudioSource[SoundSourceCount];
        private readonly Object[] sourceOwners = new Object[SoundSourceCount];
        private MusicController musicController;
        private Transform player;
        private bool paused;
        private int nextZombieVoice;

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

            GameAudioBank audioBank = Resources.Load<GameAudioBank>("GameAudioBank");
            if (audioBank == null) return null;

            GameObject audioObject = new GameObject("Game Audio");
            instance = audioObject.AddComponent<GameAudio>();
            instance.bank = audioBank;
            instance.SetupAudio();
            return instance;
        }

        private void SetupAudio()
        {
            for (int i = 0; i < soundSources.Length; i++)
            {
                GameObject soundObject = new GameObject("Sound Effect " + (i + 1));
                soundObject.transform.SetParent(transform);

                AudioSource source = soundObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = 2f;
                source.maxDistance = 28f;
                source.dopplerLevel = 0f;
                source.priority = 64;
                soundSources[i] = source;
            }

            GameObject musicObject = new GameObject("Music");
            musicObject.transform.SetParent(transform);
            musicController = musicObject.AddComponent<MusicController>();
            musicController.Setup(bank);
        }

        // Both player controllers call this so every floor can start its music independently.
        public static void RegisterPlayer(Transform transform, Health health)
        {
            GameAudio audio = GetInstance();
            if (audio == null) return;
            audio.player = transform;
            audio.musicController.SetPlayer(transform, health);
        }

        // Only nearby zombies influence the music; distant encounters stay out of the mix.
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
            float volume = 0.7f;
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

        private void PlayClip(AudioClip clip, Vector3 position, bool spatial, float volume, Object owner)
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
                source.volume = Mathf.Clamp01(volume * bank.effectsVolume);
                source.PlayOneShot(clip);
                return;
            }
        }

        private void Update()
        {
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

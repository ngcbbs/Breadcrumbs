using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace GamePortfolio.Core
{
    /// <summary>
    /// Manages all audio in the game, including background music and sound effects
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource uiSource;
        
        [Header("Audio Mixers")]
        [SerializeField] private AudioMixer masterMixer;
        
        [Header("Audio Clips")]
        [SerializeField] private AudioClip[] musicClips;
        [SerializeField] private AudioClip[] ambientClips;
        
        // Dictionary to store sound effects by name
        private Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioClip> uiClips = new Dictionary<string, AudioClip>();
        
        // Volume parameters
        private const string MasterVolParam = "MasterVolume";
        private const string MusicVolParam = "MusicVolume";
        private const string SfxVolParam = "SfxVolume";
        private const string AmbientVolParam = "AmbientVolume";
        private const string UiVolParam = "UIVolume";
        
        // Current playing music index
        private int currentMusicIndex = -1;
        private int currentAmbientIndex = -1;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Initialize audio sources if not set
            InitializeAudioSources();
            
            // Load all sound effects
            LoadSoundEffects();
        }
        
        private void Start()
        {
            // Apply initial settings from GameSettings
            if (GameManager.HasInstance && GameManager.Instance.Settings != null)
            {
                GameSettings settings = GameManager.Instance.Settings;
                SetMasterVolume(settings.MasterVolume);
                SetMusicVolume(settings.MusicVolume);
                SetSfxVolume(settings.SfxVolume);
            }
        }
        
        /// <summary>
        /// Initialize audio sources if they aren't set in the inspector
        /// </summary>
        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("Music Source");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFX Source");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
            
            if (ambientSource == null)
            {
                GameObject ambientObj = new GameObject("Ambient Source");
                ambientObj.transform.SetParent(transform);
                ambientSource = ambientObj.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }
            
            if (uiSource == null)
            {
                GameObject uiObj = new GameObject("UI Source");
                uiObj.transform.SetParent(transform);
                uiSource = uiObj.AddComponent<AudioSource>();
                uiSource.loop = false;
                uiSource.playOnAwake = false;
            }
        }
        
        /// <summary>
        /// Load all sound effects from Resources folder
        /// </summary>
        private void LoadSoundEffects()
        {
            // Load SFX clips
            AudioClip[] loadedSfx = Resources.LoadAll<AudioClip>("Audio/SFX");
            foreach (AudioClip clip in loadedSfx)
            {
                sfxClips[clip.name] = clip;
            }
            
            // Load UI clips
            AudioClip[] loadedUi = Resources.LoadAll<AudioClip>("Audio/UI");
            foreach (AudioClip clip in loadedUi)
            {
                uiClips[clip.name] = clip;
            }
            
            Debug.Log($"Loaded {sfxClips.Count} sound effects and {uiClips.Count} UI sounds");
        }
        
        #region Volume Control
        
        /// <summary>
        /// Set master volume level (0 to 1)
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            SetMixerVolume(MasterVolParam, volume);
        }
        
        /// <summary>
        /// Set music volume level (0 to 1)
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            SetMixerVolume(MusicVolParam, volume);
        }
        
        /// <summary>
        /// Set sound effects volume level (0 to 1)
        /// </summary>
        public void SetSfxVolume(float volume)
        {
            SetMixerVolume(SfxVolParam, volume);
        }
        
        /// <summary>
        /// Set ambient sound volume level (0 to 1)
        /// </summary>
        public void SetAmbientVolume(float volume)
        {
            SetMixerVolume(AmbientVolParam, volume);
        }
        
        /// <summary>
        /// Set UI sound volume level (0 to 1)
        /// </summary>
        public void SetUiVolume(float volume)
        {
            SetMixerVolume(UiVolParam, volume);
        }
        
        /// <summary>
        /// Convert linear volume (0 to 1) to logarithmic mixer value
        /// </summary>
        private void SetMixerVolume(string paramName, float linearVolume)
        {
            // Ensure volume is in valid range
            linearVolume = Mathf.Clamp01(linearVolume);
            
            // Convert linear volume to logarithmic (-80dB to 0dB)
            // -80dB is effectively silent
            float mixerValue = linearVolume > 0.001f ? 
                20f * Mathf.Log10(linearVolume) : 
                -80f;
            
            // Set mixer parameter
            if (masterMixer != null)
            {
                masterMixer.SetFloat(paramName, mixerValue);
            }
        }
        
        #endregion
        
        #region Music Playback
        
        /// <summary>
        /// Play background music by index
        /// </summary>
        public void PlayMusic(int index, float fadeTime = 1.0f)
        {
            if (musicClips == null || musicClips.Length == 0)
            {
                Debug.LogWarning("No music clips assigned to AudioManager");
                return;
            }
            
            // Make sure index is valid
            index = Mathf.Clamp(index, 0, musicClips.Length - 1);
            
            // Skip if already playing this track
            if (index == currentMusicIndex && musicSource.isPlaying)
            {
                return;
            }
            
            // If already playing something, fade out and then fade in new track
            if (musicSource.isPlaying)
            {
                // Start fade out coroutine
                StartCoroutine(FadeMusicRoutine(0f, fadeTime, () => {
                    // After fade out, change clip and fade in
                    musicSource.clip = musicClips[index];
                    currentMusicIndex = index;
                    musicSource.Play();
                    StartCoroutine(FadeMusicRoutine(1f, fadeTime));
                }));
            }
            else
            {
                // Not playing anything, just start new track
                musicSource.clip = musicClips[index];
                currentMusicIndex = index;
                musicSource.volume = 0f;
                musicSource.Play();
                StartCoroutine(FadeMusicRoutine(1f, fadeTime));
            }
        }
        
        /// <summary>
        /// Play a random background music track
        /// </summary>
        public void PlayRandomMusic(float fadeTime = 1.0f)
        {
            if (musicClips == null || musicClips.Length == 0)
            {
                Debug.LogWarning("No music clips assigned to AudioManager");
                return;
            }
            
            int randomIndex;
            // Avoid playing the same track twice in a row
            do
            {
                randomIndex = Random.Range(0, musicClips.Length);
            } while (randomIndex == currentMusicIndex && musicClips.Length > 1);
            
            PlayMusic(randomIndex, fadeTime);
        }
        
        /// <summary>
        /// Stop playing background music
        /// </summary>
        public void StopMusic(float fadeTime = 1.0f)
        {
            if (musicSource.isPlaying)
            {
                StartCoroutine(FadeMusicRoutine(0f, fadeTime, () => {
                    musicSource.Stop();
                    currentMusicIndex = -1;
                }));
            }
        }
        
        /// <summary>
        /// Coroutine to fade music volume
        /// </summary>
        private System.Collections.IEnumerator FadeMusicRoutine(float targetVolume, float duration, System.Action onComplete = null)
        {
            float startVolume = musicSource.volume;
            float t = 0f;
            
            while (t < duration)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, targetVolume, t / duration);
                yield return null;
            }
            
            musicSource.volume = targetVolume;
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region Sound Effects
        
        /// <summary>
        /// Play a sound effect by name
        /// </summary>
        public void PlaySfx(string sfxName, float volumeScale = 1.0f)
        {
            if (sfxClips.TryGetValue(sfxName, out AudioClip clip))
            {
                sfxSource.PlayOneShot(clip, volumeScale);
            }
            else
            {
                Debug.LogWarning($"Sound effect '{sfxName}' not found");
            }
        }
        
        /// <summary>
        /// Play a UI sound by name
        /// </summary>
        public void PlayUiSound(string soundName, float volumeScale = 1.0f)
        {
            if (uiClips.TryGetValue(soundName, out AudioClip clip))
            {
                uiSource.PlayOneShot(clip, volumeScale);
            }
            else
            {
                Debug.LogWarning($"UI sound '{soundName}' not found");
            }
        }
        
        /// <summary>
        /// Play a sound effect at a specific position in 3D space
        /// </summary>
        public void PlaySfxAt(string sfxName, Vector3 position, float volumeScale = 1.0f)
        {
            if (sfxClips.TryGetValue(sfxName, out AudioClip clip))
            {
                AudioSource.PlayClipAtPoint(clip, position, volumeScale);
            }
            else
            {
                Debug.LogWarning($"Sound effect '{sfxName}' not found");
            }
        }
        
        #endregion
        
        #region Ambient Sounds
        
        /// <summary>
        /// Play ambient sound by index
        /// </summary>
        public void PlayAmbient(int index, float fadeTime = 1.0f)
        {
            if (ambientClips == null || ambientClips.Length == 0)
            {
                Debug.LogWarning("No ambient clips assigned to AudioManager");
                return;
            }
            
            // Make sure index is valid
            index = Mathf.Clamp(index, 0, ambientClips.Length - 1);
            
            // Skip if already playing this ambient sound
            if (index == currentAmbientIndex && ambientSource.isPlaying)
            {
                return;
            }
            
            // If already playing something, fade out and then fade in new track
            if (ambientSource.isPlaying)
            {
                // Start fade out coroutine
                StartCoroutine(FadeAmbientRoutine(0f, fadeTime, () => {
                    // After fade out, change clip and fade in
                    ambientSource.clip = ambientClips[index];
                    currentAmbientIndex = index;
                    ambientSource.Play();
                    StartCoroutine(FadeAmbientRoutine(1f, fadeTime));
                }));
            }
            else
            {
                // Not playing anything, just start new track
                ambientSource.clip = ambientClips[index];
                currentAmbientIndex = index;
                ambientSource.volume = 0f;
                ambientSource.Play();
                StartCoroutine(FadeAmbientRoutine(1f, fadeTime));
            }
        }
        
        /// <summary>
        /// Stop playing ambient sound
        /// </summary>
        public void StopAmbient(float fadeTime = 1.0f)
        {
            if (ambientSource.isPlaying)
            {
                StartCoroutine(FadeAmbientRoutine(0f, fadeTime, () => {
                    ambientSource.Stop();
                    currentAmbientIndex = -1;
                }));
            }
        }
        
        /// <summary>
        /// Coroutine to fade ambient sound volume
        /// </summary>
        private System.Collections.IEnumerator FadeAmbientRoutine(float targetVolume, float duration, System.Action onComplete = null)
        {
            float startVolume = ambientSource.volume;
            float t = 0f;
            
            while (t < duration)
            {
                t += Time.deltaTime;
                ambientSource.volume = Mathf.Lerp(startVolume, targetVolume, t / duration);
                yield return null;
            }
            
            ambientSource.volume = targetVolume;
            onComplete?.Invoke();
        }
        
        #endregion
    }
}

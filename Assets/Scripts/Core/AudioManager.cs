using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    //Adding this for now so we can set up voicelines and music soon
    
    public static AudioManager instance;

    [SerializeField] private Sound[] Sounds;

    private Dictionary<string, Sound> Clips;
    private Dictionary<Tuple<GameObject, string>, AudioSource> activeLoopingSounds;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup sfxMixerGroup;

    private Coroutine musicFadeCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Clips = new Dictionary<string, Sound>();

        activeLoopingSounds = new Dictionary<Tuple<GameObject, string>, AudioSource>();
        
        
        foreach (Sound s in Sounds)
        {
            if (!Clips.ContainsKey(s.name))
            {
                Clips.Add(s.name, s);
            }
            else
            {
                Debug.LogWarning("Duplicate sound name found in AudioManager: " + s.name);
            }
        }
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForLevel();
    }
    
    public void PlayOneshotSFX(string name, GameObject targetObject = null, bool randomPitch = true)
    {

        float pitch = 1.0f;

        if (randomPitch)
            pitch = UnityEngine.Random.Range(0.8f, 1.2f);


        if (Clips.TryGetValue(name, out Sound sound))
        {
            GameObject tempAudioHost = new GameObject($"SFX_{name}");
            if (targetObject == null)
                tempAudioHost.transform.position = Camera.main.transform.position;
            else
                tempAudioHost.transform.position = targetObject.transform.position;

            AudioSource newSource = tempAudioHost.AddComponent<AudioSource>();
            newSource.clip = sound.clip;
            newSource.pitch = pitch;
            newSource.volume = sound.volume;
            newSource.loop = true;
            newSource.playOnAwake = false;
            newSource.spatialBlend = 1.0f;
            newSource.minDistance = 25.0f;
            newSource.maxDistance = 150.0f;
            newSource.outputAudioMixerGroup = sfxMixerGroup;

            // Play the sound
            newSource.PlayOneShot(sound.clip);

            Destroy(tempAudioHost, sound.clip.length);
        }
        else
        {
            Debug.LogWarning("Sound not found in dictionary: " + name);
        }
    }

    public void StartLoopingSFX(string name, GameObject targetObject, bool randomPitch = false)
    {
        // Create a unique key for this sound instance
        var key = Tuple.Create(targetObject, name);
        
        //Prevents duplicates
        if (activeLoopingSounds.ContainsKey(key))
        {
            return;
        }

        if (Clips.TryGetValue(name, out Sound sound))
        {
            AudioSource newSource = targetObject.AddComponent<AudioSource>();

            float pitch = 1.0f;

            if (randomPitch)
                pitch = UnityEngine.Random.Range(0.8f, 1.2f);

            newSource.clip = sound.clip;
            newSource.volume = sound.volume;
            newSource.loop = true;
            newSource.pitch = pitch;
            newSource.playOnAwake = false;
            newSource.spatialBlend = 1.0f;
            newSource.minDistance = 25.0f;
            newSource.maxDistance = 150.0f;
            

            newSource.Play();
            activeLoopingSounds.Add(key, newSource);
        }
        else
        {
            Debug.LogWarning("Looping sound not found in dictionary: " + name);
        }
    }
    public void StopLoopingSFX(string name, GameObject targetObject)
    {
        var key = Tuple.Create(targetObject, name);

        if (activeLoopingSounds.TryGetValue(key, out AudioSource sourceToStop))
        {
            sourceToStop.Stop();
            Destroy(sourceToStop);
            activeLoopingSounds.Remove(key);
        }
        else
        {
            Debug.LogWarning("Tried to stop a looping sound that wasn't playing or was already stopped: " + name);
        }
    }

    public void PlayMusic(string name, float fadeDuration = 2.0f)
    {
        if (Clips.TryGetValue(name, out Sound musicSound))
        {
            if (musicSource.clip == musicSound.clip && musicSource.isPlaying)
            {
                return;
            }

            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }

            musicFadeCoroutine = StartCoroutine(FadeMusicTransition(musicSound, fadeDuration));
        }
        else
        {
            Debug.LogWarning("Music track not found: " + name);
        }
    }
    private IEnumerator FadeMusicTransition(Sound newMusic, float duration)
    {
        musicSource.outputAudioMixerGroup = musicMixerGroup;
        float halfDuration = duration / 2.0f;
        float startVolume = musicSource.isPlaying ? musicSource.volume : 0;
        float timer = 0;

        if (musicSource.isPlaying)
        {
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0, timer / halfDuration);
                yield return null;
            }
            musicSource.Stop();
        }
        
        musicSource.clip = newMusic.clip;
        musicSource.loop = true;
        musicSource.Play();

        float targetVolume = newMusic.volume;
        timer = 0; 

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0, targetVolume, timer / halfDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;
        musicFadeCoroutine = null; 
    }

    [Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0.0f, 1.0f)]
        public float volume;
    }
    
    public void PlayMusicForLevel()
    {
        /*int level = SceneManager.GetActiveScene().buildIndex;

        switch (level) //Guesses for now
        {
            case 0: 
                PlayMusic("MenuMusic"); 
                break;
            case 1:
                PlayMusic("MainLevel");
                break;
            case 2:
                PlayMusic("Surgery");
                break;
            case 3:
                PlayMusic("Runway");
                break;
        }*/
    }

    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }
    
    
    public void PlayUISFX()
    {
        sfxSource.PlayOneShot(Clips["UI_Button"].clip);
    }
}

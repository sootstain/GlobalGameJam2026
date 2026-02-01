using UnityEngine;
using Yarn.Unity;

public class GlobalAudioStuff : MonoBehaviour
{

    private static GlobalAudioStuff _instance;
    public static GlobalAudioStuff instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<GlobalAudioStuff>();
            }
            return _instance;
        }
    }

    public AudioClip defaultVoiceOverClip;

    [YarnCommand("set_bg_voices")]
    public static void SetBgVoices(bool loud)
    {

    }

}
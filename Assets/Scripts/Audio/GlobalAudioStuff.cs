using System.Linq;
using ToyBoxHHH;
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

    public SmartSound npcAudio;

    public SmartSound bgVoices;
    public SmartSound crowdWooSound;

    [YarnCommand("set_bg_voices")]
    public static void SetBgVoices(bool loud)
    {
        if (instance.bgVoices != null)
        {
            var initVol = instance.bgVoices.audio.volume;
            var targetVolume = loud ? 1f : 0f;
            instance.StartCoroutine(pTween.To(1f, t =>
            {
                instance.bgVoices.audio.volume = Mathf.Lerp(initVol, targetVolume, t);

            }));
        }
    }

    [YarnCommand("crowd_woo")]
    public static void CrowdWoo()
    {
        instance.crowdWooSound.Play();
    }

    public void PlayAudio(LocalizedLine line)
    {
        // find character
        var npc = NpcManager.instance.FindNpc(line.CharacterName);

        if (npc == null)
            return;

        if (npc.languages.Length == 0)
            return;
        
        if (line.Metadata.Contains("greeting"))
        {
            var clip = npc.languages[0].audioGreeting;
            if (clip == null)
                return;

            npcAudio.clips.Clear();
            npcAudio.clips.Add(clip);
        }
        else if (line.Metadata.Contains("like"))
        {
            var clips = npc.languages[0].audioILikeThis;
            if (clips == null)
                return;

            npcAudio.clips.Clear();
            npcAudio.clips.AddRange(clips);
        } else
        {
            
            var clips = npc.languages[0].audioMisc;
            if (clips == null)
                return;

            npcAudio.clips.Clear();
            npcAudio.clips.AddRange(clips);
        }
        
        npcAudio.Play();

    }
}
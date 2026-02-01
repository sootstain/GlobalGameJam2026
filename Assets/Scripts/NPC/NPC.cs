using System;
using System.Linq;
using UnityEngine;

public enum NpcLanguages
{
    ENGLISH
    , RUSSIAN
    , SPANISH
    , DANISH
    , SWEDISH
    , FRENCH
    , CANTONESE
    , JAPANESE
}

[CreateAssetMenu(fileName = "NPCSO", menuName = "Scriptable Objects/NPC")]
public class NPC : ScriptableObject
{
    [Header("Attributes")]
    //Set up for original photos
    public Sprite photo;
    public Sprite deadPhoto;
    public Sprite deadPhoto2;
    public Sprite mouthPhoto;
    public Sprite nosePhoto;
    public Sprite rightEyePhoto;
    public Sprite leftEyePhoto;
    public Sprite talking1;
    public Sprite talking2;
    public Sprite talking3;

    public bool isDead;

    [Header("Types")]
    public EyeType eyeType;
    public MouthType mouthType;
    public NoseType noseType;

    [Header("Loves")]
    //LOVE
    public EyeType loveEyes;
    public MouthType loveMouth;
    public NoseType loveNose;

    [Header("Languages Spoken")]
    public LanguageInfo[] languages;

    [Serializable]
    public class LanguageInfo
    {
        public NpcLanguages language;

        [Header("Audio")]
        public AudioClip audioGreeting;
        public AudioClip[] audioILikeThis;
        public AudioClip[] audioMisc;

    }

    public bool CanSpeak(NpcLanguages playerLang)
    {
        return languages.Any(l => l.language == playerLang);
    }

}

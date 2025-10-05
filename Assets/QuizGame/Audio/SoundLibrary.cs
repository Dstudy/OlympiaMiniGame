using System.Collections.Generic;
using UnityEngine;

namespace QuizGame.Audio
{
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "QuizGame/SoundLibrary")]
    public class SoundLibrary : ScriptableObject
    {
        [Header("Background Music (will loop first or chosen index)")]
        public List<AudioClip> bgmClips = new List<AudioClip>();

        [Header("SFX")]
        public AudioClip tickClip;     // quiet metronome tick
        public AudioClip correctClip;  // “ding” for correct
        public AudioClip timeoutClip;
        public AudioClip chooseStarClip; // “choose” sound when starting a turn
        public AudioClip Endclip;
        public AudioClip confettiClip;

        [Header("Volumes (0..1)")]
        [Range(0f, 1f)] public float bgmVolume = 0.5f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float tickVolume = 0.4f;
    }
}
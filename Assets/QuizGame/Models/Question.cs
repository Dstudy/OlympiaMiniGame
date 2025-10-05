using UnityEngine;

namespace QuizGame.Models
{
    /// <summary>
    /// A single quiz question. Extend with media if needed.
    /// </summary>
    [System.Serializable]
    public class Question
    {
        [TextArea(2, 6)] public string Text;
        // Optional future fields:
        // public Sprite Image;
        // public AudioClip Audio;
        // public string Answer; // if you later auto-check answers
    }
}
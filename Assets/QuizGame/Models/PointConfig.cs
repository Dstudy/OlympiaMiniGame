using UnityEngine;

namespace QuizGame.Models
{
    [CreateAssetMenu(fileName = "PointsConfig", menuName = "QuizGame/PointsConfig")]
    public class PointsConfig : ScriptableObject
    {
        [Tooltip("Points for the 5 questions (default: 5,10,15,20,25)")]
        public int[] points = new int[] { 5, 10, 15, 20, 25 };

        public int Count => points != null ? points.Length : 0;
        public int Get(int questionIndex) => points[questionIndex];
    }
}
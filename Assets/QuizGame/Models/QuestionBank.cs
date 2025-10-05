using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace QuizGame.Models
{
    /// <summary>
    /// ScriptableObject holding 5 lists (tiers: 5/10/15/20/25 points).
    /// Provides shuffled queues at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "QuestionBank", menuName = "QuizGame/QuestionBank")]
    public class QuestionBank : ScriptableObject
    {
        [FormerlySerializedAs("Tier1")] [Header("10 points")]
        public List<Question> Tier1 = new List<Question>();
        [FormerlySerializedAs("Tier2")] [Header("10 points")]
        public List<Question> Tier2 = new List<Question>();
        [FormerlySerializedAs("Tier3")] [Header("20 points")]
        public List<Question> Tier3 = new List<Question>();
        [FormerlySerializedAs("Tier4")] [Header("25 points")]
        public List<Question> Tier4 = new List<Question>();
        [FormerlySerializedAs("Tier5")] [Header("35 points")]
        public List<Question> Tier5 = new List<Question>();

        // Runtime queues (built on Init)
        private Queue<Question> _q1, _q2, _q3, _q4, _q5;
        [NonSerialized] public bool initialized = false;

        public void InitIfNeeded()
        {
            // Debug.Log("Initializing QuestionBank..." + initialized);
            if (initialized) return;
            _q1  = new Queue<Question>(Shuffle(Tier1));
            _q2 = new Queue<Question>(Shuffle(Tier2));
            _q3 = new Queue<Question>(Shuffle(Tier3));
            _q4 = new Queue<Question>(Shuffle(Tier4));
            _q5 = new Queue<Question>(Shuffle(Tier5));
        }

        /// <summary>Pull next question for a given point value. Returns null if exhausted.</summary>
        public Question NextForPoints(int levels)
        {
            switch (levels+1)
            {
                case 1: return DequeueSafe(_q1);
                case 2: return DequeueSafe(_q2);
                case 3: return DequeueSafe(_q3);
                case 4: return DequeueSafe(_q4);
                case 5: return DequeueSafe(_q5);
                default: return null;
            }
        }

        private static Question DequeueSafe(Queue<Question> q) => (q != null && q.Count > 0) ? q.Dequeue() : null;

        private static IEnumerable<Question> Shuffle(List<Question> list)
        {
            // Fisher–Yates
            if (list == null) yield break;
            var arr = new List<Question>(list);
            for (int i = arr.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            foreach (var q in arr) yield return q;
        }
        
    }
}

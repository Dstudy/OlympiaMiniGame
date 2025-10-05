using UnityEngine;

namespace QuizGame.Models
{
    [System.Serializable]
    public class Student
    {
        public string Name;
        public int Score;
        public bool chooseStarMode = false;
        public int StarCount = 1;

        public Student(string name)
        {
            Name = name;
            Score = 0;
            chooseStarMode = false;
        }

        public void AddScore(int pts) => Score += pts;
    }
}
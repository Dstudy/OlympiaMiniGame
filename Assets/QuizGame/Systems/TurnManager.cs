using System;
using System.Collections.Generic;
using UnityEngine;
using QuizGame.Models;

namespace QuizGame.Systems
{
    /// <summary>
    /// Core game state machine: manages students, turn flow, redirects on "Another",
    /// awards points, and raises events for the UI layer.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        // ===== Events to decouple UI =====
        public event Action<Student> OnTurnStarted;
        public event Action<Student, int, int, bool> OnQuestionStarted; // (student, qIndex1Based, pointValue)
        public event Action<Student, int> OnPointsAwarded;        // (student, pointsAwarded)
        public event Action OnShowTimeoutChoices;
        public event Action OnHideTimeoutChoices;
        public event Action<Student> OnStudentFinished;           // fired after a student completes 5
        public event Action<List<Student>> OnScoreboardChanged;
        public event Action OnHideChooseStar; // when star student is chosen
        public event Action<List<Student>> OnEndGame;
        public event System.Action OnAnswerCorrect;

        [Header("Config")]
        [SerializeField] private PointsConfig pointsConfig;
        [SerializeField] private float questionTimeSeconds = 15f;

        [Header("Timer")]
        [SerializeField] private QuestionTimer questionTimer;

        [Header("Students (6)")]
        [SerializeField] public List<Student> students = new List<Student>
        {
            new Student("Thúy"),
            new Student("Thu"),
            new Student("Phương"),
            new Student("Linh"),
            new Student("Student 5"),
            new Student("Student 6"),
        };
        
        private int studentTurns = 0; // how many turns have been played
        
        [Header("Questions")]
        [SerializeField] private QuestionBank questionBank;
        
        // runtime
        [SerializeField]private Question _lockedQuestionForCurrentSlot = null;
        
        // in TurnManager
        public string CurrentQuestionText => _lockedQuestionForCurrentSlot != null ? _lockedQuestionForCurrentSlot.Text : "";

        // Internal state
        public int _currentStudent = -1;
        private int _currentQIndex = 0; // 0..4
        private bool _waitingTimeoutChoice = false;
        private int multiplier = 1;

        private struct ReturnPoint
        {
            public int studentIndex;
            public int nextQuestionIndex;
        }
        private Stack<ReturnPoint> _returnStack = new Stack<ReturnPoint>();

        private void Awake()
        {
            if (questionTimer != null)
            {
                questionTimer.OnElapsed += HandleTimerElapsed;
            }
            questionBank?.InitIfNeeded();   // ensure queues ready
            questionBank.initialized = true;
            OnScoreboardChanged?.Invoke(students);
        }

        private void OnDestroy()
        {
            if (questionTimer != null)
            {
                questionTimer.OnElapsed -= HandleTimerElapsed;
            }
        }

        // ===== Public API called by UI layer =====

        public void StartNewStudentTurn(int studentIndex)
        {
            Debug.Log(studentIndex);
            if (studentIndex < 0 || studentIndex >= students.Count) return;
            _currentStudent = studentIndex;
            _currentQIndex = 0;
            _lockedQuestionForCurrentSlot = null;
            RaiseTurnStarted();
            StartQuestion();
        }

        public void StartTimer()
        {
            questionTimer.StartTimer(questionTimeSeconds);
        }

        /// <summary>Called when user picks a different student for "Another".</summary>
        public void RedirectThisQuestionTo(int newStudentIndex)
        {
            if (newStudentIndex < 0 || newStudentIndex >= students.Count) return;

            var ret = new ReturnPoint
            {
                studentIndex = _currentStudent,
                nextQuestionIndex = _currentQIndex + 1
            };
            _returnStack.Push(ret);

            _currentStudent = newStudentIndex;
            // same _currentQIndex and same _lockedQuestionForCurrentSlot
            _waitingTimeoutChoice = false;
            OnHideTimeoutChoices?.Invoke();
            StartQuestion(true);
            OnShowTimeoutChoices?.Invoke();
        }

        public void OnChooseStar()
        {
            if(students[_currentStudent].StarCount <= 0) return; // no stars left
            students[_currentStudent].chooseStarMode = true;
            students[_currentStudent].StarCount--;
            OnHideChooseStar?.Invoke();
        }

        public void TimeoutChooseOK() // award points after timeout
        {
            AwardAndAdvance(pointsConfig.Get(_currentQIndex));
        }
        
        public void SkipQuestion()
        {
            if(students[_currentStudent].chooseStarMode == true)
                Punish(pointsConfig.Get(_currentQIndex));
            AwardAndAdvance(0); // award 0 points
        }

        public void TimeoutChooseAnother()
        {
            _waitingTimeoutChoice = false;
            OnHideTimeoutChoices?.Invoke();
            questionTimer?.StopTimer();
            if(students[_currentStudent].chooseStarMode == true)
                Punish(pointsConfig.Get(_currentQIndex)); // punish for timeout
        }

        // ===== Internals =====

        private void StartQuestion(bool force = false)
        {
            if (_currentQIndex >= pointsConfig.Count)
            {
                var s = students[_currentStudent];
                OnStudentFinished?.Invoke(s);
                return;
            }

            var student = students[_currentStudent];
            int points = pointsConfig.Get(_currentQIndex);

            // If we don't have a locked question for this slot yet, draw one from the bank
            if (_lockedQuestionForCurrentSlot == null)
            {
                _lockedQuestionForCurrentSlot = questionBank?.NextForPoints(_currentQIndex);
                if (_lockedQuestionForCurrentSlot == null)
                {
                    // Graceful fallback if pool exhausted: create a placeholder
                    _lockedQuestionForCurrentSlot = new Question { Text = $"[No more questions for {points} pts]" };
                }
            }

            // Notify UI (extend the event or add a new one if you prefer)
            OnQuestionStarted?.Invoke(student, _currentQIndex + 1, points, force);

            _waitingTimeoutChoice = false;
            OnHideTimeoutChoices?.Invoke();

            questionTimer?.InitTimer(questionTimeSeconds);
            OnScoreboardChanged?.Invoke(students);
        }
        

        private void HandleTimerElapsed()
        {
            if (_waitingTimeoutChoice) return;
            _waitingTimeoutChoice = true;
            OnShowTimeoutChoices?.Invoke();
        }

        private void Punish(int pts)
        {
            var student = students[_currentStudent];
            multiplier = -1; // negative points for punishment
            student.AddScore(pts * multiplier);
            if (students[_currentStudent].chooseStarMode)
            {
                students[_currentStudent].chooseStarMode = false; // reset after use
            }
        }
        
        public void PunishRedirect()
        {
            var student = students[_currentStudent];
            multiplier = -1; // negative points for punishment
            int points = pointsConfig.Get(_currentQIndex)/2;
            student.AddScore(points * multiplier);
            // OnPointsAwarded?.Invoke(student, points);
            AwardAndAdvance(0);
        }

        private void AwardAndAdvance(int pts)
        {
            var student = students[_currentStudent];
            if (students[_currentStudent].chooseStarMode)
            {
                multiplier = 2; // double points for star student
                students[_currentStudent].chooseStarMode = false; // reset after use
            }
            else multiplier = 1;
            if(pts* multiplier > 0)
                OnAnswerCorrect?.Invoke();   
            
            student.AddScore(pts * multiplier);
            OnPointsAwarded?.Invoke(student, pts);
            OnScoreboardChanged?.Invoke(students);

            questionTimer?.StopTimer();
            _waitingTimeoutChoice = false;
            OnHideTimeoutChoices?.Invoke();

            // This question is done; release lock for the slot
            _lockedQuestionForCurrentSlot = null;

            if (_returnStack.Count > 0)
            {
                var back = _returnStack.Pop();
                _currentStudent = back.studentIndex;
                _currentQIndex = back.nextQuestionIndex;

                if (_currentQIndex >= pointsConfig.Count)
                {
                    OnStudentFinished?.Invoke(students[_currentStudent]);
                    studentTurns++;
                    if (studentTurns >= students.Count)
                    {
                        Debug.Log("All students have finished their turns.");
                        OnEndGame?.Invoke(students);
                    }
                }
                else
                {
                    StartQuestion();
                }
                return;
            }

            _currentQIndex++;
            if (_currentQIndex >= pointsConfig.Count)
            {
                studentTurns++;
                if (studentTurns >= students.Count)
                {
                    Debug.Log("All students have finished their turns.");
                    OnEndGame?.Invoke(students);
                }
                else OnStudentFinished?.Invoke(student);
            }
            else
            {
                StartQuestion();
            }
        }
        
        public void RemoveStudent()
        {
            students.RemoveAt(0);
        }

        private void RaiseTurnStarted() => OnTurnStarted?.Invoke(students[_currentStudent]);

        // ===== Exposed for UI =====
        public IReadOnlyList<Student> Students => students;
        public int CurrentStudentIndex => _currentStudent;
    }
}

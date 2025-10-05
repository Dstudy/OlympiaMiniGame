using System;
using UnityEngine;

namespace QuizGame.Systems
{
    /// <summary>
    /// Standalone countdown timer. Start with StartTimer(seconds).
    /// Fires OnTick every frame, and OnElapsed when time hits 0.
    /// </summary>
    ///
    /// 
    public class QuestionTimer : MonoBehaviour
    {
        public event Action<float> OnTick;     // remaining seconds
        public event Action OnElapsed;
        // inside QuestionTimer.cs
        public event System.Action OnStarted;
        public event System.Action OnStopped;

        [SerializeField] private bool autoStart = false;
        [SerializeField] private float initialSeconds = 15f;

        private float _timeLeft;
        private bool _running;

        private void Start()
        {
            
        }

        private void Update()
        {
            if (!_running) return;

            _timeLeft -= Time.deltaTime;
            if (_timeLeft < 0) _timeLeft = 0;

            OnTick?.Invoke(_timeLeft);

            if (_timeLeft <= 0f)
            {
                _running = false;
                OnElapsed?.Invoke();
            }
        }

        public void StartTimer(float seconds)
        {
            _timeLeft = Mathf.Max(0, seconds);
            _running = true;
            OnStarted?.Invoke();
            OnTick?.Invoke(_timeLeft);
        }
        
        public void InitTimer(float seconds)
        {
            _timeLeft = Mathf.Max(0, seconds);
            _running = false; // don't start automatically
            OnTick?.Invoke(_timeLeft);
        }

        public void StopTimer()
        {
            bool wasRunning = _running;
            _running = false;
            if (wasRunning) OnStopped?.Invoke();
        }
        public float TimeLeft => _timeLeft;
        public bool IsRunning => _running;
    }
}
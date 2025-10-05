using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QuizGame.Systems;
using QuizGame.Models;

namespace QuizGame.UI
{
    /// <summary>
    /// Student picker panel: shows 6 buttons, routes selections either to
    /// StartNewStudentTurn (normal) or RedirectThisQuestionTo (on "Another").
    /// </summary>
    public class StudentPickerView : MonoBehaviour
    {
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private GameObject panel;
        [SerializeField] private Button[] studentButtons; // size 6
        [SerializeField] private Text[] labels;       // optional: show names on buttons

        public bool _isRedirectMode = false;
        
        private bool _isInitialized = false;

        private void OnEnable()
        {
            if (_isInitialized) return;
            // Label and wire buttons
            for (int i = 0; i < turnManager.Students.Count; i++)
            {
                studentButtons[i].gameObject.SetActive(true);
                int idx = i;
                studentButtons[i].onClick.AddListener(() => OnPick(idx));
                if (labels != null && i < labels.Length && i < turnManager.Students.Count)
                {
                    labels[i].text = turnManager.Students[i].Name;
                }
            }

            Show(true); // show at very start to pick the first student

            // When a student finishes, show picker for next turn
            turnManager.OnStudentFinished += _ => { _isRedirectMode = false; Show(true); };

            // When HUD says "Another", we flip to redirect mode and UI (you) should call Show(true)
            // You can hook this from HUD by exposing a public method below.
            _isInitialized = true;
        }

        public void Show(bool show)
        {
            if (panel != null) panel.SetActive(show);
        }

        /// <summary>
        /// HUD calls this when the host pressed "Another" and after that button hides.
        /// This makes the picker choose a different student to attempt the same question.
        /// </summary>
        public void EnterRedirectModeAndShow()
        {
            _isRedirectMode = true;
            Show(true);
        }
        
        public void ExitRedirectMode()
        {
            _isRedirectMode = false;
            Show(false);
        }

        private void OnPick(int index)
        {
            if (_isRedirectMode)
            {
                // Same question → new student
                turnManager.RedirectThisQuestionTo(index);
                _isRedirectMode = false;
                Show(false);
            }
            else
            {
                // Start a fresh 5-question turn for this student
                turnManager.StartNewStudentTurn(index);
                Debug.Log("Picked student: " + turnManager.Students[index].Name);
                Show(false);
            }
        }
    }
}

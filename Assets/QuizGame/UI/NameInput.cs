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
    public class NameInput : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private GameObject nameEntryPanel;
        [SerializeField] private GameObject studentPickerPanel;
        
        [SerializeField] private TMP_InputField numInput;

        private int currentIndex = 0;

        private void Start()
        {
            currentIndex = 0;
            ShowPrompt();
            
            // Listen for submit (pressing Enter in TMP_InputField)
            nameInput.onSubmit.AddListener(HandleSubmit);
            numInput.onSubmit.AddListener(HandleNumSubmit);
        }

        private void ShowPrompt()
        {
            if (currentIndex < turnManager.Students.Count)
            {
                promptText.text = $"Enter name for Student {currentIndex + 1}";
                nameInput.text = "";
                nameInput.ActivateInputField(); // focus input
            }
            else
            {
                // Finished all students
                nameEntryPanel.SetActive(false);
                studentPickerPanel.SetActive(true);
            }
        }

        private void HandleSubmit(string typed)
        {
            if (string.IsNullOrWhiteSpace(typed))
                return; // ignore empty

            // Assign name to student
            turnManager.Students[currentIndex].Name = typed.Trim();
            currentIndex++;

            ShowPrompt();
        }
        
        private void HandleNumSubmit(string typed)
        {
            int studentNum = int.Parse(typed);
            if (studentNum < 1 || studentNum > turnManager.Students.Count)
                return; // ignore invalid
            for(int i = studentNum; i <= turnManager.Students.Count; i++)
            {
                turnManager.RemoveStudent();
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QuizGame.Systems;
using QuizGame.Models;
using UnityEngine.Serialization;

namespace QuizGame.UI
{
    /// <summary>
    /// Handles HUD: texts, timer readout, and buttons (Correct / Timeout OK / Timeout Another).
    /// Pure UI; listens to TurnManager events and calls its public API.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private QuestionTimer timer;
        
        [Header("Text")]
        [SerializeField] private TMP_Text studentNameText;
        [SerializeField] private TMP_Text questionIndexText; // "Q 3 / 5 — 15 pts"
        [SerializeField] private TMP_Text scoreboardText;
        [SerializeField] private TMP_Text scoreboardTextOutSide;
        [SerializeField] private TMP_Text questionText; // assign in inspector
        [SerializeField] private TextMeshProUGUI[] rankTexts;

        [FormerlySerializedAs("correctButton")]
        [Header("Buttons")]
        [SerializeField] private Button startButton;          // host marks correct
        [SerializeField] private Button timeoutOkButton;        // after timeout: give points
        [SerializeField] private Button timeoutAnotherButton;   // after timeout: choose another student
        [SerializeField] private Button wrongButton; // for restricted mode, to mark wrong answer
        [SerializeField] private Button skipButton; // for restricted mode, to skip question
        [SerializeField] private Button chooseStarButton;
        [SerializeField] private Button backButton; 
        [SerializeField] private Animator starEffectAnimator;
        [SerializeField] private Sprite starEffectSprite; // optional: sprite to show on star effect
        [SerializeField] private Animator countdownAnimator; // for countdown effect
        [SerializeField] private Sprite startCountdownSprite; // optional: sprite to show on start button

        [SerializeField] private Button testButton;
        private bool testMode = false;
        
        
        [SerializeField] private StudentPickerView picker;
        [SerializeField] private EndGame endGame;
        

        private void Awake()
        {
            // Wire TurnManager events
            turnManager.OnTurnStarted += HandleTurnStarted;
            turnManager.OnQuestionStarted += HandleQuestionStarted;
            turnManager.OnPointsAwarded += HandlePointsAwarded;
            turnManager.OnShowTimeoutChoices += () => ShowTimeoutButtons(true);
            turnManager.OnHideTimeoutChoices += () => ShowTimeoutButtons(false);
            turnManager.OnStudentFinished += HandleStudentFinished;
            turnManager.OnScoreboardChanged += RefreshCurrentStudentScore;
            turnManager.OnEndGame += Endgame; 

            if (starEffectAnimator) starEffectAnimator.enabled = false;
            if (countdownAnimator) countdownAnimator.enabled = false; // disable countdown effect at start
            

            // Wire button clicks
            if (startButton) startButton.onClick.AddListener(() =>
            {
                if (testMode)
                {
                    skipButton.gameObject.SetActive(true);
                }
                turnManager.StartTimer();
                countdownAnimator.enabled = true;
                if (questionText) questionText.text = turnManager.CurrentQuestionText;
                countdownAnimator.Play("Countdown", 0, 0f); // play from beginning
                startButton.gameObject.SetActive(false); // hide start button
            });
            if (timeoutOkButton) timeoutOkButton.onClick.AddListener(() => turnManager.TimeoutChooseOK());
            if (timeoutAnotherButton)
                timeoutAnotherButton.onClick.AddListener(() =>
                {
                    backButton.gameObject.SetActive(true);
                    turnManager.TimeoutChooseAnother();   // stop timer + hide timeout buttons
                    picker.EnterRedirectModeAndShow();    // <-- open the picker in redirect mode
                });
            if (wrongButton)
                wrongButton.onClick.AddListener(() => turnManager.PunishRedirect());
            if (skipButton) 
                skipButton.onClick.AddListener(() => turnManager.SkipQuestion());

            ShowTimeoutButtons(false);
            if (startButton) startButton.gameObject.SetActive(false);
            if (chooseStarButton)
            {
                chooseStarButton.onClick.AddListener(() =>
                {
                    turnManager.OnChooseStar(); // raise event to pick a star student
                    starEffectAnimator.enabled = true;
                    starEffectAnimator.Play("StarEffect", 0, 0f); // play from beginning
                });
            };

            if (backButton)
            {
                backButton.onClick.AddListener((() =>
                {
                    picker.ExitRedirectMode();
                    ShowTimeoutButtons(true);
                }));
                backButton.gameObject.SetActive(false);
            }
            
            if(testButton) testButton.onClick.AddListener(() =>
            {
                testMode = !testMode;
                Endgame(turnManager.students);
            });
        }

        private void OnDestroy()
        {
            if (turnManager != null)
            {
                turnManager.OnTurnStarted -= HandleTurnStarted;
                turnManager.OnQuestionStarted -= HandleQuestionStarted;
                turnManager.OnPointsAwarded -= HandlePointsAwarded;
                turnManager.OnShowTimeoutChoices -= () => ShowTimeoutButtons(true);
                turnManager.OnHideTimeoutChoices -= () => ShowTimeoutButtons(false);
                turnManager.OnStudentFinished -= HandleStudentFinished;
            }
        }

        // ===== Event handlers =====

        private void HandleTurnStarted(Student s)
        {
            if (studentNameText) studentNameText.text = s.Name;
            // Debug.Log(studentNameText.text + " started turn");
            if (startButton) startButton.gameObject.SetActive(true);
        }

        private void HandleQuestionStarted(Student s, int qIndex1Based, int points, bool force)
        {
            if (studentNameText) studentNameText.text = s.Name;
            if (picker._isRedirectMode) 
                questionText.text = turnManager.CurrentQuestionText;
            else
                questionText.text = "";
            if (questionIndexText) questionIndexText.text = $"Q{qIndex1Based} - {points} pts";
            if (startButton) startButton.gameObject.SetActive(true);
            
            if(countdownAnimator)
            {
                countdownAnimator.gameObject.SetActive(true);
                countdownAnimator.gameObject.GetComponent<Image>().sprite = startCountdownSprite;
                countdownAnimator.enabled = false; // enable countdown effect
            }
            
            if(s.StarCount <= 0 || force)
            {
                starEffectAnimator.gameObject.GetComponent<Image>().sprite = null;
                starEffectAnimator.gameObject.SetActive(false); // hide star effect
                chooseStarButton.enabled = false;
                starEffectAnimator.enabled = false;
            }
            else
            {
                if (starEffectAnimator)
                {
                    starEffectAnimator.gameObject.SetActive(true); // show star effect
                    starEffectAnimator.gameObject.GetComponent<Image>().sprite = starEffectSprite;
                    starEffectAnimator.enabled = false; // disable star effect at start
                    chooseStarButton.enabled = true;
                }
            }
        }
        
        private void RefreshCurrentStudentScore(System.Collections.Generic.List<Student> list)
        {
            // Debug.Log("Showing scoreboard for " + list.Count + " students");
            if (!scoreboardText) return;

            int idx = turnManager.CurrentStudentIndex;
            if (idx < 0 || idx >= turnManager.Students.Count)
            {
                scoreboardText.text = "";
                return;
            }

            if (!scoreboardText) return;
            var sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                sb.Append($"{list[i].Name}:{list[i].Score}");
                if(i < list.Count - 1)
                    sb.Append(" | ");
            }
            scoreboardText.text = sb.ToString();
            scoreboardTextOutSide.text = sb.ToString();
        }

        private void Endgame(List<Student> list)
        {
            List<Student> sortedList = list.OrderByDescending(list => list.Score).ToList();
            for (int i = 0; i < rankTexts.Length; i++)
            {
                if (i < list.Count)
                {
                    rankTexts[i].text = $"{i + 1}. {sortedList[i].Name} - {sortedList[i].Score} pts";
                }
                else
                {
                    rankTexts[i].text = "";
                }
            }
            endGame.ShowEnd();
        }
        
        private void HandlePointsAwarded(Student s, int pts)
        {
            // Optional: flash UI, etc.
            if (startButton) startButton.gameObject.SetActive(false);
        }

        private void HandleStudentFinished(Student s)
        {
            if (questionIndexText) questionIndexText.text = $"{s.Name} finished!";
            if (startButton) startButton.gameObject.SetActive(false);
            ShowTimeoutButtons(false);
        }
        
        private void ShowTimeoutButtons(bool show)
        {
            if (picker._isRedirectMode)
            {
                wrongButton.gameObject.SetActive(show);
                countdownAnimator.gameObject.GetComponent<Image>().sprite = null; // hide countdown effect
                countdownAnimator.gameObject.SetActive(false);
            }
            else
            {
                wrongButton.gameObject.SetActive(false);
                if (timeoutAnotherButton) timeoutAnotherButton.gameObject.SetActive(show);
                if(skipButton) skipButton.gameObject.SetActive(show);
            }
            if (timeoutOkButton) timeoutOkButton.gameObject.SetActive(show);
            if (startButton) startButton.gameObject.SetActive(!show); // hide Correct when timeout choice is visible
        }
        
    }
}

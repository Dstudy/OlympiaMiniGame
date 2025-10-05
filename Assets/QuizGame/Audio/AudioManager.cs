using System.Collections;
using UnityEngine;
using QuizGame.Audio;
using QuizGame.Systems;

namespace QuizGame.AudioRuntime
{
    /// <summary>
    /// Single point for BGM + SFX. Subscribes to TurnManager and QuestionTimer.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private SoundLibrary library;

        [Header("Game Events")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private QuestionTimer questionTimer;

        private AudioSource _bgmSrc;   // loops music
        private AudioSource _sfxSrc;   // one-shot SFX (correct)
        private AudioSource _tickSrc;  // looping tick during countdown
        private AudioSource _sfxSrc2;

        private void Awake()
        {
            // Create AudioSources programmatically (so scene stays clean)
            _bgmSrc  = gameObject.AddComponent<AudioSource>();
            _sfxSrc  = gameObject.AddComponent<AudioSource>();
            _tickSrc = gameObject.AddComponent<AudioSource>();
            _sfxSrc2 = gameObject.AddComponent<AudioSource>();
            

            _bgmSrc.loop = true;
            _tickSrc.loop = true;

            ApplyVolumes();
        }

        private void OnEnable()
        {
            if (questionTimer != null)
            {
                questionTimer.OnElapsed += HandleTimeoutElapsed;
                questionTimer.OnStarted += StartTickLoop;
                questionTimer.OnStopped += StopTickLoop;
            }

            if (turnManager != null)
            {
                turnManager.OnAnswerCorrect += PlayCorrectSfx;
                turnManager.OnTurnStarted += _ => EnsureBgm();
                turnManager.OnHideChooseStar += PLayChooseStarSfx;
                turnManager.OnEndGame += _ => PlayEndSfx();
            }
        }
        private void OnDisable()
        {
            if (questionTimer != null)
            {
                questionTimer.OnElapsed -= HandleTimeoutElapsed;
                questionTimer.OnStarted -= StartTickLoop;
                questionTimer.OnStopped -= StopTickLoop;
            }

            if (turnManager != null)
            {
                turnManager.OnAnswerCorrect -= PlayCorrectSfx;
                turnManager.OnTurnStarted -= _ => EnsureBgm();
                turnManager.OnHideChooseStar -= PLayChooseStarSfx;
                turnManager.OnEndGame -= _ => PlayEndSfx();
            }
        }
        
        private void HandleTimeoutElapsed()
        {
            StopTickLoop(); // stop ticking sound
            PlayTimeoutBell();
        }
        
        private void PlayTimeoutBell()
        {
            if (library?.timeoutClip == null) return;
            _sfxSrc.PlayOneShot(library.timeoutClip, library.sfxVolume);
        }

        private void ApplyVolumes()
        {
            if (library == null) return;
            _bgmSrc.volume  = library.bgmVolume;
            _sfxSrc.volume  = library.sfxVolume;
            _tickSrc.volume = library.tickVolume;
            _sfxSrc2.volume = library.sfxVolume;
        }

        // ==== BGM ====

        private void EnsureBgm()
        {
            if (_bgmSrc.isPlaying) return;
            if (library == null || library.bgmClips.Count == 0) return;

            _bgmSrc.clip = library.bgmClips[0];
            _bgmSrc.Play();
        }

        public void PlayBgmIndex(int index)
        {
            if (library == null || index < 0 || index >= library.bgmClips.Count) return;
            _bgmSrc.clip = library.bgmClips[index];
            _bgmSrc.loop = true;
            _bgmSrc.Play();
        }

        public void StopBgm(bool fade = true)
        {
            if (!fade) { _bgmSrc.Stop(); return; }
            StartCoroutine(FadeOut(_bgmSrc, 0.4f));
        }

        // ==== Tick ====

        private void StartTickLoop()
        {
            if (library?.tickClip == null) return;
            if (_tickSrc.isPlaying && _tickSrc.clip == library.tickClip) return;
            
            _tickSrc.clip = library.tickClip;
            _tickSrc.loop = true;
            _tickSrc.Play();
        }

        private void StopTickLoop()
        {
            if (_tickSrc.isPlaying) _tickSrc.Stop();
        }

        // ==== Correct ====

        private void PlayCorrectSfx()
        {
            if (library?.correctClip == null) return;
            _sfxSrc.PlayOneShot(library.correctClip, library.sfxVolume);
        }
        
        private void PLayChooseStarSfx()
        {
            if (library?.chooseStarClip == null) return;
            _sfxSrc.PlayOneShot(library.chooseStarClip, library.sfxVolume);
        }
        
        private void PlayEndSfx()
        {
            if (library?.Endclip == null) return;
            _sfxSrc2.PlayOneShot(library.confettiClip, library.sfxVolume);
            _tickSrc.PlayOneShot(library.correctClip, library.sfxVolume);
            _sfxSrc.PlayOneShot(library.Endclip, library.sfxVolume);
            StopBgm();
        }

        // ==== Utils ====
        private IEnumerator FadeOut(AudioSource src, float duration)
        {
            float startVol = src.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                src.volume = Mathf.Lerp(startVol, 0f, t / duration);
                yield return null;
            }
            src.Stop();
            src.volume = startVol; // restore for next time
        }
    }
}

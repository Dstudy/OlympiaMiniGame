using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using QuizGame.Systems;
using TMPro;
using UnityEngine;


public class EndGame : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private ParticleSystem winPuzzleAnim;
    [SerializeField] private ParticleSystem winConfetti;
    [SerializeField] private GameObject[] scoreTexts;
    [SerializeField] private TurnManager turnManager;
    public void ShowEnd()
    {
        if(!panel.activeInHierarchy)
            panel.SetActive(true);
        if(!winPuzzleAnim.gameObject.activeInHierarchy)
            winPuzzleAnim.gameObject.SetActive(true);
        winPuzzleAnim.Play();
        for(int i = 0; i < turnManager.Students.Count; i++)
        {
            scoreTexts[i].SetActive(false);
        }
        StartCoroutine(WinAnim());
    }
    
    IEnumerator WinAnim()
    {
        if(!winConfetti.gameObject.activeInHierarchy)
            winConfetti.gameObject.SetActive(true);
        winConfetti.Play();
        
        yield return new WaitForSeconds(1f);
        
        for(int i = 0; i < turnManager.Students.Count; i++)
        {
            scoreTexts[i].SetActive(true);
            scoreTexts[i].GetComponent<TextMeshProUGUI>().DOFade(1f, 1f);
            scoreTexts[i].transform.DOMoveY(scoreTexts[i].transform.position.y + 3.2f, 1f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(1f);
        }
    }
}

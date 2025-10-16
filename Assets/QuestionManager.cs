using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager Instance;
    public List<GameObject> questions;
    public List<GameObject> HeartsPanal;
    public Image[] heartsWinPanal;
    public TextMeshProUGUI score;
    public GameObject WinPanal;
    public GameObject LosePanal;
    private int currentQuestionIndex = 0;
    private int hearts = 3;
    private int currentScore = 0;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        questions[currentQuestionIndex].GetComponent<CanvasGroup>().DOFade(1, 2f);
        questions[currentQuestionIndex].GetComponent<CanvasGroup>().interactable = true;
        score.text = "0 / 5";
    }

    public void NextQuestion()
    {
        CanvasGroup previousQuestion = questions[currentQuestionIndex].GetComponent<CanvasGroup>();
        currentQuestionIndex++;
        if(currentQuestionIndex > questions.Count-1)
        {
            if (currentQuestionIndex > questions.Count - 1 && hearts > 0)
            {
                WinPanal.SetActive(true);
                var currentLevelIndex = PlayerPrefs.GetInt("HighestUnlockedLevel", 0);
                LevelManager.Instance?.IncreaseLevelOpen(currentLevelIndex + 1);

                for (int i = 0; i < heartsWinPanal.Length; i++)
                {
                    heartsWinPanal[i].gameObject.SetActive(i < hearts);
                }
                return;
            }
            else if (hearts <= 0)
            {
                LosePanal.SetActive(true);
                return;
            }
        }
        CanvasGroup currentQuestion = questions[currentQuestionIndex].GetComponent<CanvasGroup>();

        previousQuestion.DOFade(0, 0.5f).OnComplete(() =>
        {
            previousQuestion.gameObject.SetActive(false);
            currentQuestion.gameObject.SetActive(true);
            currentQuestion.DOFade(1, 0.5f).OnComplete(() =>
            {
                currentQuestion.interactable = true;
                currentQuestion.blocksRaycasts = true;
            });
        });
    }

    public void CorrectAnswer()
    {
        currentScore++;
        score.text = currentScore + "/" + questions.Count;
        StartCoroutine(WaitBeforNextQuestion());
    }

    public void WrongAnswer()
    {
        HeartsPanal[hearts - 1].SetActive(false);
        hearts--;
        if(hearts == 0)
        {
            LosePanal.SetActive(true);
            return;
        }
        StartCoroutine(WaitBeforNextQuestion());
    }

    private IEnumerator WaitBeforNextQuestion()
    {
        yield return new WaitForSeconds(1f);
        NextQuestion();
    }
}

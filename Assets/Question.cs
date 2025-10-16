using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Question : MonoBehaviour
{
    public List<Chooice> chooices;
    public Sprite correctImage;
    public Sprite wrongImage;

    private void Start()
    {
        foreach (Chooice c in chooices)
        {
            Chooice capturedChoice = c;
            capturedChoice.GetComponent<Button>().onClick.AddListener(() => CheckAnswer(capturedChoice));
        }
    }

    public void CheckAnswer(Chooice answer)
    {
        foreach (Chooice c in chooices)
        {
            c.GetComponent<Button>().interactable = false;
        }
        Chooice s = chooices.Find(c => c != answer);
        answer.answerImage.gameObject.SetActive(true);
        s.answerImage.gameObject.SetActive(true);
        if (answer.isCorrect)
        {
            answer.answerImage.sprite = correctImage;
            s.answerImage.sprite = wrongImage;
            QuestionManager.Instance.CorrectAnswer();
        }
        else
        {
            answer.answerImage.sprite = wrongImage;
            s.answerImage.sprite = correctImage;
            QuestionManager.Instance.WrongAnswer();
        }
    }
}

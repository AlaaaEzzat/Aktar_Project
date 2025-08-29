using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableItem : MonoBehaviour , IPointerClickHandler
{
    [Header("Selectable Item Settings")]
    public bool isCorrect = false;
    public Animator animator; 
    public List<fadeAnimationSequance> fadeAnimations;
    public Points pointsType;
    public GameObject MarkItems;
    public TweenEffect tweenEffect;
    public bool canBeSkipped = true;

    [Header("PointPosition")]
    public RectTransform pointPosition;
    public int Duration = 1;

    [Header("Event On Sucsses")]
    public UnityEvent OnSuccessEvent;


    private int CurrentFadeIndex = 0;
    private bool isClicked = false;
    private string currentAnimationToPlay = "Correct";



    public void OnPointerClick(PointerEventData eventData)
    {
        FindAnyObjectByType<GameManager>().OnItemSelected(this);
    }

    public void PlayCorrectAnimation()
    {
        if(tweenEffect != null)
        {
            Vector3 originaPosition = transform.position;
            if (tweenEffect.controlImage == true && tweenEffect.targetTransform != null)
            {
                foreach(var image in tweenEffect.images)
                {
                    image.DOFade(0f, tweenEffect.FadeInDuration);
                }
            }
            if(tweenEffect.controlScale == true && tweenEffect.targetTransform != null)
            {
                transform.DOScale(tweenEffect.targetTransform.localScale, tweenEffect.ScaleDuration);
            }
            if (tweenEffect.controlPosition == true && tweenEffect.targetTransform != null)
            {
                transform.DOMove(tweenEffect.targetTransform.position, tweenEffect.MoveDuration).OnComplete(() =>
                {
                    if (tweenEffect.resetAfter)
                    {
                        transform.localScale = Vector3.one;
                        foreach (var image in tweenEffect.images)
                        {
                            transform.DOMove(originaPosition, 0.1f);
                            image.DOFade(1f, tweenEffect.FadeInDuration);
                        }
                    }
                });
            }
        }
        if(MarkItems != null)
        {
            MarkItems.SetActive(false);
        }
        if (animator != null)
        {
            animator.enabled = true;
            animator.SetTrigger(currentAnimationToPlay);
        }
        else
        {
            if(pointPosition != null)
            {
                transform.DOMove(pointPosition.position, Duration);
            }
            PlayFadeEffect();
        }

        if(OnSuccessEvent != null)
        {
            OnSuccessEvent.Invoke();
        }
    }

    public void DisableInteraction()
    {
        GetComponent<Button>().interactable = false;
    }

    public void PlayFadeEffect()
    {
        if (CurrentFadeIndex < fadeAnimations.Count && fadeAnimations[CurrentFadeIndex] != null)
        {
            foreach(var anim in fadeAnimations[CurrentFadeIndex].Images)
            {
                anim.DOFade(0f, fadeAnimations[CurrentFadeIndex].FadeInDuration)
                .OnComplete(() =>
                {
                    CurrentFadeIndex++;
                });
            }

        }
    }

    public void PlayFadeEffectAnimation(int startposition = 0)
    {
        if (startposition < fadeAnimations.Count && fadeAnimations[startposition] != null)
        {
            foreach (var anim in fadeAnimations[startposition].Images)
            {
                anim.DOFade(0f, fadeAnimations[startposition].FadeInDuration);
                currentAnimationToPlay = "Correct";
            }

        }
    }

    public void SetCurrentAnimation(string S)
    {
        currentAnimationToPlay = S;
    }

    public void SkipEffect()
    {
        if (canBeSkipped)
        {
            foreach (var anim in fadeAnimations)
            {
                foreach (var image in anim.Images)
                {
                    image.DOFade(0f, 0.1f);
                }
            }
            GetComponent<Animator>().enabled = false;
            GetComponent<Image>()?.DOFade(0, 0.1f);
        }
    }

    public void PlayWrongShake()
    {
        transform.DOShakePosition(0.5f, 10f, 20, 90);
    }
}

public enum Points
{
    Spending,
    Saving,
    Keeping
}

[System.Serializable]
public class fadeAnimationSequance
{
    public List<Image> Images;
    public float FadeInDuration = 0.5f;
}

[System.Serializable]
public class TweenEffect
{
    [Header("Image Effect")]
    public bool controlImage = false;
    public List<Image> images;
    public float FadeInDuration = 0.5f;
    public Transform targetTransform;

    [Header("Scale Effect")]
    public bool controlScale = false;
    public float ScaleDuration = 0.5f;

    [Header("Position Effect")]
    public bool controlPosition = false;
    public float MoveDuration = 0.5f;

    [Header("General")]
    public bool resetAfter = false;

}
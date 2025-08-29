using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Cinemachine;
using System;
using TMPro;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	[Header("Level -1 ")]
	public int currentLevelIndex;
	public int LevelPart;
	public List<GameObject> parts;

	[Header("Hearts UI")]
	public List<Image> hearts;

	[Header("Panels")]
	public GameObject winPanel;
	public GameObject losePanel;
	public GameObject itemsPanal;

	[Header("Objects to disable on Win/Lose")]
	public GameObject[] objectsToDisable;

	[Header("Win Stars")]
	public GameObject[] winStars;

	[Header("Star Pop Effect")]
	public float starPopPunch = 0.3f;
	public float starPopDuration = 0.35f;
	public int starPopVibrato = 2;
	public float starPopElasticity = 1.0f;

	[Header("Sounds")]
	public string backgroundSoundKey = "background";
	public string winSoundKey = "win";
	public string loseSoundKey = "lose";

    [Header("Game Setup")]
    public List<SelectableItem> allItems;
    public List<int> totalRightItems;

    [Header("ScorePoints")]
	public int spendingPoints = 0;
    public int savingPoints = 0;
    public int keepingPoints = 0;
	public Image spendingPointsEffectImage;
    public Image savingPointsEffectImage;
    public Image keepingPointsEffectImage;


    [Header("ScorePoints TexrRefrance")]
    public TextMeshProUGUI spendingPointsText;
    public TextMeshProUGUI savingPointsText;
    public TextMeshProUGUI keepingPointsText;

	[SerializeField] protected Image background;

	[SerializeField] protected Sprite part2Background;

    protected int lives = 3;
    protected int correctChoices = 0;
    protected int wrongChoices = 0;
    protected bool gameFinished = false;
    protected int CurrentLevelPart = 0;
    protected SelectableItem currentCorrectItem;
    protected SelectableItem currentSelectedItem;


    protected void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	protected void Start()
	{
		SoundManager.Instance?.LoopSound(backgroundSoundKey, true);
		ResetHearts();
		HidePanels();
		HideStars();
		UpdateScoreTexts();
    }

	public void LoseHeart()
	{
		Debug.Log($"LoseHeart called! Lives BEFORE loss: {lives}");

		if (lives <= 0)
		{
			Debug.LogWarning("Tried to lose heart, but already at zero lives!");
			return;
		}

		lives--;

		Debug.Log($"Lives AFTER loss: {lives}");


		if (lives >= 0 && lives < hearts.Count)
			hearts[lives].DOFade(0.2f, 0.5f).SetEase(Ease.OutBounce);

		if (lives == 0)
		{
			StartCoroutine(LoseSequence());
		}
	}


    public virtual IEnumerator LoseSequence()
	{
		DisableObjects();
		SoundManager.Instance?.PlaySound(loseSoundKey);
		if (losePanel) losePanel.SetActive(true);
		yield break;
	}

	public virtual IEnumerator WinSequence()
	{
        gameFinished = true;
		if(CurrentLevelPart < parts.Count - 1)
		{
            yield return new WaitForSeconds(4);
            parts[CurrentLevelPart].SetActive(false);
            CurrentLevelPart++;
			parts[CurrentLevelPart].SetActive(true);
			correctChoices = 0;
			if(part2Background != null)
				background.sprite = part2Background;
			foreach(var tm in allItems)
			{
				tm.GetComponent<Image>().raycastTarget = true;
			}
            gameFinished = false;
            yield break;
        }
        yield return new WaitForSeconds(2);

        LevelManager.Instance?.IncreaseLevelOpen(currentLevelIndex);
		DisableObjects();

		HideStars();

		yield return new WaitForSeconds(0f);
		SoundManager.Instance?.PlaySound(winSoundKey);
		if (winPanel) winPanel.SetActive(true);

        int finalCorrectAnswers = correctChoices - wrongChoices;

        int starsToShow = finalCorrectAnswers >= totalRightItems[CurrentLevelPart]
            ? 3
            : finalCorrectAnswers >= totalRightItems[CurrentLevelPart] / 2
                ? 2
                : 1;

        for (int i = 0; i < starsToShow; i++)
		{
			if (winStars[i] != null)
			{
				winStars[i].SetActive(true);
				yield return StartCoroutine(PlayStarPop(winStars[i]));
			}
		}

	}


    protected IEnumerator PlayStarPop(GameObject starObj)
	{
		Vector3 originalScale = starObj.transform.localScale;
		starObj.transform.localScale = originalScale;

		Tween t = starObj.transform.DOPunchScale(Vector3.one * starPopPunch, starPopDuration, starPopVibrato, starPopElasticity);
		bool finished = false;
		t.OnComplete(() => finished = true);
		yield return new WaitUntil(() => finished);

		starObj.transform.localScale = originalScale;
		yield break;
	}


	protected void DisableObjects()
	{
		foreach (var go in objectsToDisable)
		{
			if (go) go.SetActive(false);
		}
	}

	protected void ResetHearts()
	{
		lives = hearts.Count;
		Debug.Log("Lives set to: " + lives);
		foreach (var img in hearts)
		{
			img.DOFade(1f, 0.2f);
		}
	}

    protected void HidePanels()
	{
		if (winPanel) winPanel.SetActive(false);
		if (losePanel) losePanel.SetActive(false);
	}

    protected void HideStars()
	{
		foreach (var s in winStars)
			if (s) s.SetActive(false);
	}

	public void RestartGame()
	{
		ResetHearts();
		HidePanels();
		HideStars();
		foreach (var go in objectsToDisable)
			if (go) go.SetActive(true);
	}

    public virtual void OnItemSelected(SelectableItem item)
    {
		if(gameFinished) return;
        if (item.isCorrect && (item == currentCorrectItem || currentCorrectItem == null))
        {
			if(currentSelectedItem != null && currentSelectedItem.GetComponent<Animator>() != null)
			{
                currentSelectedItem.SkipEffect();
            }
            correctChoices++;
            item.PlayCorrectAnimation();
			ApplyImageEffect(item);
            if (item.GetComponent<CanvasGroup>() != null)
				item.GetComponent<CanvasGroup>().ignoreParentGroups = true;
			currentCorrectItem = null;
            currentSelectedItem = item;
            if (itemsPanal != null && CurrentLevelPart < 1)
				HideItemsPanal();

            savingPoints += item.pointsType == Points.Saving ? 10 : 0;

            if (correctChoices >= totalRightItems[CurrentLevelPart])
                StartCoroutine(WinSequence());

        }
        else
        {
			if (hearts[wrongChoices] != null)
				hearts[wrongChoices].enabled = false;

            item.DisableInteraction();
            wrongChoices++;
			item.PlayWrongShake();
            if (item.pointsType == Points.Spending)
                spendingPoints += 10;
            else if (item.pointsType == Points.Keeping)
                keepingPoints += 10;

            if (wrongChoices >= hearts.Count)
                Lose();
        }
        item.GetComponent<Image>().raycastTarget = false;
        UpdateScoreTexts();
    }

    protected void Lose()
    {
        losePanel.SetActive(true);
    }

	public void UpdateScoreTexts()
    {
		if(spendingPointsText == null || savingPointsText == null || keepingPointsText == null)
			return;
        spendingPointsText.text = spendingPoints.ToString();
        savingPointsText.text = savingPoints.ToString();
        keepingPointsText.text = keepingPoints.ToString();
    }

	public void ShowItemsPanal()
	{
        itemsPanal.GetComponent<CanvasGroup>().DOFade(1, 1f).SetEase(Ease.OutQuad);
        itemsPanal.GetComponent<CanvasGroup>().blocksRaycasts = true;
        foreach (var item in allItems)
		{
			item.GetComponent<Image>().raycastTarget = true;
        }
    }

    public void HideItemsPanal()
    {
		itemsPanal.GetComponent<CanvasGroup>().DOFade(0, 1f).SetEase(Ease.OutQuad);
        itemsPanal.GetComponent<CanvasGroup>().blocksRaycasts = false;
        foreach (var item in allItems)
        {
            item.GetComponent<Image>().raycastTarget = false;
			item.GetComponent<Button>().interactable = true;
        }
    }

	public void SetCurrentCorrectItem(SelectableItem item)
	{
        currentCorrectItem = item;
    }

	public void ApplyImageEffect(SelectableItem itm)
	{
		if(itm.pointsType == Points.Spending && spendingPointsEffectImage != null)
            spendingPointsEffectImage.DOFade(1f, 1f).SetEase(Ease.InOutQuad)
                .OnComplete(() => spendingPointsEffectImage.DOFade(0f, 1f).SetEase(Ease.InOutQuad));
        else if(itm.pointsType == Points.Saving && savingPointsEffectImage != null)
            savingPointsEffectImage.DOFade(1f, 1f).SetEase(Ease.InOutQuad)
                .OnComplete(() => savingPointsEffectImage.DOFade(0f, 1f).SetEase(Ease.InOutQuad));
        else if(itm.pointsType == Points.Keeping && keepingPointsEffectImage != null)
            keepingPointsEffectImage.DOFade(1f, 1f).SetEase(Ease.InOutQuad)
				.OnComplete(() => keepingPointsEffectImage.DOFade(0f, 1f).SetEase(Ease.InOutQuad));
    }
}

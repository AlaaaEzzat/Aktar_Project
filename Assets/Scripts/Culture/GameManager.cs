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
	public int starsToShow;

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
		HideStars();
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
		SoundManager.Instance?.PlayLevelKey("Lose");
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

        LevelManager.Instance?.IncreaseLevelOpen(currentLevelIndex);

		HideStars();

		yield return new WaitForSeconds(0f);
		SoundManager.Instance?.PlayLevelKey("Win");
		if (winPanel) winPanel.SetActive(true);

        for (int i = 0; i < starsToShow; i++)
		{
			if (winStars[i] != null)
			{
				winStars[i].SetActive(true);
				yield return StartCoroutine(PlayStarPop(winStars[i]));
			}
		}
		SoundManager.Instance.ChangeKey();
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

    protected void HideStars()
	{
		foreach (var s in winStars)
			if (s) s.SetActive(false);
	}
}

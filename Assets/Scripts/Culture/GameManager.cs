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
	public HealthSystem healthSystem;


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

    public virtual IEnumerator LoseSequence()
	{
		SoundManager.Instance?.PlaySoundWithKey("Lose");
		if (losePanel) losePanel.SetActive(true);
		yield break;
	}

	public virtual IEnumerator WinSequence()
	{
		healthSystem.canTakeDmg = false;
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

		SoundManager.Instance?.PlaySoundWithKey("Win");
		if (winPanel) winPanel.SetActive(true);

        for (int i = 0; i < healthSystem.currentLives; i++)
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

    protected void HideStars()
	{
		foreach (var s in winStars)
			if (s) s.SetActive(false);
	}

}

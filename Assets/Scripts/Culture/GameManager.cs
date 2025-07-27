using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Cinemachine;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	[Header("Level -1 ")]
	public int currentLevelIndex;

	[Header("Hearts UI")]
	public Image[] hearts;
	private int lives = 3;

	[Header("Player References")]
	public GameObject player;
	public Transform resetPoint;

	[Header("Panels")]
	public GameObject winPanel;
	public GameObject losePanel;

	[Header("Objects to disable on Win/Lose")]
	public GameObject[] objectsToDisable;

	[Header("Win Stars")]
	public GameObject[] winStars;

	[Header("Star Pop Effect")]
	public float starPopPunch = 0.3f;
	public float starPopDuration = 0.35f;
	public int starPopVibrato = 2;
	public float starPopElasticity = 1.0f;

	[Header("Cinemachine Cameras")]
	public CinemachineVirtualCamera mainCam;
	public CinemachineVirtualCamera focusCam;
	public CinemachineVirtualCamera introCam;

	[Header("Cinematic Key Reveal")]
	public List<GameObject> keyObjects;
	public float keyMoveDistance = 2f;
	public float keyMoveDuration = 1f;
	public float focusDuration = 1f;

	private bool keysCollected = false;
	private List<EnemySensor> enemies = new List<EnemySensor>();

	[Header("Intro Sequence")]
	public float introCamDuration = 2.5f;
	public float betweenActivations = 0.6f;

	[Header("Particles")]
	public ParticleSystem deathParticlePrefab;
	public ParticleSystem respawnParticlePrefab;
	public float deathParticleDuration = 1f;
	public float respawnParticleDuration = 1f;
	public string respawnParticleTag = "RespawnParticle";

	private Rigidbody2D playerRb;
	private Collider2D playerCollider;

	[Header("Question Panel")]
	public GameObject questionPanel; // Assign your panel in the Inspector
	public string questionSoundKey = "question";
	// --- Heart loss cooldown ---
	private bool canLoseHeart = true;
	public float loseHeartCooldown = 0.5f;
	// ---------------------------


	[Header("UI Control")]
	private List<Button> allButtons = new List<Button>();
	private Coroutine questionBlockRoutine;


	[Header("Sounds")]
	public string backgroundSoundKey = "background";
	public string winSoundKey = "win";
	public string loseSoundKey = "lose";

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	void Start()
	{
		SoundManager.Instance?.LoopSound(backgroundSoundKey, true);
		if (player)
		{
			playerRb = player.GetComponent<Rigidbody2D>();
			playerCollider = player.GetComponent<Collider2D>();
		}
		ResetHearts();
		HidePanels();
		HideStars();
		if (questionPanel) questionPanel.SetActive(false);
		StartCoroutine(IntroSequenceCoroutine());
	}

	IEnumerator IntroSequenceCoroutine()
	{
		enemies = new List<EnemySensor>(FindObjectsByType<EnemySensor>(FindObjectsSortMode.None));
		foreach (var enemy in enemies)
		{
			if (enemy != null)
			{
				enemy.LockMovement();
				enemy.gameObject.SetActive(false);
			}
		}

		var playerController = player ? player.GetComponent<PlayerController2D>() : null;
		if (playerController) playerController.LockMovement();
		if (player) player.SetActive(false);

		if (introCam && mainCam)
		{
			introCam.Priority = 30;
			mainCam.Priority = 0;
			if (focusCam) focusCam.Priority = 0;
		}
		yield return new WaitForSeconds(introCamDuration);

		if (player)
		{
			player.SetActive(true);
			PlayRespawnParticle(player.transform.position);
			yield return new WaitForSeconds(respawnParticleDuration);
		}

		foreach (var enemy in enemies)
		{
			if (enemy != null)
			{
				enemy.gameObject.SetActive(true);
				PlayRespawnParticle(enemy.transform.position);
				yield return new WaitForSeconds(respawnParticleDuration + betweenActivations);
			}
		}

		if (introCam && mainCam)
		{
			mainCam.Priority = 20;
			introCam.Priority = 0;
		}
		if (focusCam) focusCam.Priority = 0;

		yield return new WaitForSeconds(4f);

		if (playerController)
			playerController.UnlockMovement();
		foreach (var enemy in enemies)
			if (enemy != null) enemy.UnlockMovement();
		yield break;
	}

	void PlayRespawnParticle(Vector3 pos)
	{
		GameObject scenePSObj = GameObject.FindGameObjectWithTag(respawnParticleTag);
		if (scenePSObj != null)
		{
			ParticleSystem ps = scenePSObj.GetComponent<ParticleSystem>();
			scenePSObj.transform.position = pos;
			ps.Play();
		}
		else if (respawnParticlePrefab != null)
		{
			ParticleSystem ps = Instantiate(respawnParticlePrefab, pos, Quaternion.identity);
			ps.Play();
			Destroy(ps.gameObject, respawnParticleDuration);
		}
	}

	// ==== Question Panel Logic (no text) ====
	public void ShowQuestionPanel()
	{
		SoundManager.Instance?.StopSound(backgroundSoundKey);
		if (questionPanel)
		{
			questionPanel.SetActive(true);
		}

		if (questionBlockRoutine != null)
			StopCoroutine(questionBlockRoutine);

		questionBlockRoutine = StartCoroutine(DisableButtonsDuringQuestion());
	}
	IEnumerator DisableButtonsDuringQuestion()
	{
		// Find and disable all active buttons
		allButtons.Clear();
		allButtons.AddRange(FindObjectsOfType<Button>(true)); // include inactive too

		foreach (var btn in allButtons)
			btn.interactable = false;

		// Play question sound
		SoundManager.Instance?.PlaySound(questionSoundKey);

		// Wait for sound duration
		float waitTime = SoundManager.Instance != null ?
			SoundManager.Instance.GetSoundDuration(questionSoundKey) :
			2f;

		yield return new WaitForSeconds(waitTime);

		// Re-enable buttons
		foreach (var btn in allButtons)
			btn.interactable = true;

		questionBlockRoutine = null;
	}



	// Call from UnityEvent on your buttons
	public void OnCorrectAnswer()
	{
		if (questionPanel) questionPanel.SetActive(false);
		StartCoroutine(WinSequence());
	}

	public void OnWrongAnswer()
	{
		if (!canLoseHeart) return; // Cooldown active? Ignore.
		if (questionPanel) questionPanel.SetActive(false);
		StartCoroutine(LoseHeartForQuestionCooldown());
	}

	private IEnumerator LoseHeartForQuestionCooldown()
	{
		canLoseHeart = false;
		LoseHeartForQuestion();
		yield return new WaitForSeconds(loseHeartCooldown);
		canLoseHeart = true;
	}
	// =========================================

	// This is only used for the question answer scenario
	private void LoseHeartForQuestion()
	{
		Debug.Log($"[LoseHeartForQuestion] CALLED. Lives BEFORE loss: {lives}");
		if (lives <= 0)
		{
			Debug.LogWarning("Tried to lose heart, but already at zero lives!");
			return;
		}

		lives = 0 ;

		Debug.Log($"[LoseHeartForQuestion] AFTER loss: {lives}");

		// Fade hearts only when lives drops to 3, 2, or 1
		if (lives <= hearts.Length && lives > 0)
			hearts[lives - 1].DOFade(0.2f, 0.5f).SetEase(Ease.OutBounce);

		if (lives == 0)
		{
			Debug.Log("[LoseHeartForQuestion] Triggering LoseSequence!");
			StartCoroutine(LoseSequence());
		}
		else
		{
			Debug.Log("[LoseHeartForQuestion] Triggering WinSequence!");
			StartCoroutine(WinSequence());
		}
	}

	// Use this for other game mechanics (damage, traps, etc.)
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

		// Fade the lost heart (fade the heart at index == lives)
		if (lives >= 0 && lives < hearts.Length)
			hearts[lives].DOFade(0.2f, 0.5f).SetEase(Ease.OutBounce);

		if (lives == 0)
		{
			StartCoroutine(LoseSequence());
		}
		else
		{
			StartCoroutine(LoseSequenceWithParticles());
		}
	}


	/// <summary>
	/// Call this when player triggers win condition.
	/// </summary>
	public void WinGame()
	{
		ShowQuestionPanel(); // Only show the panel and wait for answer!
							 // Do not start WinSequence() here.
	}

	void ResetPlayer()
	{
		PlayerController2D controller = player.GetComponent<PlayerController2D>();
		controller.ResetToStartPosition();
		controller.gameObject.SetActive(true);
	}

	IEnumerator LoseSequenceWithParticles()
	{
		if (deathParticlePrefab)
		{
			ParticleSystem deathPS = Instantiate(deathParticlePrefab, player.transform.position, Quaternion.identity);
			deathPS.Play();
			Destroy(deathPS.gameObject, deathParticleDuration);
		}

		DisablePlayerMovement();

		enemies = new List<EnemySensor>(FindObjectsByType<EnemySensor>(FindObjectsSortMode.None));
		foreach (var enemy in enemies)
			if (enemy != null) enemy.LockMovement();

		// Switch to focus cam
		if (mainCam && focusCam)
		{
			mainCam.Priority = 0;
			focusCam.Priority = 20;
		}

		yield return new WaitForSeconds(focusDuration);

		// Reset player position
		ResetPlayer();

		// Play respawn particle
		if (respawnParticlePrefab)
		{
			ParticleSystem respawnPS = Instantiate(respawnParticlePrefab, resetPoint.position, Quaternion.identity);
			respawnPS.Play();
			Destroy(respawnPS.gameObject, respawnParticleDuration);
		}

		yield return new WaitForSeconds(0.3f); // small buffer

		// Switch back to main cam
		if (mainCam && focusCam)
		{
			mainCam.Priority = 20;
			focusCam.Priority = 0;

			// ✅ Wait until mainCam is active before continuing
			yield return new WaitUntil(() => mainCam.Priority > focusCam.Priority);
		}

		yield return new WaitForSeconds(4f); // extra buffer (if needed for animation)

		// ✅ FINAL STEP: Re-enable player and enemies
		if (playerRb) playerRb.simulated = true;
		if (playerCollider) playerCollider.enabled = true;

		foreach (var enemy in enemies)
			if (enemy != null) enemy.UnlockMovement();

		if (player != null)
		{
			var controller = player.GetComponent<PlayerController2D>();
			if (controller != null)
				controller.UnlockMovement(); // ✅ Finally unlock player input + animation
		}

		yield break;
	}


	IEnumerator LoseSequence()
	{
		DisablePlayerMovement();
		DisableObjects();
		if (player != null)
		{
			var controller = player.GetComponent<PlayerController2D>();
			if (controller != null)
				controller.SetToIdle();
		}
		SoundManager.Instance?.PlaySound(loseSoundKey);
		if (losePanel) losePanel.SetActive(true);
		yield break;
	}

	IEnumerator WinSequence()
	{
		LevelManager.Instance?.IncreaseLevelOpen(currentLevelIndex);
		DisablePlayerMovement();
		DisableObjects();

		if (player != null)
		{
			var controller = player.GetComponent<PlayerController2D>();
			if (controller != null)
				controller.SetToIdle();
		}

		HideStars();

		yield return new WaitForSeconds(0f); // Optional: Delay before showing win panel
		SoundManager.Instance?.PlaySound(winSoundKey);
		if (winPanel) winPanel.SetActive(true);

		// Clamp stars to available range
		int starsToShow = lives ;

		if (lives > winStars.Length)
		{
			Debug.LogWarning($"⚠️ Lives ({lives}) exceed winStars.Length ({winStars.Length}) — visual mismatch possible.");
		}

		for (int i = 0; i < starsToShow; i++)
		{
			if (winStars[i] != null)
			{
				winStars[i].SetActive(true);
				yield return StartCoroutine(PlayStarPop(winStars[i]));
			}
		}

		yield break;
	}


	IEnumerator PlayStarPop(GameObject starObj)
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

	void DisablePlayerMovement()
	{
		if (playerRb) playerRb.simulated = false;
		if (playerCollider) playerCollider.gameObject.SetActive(false);
	}

	void DisableObjects()
	{
		foreach (var go in objectsToDisable)
		{
			if (go) go.SetActive(false);
		}
	}

	void ResetHearts()
	{
		lives = hearts.Length; // Add extra hidden buffer life
		Debug.Log("Lives set to: " + lives);
		foreach (var img in hearts)
		{
			img.DOFade(1f, 0.2f);
		}
	}

	void HidePanels()
	{
		if (winPanel) winPanel.SetActive(false);
		if (losePanel) losePanel.SetActive(false);
	}

	void HideStars()
	{
		foreach (var s in winStars)
			if (s) s.SetActive(false);
	}

	public void RestartGame()
	{
		ResetHearts();
		if (playerRb) playerRb.simulated = true;
		if (playerCollider) playerCollider.enabled = true;
		HidePanels();
		HideStars();
		foreach (var go in objectsToDisable)
			if (go) go.SetActive(true);
		player.transform.position = resetPoint.position;
		playerRb.linearVelocity = Vector2.zero;
		canLoseHeart = true;
	}

	public void OnAllKeysCollected()
	{
		if (keysCollected) return;
		keysCollected = true;
		StartCoroutine(AllKeysCollectedSequence());
	}

	IEnumerator AllKeysCollectedSequence()
	{
		enemies = new List<EnemySensor>(FindObjectsByType<EnemySensor>(FindObjectsSortMode.None));

		var playerController = player.GetComponent<PlayerController2D>();
		if (playerController) playerController.LockMovement();
		foreach (var enemy in enemies)
			if (enemy != null) enemy.LockMovement();

		if (mainCam && focusCam)
		{
			mainCam.Priority = 0;
			introCam.Priority = 20;
		}

		yield return new WaitForSeconds(focusDuration);

		if (KeyManager.Instance != null)
			yield return StartCoroutine(KeyManager.Instance.MoveAllKeysToTarget());

		yield return new WaitForSeconds(0.5f);

		if (mainCam && focusCam)
		{
			mainCam.Priority = 20;
			introCam.Priority = 0;
		}

		if (playerController) playerController.UnlockMovement();
		foreach (var enemy in enemies)
			if (enemy != null) enemy.UnlockMovement();
		yield break;
	}
}

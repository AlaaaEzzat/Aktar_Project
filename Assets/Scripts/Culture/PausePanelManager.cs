using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
	public static PauseManager Instance;

	[Header("لوحة التوقف المؤقت")]
	public GameObject pausePanel;
	[Tooltip("هل اللعبة متوقفة الآن؟")]
	public bool isPaused = false;
	public List<GameObject> DisabledObjects = new List<GameObject>();
	private Coroutine autoCloseCoroutine;
	private bool panelWasClosedByClick = false;

	[Header("Cinemachine Cameras")]
	public Cinemachine.CinemachineVirtualCamera gameplayCam;
	public Cinemachine.CinemachineVirtualCamera pauseCam;

	[Header("الصوت")]
	public string pauseSoundKey = "Egypt"; // Set the sound key in the inspector

	void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);

		if (pausePanel != null)
			pausePanel.SetActive(true); // يمكنك جعلها false عند البدء حسب تصميم لعبتك

		if (gameplayCam && pauseCam)
		{
			gameplayCam.Priority = 0;
			pauseCam.Priority = 20;
		}
	}

	void Update()
	{
		if (isPaused)
		{
			if (Time.timeScale != 0f)
				Time.timeScale = 0f;

			// Check for click/tap outside the panel to resume game
			if (Input.GetMouseButtonDown(0) && pausePanel.activeSelf)
			{
				if (!IsPointerOverUIObject(pausePanel))
				{
					panelWasClosedByClick = true;
					ImmediateResume();
				}
			}
		}
		else
		{
			if (Time.timeScale != 1f)
				Time.timeScale = 1f;
		}
	}

	public void PauseGame()
	{
		foreach (GameObject obj in DisabledObjects)
		{
			if (obj != null)
				obj.SetActive(false);
		}
		isPaused = true;
		if (pausePanel != null)
			pausePanel.SetActive(true);

		// Camera switch
		if (gameplayCam && pauseCam)
		{
			gameplayCam.Priority = 0;
			pauseCam.Priority = 20;
		}

		// أوقف أي صوت قديم
		SoundManager.Instance?.StopSound(pauseSoundKey);

		// Start auto-close logic
		if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
		panelWasClosedByClick = false;
		autoCloseCoroutine = StartCoroutine(PausePanelSequence());
	}

	// تسلسل ظهور البانل، ثم تشغيل الصوت، ثم الإغلاق بعد نهاية الصوت + ثانية واحدة
	IEnumerator PausePanelSequence()
	{
		// 1- انتظر 2 ثانية قبل تشغيل الصوت
		float delayBeforeSound = 1f;
		float timer = 0f;
		while (timer < delayBeforeSound)
		{
			if (panelWasClosedByClick) yield break;
			timer += Time.unscaledDeltaTime;
			yield return null;
		}

		// 2- شغل الصوت
		SoundManager.Instance?.PlaySound(pauseSoundKey);

		// 3- انتظر حتى ينتهي الصوت فعلياً
		while (SoundManager.Instance != null && SoundManager.Instance.IsPlaying(pauseSoundKey))
		{
			if (panelWasClosedByClick) yield break;
			yield return null;
		}

		// 4- انتظر 1 ثانية إضافية
		float extraWait = 1f;
		timer = 0f;
		while (timer < extraWait)
		{
			if (panelWasClosedByClick) yield break;
			timer += Time.unscaledDeltaTime;
			yield return null;
		}

		// 5- أغلق البانل وأرجع اللعبة
		ImmediateResume();
	}

	// إغلاق فوري عند الضغط خارج البانل أو إذا احتاج النظام ذلك
	public void ImmediateResume()
	{
		foreach (GameObject obj in DisabledObjects)
		{
			if (obj != null)
				obj.SetActive(true);
		}
		isPaused = false;
		if (pausePanel != null)
			pausePanel.SetActive(false);

		// Camera switch back
		if (gameplayCam && pauseCam)
		{
			gameplayCam.Priority = 20;
			pauseCam.Priority = 0;
		}

		SoundManager.Instance?.StopSound(pauseSoundKey);

		if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
		autoCloseCoroutine = null;
	}

	// Helper to check if pointer is over a specific UI GameObject (and its children)
	bool IsPointerOverUIObject(GameObject uiObject)
	{
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = Input.mousePosition;
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

		foreach (RaycastResult result in results)
		{
			if (result.gameObject == uiObject || result.gameObject.transform.IsChildOf(uiObject.transform))
				return true;
		}
		return false;
	}
}

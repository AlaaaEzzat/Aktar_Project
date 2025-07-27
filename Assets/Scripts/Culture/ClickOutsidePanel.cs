using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ClickOutsidePanel : MonoBehaviour
{
	[Header("مدة ظهور اللوحة عند بدء المشهد")]
	public float showDuration = 200f;

	[Header("لوحة التوقف المؤقت (أربط نفس اللوحة هنا وداخل PauseManager)")]
	public GameObject panel;

	private GraphicRaycaster raycaster;
	private PointerEventData pointerData;
	private EventSystem eventSystem;

	private bool isTemporaryPanel = false; // نعرف إذا كان الظهور التلقائي في البداية

	void Start()
	{
		// دعم PauseManager بشكل تلقائي
		if (PauseManager.Instance != null && PauseManager.Instance.pausePanel == null)
			PauseManager.Instance.pausePanel = panel;

		if (panel == null)
			panel = gameObject;

		raycaster = GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>();
		eventSystem = EventSystem.current;

		// إظهار اللوحة تلقائيًا عند بداية المشهد
		StartCoroutine(ShowPanelTemporarily());
	}

	IEnumerator ShowPanelTemporarily()
	{
		isTemporaryPanel = true;
		panel.SetActive(true);

		// أوقف اللعبة
		if (PauseManager.Instance != null)
			PauseManager.Instance.PauseGame();

		yield return new WaitForSecondsRealtime(showDuration);

		if (isTemporaryPanel)
			HidePanel();
	}

	void Update()
	{
		if (panel.activeSelf && PauseManager.Instance != null && PauseManager.Instance.isPaused)
		{
			// للماوس
			if (Input.GetMouseButtonDown(0))
			{
				if (!IsClickInsideUI(Input.mousePosition))
					HidePanel();
			}

			// للموبايل
#if UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (!IsClickInsideUI(Input.GetTouch(0).position))
                    HidePanel();
            }
#endif
		}
	}

	// نداء من زر أو سكربت خارجي لفتح اللوحة (وتوقف اللعبة)
	public void ShowPanel()
	{
		panel.SetActive(true);
		if (PauseManager.Instance != null)
			PauseManager.Instance.PauseGame();
		isTemporaryPanel = false;
	}

	// إغلاق اللوحة (واستئناف اللعبة)
	public void HidePanel()
	{
		panel.SetActive(false);
		if (PauseManager.Instance != null)
			PauseManager.Instance.ImmediateResume();
		isTemporaryPanel = false;
	}

	// هل النقرة كانت داخل أي UI؟
	bool IsClickInsideUI(Vector2 screenPosition)
	{
		pointerData = new PointerEventData(eventSystem);
		pointerData.position = screenPosition;

		List<RaycastResult> results = new List<RaycastResult>();
		raycaster.Raycast(pointerData, results);

		return results.Count > 0;
	}
}

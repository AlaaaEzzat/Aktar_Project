using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeImage : MonoBehaviour
{
	public Image fadeImage;           // The UI Image used for fading
	public float fadeDuration = 0.5f; // Duration of fade in/out
	public int nextSceneIndex = 1;    // Scene to load after fade sequence

	[Header("Assign your button panel or button here (to activate after fade in)")]
	public GameObject continueButtonObject;

	private bool continueClicked = false;

	void Start()
	{
		if (fadeImage == null)
		{
#if UNITY_EDITOR
			Debug.LogError("FadeImage: No Image assigned! Assign an Image component.");
#endif
			return;
		}
		if (continueButtonObject != null)
			continueButtonObject.SetActive(false);

		fadeImage.gameObject.SetActive(true); // Ensure the fade image is active
		StartCoroutine(FadeSequence());
	}

	IEnumerator FadeSequence()
	{
		// 1. Fade In
		yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

		// 2. Show Continue button/panel and wait for click
		if (continueButtonObject != null)
			continueButtonObject.SetActive(true);

		while (!continueClicked)
			yield return null;

		// 3. Hide button/panel before fade out (optional)
		if (continueButtonObject != null)
			continueButtonObject.SetActive(false);

		// 4. Fade Out
		yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

		// 5. Load next scene
		if (nextSceneIndex >= 0 && nextSceneIndex < SceneManager.sceneCountInBuildSettings)
		{
			SceneManager.LoadScene(nextSceneIndex);
		}
		else
		{
#if UNITY_EDITOR
			Debug.LogError("FadeImage: Invalid nextSceneIndex! Check your Build Settings.");
#endif
		}
	}

	// Call this method from the UI Button's OnClick event!
	public void ContinueFadeOut()
	{
		continueClicked = true;
	}

	IEnumerator Fade(float startAlpha, float targetAlpha, float duration)
	{
		float elapsed = 0f;
		Color color = fadeImage.color;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
			fadeImage.color = color;
			yield return null;
		}

		color.a = targetAlpha;
		fadeImage.color = color;
	}
}

using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

/// <summary>
/// Handles scene loading/reloading and ensures all DOTween tweens are killed before transitions.
/// Attach this MonoBehaviour to any GameObject in your scene to use these methods.
/// </summary>
public class SceneLoader : MonoBehaviour
{
	/// <summary>
	/// Loads a scene by its index in Build Settings and kills all DOTween tweens.
	/// </summary>
	public void LoadSceneByIndex(int sceneIndex)
	{
		if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
		{
			DOTween.KillAll(); // Clean up all DOTween tweens before switching scenes
			SceneManager.LoadScene(sceneIndex);
			Debug.Log($"➡️ Loading Scene Index: {sceneIndex}");
		}
		else
		{
			Debug.LogError("❌ Invalid scene index. Make sure the scene is added to Build Settings.");
		}
	}

	/// <summary>
	/// Reloads the currently active scene and kills all DOTween tweens.
	/// </summary>
	public void ReloadCurrentScene()
	{
		int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
		DOTween.KillAll();
		SceneManager.LoadScene(currentSceneIndex);
		Debug.Log($"🔄 Reloading Scene Index: {currentSceneIndex}");
	}

	/// <summary>
	/// Loads the next scene in Build Settings and kills all DOTween tweens.
	/// </summary>
	public void LoadNextScene()
	{
		int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
		int nextSceneIndex = currentSceneIndex + 1;

		if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
		{
			DOTween.KillAll();
			SceneManager.LoadScene(nextSceneIndex);
			Debug.Log($"➡️ Loading Next Scene Index: {nextSceneIndex}");
		}
		else
		{
			Debug.LogWarning("🔚 No more scenes in Build Settings to load.");
		}
	}

	/// <summary>
	/// Loads a scene asynchronously by its index (optional, smoother transitions).
	/// </summary>
	public void LoadSceneByIndexAsync(int sceneIndex)
	{
		if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
		{
			DOTween.KillAll();
			SceneManager.LoadSceneAsync(sceneIndex);
			Debug.Log($"➡️ [Async] Loading Scene Index: {sceneIndex}");
		}
		else
		{
			Debug.LogError("❌ Invalid scene index. Make sure the scene is added to Build Settings.");
		}
	}

	/// <summary>
	/// Reloads the currently active scene asynchronously.
	/// </summary>
	public void ReloadCurrentSceneAsync()
	{
		int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
		DOTween.KillAll();
		SceneManager.LoadSceneAsync(currentSceneIndex);
		Debug.Log($"🔄 [Async] Reloading Scene Index: {currentSceneIndex}");
	}

	/// <summary>
	/// Loads the next scene asynchronously.
	/// </summary>
	public void LoadNextSceneAsync()
	{
		int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
		int nextSceneIndex = currentSceneIndex + 1;

		if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
		{
			DOTween.KillAll();
			SceneManager.LoadSceneAsync(nextSceneIndex);
			Debug.Log($"➡️ [Async] Loading Next Scene Index: {nextSceneIndex}");
		}
		else
		{
			Debug.LogWarning("🔚 No more scenes in Build Settings to load.");
		}
	}
}

/// <summary>
/// Ensures all DOTween tweens are killed on every scene change,
/// compatible with all Unity versions.
/// Add this script ONCE anywhere in your project (does not need to be on a GameObject).
/// </summary>
public static class DOTweenSceneKillHelper
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void RegisterSceneCallback()
	{
		SceneManager.activeSceneChanged += OnAnyActiveSceneChanged;
	}

	private static void OnAnyActiveSceneChanged(Scene oldScene, Scene newScene)
	{
		DOTween.KillAll();
		Debug.Log($"🔪 [DOTween] All tweens killed on active scene change: {oldScene.name} ➡️ {newScene.name}");
	}
}

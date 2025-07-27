using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if TMP_PRESENT
using TMPro;
#endif

[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
	[Tooltip("The build index of the level represented by this button.")]
	public int levelIndex = 0;

	private Button btn;
	private Color lockedColor = Color.gray;
	private Color unlockedColor = Color.white; // Set your desired normal color

	void Start()
	{
		btn = GetComponent<Button>();

		if (LevelManager.Instance == null)
		{
#if UNITY_EDITOR
			Debug.LogWarning("No LevelManager instance found in scene.");
#endif
			return;
		}

		bool isUnlocked = LevelManager.Instance.IsLevelUnlocked(levelIndex);

		// Enable/disable button
		btn.interactable = isUnlocked;

		// Set only the button's main graphic color (no children)
		if (btn.targetGraphic != null)
		{
			btn.targetGraphic.color = isUnlocked ? unlockedColor : lockedColor;
		}
	}

	public void OnButtonClicked()
	{
		if (LevelManager.Instance == null) return;

		if (LevelManager.Instance.IsLevelUnlocked(levelIndex))
			SceneManager.LoadScene(levelIndex);
#if UNITY_EDITOR
		else
			Debug.Log($"Level {levelIndex} is locked.");
#endif
	}
}

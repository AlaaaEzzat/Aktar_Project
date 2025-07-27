using UnityEngine;

public class LevelManager : MonoBehaviour
{
	public static LevelManager Instance;
	public bool LoadingBefore = false;
	public bool startButton = false;

	// Represents the highest unlocked level (0-based)
	public int HighestUnlockedLevel { get; private set; } = 0;

	private const int MaxStars = 3;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	void Start()
	{
		HighestUnlockedLevel = PlayerPrefs.GetInt("HighestUnlockedLevel", 0);
#if UNITY_EDITOR
		Debug.Log("Highest unlocked level loaded: " + HighestUnlockedLevel);
#endif
	}

	/// <summary>
	/// Call this when a level is completed. Pass its index (0-based).
	/// Only the next level (index+1) will be unlocked, and only once.
	/// </summary>
	public void IncreaseLevelOpen(int completedLevelIndex)
	{
		int nextLevel = completedLevelIndex + 1;
		if (nextLevel > HighestUnlockedLevel)
		{
			HighestUnlockedLevel = nextLevel;
			PlayerPrefs.SetInt("HighestUnlockedLevel", HighestUnlockedLevel);
			PlayerPrefs.Save();
#if UNITY_EDITOR
			Debug.Log($"Unlocked level {nextLevel}. HighestUnlockedLevel is now {HighestUnlockedLevel}");
#endif
		}
	}

	public bool IsLevelUnlocked(int levelIndex)
	{
		return levelIndex <= HighestUnlockedLevel;
	}

	public int GetUnlockedLevelCount()
	{
		return HighestUnlockedLevel + 1; // count of unlocked levels
	}

	// ========== STARS & COINS SECTION ==========

	public void SetLevelStars(int levelIndex, int stars)
	{
		stars = Mathf.Clamp(stars, 0, MaxStars);
		string key = $"Level_{levelIndex}_Stars";
		int previous = GetLevelStars(levelIndex);
		if (stars > previous)
		{
			PlayerPrefs.SetInt(key, stars);
			PlayerPrefs.Save();
		}
	}

	public int GetLevelStars(int levelIndex)
	{
		return PlayerPrefs.GetInt($"Level_{levelIndex}_Stars", 0);
	}

	public void SetLevelCoins(int levelIndex, int coins)
	{
		coins = Mathf.Max(coins, 0);
		string key = $"Level_{levelIndex}_Coins";
		int previous = GetLevelCoins(levelIndex);
		if (coins > previous)
		{
			PlayerPrefs.SetInt(key, coins);
			PlayerPrefs.Save();
		}
	}

	public int GetLevelCoins(int levelIndex)
	{
		return PlayerPrefs.GetInt($"Level_{levelIndex}_Coins", 0);
	}
}

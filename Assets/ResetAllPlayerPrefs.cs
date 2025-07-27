using UnityEngine;

public class ResetAllPlayerPrefs : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
		SoundManager.Instance.PlaySound("GameSound");

	}

   
}

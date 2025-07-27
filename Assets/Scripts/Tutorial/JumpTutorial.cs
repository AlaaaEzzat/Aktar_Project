using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class JumpTutorial : MonoBehaviour
{
	[SerializeField] private bool inputFired = false;

	[Header("UI Panels or Buttons")]
	public GameObject mobileUI;
	public GameObject desktopUI;
	public GameObject sharedPanel;
	public List<GameObject> Coins;
	private bool tutorialTriggered = false;

	[DllImport("__Internal")]
	private static extern string GetUserAgent();

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (tutorialTriggered) return;

		if (other.CompareTag("Player"))
		{
			tutorialTriggered = true;
			
			

#if UNITY_WEBGL && !UNITY_EDITOR
			string ua = GetUserAgent();
			Debug.Log("User Agent: " + ua);

			if (IsMobile(ua))
			{
				ShowMobileUI();
			}
			else
			{
				ShowDesktopUI();
			}
#else
			// Default to desktop in Editor
			ShowDesktopUI();
#endif
		}
	}

	private void Update()
	{
		if (!tutorialTriggered) return;

		if (Input.GetKeyDown(KeyCode.Space) || inputFired)
		{
			if (sharedPanel) sharedPanel.SetActive(false);
			if (mobileUI) mobileUI.SetActive(false);
			if (desktopUI) desktopUI.SetActive(false);
			gameObject.SetActive(false);
			foreach (GameObject go in Coins) {
				go.SetActive(true);
			}
			
		}
		else
		{
			
		}
	}

	bool IsMobile(string ua)
	{
		string lowerUA = ua.ToLower();
		return lowerUA.Contains("iphone") || lowerUA.Contains("android") ||
			   lowerUA.Contains("ipad") || lowerUA.Contains("mobile");
	}

	void ShowMobileUI()
	{
		if (mobileUI) mobileUI.SetActive(true);
		if (desktopUI) desktopUI.SetActive(false);
		if (sharedPanel) sharedPanel.SetActive(true);
		Debug.Log("📱 Jump tutorial: Mobile UI active.");
	}

	void ShowDesktopUI()
	{
		if (mobileUI) mobileUI.SetActive(false);
		if (desktopUI) desktopUI.SetActive(true);
		if (sharedPanel) sharedPanel.SetActive(true);
		Debug.Log("🖥️ Jump tutorial: Desktop UI active.");
	}

	// 👇 Call this from the mobile jump button OnClick()
	public void FireMobileInput()
	{
		Debug.Log("📲 Mobile jump input fired");
		inputFired = true;
	}
}

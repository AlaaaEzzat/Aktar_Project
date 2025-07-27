using UnityEngine;
using System.Runtime.InteropServices;

public class PlatformUIHandler : MonoBehaviour
{
	[Header("UI Panels or Buttons")]
	public GameObject mobileUI;
	public GameObject desktopUI;

	[DllImport("__Internal")]
	private static extern string GetUserAgent();

	void Start()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
        string ua = GetUserAgent();
        Debug.Log("User Agent: " + ua);

        if (IsMobile(ua))
        {
            ShowMobileUI();
        }
        else
        {
		if(desktopUI!=null)
            ShowDesktopUI();
        }
#else
		// Default to desktop in Editor
		ShowDesktopUI();
#endif
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
		Debug.Log("📱 Mobile UI active.");
	}

	void ShowDesktopUI()
	{
		if (mobileUI) mobileUI.SetActive(false);
		if (desktopUI != null)
		if (desktopUI) desktopUI.SetActive(true);
		Debug.Log("🖥️ Desktop UI active.");
	}
}

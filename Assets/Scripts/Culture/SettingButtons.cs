using System.Threading;
using UnityEngine;

public class SettingButtons : MonoBehaviour
{
	public GameObject muteButton;
	public GameObject unmuteButton;
	private void Awake()
	{
		if (SoundManager.Instance != null)
		{
			if(SoundManager.Instance.isMuted)
			{
				muteButton.SetActive(true);
				unmuteButton.SetActive(false);
			}else
			{	
				muteButton.SetActive(false);
				unmuteButton.SetActive(true);
			}
			
				SoundManager.Instance.LoopSound("Back", true);
		}
	}
	public void mute()
	{
		if (SoundManager.Instance != null)
			SoundManager.Instance.MuteAll(true);

	}
	public void unmute()
	{
		if (SoundManager.Instance != null)
			SoundManager.Instance.MuteAll(false);
	}

	private void Update()
	{
		SoundManager.Instance?.LoopSound("Back", true);
	}

}

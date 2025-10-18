using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public void PlaySoundEffect(string sound)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(sound);
        }
    }
}

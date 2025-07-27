using UnityEngine;

public class Key : MonoBehaviour
{
	private KeyManager keyManager;

	[Header("Particle Settings")]
	public string particleTag = "KeyParticle"; // Tag for the pooled/scene particle object

	[Header("Sound Settings")]
	public string soundKey = "key"; // Key for SoundManager

	void Start()
	{
		keyManager = FindAnyObjectByType<KeyManager>();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			// Play particles
			PlayKeyParticle();

			// Play sound
			SoundManager.Instance?.PlaySound(soundKey);

			keyManager.CollectKey();
			Destroy(gameObject);
		}
	}

	void PlayKeyParticle()
	{
		// Find particle in scene by tag
		GameObject particleObj = GameObject.FindGameObjectWithTag(particleTag);
		if (particleObj != null)
		{
			ParticleSystem ps = particleObj.GetComponent<ParticleSystem>();
			if (ps != null)
			{
				particleObj.transform.position = transform.position;
				ps.Play();
			}
		}
	}
}

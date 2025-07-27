using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;

public class CollisionWithPlayer : MonoBehaviour
{
	[Tooltip("If true, player wins when colliding with this enemy; if false, player loses a heart.")]
	public bool winOnCollision = false;

	[Header("Particle Tag")]
	public string winParticleTag = "WinParticle"; // The tag to find the win particle in the scene

	[Header("DOTween Settings")]
	public float moveDuration = 0.4f;
	public float scaleDuration = 0.4f;
	public float winDelay = 2f; // Wait 2 seconds before win

	private Vector3 startPosition;
	private Tween playerMoveTween;
	private Tween playerScaleTween;

	private bool hasCollided = false;
	private Collider2D myCollider;

	private void Start()
	{
		startPosition = transform.position;
		SceneManager.sceneUnloaded += OnSceneUnloaded;
		myCollider = GetComponent<Collider2D>();
	}

	private void OnEnable()
	{
		hasCollided = false;
		if (!myCollider)
			myCollider = GetComponent<Collider2D>();
		if (myCollider) myCollider.enabled = true;
	}

	private void OnDestroy()
	{
		KillAllTweens();
		SceneManager.sceneUnloaded -= OnSceneUnloaded;
	}

	private void OnSceneUnloaded(Scene scene)
	{
		KillAllTweens();
	}

	private void KillAllTweens()
	{
		if (playerMoveTween != null && playerMoveTween.IsActive())
			playerMoveTween.Kill();
		if (playerScaleTween != null && playerScaleTween.IsActive())
			playerScaleTween.Kill();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (hasCollided) return;
		if (!other.CompareTag("Player"))
			return;

		hasCollided = true;
		Debug.Log("Player collided with " + gameObject.name);

		// Temporarily disable the collider to prevent further triggers
		if (myCollider) myCollider.enabled = false;
		StartCoroutine(ReenableColliderAfterDelay(1f));

		transform.position = startPosition;

		if (winOnCollision)
		{
			SoundManager.Instance?.StopAllSounds();
			Transform player = other.transform;
			player.GetComponent<PlayerController2D>().isWin = true;

			Vector3 targetPos = GetColliderCenter();

			DOTween.Kill(player);

			// 1. Move player to target position
			playerMoveTween = player.DOMove(targetPos, moveDuration)
				.SetEase(Ease.OutCubic)
				.SetTarget(player);

			// 2. Scale player to zero, then play particle and win logic
			playerScaleTween = player.DOScale(Vector3.zero, scaleDuration)
				.SetEase(Ease.InBack)
				.SetTarget(player)
				.OnComplete(() =>
				{
					// Play Particle at the same place
					GameObject taggedParticle = GameObject.FindWithTag(winParticleTag);
					if (taggedParticle != null)
					{
						GameObject particle = Instantiate(taggedParticle, player.position, Quaternion.identity);
						ParticleSystem ps = particle.GetComponent<ParticleSystem>();
						if (ps != null)
							Destroy(particle, ps.main.duration + 0.5f);
						else
							Destroy(particle, 2f);
					}
					else
					{
						Debug.LogWarning("No GameObject found with tag: " + winParticleTag);
					}

					// 3. Wait 2 seconds then call WinGame
					StartCoroutine(WaitAndWin(winDelay));
				});
		}
		else
		{
			SoundManager.Instance?.PlaySound("reset");
			GameManager.Instance.LoseHeart();
		}
	}

	private IEnumerator ReenableColliderAfterDelay(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		hasCollided = false;
		if (myCollider) myCollider.enabled = true;
	}

	private IEnumerator WaitAndWin(float delay)
	{
		yield return new WaitForSeconds(delay);
		GameManager.Instance.WinGame();
	}

	/// <summary>
	/// Gets the center of this collider in world space.
	/// </summary>
	private Vector3 GetColliderCenter()
	{
		Collider2D col = GetComponent<Collider2D>();
		if (col is BoxCollider2D box)
			return box.bounds.center;
		else if (col is CircleCollider2D circle)
			return circle.bounds.center;
		else
			return transform.position;
	}
}

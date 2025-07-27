using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class SliderSequenceAnimator : MonoBehaviour
{
	[Header("General Control")]
	public bool isLoadingBefore = false;

	[Header("Slider Settings")]
	public GameObject sliderObject;
	public Slider slider;
	[Range(0f, 1f)] public float targetSliderValue = 1f;
	[Tooltip("Duration for the slider fill (seconds)")]
	public float sliderDuration = 1f;

	[Header("Panel Fade Settings")]
	public List<Image> panelImage;
	[Tooltip("Duration for panel fade (seconds)")]
	public float fadeDuration = 1f;

	[Header("Crash Object Settings")]
	public List<CrashObject> crashObjects = new();
	[Tooltip("Distance to offset crash objects at start (units)")]
	public float crashOffsetDistance = 2000f;

	[Header("UI Controls")]
	public GameObject startButton;

	[Header("Start Button")]
	[SerializeField] List<GameObject> objectsToEnable;
	[SerializeField] List<GameObject> objectsToDisable;

	private void Start()
	{
		if (LevelManager.Instance != null && LevelManager.Instance.startButton)
		{
			LevelManager.Instance.startButton = false;
			foreach (var obj in objectsToEnable) if (obj != null) obj.SetActive(true);
			foreach (var obj in objectsToDisable) if (obj != null) obj.SetActive(false);
			return;
		}

		if (LevelManager.Instance != null && LevelManager.Instance.LoadingBefore)
		{
			if (sliderObject != null) sliderObject.SetActive(false);
			if (panelImage != null) SetImageAlpha(panelImage, 1f);
			if (startButton != null) startButton.SetActive(false);
			foreach (var obj in objectsToEnable) if (obj != null) obj.SetActive(true);
			foreach (var obj in objectsToDisable) if (obj != null) obj.SetActive(false);
			return;
		}
		if (LevelManager.Instance != null)
			LevelManager.Instance.LoadingBefore = true;

		if (slider != null) slider.value = 0f;
		if (sliderObject != null) sliderObject.SetActive(true);
		if (panelImage != null) SetImageAlpha(panelImage, 0f);
		if (startButton != null) startButton.SetActive(false);

		// Initialize crash objects
		foreach (var obj in crashObjects)
			obj?.Setup(crashOffsetDistance);

		RunAllAnimations();
	}

	private void RunAllAnimations()
	{
		// Master sequence to run everything in parallel
		Sequence fullSequence = DOTween.Sequence();

		// Slider fill
		if (slider != null)
		{
			Tween sliderTween = DOTween.To(() => slider.value, x => slider.value = x, targetSliderValue, sliderDuration)
				.OnComplete(() =>
				{
					if (startButton != null) startButton.SetActive(true);
					if (sliderObject != null) sliderObject.SetActive(false);
				});
			fullSequence.Join(sliderTween);
		}

		// Panel fade
		if (panelImage != null)
		{
			foreach (var img in panelImage)
				fullSequence.Join(img.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine));
		}

		// Crash animations
		foreach (var crash in crashObjects)
		{
			if (crash.target == null) continue;
			// Animate each crash object using its totalDuration
			fullSequence.Join(crash.AnimateCrashTween());
		}
	}

	private void SetImageAlpha(List<Image> images, float alpha)
	{
		foreach (var img in images)
		{
			if (img != null)
			{
				Color c = img.color;
				c.a = alpha;
				img.color = c;
			}
		}
	}

	[System.Serializable]
	public class CrashObject
	{
		public Transform target;
		public CrashDirection direction;
		[Tooltip("Total duration for this crash animation (seconds)")]
		public float totalDuration = 1f;
		[Tooltip("Delay before this crash starts (seconds)")]
		public float delay = 0f;
		[Tooltip("Easing to use for the crash movement")]
		public Ease moveEase = Ease.InExpo;

		[HideInInspector] public Vector3 originalPosition;
		[HideInInspector] public Vector3 originalScale;
		private float offsetDistance;

		public void Setup(float distance)
		{
			offsetDistance = distance;
			if (target == null) return;
			originalPosition = target.position;
			originalScale = target.localScale;
			// Offset start position
			Vector3 offset = direction switch
			{
				CrashDirection.Top => Vector3.up * offsetDistance,
				CrashDirection.Down => Vector3.down * offsetDistance,
				CrashDirection.Left => Vector3.left * offsetDistance,
				CrashDirection.Right => Vector3.right * offsetDistance,
				_ => Vector3.zero
			};
			target.position = originalPosition + offset;
			target.localScale = originalScale;
		}

		/// <summary>
		/// Builds and returns the tween sequence for this crash.
		/// </summary>
		public Tween AnimateCrashTween()
		{
			// Calculate move and squash times based on totalDuration
			float moveTime = totalDuration * 0.6f;
			float squashTime = totalDuration - moveTime;

			// Movement tween with delay
			Sequence seq = DOTween.Sequence()
				.AppendInterval(delay)
				.Append(target.DOMove(originalPosition, moveTime).SetEase(moveEase));

			// Squash & stretch appended
			seq.Append(
				DOTween.Sequence()
					.Append(target.DOScaleY(originalScale.y * 0.6f, squashTime * 0.375f).SetEase(Ease.OutQuad))
					.Join(target.DOScaleX(originalScale.x * 1.2f, squashTime * 0.375f).SetEase(Ease.OutQuad))
					.Append(target.DOScale(originalScale, squashTime * 0.625f).SetEase(Ease.OutElastic))
			);

			return seq;
		}
	}

	public enum CrashDirection { Top, Down, Left, Right }
}

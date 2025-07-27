using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KeyManager : MonoBehaviour
{
	public static KeyManager Instance;

	[Header("Required for Animation")]
	[Tooltip("How many keys must be collected to trigger the animation/move sequence.")]
	public int requiredKeyCountToAnimate = 3; // Set this in Inspector!

	[System.Serializable]
	public struct KeyMoveData
	{
		public Transform keyTransform;
		public float moveAmount;
		public float moveDuration;
		public int requiredCount;
		public MoveDirection moveDirection;
	}

	public enum MoveDirection { Left, Right, Up, Down }

	[Header("Key Objects")]
	public List<KeyMoveData> keyObjects;

	[Header("Gate Settings")]
	public Transform gateTransform;             // حدد الـ Gate من الـ Inspector
	public Vector3 gateOpenOffset = new Vector3(0, 3, 0); // أين تنتقل البوابة عند الفتح (مثلاً للأعلى)
	[Tooltip("هل تختفي البوابة بعد الفتح؟")]
	public bool disableGateAfterOpen = false;

	private int collectedCount = 0;

	void Awake()
	{
		Instance = this;
	}

	/// <summary>
	/// Call this method when a key is collected.
	/// </summary>
	public void CollectKey()
	{
		collectedCount++;
		if (collectedCount >= requiredKeyCountToAnimate)
		{
			GameManager.Instance.OnAllKeysCollected();

			// ابدأ فتح البوابة مع الصوت (يمكنك نقلها لمنطق GameManager حسب الحاجة)
			if (gateTransform != null)
				StartCoroutine(OpenGateWithSound(gateTransform, gateTransform.position + gateOpenOffset));
		}
	}

	/// <summary>
	/// Get how many keys have been collected.
	/// </summary>
	public int CollectedKeyCount => collectedCount;

	/// <summary>
	/// Get the total number of keys present in the level.
	/// </summary>
	public int TotalKeyCount => keyObjects.Count;

	/// <summary>
	/// GameManager will call this after camera moves.
	/// </summary>
	public IEnumerator MoveAllKeysToTarget()
	{
		Debug.Log("MoveAllKeysToTarget");

		// شغّل صوت البوابة
		SoundManager.Instance?.PlaySound("Gate");

		// احصل على مدة الصوت
		float maxDuration = SoundManager.Instance != null ? SoundManager.Instance.GetSoundDuration("Gate") : 1.5f;

		// حفظ المواضع الابتدائية والنهائية لكل مفتاح
		List<Vector3> startPositions = new List<Vector3>();
		List<Vector3> targetPositions = new List<Vector3>();

		foreach (var keyData in keyObjects)
		{
			startPositions.Add(keyData.keyTransform.position);
			Vector3 offset = GetDirectionVector(keyData.moveDirection) * keyData.moveAmount;
			targetPositions.Add(keyData.keyTransform.position + offset);
		}

		// تحريك المفاتيح على نفس مدة الصوت
		float elapsed = 0f;
		while (elapsed < maxDuration)
		{
			for (int i = 0; i < keyObjects.Count; i++)
			{
				var keyData = keyObjects[i];
				float t = Mathf.Clamp01(elapsed / maxDuration); // استخدم maxDuration وليس keyData.moveDuration
				keyData.keyTransform.position = Vector3.Lerp(startPositions[i], targetPositions[i], t);
			}
			elapsed += Time.deltaTime;
			yield return null;
		}

		// التأكد من الموضع النهائي وإخفاء المفاتيح
		for (int i = 0; i < keyObjects.Count; i++)
		{
			keyObjects[i].keyTransform.position = targetPositions[i];
			keyObjects[i].keyTransform.gameObject.SetActive(false);
		}
	}


	private Vector3 GetDirectionVector(MoveDirection dir)
	{
		switch (dir)
		{
			case MoveDirection.Left: return Vector3.left;
			case MoveDirection.Right: return Vector3.right;
			case MoveDirection.Up: return Vector3.up;
			case MoveDirection.Down: return Vector3.down;
			default: return Vector3.zero;
		}
	}

	/// <summary>
	/// Coroutine لفتح البوابة مع تشغيل صوت "Gate" في نفس الوقت.
	/// تتحرك البوابة حسب مدة الصوت تلقائيًا.
	/// </summary>
	public IEnumerator OpenGateWithSound(Transform gate, Vector3 targetPos)
	{
		if (gate == null) yield break;

		float duration = SoundManager.Instance != null ? SoundManager.Instance.GetSoundDuration("Gate") : 1.5f;
		Vector3 startPos = gate.position;

		// 2. حرّك البوابة تدريجيًا طوال مدة الصوت
		float elapsed = 0f;
		while (elapsed < duration)
		{
			float t = Mathf.Clamp01(elapsed / duration);
			gate.position = Vector3.Lerp(startPos, targetPos, t);
			elapsed += Time.deltaTime;
			yield return null;
		}
		gate.position = targetPos;

		// 3. عند الانتهاء: يمكنك إضافة مؤثر آخر أو غلق البوابة مثلاً
		if (disableGateAfterOpen)
			gate.gameObject.SetActive(false);
	}

	/// <summary>
	/// Optionally reset count at the start of the level.
	/// Call this from your GameManager or when starting a new level.
	/// </summary>
	public void ResetKeyCollection()
	{
		collectedCount = 0;
	}
}

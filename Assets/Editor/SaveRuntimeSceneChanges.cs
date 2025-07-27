#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[InitializeOnLoad]
public static class RuntimeStateSaver
{
	static bool shouldSave = false;

	// Store snapshot of positions during runtime
	static Dictionary<Transform, Vector3> positionMap = new();

	static RuntimeStateSaver()
	{
		// Hook into Play Mode state change
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
	}

	[MenuItem("Tools/Runtime Saver/Save Transforms Now %#r")] // Ctrl+Shift+R
	public static void SaveCurrentRuntimeTransforms()
	{
		if (!Application.isPlaying)
		{
			Debug.LogWarning("Play Mode must be active.");
			return;
		}

		positionMap.Clear();
		foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
		{
			if (!EditorUtility.IsPersistent(obj) && obj.scene.IsValid())
			{
				positionMap[obj.transform] = obj.transform.position;
			}
		}

		Debug.Log($"⏺️ Captured {positionMap.Count} transforms to apply after Play Mode.");
		shouldSave = true;
	}

	private static void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.ExitingPlayMode && shouldSave)
		{
			EditorApplication.delayCall += ApplySavedTransforms;
		}
	}

	private static void ApplySavedTransforms()
	{
		GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
		int count = 0;

		foreach (GameObject obj in allObjects)
		{
			if (positionMap.TryGetValue(obj.transform, out Vector3 savedPos))
			{
				Undo.RecordObject(obj.transform, "Apply Runtime Position");
				obj.transform.position = savedPos;
				EditorUtility.SetDirty(obj.transform);
				count++;
			}
		}

		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		Debug.Log($"✅ Applied {count} runtime transforms after Play Mode.");
		positionMap.Clear();
		shouldSave = false;
	}
}
#endif

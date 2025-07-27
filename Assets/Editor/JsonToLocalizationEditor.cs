/*using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic; 
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
public class QuestionData
{
	public string id;
	public LocalizedText ar;
}

public class LocalizedText
{
	public string question;
	public string correct_answer;
	public string wrong_answer;
}

public class QuestionList
{
	public List<QuestionData> questions;
}


public class JsonToLocalizationEditor : EditorWindow
{
	private string jsonFilePath = "Assets/StreamingAssets/questions.json"; // Path to JSON file
	private StringTableCollection stringTableCollection;

	[MenuItem("Tools/Update Localization Table from JSON")]
	public static void ShowWindow()
	{
		GetWindow<JsonToLocalizationEditor>("Update Localization Table");
	}

	void OnGUI()
	{
		GUILayout.Label("Update Localization Table from JSON", EditorStyles.boldLabel);

		jsonFilePath = EditorGUILayout.TextField("JSON File Path:", jsonFilePath);

		if (GUILayout.Button("Update Table"))
		{
			UpdateLocalizationTable();
		}
	}

	void UpdateLocalizationTable()
	{
		if (!File.Exists(jsonFilePath))
		{
#if UNITY_EDITOR
			Debug.LogError("? JSON file not found at: " + jsonFilePath);
#endif
			return;
		}

		string jsonData = File.ReadAllText(jsonFilePath);
		QuestionList questionList = JsonConvert.DeserializeObject<QuestionList>(jsonData);

		if (questionList == null || questionList.questions.Count == 0)
		{
#if UNITY_EDITOR
			Debug.LogError("? Failed to parse JSON or JSON is empty!");
#endif
			return;
		}

		// Load existing localization table
		stringTableCollection = AssetDatabase.LoadAssetAtPath<StringTableCollection>("Assets/Localization/QuestionsTable.asset");

		if (stringTableCollection == null)
		{
#if UNITY_EDITOR
			Debug.LogError("? QuestionsTable.asset not found! Create it manually in Unity first.");
#endif
			return;
		}

		// Get the Arabic table
		var stringTable = stringTableCollection.GetTable("ar") as StringTable;

		if (stringTable == null)
		{
#if UNITY_EDITOR
			Debug.LogError("? Arabic table not found in QuestionsTable.asset.");
#endif
			return;
		}

		// ? Log entries before removing them
#if UNITY_EDITOR
		Debug.Log($"?? Removing old entries (Current entries: {stringTable.Values.Count})...");
#endif
		List<long> keysToRemove = new List<long>();
		foreach (var entry in stringTable.Values)
		{
			keysToRemove.Add(entry.KeyId);
		}
		foreach (var key in keysToRemove)
		{
			stringTable.RemoveEntry(key);
		}

		// ? Log before adding new data
#if UNITY_EDITOR
		Debug.Log($"?? Adding {questionList.questions.Count} new questions to localization table...");
#endif

		foreach (var question in questionList.questions)
		{
			string questionKey = "Q_" + question.id;
			stringTable.AddEntry(questionKey, question.ar.question);
			stringTable.AddEntry(questionKey + "_Correct", question.ar.correct_answer);
			stringTable.AddEntry(questionKey + "_Wrong", question.ar.wrong_answer);

#if UNITY_EDITOR
			Debug.Log($"? Added: {questionKey} ? {question.ar.question}");
#endif
		}

		// Save changes
		EditorUtility.SetDirty(stringTable);
		AssetDatabase.SaveAssets();
#if UNITY_EDITOR
		Debug.Log("? Localization Table updated successfully!");
#endif
	}
}

*/

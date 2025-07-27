using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class WrapDebugWithIfEditor : EditorWindow
{
	[MenuItem("Tools/Wrap Debug Logs with UNITY_EDITOR")]
	public static void WrapAllDebugLogs()
	{
		string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
		int wrappedCount = 0;

		foreach (string file in files)
		{
			string[] lines = File.ReadAllLines(file);
			bool fileModified = false;

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i].Trim();

				// Match Debug.Log / Debug.LogWarning / Debug.LogError
				if (Regex.IsMatch(line, @"^\s*Debug\.(Log|LogWarning|LogError|LogException|Assert)\s*\(.*\)\s*;"))
				{
					// Don't wrap if already inside a UNITY_EDITOR block
					if (i > 0 && lines[i - 1].Contains("#if UNITY_EDITOR"))
						continue;

					lines[i] = $"#if UNITY_EDITOR\n{lines[i]}\n#endif";
					fileModified = true;
					wrappedCount++;
				}
			}

			if (fileModified)
				File.WriteAllLines(file, lines);
		}

		AssetDatabase.Refresh();
#if UNITY_EDITOR
		Debug.Log($"✅ Wrapped {wrappedCount} Debug.Log lines with #if UNITY_EDITOR.");
#endif
	}
}

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class OptimizeWebGLTemplate : EditorWindow
{
	private const string templateName = "MyOptimizedTemplate";
	private static readonly string projectTemplatePath = Path.Combine("Assets/WebGLTemplates", templateName);
	private static readonly string indexFilePath = Path.Combine(projectTemplatePath, "index.html");

	[MenuItem("Tools/Optimize WebGL Template")]
	public static void OptimizeTemplate()
	{
		if (!Directory.Exists(projectTemplatePath))
		{
			Debug.LogError($"❌ Template folder not found at {projectTemplatePath}. Please create it manually and add index.html.");
			return;
		}

		if (!File.Exists(indexFilePath))
		{
			Debug.LogError($"❌ index.html not found in template. Expected at: {indexFilePath}");
			return;
		}

		string html = File.ReadAllText(indexFilePath);

		// ✅ Keep only charset and viewport meta tags
		html = Regex.Replace(html, @"<meta\s+(?![^>]*(charset|viewport)).*?>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

		// ✅ Remove Unity spinner div
		html = Regex.Replace(html, @"<div\s+class=""spinner"".*?</div>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

		// ✅ Remove all external CSS/JS links
		html = Regex.Replace(html, @"<link\s+rel=[""']stylesheet[""'][^>]+>", "", RegexOptions.IgnoreCase);
		html = Regex.Replace(html, @"<script\s+src=[""'][^""]+[""'][^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

		// ✅ Remove all inline <script> blocks except UnityLoader
		html = Regex.Replace(html, @"<script[^>]*>.*?(unityProgress|createUnityInstance).*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		html = Regex.Replace(html, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

		// ✅ Remove console.log / warn / error
		html = Regex.Replace(html, @"console\.(log|warn|error)\s*\(.*?\);?", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

		// ✅ Remove HTML comments
		html = Regex.Replace(html, @"<!--.*?-->", "", RegexOptions.Singleline);

		// ✅ Clean excessive whitespace
		html = Regex.Replace(html, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
		html = Regex.Replace(html, @"\s{2,}", " ");

		// ✅ Overwrite the index.html
		File.WriteAllText(indexFilePath, html);
		AssetDatabase.Refresh();

		// ✅ Set this template as active
		PlayerSettings.WebGL.template = templateName;

		Debug.Log($"✅ Optimized and applied local WebGL template: {templateName}");
	}
}

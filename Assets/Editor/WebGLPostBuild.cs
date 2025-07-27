using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class WebGLPostBuild
{
	[PostProcessBuild]
	public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
	{
		if (target == BuildTarget.WebGL)
		{
			string indexPath = Path.Combine(pathToBuiltProject, "index.html");
			if (File.Exists(indexPath))
			{
				string indexContent = File.ReadAllText(indexPath);

				string productName = PlayerSettings.productName;
				indexContent = Regex.Replace(indexContent,
					"<title>.*?</title>", $"<title>{productName}</title>");

				string[] assetGuids = AssetDatabase.FindAssets("t:Texture2D Favicon", new[] { "Assets" });
				string faviconPath = "";

				if (assetGuids.Length > 0)
				{
					string guid = assetGuids[0];
					faviconPath = AssetDatabase.GUIDToAssetPath(guid);

					string buildIconPath = Path.Combine(pathToBuiltProject, "Logo.png");
					if (!File.Exists(buildIconPath))
					{
						File.Copy(faviconPath, buildIconPath, true);
						Debug.Log($"Copied favicon to: {buildIconPath}");
					}

					string faviconLink = $"<link rel=\"icon\" type=\"image/x-icon\" href=\"Logo.png\">";
					indexContent = indexContent.Replace("</head>", faviconLink + "\n</head>");
				}
				else
				{
					Debug.LogWarning("Favicon.png not found in the Assets folder.");
				}

				string customJS = @"
                <script>
                    document.body.style.overflow = 'hidden';
                    // Mobile viewport settings
                    if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
                        var meta = document.createElement('meta');
                        meta.name = 'viewport';
                        meta.content = 'width=device-width, initial-scale=1.0, user-scalable=no, shrink-to-fit=yes';
                        document.getElementsByTagName('head')[0].appendChild(meta);
                        
                        // Adjust canvas style for mobile devices
                        var canvas = document.querySelector('#unity-canvas');
                        if (canvas) {
                            canvas.style.position = 'absolute';
                            canvas.style.top = '50%';
                            canvas.style.left = '50%';
                            canvas.style.transform = 'translate(-50%, -50%)';
                            canvas.style.width = '100vw';
                            //canvas.style.height = 'auto';
                            canvas.style.maxHeight = '100vh';
                        }
                    } else {
                        // Adjust canvas style for desktop
                        var canvas = document.querySelector('#unity-canvas');
                        if (canvas) {
                            canvas.style.width = '100%';
                            canvas.style.height = '100vh';
                            canvas.style.background = '#231F20';
                        }
                    }
                </script>
<script>
      document.addEventListener(""DOMContentLoaded"", function () {
          function isMobileDevice() {
              return /Mobi|Android|iPhone|iPad|iPod/i.test(navigator.userAgent);
          }
          function enterFullScreen() {
              let elem = document.documentElement;
              if (!document.fullscreenElement) { // Only enter full-screen if not already active
                  if (elem.requestFullscreen) {
                      elem.requestFullscreen();
                  } else if (elem.mozRequestFullScreen) { // Firefox
                      elem.mozRequestFullScreen();
                  } else if (elem.webkitRequestFullscreen) { // Chrome, Safari, Opera
                      elem.webkitRequestFullscreen();
                  } else if (elem.msRequestFullscreen) { // Internet Explorer/Edge
                      elem.msRequestFullscreen();
                  }
              }
          }
          function forceFullScreenOnGameStart() {
              if (isMobileDevice()) {
                  enterFullScreen(); // Force full-screen for mobile devices
              }
          }
          // Attempt fullscreen when the game starts
          window.addEventListener(""click"", forceFullScreenOnGameStart);
          window.addEventListener(""touchstart"", forceFullScreenOnGameStart);
          // Ensure fullscreen is enabled when switching to landscape mode
          window.addEventListener(""resize"", function () {
              if (window.innerWidth > window.innerHeight) {
                  enterFullScreen();
              }
          });
          // Initial attempt for fullscreen when the page loads
          forceFullScreenOnGameStart();
      });
  </script>";
				indexContent = indexContent.Replace("</body>", customJS + "\n</body>");

				File.WriteAllText(indexPath, indexContent);
				Debug.Log("WebGL index.html modification completed!");
			}
			else
			{
				Debug.LogError("index.html not found in WebGL build directory.");
			}
		}
	}
}
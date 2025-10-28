using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class WebGLPostBuild
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.WebGL) return;

        // ====== CONFIG ======
        // حجم الأساس على الموبايل (من PlayerSettings أو افتراضي 960x540)
        int MOBILE_BASE_W = PlayerSettings.defaultScreenWidth > 0 ? PlayerSettings.defaultScreenWidth : 960;
        int MOBILE_BASE_H = PlayerSettings.defaultScreenHeight > 0 ? PlayerSettings.defaultScreenHeight : 540;
        float TARGET_ASPECT = (MOBILE_BASE_H > 0) ? (float)MOBILE_BASE_W / MOBILE_BASE_H : (16f / 9f);

        // فعّل PWA؟ (يضيف manifest.json + meta tags + service worker)
        bool ENABLE_PWA = false; // غيّرها إلى true لو عايز تفعيل PWA تلقائيًا

        // ====== Paths ======
        string indexPath = Path.Combine(pathToBuiltProject, "index.html");
        if (!File.Exists(indexPath))
        {
            Debug.LogError("index.html not found in WebGL build directory.");
            return;
        }

        string indexContent = File.ReadAllText(indexPath, new UTF8Encoding(false));

        // ====== 0) <meta viewport> بخصائص مناسبة (viewport-fit=cover) ======
        if (!Regex.IsMatch(indexContent, "<meta[^>]*name=[\"']viewport[\"'][^>]*>", RegexOptions.IgnoreCase))
        {
            string meta = "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, viewport-fit=cover\">";
            indexContent = indexContent.Replace("</head>", meta + "\n</head>");
        }
        else
        {
            indexContent = Regex.Replace(
                indexContent,
                "<meta[^>]*name=[\"']viewport[\"'][^>]*>",
                "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, viewport-fit=cover\">",
                RegexOptions.IgnoreCase
            );
        }

        // ====== 1) <title> = productName ======
        string productName = PlayerSettings.productName;
        if (Regex.IsMatch(indexContent, "<title>.*?</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            indexContent = Regex.Replace(indexContent, "<title>.*?</title>", $"<title>{productName}</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        else
            indexContent = indexContent.Replace("</head>", $"<title>{productName}</title>\n</head>");

        // ====== 2) Favicon (اختياري): أول Texture2D اسمه فيه "Favicon" → Logo.png ======
        string[] assetGuids = AssetDatabase.FindAssets("t:Texture2D Favicon", new[] { "Assets" });
        if (assetGuids.Length > 0)
        {
            string faviconPath = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
            string buildIconPath = Path.Combine(pathToBuiltProject, "Logo.png");
            try
            {
                if (!File.Exists(buildIconPath))
                {
                    File.Copy(faviconPath, buildIconPath, true);
                    Debug.Log($"[WebGLPostBuild] Copied favicon to: {buildIconPath}");
                }
                if (!indexContent.Contains("rel=\"icon\""))
                {
                    string faviconLink = "<link rel=\"icon\" type=\"image/png\" href=\"Logo.png\">";
                    indexContent = indexContent.Replace("</head>", faviconLink + "\n</head>");
                }
            }
            catch { /* ignore */ }
        }

        // ====== 3) CSS أساسي (Safe Areas + --vh + تمركز) ======
        var styleSb = new StringBuilder();
        styleSb.AppendLine("<style id=\"unity-fit-centered-style\">");
        styleSb.AppendLine("  :root { --vh: 1vh; }");
        styleSb.AppendLine("  html, body { height: 100%; margin: 0; padding: 0; overflow: hidden; background: #000; }");
        styleSb.AppendLine("  body { overscroll-behavior: none; }");
        styleSb.AppendLine("  #unity-container { position: fixed; inset: 0; display: flex; align-items: center; justify-content: center; background: #000; }");
        styleSb.AppendLine("  /* احترام الـSafe Areas على iOS */");
        styleSb.AppendLine("  #unity-container { padding: env(safe-area-inset-top) env(safe-area-inset-right) env(safe-area-inset-bottom) env(safe-area-inset-left); }");
        styleSb.AppendLine("  #unity-canvas { display: block; background: #231F20; transform-origin: center center; }");
        styleSb.AppendLine("</style>");

        if (!indexContent.Contains("unity-fit-centered-style"))
            indexContent = indexContent.Replace("</head>", styleSb.ToString() + "\n</head>");

        // ====== 4) JS: Landscape يثبت Aspect, Portrait يملأ الشاشة + Fullscreen عند أول تفاعل ======
        string js = $@"
<script id=""unity-fit-centered-script"">
(function () {{
  var TARGET_ASPECT = {TARGET_ASPECT.ToString(System.Globalization.CultureInfo.InvariantCulture)}; // width/height
  var MOBILE_BASE_W = {MOBILE_BASE_W};
  var MOBILE_BASE_H = {MOBILE_BASE_H};

  function isMobile() {{
    return /Mobi|Android|iPhone|iPad|iPod/i.test(navigator.userAgent);
  }}
  function isPortrait() {{
    var m = window.matchMedia && window.matchMedia('(orientation: portrait)');
    return m ? m.matches : (window.innerHeight >= window.innerWidth);
  }}
  function isLandscape() {{ return !isPortrait(); }}

  function vpWidth() {{
    return (window.visualViewport ? Math.floor(window.visualViewport.width) : window.innerWidth);
  }}
  function vpHeight() {{
    // استخدم --vh لتفادي شريط العنوان في iOS
    var cssVh = parseFloat(getComputedStyle(document.documentElement).getPropertyValue('--vh'));
    if (!isNaN(cssVh) && cssVh > 0) return Math.floor(cssVh * 100);
    var h = (window.visualViewport ? window.visualViewport.height : window.innerHeight);
    return Math.floor(h);
  }}
  function setVhVar() {{
    var h = (window.visualViewport ? window.visualViewport.height : window.innerHeight);
    if (h && h > 0) document.documentElement.style.setProperty('--vh', (h / 100) + 'px');
  }}

  // --- Layout ---
  function sizeLandscape(canvas) {{
    // نحافظ على الـAspect (Letterbox)
    var vw = vpWidth(), vh = vpHeight();
    var viewportAspect = vw / vh, drawW, drawH;
    if (viewportAspect > TARGET_ASPECT) {{ drawH = vh; drawW = Math.floor(drawH * TARGET_ASPECT); }}
    else {{ drawW = vw; drawH = Math.floor(drawW / TARGET_ASPECT); }}
    canvas.style.width  = drawW + 'px';
    canvas.style.height = drawH + 'px';
    canvas.style.transform = '';
  }}
  function sizePortrait(canvas) {{
    // نملأ الشاشة بالكامل
    var vw = vpWidth(), vh = vpHeight();
    canvas.style.width  = vw + 'px';
    canvas.style.height = vh + 'px';
    canvas.style.transform = '';
  }}

  function applyLayout() {{
    var container = document.getElementById('unity-container') || document.body;
    var canvas = document.getElementById('unity-canvas') || document.querySelector('canvas');
    if (!canvas) return;

    var c = container.style;
    c.position = 'fixed';
    c.inset = '0';
    c.display = 'flex';
    c.alignItems = 'center';
    c.justifyContent = 'center';
    c.background = '#000';

    if (isMobile() && isPortrait()) sizePortrait(canvas);
    else sizeLandscape(canvas);

    document.body.style.overscrollBehavior = 'none';
  }}

  // --- Fullscreen (بدون زر) ---
  var userInteracted = false;
  var triedLockOnce = false;

  function isIOS() {{
    return /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
  }}
  function canUseRealFullscreen() {{
    // على iOS غالبًا لا يعمل Fullscreen API لعناصر الويب كـcanvas (نكتفي بالـCSS)
    return !isIOS() && !!(document.documentElement.requestFullscreen || document.documentElement.webkitRequestFullscreen);
  }}

  async function requestFullscreenIfLandscape() {{
    if (!isMobile() || !isLandscape() || document.fullscreenElement) return;
    if (!canUseRealFullscreen()) return; // iOS: نتجاهل ونكتفي بالـCSS

    try {{
      var el = document.documentElement;
      var req = el.requestFullscreen || el.webkitRequestFullscreen || el.msRequestFullscreen;
      if (req) {{
        await req.call(el);
        if (!triedLockOnce && screen.orientation && screen.orientation.lock) {{
          triedLockOnce = true;
          screen.orientation.lock('landscape').catch(function(){{}});
        }}
      }}
    }} catch (e) {{ /* ignore */ }}
  }}

  function onFirstUserGesture() {{
    if (!userInteracted) userInteracted = true;
    requestFullscreenIfLandscape();
  }}

  function onOrientationChange() {{
    if (isMobile() && isLandscape() && userInteracted && !document.fullscreenElement) {{
      requestFullscreenIfLandscape();
    }}
    schedule();
  }}

  // --- Scheduler ---
  var scheduled = false;
  function schedule() {{
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(function () {{
      scheduled = false;
      setVhVar();
      applyLayout();
    }});
  }}

  // Hooks
  window.addEventListener('load', schedule, {{ passive: true }});
  window.addEventListener('resize', schedule, {{ passive: true }});
  window.addEventListener('orientationchange', onOrientationChange, {{ passive: true }});
  if (window.visualViewport) {{
    window.visualViewport.addEventListener('resize', schedule, {{ passive: true }});
    window.visualViewport.addEventListener('scroll', schedule, {{ passive: true }});
  }}
  ['pointerdown','touchend','mousedown','keydown'].forEach(function(ev){{
    window.addEventListener(ev, onFirstUserGesture, {{ once:false, passive:true }});
  }});
  document.addEventListener('fullscreenchange', schedule, {{ passive: true }});

  // Run asap
  setVhVar();
  applyLayout();

  // --- (اختياري) تسجيل Service Worker للـPWA إن وُجد ---
  if ('serviceWorker' in navigator) {{
    navigator.serviceWorker.getRegistrations && navigator.serviceWorker.getRegistrations().then(function(regs){{
      // لا نفعل أي شيء هنا — تسجيل يتم أسفل عند تمكين PWA
    }});
  }}
}})();
</script>";

        if (!indexContent.Contains("unity-fit-centered-script"))
            indexContent = indexContent.Replace("</body>", js + "\n</body>");
        else
            indexContent = Regex.Replace(indexContent, "<script id=\"unity-fit-centered-script\">[\\s\\S]*?</script>", js, RegexOptions.Singleline);

        // ====== 5) (اختياري) تمكين PWA ======
        if (ENABLE_PWA)
        {
            // manifest.json
            string manifestPath = Path.Combine(pathToBuiltProject, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                var manifest = new StringBuilder();
                manifest.AppendLine("{");
                manifest.AppendLine($"  \"name\": \"{EscapeJson(productName)}\",");
                manifest.AppendLine($"  \"short_name\": \"{EscapeJson(productName)}\",");
                manifest.AppendLine("  \"display\": \"standalone\",");
                manifest.AppendLine("  \"background_color\": \"#000000\",");
                manifest.AppendLine("  \"theme_color\": \"#000000\",");
                manifest.AppendLine("  \"start_url\": \"./index.html\",");
                manifest.AppendLine("  \"icons\": [");
                manifest.AppendLine("    { \"src\": \"Logo.png\", \"sizes\": \"512x512\", \"type\": \"image/png\" }");
                manifest.AppendLine("  ]");
                manifest.AppendLine("}");
                File.WriteAllText(manifestPath, manifest.ToString(), new UTF8Encoding(false));
            }

            // link rel=manifest + theme + apple meta
            if (!indexContent.Contains("id=\"unity-pwa-manifest\""))
            {
                var pwaHead = new StringBuilder();
                pwaHead.AppendLine("<link id=\"unity-pwa-manifest\" rel=\"manifest\" href=\"manifest.json\">");
                pwaHead.AppendLine("<meta name=\"theme-color\" content=\"#000000\">");
                pwaHead.AppendLine("<meta name=\"apple-mobile-web-app-capable\" content=\"yes\">");
                pwaHead.AppendLine("<meta name=\"apple-mobile-web-app-status-bar-style\" content=\"black-translucent\">");
                pwaHead.AppendLine($"<meta name=\"apple-mobile-web-app-title\" content=\"{SecurityElementEscape(productName)}\">");
                pwaHead.AppendLine("<link rel=\"apple-touch-icon\" href=\"Logo.png\">");
                indexContent = indexContent.Replace("</head>", pwaHead.ToString() + "\n</head>");
            }

            // service-worker.js (بسيط جدًا للكاش الأساسي)
            string swPath = Path.Combine(pathToBuiltProject, "service-worker.js");
            if (!File.Exists(swPath))
            {
                string sw = @"
self.addEventListener('install', (event) => {
  event.waitUntil((async () => {
    const cache = await caches.open('unity-cache-v1');
    await cache.addAll([
      './',
      './index.html',
      './Build/',
      './Logo.png',
      './manifest.json'
    ]);
  })());
});
self.addEventListener('fetch', (event) => {
  event.respondWith((async () => {
    const cached = await caches.match(event.request);
    return cached || fetch(event.request);
  })());
});
";
                File.WriteAllText(swPath, sw, new UTF8Encoding(false));
            }

            // سجل الـSW في index.html لو مش متسجل
            if (!indexContent.Contains("unity-pwa-sw-register"))
            {
                string swRegister = @"
<script id=""unity-pwa-sw-register"">
if ('serviceWorker' in navigator) {
  window.addEventListener('load', function() {
    navigator.serviceWorker.register('./service-worker.js').catch(function(){});
  });
}
</script>";
                indexContent = indexContent.Replace("</body>", swRegister + "\n</body>");
            }
        }

        // ====== Write back ======
        File.WriteAllText(indexPath, indexContent, new UTF8Encoding(false));
        Debug.Log($"[WebGLPostBuild] index.html updated (Landscape aspect lock, Portrait fill, auto-FS on first gesture, PWA={ENABLE_PWA}).");
    }

    // Helpers
    private static string EscapeJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
    private static string SecurityElementEscape(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&#39;");
    }
}

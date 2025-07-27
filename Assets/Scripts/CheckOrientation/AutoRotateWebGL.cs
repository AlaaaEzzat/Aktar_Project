using UnityEngine;

public class AutoRotateWebGL : MonoBehaviour
{
	void Start()
	{
#if UNITY_WEBGL
		// The forced CSS rotation has been removed.
		// Instead, we only show a full-screen overlay in portrait mode.
		ShowHideRotationOverlay();
#endif
	}

	// ? This function injects JavaScript to create an overlay alert that appears only in portrait mode.
	//    When the device is rotated to landscape, the overlay is hidden.
	void ShowHideRotationOverlay()
	{
		string jsCode = @"
            function createRotationOverlay() {
                let overlay = document.createElement('div');
                overlay.id = 'rotationOverlay';
                overlay.style.position = 'fixed';
                overlay.style.top = '0';
                overlay.style.left = '0';
                overlay.style.width = '100vw';
                overlay.style.height = '100vh';
                overlay.style.background = 'black';
                overlay.style.display = 'flex';
                overlay.style.alignItems = 'center';
                overlay.style.justifyContent = 'center';
                overlay.style.color = 'white';
                overlay.style.fontSize = '24px';
                overlay.style.textAlign = 'center';
                overlay.style.zIndex = '9999';
                overlay.innerHTML = 'Please rotate your device for the best experience.';
                document.body.appendChild(overlay);
                checkOrientation();
            }

            function checkOrientation() {
                let overlay = document.getElementById('rotationOverlay');
                // In portrait mode, show the overlay; in landscape mode, hide it.
                if (window.innerHeight > window.innerWidth) {
                    overlay.style.display = 'flex';
                } else {
                    overlay.style.display = 'none';
                }
            }

            window.addEventListener('resize', checkOrientation);
            createRotationOverlay();
        ";
		Application.ExternalEval(jsCode);
	}
}

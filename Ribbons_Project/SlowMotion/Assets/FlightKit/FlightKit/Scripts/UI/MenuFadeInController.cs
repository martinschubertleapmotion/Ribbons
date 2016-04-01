using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace FlightKit
{
	/// <summary>
	/// Controls the sequence of events during start of the game (showing menus, turning on/off components, etc).
	/// </summary>
	public class MenuFadeInController : MonoBehaviour
	{
		public GameObject mainMenu;

        [Space]
		public CanvasGroup playButton;
		public CanvasGroup controlsButton;
		public Image gameLogoImage;
		public Image instructionsImage;

		IEnumerator Start()
		{
			// Init vars.
			if (mainMenu == null)
			{
				mainMenu = GameObject.Find("MainMenu");
				if (mainMenu == null)
				{
					Debug.LogError("Can't find MainMenu object in the scene.");
					yield break;
				}
			}

			mainMenu.SetActive(true);

			if (gameLogoImage) 
            {
                gameLogoImage.enabled = false;
            }

            if (playButton)
            {
                playButton.interactable = false;
                playButton.alpha = 0;
            }

            if (controlsButton)
            {
                controlsButton.interactable = false;
                controlsButton.alpha = 0;
            }

            if (instructionsImage)
            {
			    instructionsImage.enabled = false;
            }

			// Screen fade in
			yield return new WaitForSeconds(3);

			// Game logo fade in
            if (gameLogoImage)
            {
                gameLogoImage.enabled = true;
                gameLogoImage.canvasRenderer.SetAlpha(0.0f);
                gameLogoImage.CrossFadeAlpha(1.0f, 3.0f, false);
                yield return new WaitForSeconds(2);
            }

            if (playButton)
            {
                // Buttons fade in
                Fader.FadeAlpha(this, playButton, true, 0.7f);
                playButton.interactable = true;
                yield return new WaitForSeconds(0.5f);
            }

            if (controlsButton)
            {
                Fader.FadeAlpha(this, controlsButton, true, 0.7f);
                controlsButton.interactable = true;
                yield return new WaitForSeconds(0.5f);
            }

            if (instructionsImage)
            {
                // Instructions fade in
                instructionsImage.enabled = true;
                instructionsImage.canvasRenderer.SetAlpha(0.0f);
                instructionsImage.CrossFadeAlpha(1.0f, 2.5f, false);
                yield return new WaitForSeconds(2);
            }
		}
	}
}
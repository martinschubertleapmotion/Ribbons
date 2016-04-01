using UnityEngine;

namespace FlightKit
{
	[RequireComponent(typeof (CrossFadeCanvasGroups))]
	public class BackButton : MonoBehaviour
	{
		public CanvasGroup mainMenu;
		public CanvasGroup pauseMenu;

		private CrossFadeCanvasGroups crossFade;
		private StartLevelController startLevelController;

		void Start()
		{
			crossFade = GetComponent<CrossFadeCanvasGroups>();
			startLevelController = GameObject.FindObjectOfType<StartLevelController>();
			crossFade.toGroup = mainMenu;
		}

		public virtual void Activate()
		{
			if (startLevelController != null)
			{
				crossFade.toGroup = startLevelController.levelStarted ? pauseMenu : mainMenu;
			}
			crossFade.Activate();
		}
	}
}
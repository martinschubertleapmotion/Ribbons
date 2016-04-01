using UnityEngine;
using UnityEngine.SceneManagement;

namespace FlightKit
{
	/// <summary>
	/// Performs the actions when a level is completed.
	/// </summary>
	public class LevelCompleteController : MonoBehaviour
	{
		/// <summary>
		/// Level is successfully completed, start the next level or go to menu from here.
		/// </summary>
		public virtual void HandleLevelComplete()
		{
			// Restart game.
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
}
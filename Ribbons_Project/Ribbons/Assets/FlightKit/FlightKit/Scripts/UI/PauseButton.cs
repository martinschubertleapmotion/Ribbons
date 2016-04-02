using UnityEngine;

namespace FlightKit
{   
    /// <summary>
    /// Finds PauseController and calls Pause() on click.
    /// </summary>
    public class PauseButton : MonoBehaviour {
        [Header("Finds PauseController and calls Pause on click/touch.")]
        public bool active = true;
        
        virtual public void OnClick()
        {
            if (active)
            {
                var pauseController = GameObject.FindObjectOfType<PauseController>();
                if (pauseController != null)
                {
                    pauseController.Pause();
                }
            }
        }
        
    }
}

using UnityEngine;

namespace FlightKit
{
    public class AirplaneTrails : MonoBehaviour {
        /// <summary>
        /// The GameObject that contains airplane's trails.
        /// </summary>
        public GameObject trailsContainer;

        
        public virtual void ActivateTrails()
        {
            if (trailsContainer != null)
            {
                trailsContainer.SetActive(true);
            }
        }
        
        public virtual void DeactivateTrails()
        {
            if (trailsContainer != null)
            {
                trailsContainer.SetActive(false);
            }
        }
        
        public virtual void ClearTrails()
        {
            if (trailsContainer != null)
            {
                var renderers = trailsContainer.GetComponentsInChildren<TrailRenderer>();
                foreach (TrailRenderer r in renderers)
                {
                    r.Clear();
                    //r.time = 0;
                }
            }
        }
    }

}
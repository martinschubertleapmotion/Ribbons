using UnityEngine;

namespace FlightKit
{
	/// <summary>
	/// Activates GameObjects when user's airplane takes off the ground.
	/// </summary>
    public class ActivateOnTakeOff : MonoBehaviour
    {
		/// <summary>
		/// Optional delay between take-off and activating.
		/// </summary>
        public float delay = 0;

		/// <summary>
		/// Array of objects to be activated.
		/// </summary>
        public GameObject[] objectsToActivate;

        void OnEnable()
        {
            TakeOffPublisher.OnTakeOffEvent += OnTakeOff;
        }

        void OnDisable()
        {
            TakeOffPublisher.OnTakeOffEvent -= OnTakeOff;
        }

        private void OnTakeOff()
        {
            Invoke("OnTakeOffCore", delay);
        }

        private void OnTakeOffCore()
        {
            foreach (var target in objectsToActivate)
            {
                if (target != null)
                {
                    target.SetActive(true);
                }
            }
        }
    }
}
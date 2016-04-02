using UnityEngine;
using UnityStandardAssets.Vehicles.Aeroplane;

namespace FlightKit
{
    [RequireComponent(typeof(AeroplaneController))]
    public class SimplePropellerAnimator : MonoBehaviour
    {
        /// <summary>
        /// The model of the the aeroplane's propeller.
        /// </summary>
        public Transform propellerModel;

        /// <summary>
        /// The maximum speed the propellor can turn at.
        /// </summary>
        public float maxRpm = 2000;

        /// <summary>
        /// Rotate propeller model around X axis instead of Y axis. Useful for Blender-imported models.
        /// </summary>
        public bool rotateAroundX = false;

        private AeroplaneController _airplane;

        /// <summary>
        /// For converting from revs per minute to degrees per second.
        /// </summary>
        private const float RPM_TO_DPS = 60f;

        private Renderer _propellerModelRenderer;

        private void Awake()
        {
            // Set up the reference to the airplane controller.
            _airplane = GetComponent<AeroplaneController>();
        }


        private void Update()
        {
            if (!_airplane || !propellerModel)
            {
                return;
            }

            // Rotate the propeller model at a rate proportional to the throttle.
            float rotation = maxRpm * _airplane.Throttle * Time.deltaTime*RPM_TO_DPS;

            if (rotateAroundX)
            {
                propellerModel.Rotate(rotation, 0, 0);
            }
            else
            {
                propellerModel.Rotate(0, rotation, 0);
            }
        }
    }
}

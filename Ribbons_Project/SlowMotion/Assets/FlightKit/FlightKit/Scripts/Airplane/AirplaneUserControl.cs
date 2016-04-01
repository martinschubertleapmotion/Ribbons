using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Vehicles.Aeroplane;

namespace FlightKit
{
	/// <summary>
	/// Reads the user input from standalone or mobile controls and feeds it to the user-controlled AeroplaneController.
	/// </summary>
    [RequireComponent(typeof(AeroplaneController))]
    public class AirplaneUserControl : MonoBehaviour
    {
        /// <summary>
		/// Maximum allowed roll angle on mobile.
		/// </summary>
        public float maxRollAngle = 80;
		/// <summary>
		/// Maximum allowed roll angle on mobile.
		/// </summary>
		public float maxPitchAngle = 80;

		/// <summary>
		/// Reference to the aeroplane that we're controlling
		/// </summary>
		private AeroplaneController _airplane;

        private void Awake()
        {
            // Set up the reference to the aeroplane controller.
            _airplane = GetComponent<AeroplaneController>();
        }

        IEnumerator Start()
        {
            // Disable aerodynamic effect at start to prevent the plane from jitter on ground.
            float aerodynamicEffect = _airplane.AerodynamicEffect;
            _airplane.AerodynamicEffect = 0f;

            yield return new WaitForSeconds(3);

            // Enable aerodynamic effect.
            _airplane.AerodynamicEffect = aerodynamicEffect;
        }

        private void FixedUpdate()
        {
            // Read input for the pitch, yaw, roll and throttle of the aeroplane.
            float mousePitch = ControlsPrefs.IsMouseEnabled ? CrossPlatformInputManager.GetAxis("Mouse Y") : 0;
            float mouseRoll = ControlsPrefs.IsMouseEnabled ? CrossPlatformInputManager.GetAxis("Mouse X") : 0;

            // Read inputs. They are clamped in AeroplaneController, so can go out of [-1, 1] here.
            float roll = ControlsPrefs.IsRollEnabled? CrossPlatformInputManager.GetAxis("Roll") + mouseRoll : 0;
            float pitch = (ControlsPrefs.IsInversePitch ? -1f : 1f) * CrossPlatformInputManager.GetAxis("Pitch") + mousePitch;
            float yaw = CrossPlatformInputManager.GetAxis("Yaw");
            bool airBrakes = CrossPlatformInputManager.GetButton("Brakes");

            // auto throttle up, or down if braking.
            float throttle = airBrakes ? -1 : 1;
#if MOBILE_INPUT
            // Roll is always on on mobile.
            roll = CrossPlatformInputManager.GetAxis("Roll");
            AdjustInputForMobileControls(ref roll, ref pitch, ref throttle);
#endif
            // Pass the input to the aeroplane
            _airplane.Move(roll, pitch, yaw, throttle, airBrakes);
        }

        private void AdjustInputForMobileControls(ref float roll, ref float pitch, ref float throttle)
        {
            float intendedRollAngle = roll * maxRollAngle * Mathf.Deg2Rad;
            float intendedPitchAngle = pitch * maxPitchAngle * Mathf.Deg2Rad;
            roll = Mathf.Clamp((intendedRollAngle - _airplane.RollAngle), -1, 1);
            pitch = Mathf.Clamp((intendedPitchAngle - _airplane.PitchAngle), -1, 1);
            float intendedThrottle = throttle * 0.5f + 0.5f;
            throttle = Mathf.Clamp(intendedThrottle - _airplane.Throttle, -1, 1);
        }
    }

}

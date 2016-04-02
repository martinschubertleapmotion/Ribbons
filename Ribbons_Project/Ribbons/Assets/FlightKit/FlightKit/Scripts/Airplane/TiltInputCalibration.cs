using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace FlightKit
{
	/// <summary>
	/// Calibrates the vertical axis of tilt input of airplane so that the horizont position 
	/// can be set as it's comfortable for the user.
	/// </summary>
    public class TiltInputCalibration : MonoBehaviour {
        /// <summary>
        /// Which TiltInput to calibrate.
        /// </summary>
        public TiltInput calibrationTarget;
        
		/// <summary>
		/// GameObject that contains UI that lets the user know calibration is in progress.
		/// </summary>
        public GameObject calibrationPopup;

        /// <summary>
        /// Delay before calibration after the user presses Play button.
        /// </summary>
        public float delayAfterStartPlay = 8f;

        void OnEnable()
        {
            UIEventsPublisher.OnPlayEvent += CalibrateDelayed;
            PauseController.OnUnPauseEvent += Calibrate;
        }

        void OnDisable()
        {
            UIEventsPublisher.OnPlayEvent -= CalibrateDelayed;
            PauseController.OnUnPauseEvent -= Calibrate;
        }

		/// <summary>
		/// Calibrate after a pause defined in delayAfterStartPlay.
		/// </summary>
		public virtual void CalibrateDelayed()
        {
            // Calibration is done only for tilt input.
            if (ControlsPrefs.IsTiltEnabled)
            {
                StartCoroutine(CalibrateCoroutine(delayAfterStartPlay));
            }
        }

		/// <summary>
		/// Calibrate the vertical axis offset.
		/// </summary>
        public virtual void Calibrate()
        {
            // Calibration is done only for tilt input.
            if (ControlsPrefs.IsTiltEnabled)
            {
                StartCoroutine(CalibrateCoroutine());
            }
        }

        private IEnumerator CalibrateCoroutine(float delay = 0)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            // Show popup.
            if (calibrationPopup != null)
            {
                calibrationPopup.SetActive(true);
            }

            // Wait a bit.
            yield return new WaitForSeconds(3f);
            
            if (calibrationTarget == null)
            {
                yield break;
            }

            // Actual calibration.
            float currentAngle = 0;
            if (Input.acceleration != Vector3.zero)
            {
                switch (calibrationTarget.tiltAroundAxis)
                {
                    case TiltInput.AxisOptions.ForwardAxis:
                        currentAngle = Mathf.Atan2(Input.acceleration.x, -Input.acceleration.y)*Mathf.Rad2Deg;
                        break;
                    case TiltInput.AxisOptions.SidewaysAxis:
                        currentAngle = Mathf.Atan2(Input.acceleration.z, -Input.acceleration.y)*Mathf.Rad2Deg;
                        break;
                }
            }
            
            calibrationTarget.centreAngleOffset = -currentAngle;

            // Remove popup.
            if (calibrationPopup != null)
            {
                calibrationPopup.SetActive(false);
            }
        }
    }
}
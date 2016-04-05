using UnityEngine;
using UnityEngine.UI;

namespace FlightKit
{
    public class ControlsSetting : MonoBehaviour
    {
        [Header("Standalone controls UI (Leave empty if not targeting standalone):")]
        public Toggle classicControls;
        public Toggle mouseControls;
        public Toggle casualControls;

        [Space]
        public Toggle inversePitchStandalone;

        [Header("Mobile controls UI (Leave empty if not targeting mobile):")]
        public Toggle touchControls;
        public Toggle tiltControls;

        [Space]
        public Toggle inversePitchMobile;

        void Start()
        {
            // Activate the correct control toggle (standalone).
            if (ControlsPrefs.IsMouseEnabled)
            {
                 if (mouseControls) mouseControls.isOn = true;
            }
            else if (ControlsPrefs.IsRollEnabled)
            {
                if (classicControls) classicControls.isOn = true;
            }
            else
            {
                if (casualControls) casualControls.isOn = true;
            }

            if (inversePitchStandalone)
            {
                inversePitchStandalone.isOn = ControlsPrefs.IsInversePitch;
            }

            // Activate the correct control toggle (mobile).
            if (ControlsPrefs.IsTiltEnabled)
            {
                 if (tiltControls) tiltControls.isOn = true;
            }
            else
            {
                if (touchControls) touchControls.isOn = true;
            }

            if (inversePitchMobile)
            {
                inversePitchMobile.isOn = ControlsPrefs.IsInversePitch;
            }
        }

        public virtual void OnRollEnabledChanged(bool activated)
        {
            ControlsPrefs.IsRollEnabled = activated;
        }

        public virtual void OnMouseEnabledChanged(bool activated)
        {
            ControlsPrefs.IsMouseEnabled = activated;
            if (activated)
            {
                ControlsPrefs.IsRollEnabled = true;
            }
        }

        public virtual void OnInversePitchChanged(bool activated)
        {
            ControlsPrefs.IsInversePitch = activated;
        }

        public virtual void OnTiltEnabledChanged(bool activated)
        {
            ControlsPrefs.IsTiltEnabled = activated;
        }

    }

}

using UnityEngine;

namespace FlightKit
{
	/// <summary>
	/// Controls persistance of user's preferred controls scheme.
	/// </summary>
    public class ControlsPrefs
    {
		/// <summary>
		/// This event is fired once user changes the mobile controls to tilt. Used internally to switch controls schemes.
		/// </summary>
		public static event GameActions.SimpleAction OnTiltEnabledEvent;

		/// <summary>
		/// This event is fired once user changes the mobile controls to touch. Used internally to switch controls schemes.
		/// </summary>
		public static event GameActions.SimpleAction OnTiltDisabledEvent;

        /// <summary>
        /// Whether roll of airplane is enabled.
        /// </summary>
        /// <returns></returns>
        public static bool IsRollEnabled
        {
            get { return _isRollEnabled; }
            set
            {
                _isRollEnabled = value;

                PlayerPrefs.SetInt(PREF_KEY_ROLL_ENABLED, value? 1 : 0);
                PlayerPrefs.Save();
            }
        }
        private static bool _isRollEnabled;

        /// <summary>
        /// Whether steering with mouse is enabled.
        /// </summary>
        /// <returns></returns>
        public static bool IsMouseEnabled
        {
            get { return _isMouseEnabled; }
            set
            {
                _isMouseEnabled = value;
                PlayerPrefs.SetInt(PREF_KEY_MOUSE_ENABLED, value? 1 : 0);
                PlayerPrefs.Save();
            }
        }
        private static bool _isMouseEnabled;

        /// <summary>
        /// Whether steering with tilt on mobile is enabled.
        /// </summary>
        /// <returns></returns>
        public static bool IsTiltEnabled
        {
            get { return _isTiltEnabled; }
            set
            {
                _isTiltEnabled = value;
                PlayerPrefs.SetInt(PREF_KEY_TILT_ENABLED, value? 1 : 0);
                PlayerPrefs.Save();

                if (value)
                {
                    if (OnTiltEnabledEvent != null)
                    {
                        OnTiltEnabledEvent();
                    }
                }
                else
                {
                    if (OnTiltDisabledEvent != null)
                    {
                        OnTiltDisabledEvent();
                    }
                }
            }
        }
        private static bool _isTiltEnabled;

        /// <summary>
        /// Vertical axes of controls may be inverted.
        /// </summary>
        /// <returns></returns>
        public static bool IsInversePitch
        {
            get { return _isInversePitch; }
            set
            {
                _isInversePitch = value;
                PlayerPrefs.SetInt(PREF_KEY_INVERSE_PITCH, value? 1 : 0);
                PlayerPrefs.Save();
            }
        }
        private static bool _isInversePitch;

        private static string PREF_KEY_ROLL_ENABLED = "FlightControls_RollEnabled";
        private static string PREF_KEY_MOUSE_ENABLED = "FlightControls_MouseEnabled";
        private static string PREF_KEY_TILT_ENABLED = "FlightControls_TiltEnabled";
        private static string PREF_KEY_INVERSE_PITCH = "FlightControls_InversePitch";

		/// <summary>
		/// Static constructor.
		/// </summary>
        static ControlsPrefs()
        {
            // If the controls have not been initialized in previous games.
            if (!PlayerPrefs.HasKey(PREF_KEY_ROLL_ENABLED))
            {
                // Default settings.
                IsRollEnabled = true;
                IsMouseEnabled = false;
                IsTiltEnabled = true;
                IsInversePitch = false;
            }
            else
            {
                IsRollEnabled = PlayerPrefs.GetInt(PREF_KEY_ROLL_ENABLED) == 1;
                IsMouseEnabled = PlayerPrefs.GetInt(PREF_KEY_MOUSE_ENABLED) == 1;
                IsTiltEnabled = PlayerPrefs.GetInt(PREF_KEY_TILT_ENABLED) == 1;
                IsInversePitch = PlayerPrefs.GetInt(PREF_KEY_INVERSE_PITCH) == 1;
            }
        }
    }
}

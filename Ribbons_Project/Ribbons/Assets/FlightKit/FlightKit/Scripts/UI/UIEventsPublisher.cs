using UnityEngine;

namespace FlightKit
{
	/// <summary>
	/// Publishes OnPlayEvent when user starts playing a level.
	/// </summary>
	public class UIEventsPublisher : MonoBehaviour {
        public static event GameActions.SimpleAction OnPlayEvent;

        public virtual void PublishPlay()
        {
            if (OnPlayEvent != null)
            {
                OnPlayEvent();
            }
        }

    }
}
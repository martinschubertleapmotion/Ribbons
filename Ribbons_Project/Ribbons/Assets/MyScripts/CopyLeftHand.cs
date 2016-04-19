using UnityEngine;
using System.Collections;

public class CopyLeftHand : MonoBehaviour {

		public GameObject ObjectToCopy; // This you assign in the inspector
        public Transform PlaceToCopy;

		void Start( )
		{
            var clone = Instantiate(ObjectToCopy, PlaceToCopy.position, PlaceToCopy.rotation) as GameObject;
            clone.name = "FrozenLeftHand";
            clone.SetActive(true);
		}
	}
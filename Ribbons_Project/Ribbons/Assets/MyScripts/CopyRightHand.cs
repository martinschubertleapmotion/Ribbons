using UnityEngine;
using System.Collections;

public class CopyRightHand : MonoBehaviour {

		public GameObject ObjectToCopy; // This you assign in the inspector
        public Transform PlaceToCopy;

		public void Start( )
		{
        var clone = Instantiate(ObjectToCopy, PlaceToCopy.position, PlaceToCopy.rotation) as GameObject;
        clone.name = "FrozenRightHand";
        clone.SetActive(true);
		}
	}
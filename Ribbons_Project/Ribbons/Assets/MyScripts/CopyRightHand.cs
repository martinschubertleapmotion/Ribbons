using UnityEngine;
using System.Collections;

public class CopyRightHand : MonoBehaviour {

		public GameObject ObjectToCopy; // This you assign in the inspector
		public Transform NewParent; 

		public void Start( )
		{
		Instantiate(ObjectToCopy, transform.position, transform.rotation);
		ObjectToCopy.gameObject.name = "FrozenRightHand";
		ObjectToCopy.gameObject.SetActive(true);
		ObjectToCopy.transform.SetParent (NewParent);
		}
	}
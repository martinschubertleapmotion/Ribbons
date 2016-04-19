using UnityEngine;
using System.Collections;

public class CopyLeftHand : MonoBehaviour {

		public GameObject ObjectToCopy; // This you assign in the inspector
		public Transform NewParent;  

		void Start( )
		{
		Instantiate(ObjectToCopy, transform.position, transform.rotation);
		ObjectToCopy.gameObject.name = "FrozenLeftHand";
		ObjectToCopy.gameObject.SetActive(true);
		ObjectToCopy.transform.SetParent (NewParent);
		}
	}
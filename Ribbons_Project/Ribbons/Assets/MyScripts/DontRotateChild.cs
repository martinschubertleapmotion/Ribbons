using UnityEngine;
using System.Collections;

public class DontRotateChild : MonoBehaviour {


	void Update() {
		transform.rotation = Quaternion.identity;
	}
}
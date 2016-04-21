using UnityEngine;
using System.Collections;

public class SlingShotLine : MonoBehaviour {
	public Material Material; 
	public Color c1 = Color.yellow;
	public Color c2 = Color.red;
	public int lengthOfLineRenderer = 2;
	public GameObject Ball;
	public GameObject RightFinger;

	void Start() {
		LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = Material;
		lineRenderer.SetColors(c1, c2);
		lineRenderer.SetWidth(0.01F, 0.01F);
		lineRenderer.SetVertexCount(lengthOfLineRenderer);
	}
	void Update() {
		LineRenderer lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.SetPosition(0, Ball.transform.position);
		lineRenderer.SetPosition (1, RightFinger.transform.position);
	}
}
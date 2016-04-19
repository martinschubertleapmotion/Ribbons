using UnityEngine;
using System.Collections;
using Obi;

[RequireComponent(typeof(ObiSolver))]
public class CollisionEventHandler : MonoBehaviour {

 	ObiSolver solver;

	void Awake(){
		solver = GetComponent<Obi.ObiSolver>();
	}

	void OnEnable () {
		solver.OnCollision += Solver_OnCollision;
	}

	void OnDisable(){
		solver.OnCollision -= Solver_OnCollision;
	}
	
	void Solver_OnCollision (object sender, Obi.ObiSolver.ObiCollisionEventArgs e)
	{
		for(int i = 0;  i < e.points.Length; ++i){
			if (e.distances[i] <= 0.001f){
				//Debug.DrawRay(e.points[i],e.normals[i],Color.white);
				Debug.DrawRay(e.points[i],e.normals[i]*e.normalImpulses[i]*10,Color.red);
				Debug.DrawRay(e.points[i],e.normals[i]*e.tangentImpulses[i]*10,Color.blue);
				//Debug.DrawRay(e.points[i],e.stickImpulses[i],Color.green);
			}
		}
	}

}

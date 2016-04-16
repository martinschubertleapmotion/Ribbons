using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi{

/**
 * Holds information about distance constraints for an actor.
 */
[DisallowMultipleComponent]
public class ObiDistanceConstraints : ObiConstraints
{

	[Range(0.1f,2)]
	[Tooltip("Scale of stretching constraints. Values > 1 will expand initial cloth size, values < 1 will make it shrink.")]
	public float stretchingScale = 1;				/**< Stiffness of structural spring constraints.*/
	
	[Range(0,1)]
	[Tooltip("Cloth resistance to stretching. Lower values will yield more elastic cloth.")]
	public float stretchingStiffness = 1;		   /**< Resistance of structural spring constraints to stretch..*/
	
	[Range(0,1)]
	[Tooltip("Cloth resistance to compression. Lower values will yield more elastic cloth.")]
	public float compressionStiffness = 1;		   /**< Resistance of structural spring constraints to compression.*/
		
	[HideInInspector] public List<int> springIndices = new List<int>();					/**< Distance constraint indices.*/
	[HideInInspector] public List<float> restLengths = new List<float>();				/**< Rest distances.*/
	[HideInInspector] public List<Vector2> stiffnesses = new List<Vector2>();			/**< Stiffnesses of distance constraits.*/
	[HideInInspector] public List<float> stretching = new List<float>();				/**< Constraint stretching percentage.*/

	public override void Initialize(){
		activeStatus.Clear();
		springIndices.Clear();
		restLengths.Clear();
		stiffnesses.Clear();	
		stretching.Clear();
	}

	public void AddConstraint(bool active, int index1, int index2, float restLength, float stretchStiffness, float compressionStiffness){

		if (InSolver){
			Debug.LogError("You need to remove the constraints from the solver before attempting to add new individual constraints.");
			return;
		}

		activeStatus.Add(active);
		springIndices.Add(index1);
		springIndices.Add(index2);
		restLengths.Add(restLength);
        stiffnesses.Add(new Vector2(stretchStiffness,compressionStiffness));
        stretching.Add(0);
	}
	
	public override List<int> GetConstraintsInvolvingParticle(int particleIndex){
	
		List<int> constraints = new List<int>();
		
		for (int i = 0; i < stretching.Count; i++){
			if (springIndices[i*2] == particleIndex || springIndices[i*2+1] == particleIndex) 
				constraints.Add(i);
		}
		
		return constraints;
	}

	protected override void OnAddToSolver(object info){

		ObiSolver solver = actor.solver;

		// Set solver constraint data:
		int[] solverIndices = new int[springIndices.Count];
		for (int i = 0; i < restLengths.Count; i++)
		{
			solverIndices[i*2] = actor.particleIndices[springIndices[i*2]];
			solverIndices[i*2+1] = actor.particleIndices[springIndices[i*2+1]];
		}
		
		indicesOffset = actor.solver.distanceConstraints.restLengths.Length;
		ObiUtils.AddRange(ref solver.distanceConstraints.springIndices,solverIndices);
		ObiUtils.AddRange(ref solver.distanceConstraints.restLengths,restLengths.ToArray());
		ObiUtils.AddRange(ref solver.distanceConstraints.stiffnesses,stiffnesses.ToArray());
		ObiUtils.AddRange(ref solver.distanceConstraints.stretching,stretching.ToArray());

	}

	protected override void OnRemoveFromSolver(object info){

		ObiSolver solver = actor.solver;
		
		// Update following actors' indices:
		for (int i = actor.actorID+1; i < solver.actors.Count; i++){
			ObiDistanceConstraints dc = solver.actors[i].GetComponent<ObiDistanceConstraints>();
			if (dc != null)
				dc.UpdateIndicesOffset(dc.indicesOffset - restLengths.Count);
        }
		
		ObiUtils.RemoveRange(ref solver.distanceConstraints.springIndices,indicesOffset*2,restLengths.Count*2);
		ObiUtils.RemoveRange(ref solver.distanceConstraints.restLengths,indicesOffset,restLengths.Count);
		ObiUtils.RemoveRange(ref solver.distanceConstraints.stiffnesses,indicesOffset,restLengths.Count);
		ObiUtils.RemoveRange(ref solver.distanceConstraints.stretching,indicesOffset,restLengths.Count);

	}

	protected override void OnAddedToSolver(object info){
		PushDataToSolver(new ObiSolverData(ObiSolverData.DistanceConstraintsData.ALL));
	}

	public override void PushDataToSolver(ObiSolverData data){ 

		if (actor == null || !actor.InSolver)
			return;

		if ((data.distanceConstraintsData & ObiSolverData.DistanceConstraintsData.DISTANCE_REST_LENGHTS) != 0){

			if (actor != null && actor.solver != null){

				for (int i = 0; i < restLengths.Count; i++){
					actor.solver.distanceConstraints.restLengths[indicesOffset+i] = restLengths[i]*stretchingScale;
				}
			}
		}

		if ((data.distanceConstraintsData & ObiSolverData.DistanceConstraintsData.DISTANCE_STIFFNESSES) != 0){

			for (int i = 0; i < stiffnesses.Count; i++){
				stiffnesses[i] = new Vector2(stretchingStiffness,compressionStiffness);
			}
	
			if (actor != null && actor.solver != null && stretching != null){
				Array.Copy(stiffnesses.ToArray(),0,actor.solver.distanceConstraints.stiffnesses,indicesOffset,stiffnesses.Count);
			}
		}

		if ((data.distanceConstraintsData & ObiSolverData.DistanceConstraintsData.ACTIVE_STATUS) != 0){
			UpdateConstraintActiveStatus();
		}

	}

	public override void PullDataFromSolver(ObiSolverData data){
		if (actor != null && actor.solver != null && stretching != null){
		
			if ((data.distanceConstraintsData & ObiSolverData.DistanceConstraintsData.DISTANCE_STRETCH) != 0){
				float[] stretchArray = new float[stretching.Count];
				Array.Copy(actor.solver.distanceConstraints.stretching,indicesOffset,stretchArray,0,stretching.Count);
				stretching = new List<float>(stretchArray);
			}
		}
	}	

}
}

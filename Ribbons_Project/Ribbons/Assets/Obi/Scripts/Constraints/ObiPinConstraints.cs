using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi{

/**
 * Holds information about pin constraints for an actor.
 */
[DisallowMultipleComponent]
public class ObiPinConstraints : ObiConstraints
{
	
	[Range(0,1)]
	[Tooltip("Pin resistance to stretching. Lower values will yield more elastic pin constraints.")]
	public float stiffness = 1;		   /**< Resistance of structural spring constraints to stretch..*/
	
	[HideInInspector] public List<int> pinParticleIndices = new List<int>();			/**< Pin constraint indices.*/
	[HideInInspector] public List<Collider> pinBodies = new List<Collider>();			/**< Stiffnesses of pin constraits.*/
	[HideInInspector] public List<Vector3> pinOffsets = new List<Vector3>();			/**< Offset expressed in the attachment's local space.*/
	[HideInInspector] public List<float> stiffnesses = new List<float>();				/**< Stiffnesses of pin constraits.*/

	public override void Initialize(){
		activeStatus.Clear();
		pinParticleIndices.Clear();
		pinBodies.Clear();
		pinOffsets.Clear();
		stiffnesses.Clear();	
	}

	public void AddConstraint(bool active, int index1, Collider body, Vector3 offset, float stiffness){

		if (InSolver){
			Debug.LogError("You need to remove the constraints from the solver before attempting to add new individual constraints.");
			return;
		}

		activeStatus.Add(active);
		pinParticleIndices.Add(index1);
		pinBodies.Add(body);
		pinOffsets.Add(offset);
        stiffnesses.Add(stiffness);

	}

	public void RemoveConstraint(int index){

		if (index >= 0 && index < pinOffsets.Count){
			activeStatus.RemoveAt(index);
			pinParticleIndices.RemoveAt(index);
			pinBodies.RemoveAt(index);
			pinOffsets.RemoveAt(index);
			stiffnesses.RemoveAt(index);
		}

	}
	
	public override List<int> GetConstraintsInvolvingParticle(int particleIndex){
	
		List<int> constraints = new List<int>();
		
		for (int i = 0; i < pinOffsets.Count; i++){
			if (pinParticleIndices[i*2] == particleIndex) 
				constraints.Add(i);
		}
		
		return constraints;
	}

	protected override void OnAddToSolver(object info){

		ObiSolver solver = actor.solver;

		// Set solver constraint data:
		int[] solverIndices = new int[pinParticleIndices.Count*2];
		for (int i = 0; i < pinOffsets.Count; i++)
		{
			solverIndices[i*2] = actor.particleIndices[pinParticleIndices[i]];
			
			if (actor.solver.colliderGroup != null) 
				solverIndices[i*2+1] = actor.solver.colliderGroup.GetIndexOfCollider(pinBodies[i]);
			else
				solverIndices[i*2+1] = -1;
		}
		
		indicesOffset = actor.solver.pinConstraints.pinOffsets.Length;
		ObiUtils.AddRange(ref solver.pinConstraints.pinIndices,solverIndices);
		ObiUtils.AddRange(ref solver.pinConstraints.pinOffsets,pinOffsets.ToArray());
		ObiUtils.AddRange(ref solver.pinConstraints.stiffnesses,stiffnesses.ToArray());
	
	}

	protected override void OnRemoveFromSolver(object info){

		ObiSolver solver = actor.solver;
		
		// Update following actors' indices:
		for (int i = actor.actorID+1; i < solver.actors.Count; i++){
			ObiPinConstraints pc = solver.actors[i].GetComponent<ObiPinConstraints>();
			if (pc != null)
				pc.UpdateIndicesOffset(pc.indicesOffset - pinOffsets.Count);
        }
		
		ObiUtils.RemoveRange(ref solver.pinConstraints.pinIndices,indicesOffset*2,pinOffsets.Count*2);
		ObiUtils.RemoveRange(ref solver.pinConstraints.pinOffsets,indicesOffset,pinOffsets.Count);
		ObiUtils.RemoveRange(ref solver.pinConstraints.stiffnesses,indicesOffset,pinOffsets.Count);

	}

	protected override void OnAddedToSolver(object info){
		PushDataToSolver(new ObiSolverData(ObiSolverData.PinConstraintsData.ALL));
	}

	public override void PushDataToSolver(ObiSolverData data){ 

		if (actor == null || !actor.InSolver)
			return;

		if ((data.pinConstraintsData & ObiSolverData.PinConstraintsData.PIN_STIFFNESSES) != 0){

			for (int i = 0; i < stiffnesses.Count; i++){
				stiffnesses[i] = stiffness;
			}
	
			if (actor != null && actor.solver != null){
				Array.Copy(stiffnesses.ToArray(),0,actor.solver.pinConstraints.stiffnesses,indicesOffset,stiffnesses.Count);
			}
		}

		if ((data.pinConstraintsData & ObiSolverData.PinConstraintsData.ACTIVE_STATUS) != 0){
			UpdateConstraintActiveStatus();
		}

	}

}
}

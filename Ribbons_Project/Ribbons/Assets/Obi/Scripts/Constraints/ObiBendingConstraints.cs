using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi{
	
	/**
 	* Holds information about bending constraints for an actor.
 	*/
	[DisallowMultipleComponent]
	public class ObiBendingConstraints : ObiConstraints
	{
		
		[Tooltip("Bending offset. Leave at zero to keep the original bending amount.")]
		public float maxBending = 0;				/**< Stiffness of structural spring constraints.*/
		
		[Range(0,1)]
		[Tooltip("Cloth resistance to bending. Higher values will yield more stiff cloth.")]
		public float stiffness = 1;		   /**< Resistance of structural spring constraints to stretch..*/
		
		[HideInInspector] public List<int> bendingIndices = new List<int>();				/**< Distance constraint indices.*/
		[HideInInspector] public List<float> restBends = new List<float>();					/**< Rest distances.*/
		[HideInInspector] public List<Vector2> bendingStiffnesses = new List<Vector2>();	/**< Bend offsets and stiffnesses of distance constraits.*/

		public override void Initialize(){
			activeStatus.Clear();
			bendingIndices.Clear();
			restBends.Clear();
			bendingStiffnesses.Clear();
		}
		
		public void AddConstraint(bool active, int index1, int index2, int index3, float restBend, float bending, float stiffness){

			if (InSolver){
				Debug.LogError("You need to remove the constraints from the solver before attempting to add new individual constraints.");
				return;
			}

			activeStatus.Add(active);
			bendingIndices.Add(index1);
			bendingIndices.Add(index2);
			bendingIndices.Add(index3);
			restBends.Add(restBend);
			bendingStiffnesses.Add(new Vector2(bending,stiffness));
		}
		
		public override List<int> GetConstraintsInvolvingParticle(int particleIndex){
			List<int> constraints = new List<int>();
			
			for (int i = 0; i < restBends.Count; i++){
				if (bendingIndices[i*3] == particleIndex || 
					bendingIndices[i*3+1] == particleIndex || 
					bendingIndices[i*3+2] == particleIndex) 
					constraints.Add(i);
			}
			
			return constraints;
		}
		
		protected override void OnAddToSolver(object info){
			
			ObiSolver solver = actor.solver;
			
			// Set solver constraint data:
			int[] solverIndices = new int[bendingIndices.Count];
			for (int i = 0; i < restBends.Count; i++)
			{
				solverIndices[i*3] = actor.particleIndices[bendingIndices[i*3]];
				solverIndices[i*3+1] = actor.particleIndices[bendingIndices[i*3+1]];
				solverIndices[i*3+2] = actor.particleIndices[bendingIndices[i*3+2]];
			}
			
			indicesOffset = actor.solver.bendingConstraints.restBends.Length;
			ObiUtils.AddRange(ref solver.bendingConstraints.bendingIndices,solverIndices);
			ObiUtils.AddRange(ref solver.bendingConstraints.restBends,restBends.ToArray());
			ObiUtils.AddRange(ref solver.bendingConstraints.bendingStiffnesses,bendingStiffnesses.ToArray());
			
		}
		
		protected override void OnRemoveFromSolver(object info){
			
			ObiSolver solver = actor.solver;
			
			// subtract our amount of constraints from other actor's offsets:
			for (int i = actor.actorID+1; i < solver.actors.Count; i++){
				ObiBendingConstraints bc = solver.actors[i].GetComponent<ObiBendingConstraints>();
				if (bc != null)
					bc.UpdateIndicesOffset(bc.indicesOffset - restBends.Count);
			}
			
			ObiUtils.RemoveRange(ref solver.bendingConstraints.bendingIndices,indicesOffset*3,restBends.Count*3);
			ObiUtils.RemoveRange(ref solver.bendingConstraints.restBends,indicesOffset,restBends.Count);
			ObiUtils.RemoveRange(ref solver.bendingConstraints.bendingStiffnesses,indicesOffset,restBends.Count);
			
		}

		protected override void OnAddedToSolver(object info){
			PushDataToSolver(new ObiSolverData(ObiSolverData.BendingConstraintsData.ALL));
		}
		
		public override void PushDataToSolver(ObiSolverData data){ 
			
			if (actor == null || !actor.InSolver)
				return;
			
			if ((data.bendingConstraintsData & ObiSolverData.BendingConstraintsData.BENDING_STIFFNESSES) != 0){
				
				for (int i = 0; i < bendingStiffnesses.Count; i++){
					bendingStiffnesses[i] = new Vector2(maxBending,stiffness);
				}
				
				if (actor != null && actor.solver != null && bendingStiffnesses != null){
					Array.Copy(bendingStiffnesses.ToArray(),0,actor.solver.bendingConstraints.bendingStiffnesses,indicesOffset,bendingStiffnesses.Count);
				}
			}
			
			if ((data.bendingConstraintsData & ObiSolverData.BendingConstraintsData.ACTIVE_STATUS) != 0){
				UpdateConstraintActiveStatus();
			}
			
		}

	}
}


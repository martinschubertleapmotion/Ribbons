using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi{
	
	/**
 	* Holds information about aerodynamic constraints for an actor.
 	*/
	[DisallowMultipleComponent]
	public class ObiAerodynamicConstraints : ObiConstraints
	{
		
		[Tooltip("Direction and magnitude of wind in word space.")]
		public Vector3 windVector = Vector3.zero;				/**< Wind force vector expressed in world space.*/
		
		[Tooltip("Air density in kg/m3. Higher densities will make both drag and lift forces stronger.")]
		public float airDensity = 1.225f;
		
		[Tooltip("How much is the cloth affected by drag forces. Extreme values can cause the cloth to behave unrealistically, so use with care.")]
		public float dragCoefficient = 0.05f;
		
		[Tooltip("How much is the cloth affected by lift forces. Extreme values can cause the cloth to behave unrealistically, so use with care.")]
		public float liftCoefficient = 0.05f;
		
		[HideInInspector] public List<int> aerodynamicIndices = new List<int>();				/**< particle indices.*/
		[HideInInspector] public List<Vector3> aerodynamicNormals = new List<Vector3>();		/**< particle normals.*/
		[HideInInspector] public List<Vector3> wind = new List<Vector3>();						/**< Per-particle wind vector.*/
		[HideInInspector] public List<float> aerodynamicCoeffs = new List<float>();				/**< Per-particle aerodynamic coeffs.*/

		public override void Initialize(){
			activeStatus.Clear();
			aerodynamicNormals.Clear();
			aerodynamicIndices.Clear();
			wind.Clear();
			aerodynamicCoeffs.Clear();
		}
		
		public void AddConstraint(bool active, int index, Vector3 normal, Vector3 wind, float area, float drag, float lift){

			if (InSolver){
				Debug.LogError("You need to remove the constraints from the solver before attempting to add new individual constraints.");
				return;
			}

			activeStatus.Add(active);
			aerodynamicIndices.Add(index);
			aerodynamicNormals.Add(normal);
			this.wind.Add(wind);
			aerodynamicCoeffs.Add(area);
			aerodynamicCoeffs.Add(drag);
			aerodynamicCoeffs.Add(lift);
		}
		
		public override List<int> GetConstraintsInvolvingParticle(int particleIndex){
		
			List<int> constraints = new List<int>();
			
			for (int i = 0; i < wind.Count; i++){
				if (aerodynamicIndices[i] == particleIndex)
					constraints.Add(i);
			}
			
			return constraints;
		}
		
		protected override void OnAddToSolver(object info){
		
			ObiSolver solver = actor.solver;
			
			// Set solver constraint data:
			int[] solverIndices = new int[aerodynamicIndices.Count];
			for (int i = 0; i < aerodynamicNormals.Count; i++)
			{
				solverIndices[i] = actor.particleIndices[aerodynamicIndices[i]];
			}

			indicesOffset = actor.solver.aerodynamicConstraints.aerodynamicNormals.Length;
			ObiUtils.AddRange(ref solver.aerodynamicConstraints.aerodynamicIndices,solverIndices);
			ObiUtils.AddRange(ref solver.aerodynamicConstraints.aerodynamicNormals,aerodynamicNormals.ToArray());
			ObiUtils.AddRange(ref solver.aerodynamicConstraints.wind,wind.ToArray());
			ObiUtils.AddRange(ref solver.aerodynamicConstraints.aerodynamicCoeffs,aerodynamicCoeffs.ToArray());
			
		}
		
		protected override void OnRemoveFromSolver(object info){
			
			ObiSolver solver = actor.solver;
				
			// subtract our amount of constraints from other actor's offsets:
			for (int i = actor.actorID+1; i < solver.actors.Count; i++){
				ObiAerodynamicConstraints ac = solver.actors[i].GetComponent<ObiAerodynamicConstraints>();
				if (ac != null)
					ac.UpdateIndicesOffset(ac.indicesOffset - aerodynamicNormals.Count);
			}
			
			ObiUtils.RemoveRange(ref solver.aerodynamicConstraints.aerodynamicIndices,indicesOffset,aerodynamicIndices.Count);
			ObiUtils.RemoveRange(ref solver.aerodynamicConstraints.aerodynamicNormals,indicesOffset,aerodynamicNormals.Count);
			ObiUtils.RemoveRange(ref solver.aerodynamicConstraints.wind,indicesOffset,wind.Count);
			ObiUtils.RemoveRange(ref solver.aerodynamicConstraints.aerodynamicCoeffs,indicesOffset*3,aerodynamicCoeffs.Count);
		}

		protected override void OnAddedToSolver(object info){
			PushDataToSolver(new ObiSolverData(ObiSolverData.AerodynamicConstraintsData.ALL));
		}
		
		public override void PushDataToSolver(ObiSolverData data){ 
			
			if (actor == null || !actor.InSolver)
				return;

			if ((data.aerodynamicConstraintsData & ObiSolverData.AerodynamicConstraintsData.AERODYNAMIC_NORMALS) != 0){
				if (actor != null && actor.solver != null && aerodynamicNormals != null){
					Array.Copy(aerodynamicNormals.ToArray(),0,actor.solver.aerodynamicConstraints.aerodynamicNormals,indicesOffset,aerodynamicNormals.Count);
				}
			}
			
			if ((data.aerodynamicConstraintsData & ObiSolverData.AerodynamicConstraintsData.WIND) != 0){
				
				for (int i = 0; i < wind.Count; i++){
					wind[i] = windVector;
				}
				
				if (actor != null && actor.solver != null && wind != null){
					Array.Copy(wind.ToArray(),0,actor.solver.aerodynamicConstraints.wind,indicesOffset,wind.Count);
				}
			}
			
			if ((data.aerodynamicConstraintsData & ObiSolverData.AerodynamicConstraintsData.AERODYNAMIC_COEFFS) != 0){
				
				for (int i = 0; i < aerodynamicCoeffs.Count; i+=3){
					aerodynamicCoeffs[i+1] = dragCoefficient * airDensity;
					aerodynamicCoeffs[i+2] = liftCoefficient * airDensity;
				}
				
				if (actor != null && actor.solver != null && aerodynamicCoeffs != null){
					Array.Copy(aerodynamicCoeffs.ToArray(),0,actor.solver.aerodynamicConstraints.aerodynamicCoeffs,indicesOffset*3,aerodynamicCoeffs.Count);
				}
			}
			
			if ((data.aerodynamicConstraintsData & ObiSolverData.AerodynamicConstraintsData.ACTIVE_STATUS) != 0){
				UpdateConstraintActiveStatus();
			}
			
		}
	
	}
}





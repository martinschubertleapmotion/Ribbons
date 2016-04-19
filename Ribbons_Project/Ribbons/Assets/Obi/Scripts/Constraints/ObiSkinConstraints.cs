using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi{
	
	/**
 	* Holds information about skin constraints for an actor.
 	*/
	[DisallowMultipleComponent]
	public class ObiSkinConstraints : ObiConstraints 
	{
		
		[Range(0,1)]
		[Tooltip("Skin constraints stiffness.")]
		public float stiffness = 1;		   /**< Resistance of structural spring constraints to stretch..*/
		
		[HideInInspector] public List<int> skinIndices = new List<int>();						/**< Distance constraint indices.*/
		[HideInInspector] public List<Vector3> skinPoints = new List<Vector3>();				/**< Skin constraint anchor points, in world space.*/
		[HideInInspector] public List<Vector3> skinNormals = new List<Vector3>();				/**< Rest distances.*/
		[HideInInspector] public List<float> skinRadiiBackstop = new List<float>();				/**< Rest distances.*/
		[HideInInspector] public List<float> skinStiffnesses = new List<float>();				/**< Stiffnesses of distance constraits.*/
		
		public override void Initialize(){
			activeStatus.Clear();
			skinIndices.Clear();
			skinPoints.Clear();
			skinNormals.Clear();	
			skinRadiiBackstop.Clear();
			skinStiffnesses.Clear();
		}

		public void AddConstraint(bool active, int index, Vector3 point, Vector3 normal, float radius, float backstop, float stiffness){
			
			if (InSolver){
				Debug.LogError("You need to remove the constraints from the solver before attempting to add new individual constraints.");
				return;
			}

			activeStatus.Add(active);
			skinIndices.Add(index);
			skinPoints.Add(point);
			skinNormals.Add(normal);
			skinRadiiBackstop.Add(radius);
			skinRadiiBackstop.Add(backstop);
			skinStiffnesses.Add(stiffness);
		}
		
		public override List<int> GetConstraintsInvolvingParticle(int particleIndex){
		
			List<int> constraints = new List<int>();
			
			for (int i = 0; i < skinIndices.Count; i++){
				if (skinIndices[i] == particleIndex) 
					constraints.Add(i);
			}
			
			return constraints;
		}

		protected override void OnAddToSolver(object info){

			ObiSolver solver = actor.solver;
			
			// Set solver constraint data:
			int[] solverIndices = new int[skinIndices.Count];
			for (int i = 0; i < skinIndices.Count; i++)
			{
				solverIndices[i] = actor.particleIndices[skinIndices[i]];
				solverIndices[i] = actor.particleIndices[skinIndices[i]];
			}
			
			indicesOffset = actor.solver.skinConstraints.skinIndices.Length;
			ObiUtils.AddRange(ref solver.skinConstraints.skinIndices,solverIndices);
			ObiUtils.AddRange(ref solver.skinConstraints.skinPoints,skinPoints.ToArray());
			ObiUtils.AddRange(ref solver.skinConstraints.skinNormals,skinNormals.ToArray());
			ObiUtils.AddRange(ref solver.skinConstraints.skinRadiiBackstops,skinRadiiBackstop.ToArray());
			ObiUtils.AddRange(ref solver.skinConstraints.skinStiffnesses,skinStiffnesses.ToArray());

		}
		
		protected override void OnRemoveFromSolver(object info){
			
			ObiSolver solver = actor.solver;
			
			// subtract our amount of constraints from other actor's offsets:
			for (int i = actor.actorID+1; i < solver.actors.Count; i++){
				ObiSkinConstraints dc = solver.actors[i].GetComponent<ObiSkinConstraints>();
				if (dc != null)
					dc.UpdateIndicesOffset(dc.indicesOffset - skinIndices.Count);
			}
			
			ObiUtils.RemoveRange(ref solver.skinConstraints.skinIndices,indicesOffset,skinIndices.Count);
			ObiUtils.RemoveRange(ref solver.skinConstraints.skinPoints,indicesOffset,skinIndices.Count);
			ObiUtils.RemoveRange(ref solver.skinConstraints.skinNormals,indicesOffset,skinIndices.Count);
			ObiUtils.RemoveRange(ref solver.skinConstraints.skinRadiiBackstops,indicesOffset*2,skinIndices.Count*2);
			ObiUtils.RemoveRange(ref solver.skinConstraints.skinStiffnesses,indicesOffset,skinIndices.Count);

		}

		protected override void OnAddedToSolver(object info){
			PushDataToSolver(new ObiSolverData(ObiSolverData.SkinConstraintsData.ALL));
		}
		
		public override void PushDataToSolver(ObiSolverData data){ 
			
			if (actor == null || !actor.InSolver)
				return;

			if ((data.skinConstraintsData & ObiSolverData.SkinConstraintsData.SKIN_STIFFNESSES) != 0){
	
				for (int i = 0; i < skinStiffnesses.Count; i++){
					skinStiffnesses[i] = stiffness;
				}
		
				if (actor != null && actor.solver != null && skinStiffnesses != null){
					Array.Copy(skinStiffnesses.ToArray(),0,actor.solver.skinConstraints.skinStiffnesses,indicesOffset,skinStiffnesses.Count);
				}
			}

			if ((data.skinConstraintsData & ObiSolverData.SkinConstraintsData.SKIN_POINTS) != 0){
				if (actor != null && actor.solver != null && skinPoints != null){
					Array.Copy(skinPoints.ToArray(),0,actor.solver.skinConstraints.skinPoints,indicesOffset,skinPoints.Count);
				}
			}
			if ((data.skinConstraintsData & ObiSolverData.SkinConstraintsData.SKIN_NORMALS) != 0){
				if (actor != null && actor.solver != null && skinNormals != null){
					Array.Copy(skinNormals.ToArray(),0,actor.solver.skinConstraints.skinNormals,indicesOffset,skinNormals.Count);
				}
			}
			
			if ((data.skinConstraintsData & ObiSolverData.SkinConstraintsData.ACTIVE_STATUS) != 0){
				UpdateConstraintActiveStatus();
			}
			
		}

		/**
		 * Returns the position of a skin constraint in world space. 
		 * Works both when the constraints are managed by a solver and when they aren't. 
		 */
		public Vector3 GetSkinPosition(int index){
			return skinPoints[index];
		}

		/**
		 * Returns the normal of a skin constraint in world space. 
		 * Works both when the constraints are managed by a solver and when they aren't. 
		 */
		public Vector3 GetSkinNormal(int index){
			return skinNormals[index];
		}
		
	}
}



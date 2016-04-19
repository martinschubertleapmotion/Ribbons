using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi{
	
	/**
 	 * Holds information about volume constraints for an actor.
 	 */
	[DisallowMultipleComponent]
	public class ObiVolumeConstraints : ObiConstraints 
	{
		
		[Tooltip("Amount of pressure applied to the cloth.")]
		public float overpressure = 1;

		[Range(0,1)]
		[Tooltip("Stiffness of the volume constraints. Higher values will make the constraints to try harder to enforce the set volume.")]
		public float stiffness = 1;
		
		[HideInInspector] [NonSerialized] int volumeTrianglesOffset;	/**< start of triangle indices in solver*/
		[HideInInspector] [NonSerialized] int volumeParticlesOffset;	/**< start of particle indices in solver*/

		[HideInInspector] public List<int> triangleIndices = new List<int>();			/**< Triangle indices.*/
		[HideInInspector] public List<int> firstTriangle = new List<int>();				/**< index of first triangle for each constraint.*/
		[HideInInspector] public List<int> numTriangles = new List<int>();				/**< num of triangles for each constraint.*/
		
		[HideInInspector] public List<int> particleIndices = new List<int>();			/**< Particle indices.*/
		[HideInInspector] public List<int> firstParticle = new List<int>();				/**< index of first particle for each constraint.*/
		[HideInInspector] public List<int> numParticles = new List<int>();				/**< num of particles for each constraint.*/
		
		[HideInInspector] public List<float> restVolumes = new List<float>();				/**< rest volume for each constraint.*/
		[HideInInspector] public List<Vector2> pressureStiffness = new List<Vector2>();		/**< pressure and stiffness for each constraint.*/
		
		/**
		 * Initialize with the total amount of triangles used by all constraints, and the number of constraints.
		 */
		public override void Initialize(){
			activeStatus.Clear();
			triangleIndices.Clear();
			firstTriangle.Clear();
			numTriangles.Clear();
			particleIndices.Clear();
			firstParticle.Clear();
			numParticles.Clear();
			restVolumes.Clear();
			pressureStiffness.Clear();
		}
		

		public void AddConstraint(bool active, int[] particles,  int[] triangles, float restVolume, float pressure, float stiffness){

			if (InSolver){
				Debug.LogError("You need to remove the constraints from the solver before attempting to add new individual constraints.");
				return;
			}

			activeStatus.Add(active);
			numTriangles.Add((int)triangles.Length/3);
			numParticles.Add(particles.Length);

			this.firstTriangle.Add((int)triangleIndices.Count/3);
			this.firstParticle.Add(particleIndices.Count);
			
			triangleIndices.AddRange(triangles);
			particleIndices.AddRange(particles);

			restVolumes.Add(restVolume);
			pressureStiffness.Add(new Vector2(pressure,stiffness));
			
		}

		/**
		 * Since each individual volume constraint can have a variable number of particles and/or triangles, we need a way
		 * to update the indices of the first triangle/particle in the array when constraints are added/removed from the solver.
		 */
		private void UpdateFirstParticleAndTriangleIndices(int removedParticles, int removedTriangles){

			if (!inSolver || actor == null || !actor.InSolver)
				return;

			if (indicesOffset < actor.solver.volumeConstraints.volumeFirstTriangle.Length)
				actor.solver.volumeConstraints.volumeFirstTriangle[indicesOffset] -= removedTriangles;
			if (indicesOffset < actor.solver.volumeConstraints.volumeFirstParticle.Length)
				actor.solver.volumeConstraints.volumeFirstParticle[indicesOffset] -= removedParticles;

		}
		
		public override List<int> GetConstraintsInvolvingParticle(int particleIndex){
		
			List<int> constraints = new List<int>();
			
			for (int i = 0; i < restVolumes.Count; i++){
			
				for (int j = 0; j < numParticles[i]; j++){
					if (particleIndices[firstParticle[i]+j] == particleIndex) 
						constraints.Add(i);
				}
				
			}
			
			return constraints;
		}
		
		protected override void OnAddToSolver(object info){
			
			ObiSolver solver = actor.solver;
			
			// Set solver constraint data:
			int[] solverIndices = new int[particleIndices.Count];
			for (int i = 0; i < particleIndices.Count; i++)
			{
				solverIndices[i] = actor.particleIndices[particleIndices[i]];
			}

			int[] solverFirstTriangle = new int[firstTriangle.Count];
			for (int i = 0; i < firstTriangle.Count; i++)
			{
				solverFirstTriangle[i] = (int)actor.solver.volumeConstraints.volumeTriangleIndices.Length/3 + firstTriangle[i];
			}
			
			int[] solverFirstParticle = new int[firstParticle.Count];
			for (int i = 0; i < firstParticle.Count; i++)
			{
				solverFirstParticle[i] = actor.solver.volumeConstraints.volumeParticleIndices.Length + firstParticle[i];
            }

			indicesOffset = actor.solver.volumeConstraints.volumeRestVolumes.Length;
			volumeTrianglesOffset = actor.solver.volumeConstraints.volumeTriangleIndices.Length;
			volumeParticlesOffset = actor.solver.volumeConstraints.volumeParticleIndices.Length;

			ObiUtils.AddRange(ref solver.volumeConstraints.volumeTriangleIndices,triangleIndices.ToArray());
			ObiUtils.AddRange(ref solver.volumeConstraints.volumeFirstTriangle,solverFirstTriangle);
			ObiUtils.AddRange(ref solver.volumeConstraints.volumeNumTriangles,numTriangles.ToArray());

			ObiUtils.AddRange(ref solver.volumeConstraints.volumeParticleIndices,solverIndices);
			ObiUtils.AddRange(ref solver.volumeConstraints.volumeFirstParticle,solverFirstParticle);
			ObiUtils.AddRange(ref solver.volumeConstraints.volumeNumParticles,numParticles.ToArray());

			ObiUtils.AddRange(ref solver.volumeConstraints.volumeRestVolumes,restVolumes.ToArray());
			ObiUtils.AddRange(ref solver.volumeConstraints.volumePressureStiffnesses,pressureStiffness.ToArray());
			
		}
		
		protected override void OnRemoveFromSolver(object info){
			
			ObiSolver solver = actor.solver;
			
			// subtract our amount of constraints from other actor's offsets:
			for (int i = actor.actorID+1; i < solver.actors.Count; i++){

				ObiVolumeConstraints ac = solver.actors[i].GetComponent<ObiVolumeConstraints>();

				if (ac != null){

					ac.volumeTrianglesOffset -= triangleIndices.Count;
					ac.volumeParticlesOffset -= particleIndices.Count;
					ac.UpdateFirstParticleAndTriangleIndices(particleIndices.Count,triangleIndices.Count/3);

					ac.UpdateIndicesOffset(ac.indicesOffset - firstTriangle.Count);

				}	
			}
			
			ObiUtils.RemoveRange(ref solver.volumeConstraints.volumeTriangleIndices,volumeTrianglesOffset,triangleIndices.Count);
			ObiUtils.RemoveRange(ref solver.volumeConstraints.volumeFirstTriangle,indicesOffset,firstTriangle.Count); 
			ObiUtils.RemoveRange(ref solver.volumeConstraints.volumeNumTriangles,indicesOffset,firstTriangle.Count);

			ObiUtils.RemoveRange(ref solver.volumeConstraints.volumeParticleIndices,volumeParticlesOffset,particleIndices.Count);
			ObiUtils.RemoveRange(ref solver.volumeConstraints.volumeFirstParticle,indicesOffset,firstParticle.Count); 
			ObiUtils.RemoveRange(ref solver.volumeConstraints.volumeNumParticles,indicesOffset,firstParticle.Count);

			ObiUtils.RemoveRange(ref solver.volumeConstraints.volumeRestVolumes,indicesOffset,firstTriangle.Count);
			ObiUtils.RemoveRange(ref solver.volumeConstraints.volumePressureStiffnesses,indicesOffset,firstTriangle.Count);

		}

		protected override void OnAddedToSolver(object info){
			PushDataToSolver(new ObiSolverData(ObiSolverData.VolumeConstraintsData.ALL));
		}
		
		public override void PushDataToSolver(ObiSolverData data){ 
			
			if (actor == null || !actor.InSolver)
				return;
			
			if ((data.volumeConstraintsData & ObiSolverData.VolumeConstraintsData.VOLUME_REST_VOLUMES) != 0){
				if (actor != null && actor.solver != null && restVolumes != null){
					Array.Copy(restVolumes.ToArray(),0,actor.solver.volumeConstraints.volumeRestVolumes,indicesOffset,restVolumes.Count);
				}
			}

			if ((data.volumeConstraintsData & ObiSolverData.VolumeConstraintsData.VOLUME_PRESSURE_STIFFNESSES) != 0){

				for (int i = 0; i < pressureStiffness.Count; i++){
					pressureStiffness[i] = new Vector2(overpressure,stiffness);
				}

				if (actor != null && actor.solver != null && pressureStiffness != null){
					Array.Copy(pressureStiffness.ToArray(),0,actor.solver.volumeConstraints.volumePressureStiffnesses,indicesOffset,pressureStiffness.Count);
				}
			}
			
			if ((data.volumeConstraintsData & ObiSolverData.VolumeConstraintsData.ACTIVE_STATUS) != 0){
				UpdateConstraintActiveStatus();
			}
			
		}
		
	}
}






using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Obi{
	

	/**
	 * Holds all volume constraints in a solver.
	 */
	[Serializable]
	public class ObiVolumeConstraintGroup : ObiSolverConstraintGroup
	{
		
		/** Volume constraints*/
		[HideInInspector] [NonSerialized] public int[] volumeTriangleIndices;
		[HideInInspector] [NonSerialized] public int[] volumeFirstTriangle;
		[HideInInspector] [NonSerialized] public int[] volumeNumTriangles;
		[HideInInspector] [NonSerialized] public int[] volumeParticleIndices;
		[HideInInspector] [NonSerialized] public int[] volumeFirstParticle;
		[HideInInspector] [NonSerialized] public int[] volumeNumParticles;
		[HideInInspector] [NonSerialized] public float[] volumeRestVolumes;
		[HideInInspector] [NonSerialized] public Vector2[] volumePressureStiffnesses;
		
		private GCHandle volumeTriangleIndicesHandle;
		private GCHandle volumeFirstTriangleHandle;
		private GCHandle volumeNumTrianglesHandle;
		private GCHandle volumeParticleIndicesHandle;
		private GCHandle volumeFirstParticleHandle;
		private GCHandle volumeNumParticlesHandle;
		private GCHandle volumeRestVolumesHandle;
		private GCHandle volumePressureStiffnessesHandle;
		
		public ObiVolumeConstraintGroup(ObiSolver solver) : base(solver){
			volumeTriangleIndices = new int[0];
			volumeFirstTriangle = new int[0];
			volumeNumTriangles = new int[0];
			volumeParticleIndices = new int[0];
			volumeFirstParticle = new int[0];
			volumeNumParticles = new int[0];
			volumeRestVolumes = new float[0];
			volumePressureStiffnesses = new Vector2[0];
		}
		
		protected override void CustomTeardown(){
			Oni.UnpinMemory(volumeTriangleIndicesHandle);
			Oni.UnpinMemory(volumeFirstTriangleHandle);
			Oni.UnpinMemory(volumeNumTrianglesHandle);
			Oni.UnpinMemory(volumeParticleIndicesHandle);
			Oni.UnpinMemory(volumeFirstParticleHandle);
			Oni.UnpinMemory(volumeNumParticlesHandle);
			Oni.UnpinMemory(volumeRestVolumesHandle);
			Oni.UnpinMemory(volumePressureStiffnessesHandle);
		}
		
		protected override void CustomCommitActive(int[] activeArray){
			Oni.SetActiveVolumeConstraints(solver.Solver,activeConstraintsHandle.AddrOfPinnedObject(),activeArray.Length);
		}
		
		public override void CommitToSolver(){
			
			if (volumeTriangleIndices != null && volumeFirstTriangle != null && volumeNumTriangles != null && volumeRestVolumes != null && volumePressureStiffnesses != null){
				
				Oni.UnpinMemory(volumeTriangleIndicesHandle);
				Oni.UnpinMemory(volumeFirstTriangleHandle);
				Oni.UnpinMemory(volumeNumTrianglesHandle);
				Oni.UnpinMemory(volumeParticleIndicesHandle);
				Oni.UnpinMemory(volumeFirstParticleHandle);
				Oni.UnpinMemory(volumeNumParticlesHandle);
				Oni.UnpinMemory(volumeRestVolumesHandle);
				Oni.UnpinMemory(volumePressureStiffnessesHandle);
				
				volumeTriangleIndicesHandle = Oni.PinMemory(volumeTriangleIndices);
				volumeFirstTriangleHandle = Oni.PinMemory(volumeFirstTriangle);
				volumeNumTrianglesHandle = Oni.PinMemory(volumeNumTriangles);
				
				volumeParticleIndicesHandle = Oni.PinMemory(volumeParticleIndices);
				volumeFirstParticleHandle = Oni.PinMemory(volumeFirstParticle);
				volumeNumParticlesHandle = Oni.PinMemory(volumeNumParticles);
				
				volumeRestVolumesHandle = Oni.PinMemory(volumeRestVolumes);
				volumePressureStiffnessesHandle = Oni.PinMemory(volumePressureStiffnesses);
				
				Oni.SetVolumeConstraints(solver.Solver,
				                         volumeTriangleIndicesHandle.AddrOfPinnedObject(),
				                         volumeFirstTriangleHandle.AddrOfPinnedObject(),
				                         volumeNumTrianglesHandle.AddrOfPinnedObject(),
				                         volumeParticleIndicesHandle.AddrOfPinnedObject(),
				                         volumeFirstParticleHandle.AddrOfPinnedObject(),
				                         volumeNumParticlesHandle.AddrOfPinnedObject(),
				                         volumeRestVolumesHandle.AddrOfPinnedObject(),
				                         volumePressureStiffnessesHandle.AddrOfPinnedObject());
				
				CommitActive();
			}
			
			
		}
		
	}
}




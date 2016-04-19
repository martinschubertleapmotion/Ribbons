using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Obi{
	
	/**
	 * Holds all skin constraints in a solver.
	 */
	[Serializable]
	public class ObiSkinConstraintGroup : ObiSolverConstraintGroup
	{
								
		/** Skin constraints*/				
		[HideInInspector] [NonSerialized] public int[] skinIndices;			
		[HideInInspector] [NonSerialized] public Vector3[] skinPoints;			
		[HideInInspector] [NonSerialized] public Vector3[] skinNormals;			
		[HideInInspector] [NonSerialized] public float[] skinRadiiBackstops;	
		[HideInInspector] [NonSerialized] public float[] skinStiffnesses;
		
		private GCHandle skinIndicesHandle;
		private GCHandle skinPointsHandle;
		private GCHandle skinNormalsHandle;
		private GCHandle skinRadiiBackstopsHandle;
		private GCHandle skinStiffnessesHandle;
		
		public ObiSkinConstraintGroup(ObiSolver solver) : base(solver){
			skinIndices = new int[0];
			skinPoints = new Vector3[0];
			skinNormals = new Vector3[0];
			skinRadiiBackstops = new float[0];
			skinStiffnesses = new float[0];
		}
		
		protected override void CustomTeardown(){
			Oni.UnpinMemory(skinIndicesHandle);
			Oni.UnpinMemory(skinPointsHandle);
			Oni.UnpinMemory(skinNormalsHandle);
			Oni.UnpinMemory(skinRadiiBackstopsHandle);
			Oni.UnpinMemory(skinStiffnessesHandle);
		}
		
		protected override void CustomCommitActive(int[] activeArray){
			Oni.SetActiveSkinConstraints(solver.Solver,activeConstraintsHandle.AddrOfPinnedObject(),activeArray.Length);
		}
		
		public override void CommitToSolver(){
			
			if (skinIndices != null && skinPoints != null && skinNormals != null && skinRadiiBackstops != null && skinStiffnesses != null){
				
				Oni.UnpinMemory(skinIndicesHandle);
				Oni.UnpinMemory(skinPointsHandle);
				Oni.UnpinMemory(skinNormalsHandle);
				Oni.UnpinMemory(skinRadiiBackstopsHandle);
				Oni.UnpinMemory(skinStiffnessesHandle);
				
				skinIndicesHandle = Oni.PinMemory(skinIndices);
				skinPointsHandle = Oni.PinMemory(skinPoints);
				skinNormalsHandle = Oni.PinMemory(skinNormals);
				skinRadiiBackstopsHandle = Oni.PinMemory(skinRadiiBackstops);
				skinStiffnessesHandle = Oni.PinMemory(skinStiffnesses);
				
				Oni.SetSkinConstraints(solver.Solver,
				                       skinIndicesHandle.AddrOfPinnedObject(),
				                       skinPointsHandle.AddrOfPinnedObject(),
				                       skinNormalsHandle.AddrOfPinnedObject(),
				                       skinRadiiBackstopsHandle.AddrOfPinnedObject(),
				                       skinStiffnessesHandle.AddrOfPinnedObject());
				
				CommitActive();
			}
			
		}
		
	}
}


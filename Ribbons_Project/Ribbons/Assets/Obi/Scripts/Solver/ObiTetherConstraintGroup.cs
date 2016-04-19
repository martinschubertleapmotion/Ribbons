using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Obi{
	
	/**
	 * Holds all tether constraints in a solver.
	 */
	[Serializable]
	public class ObiTetherConstraintGroup : ObiSolverConstraintGroup
	{
		
		/** Distance constraints*/					
		[HideInInspector] [NonSerialized] public int[] tetherIndices;			
		[HideInInspector] [NonSerialized] public Vector2[] maxLengthsScales;			
		[HideInInspector] [NonSerialized] public float[] stiffnesses;			
		
		private GCHandle tetherIndicesHandle;
		private GCHandle maxLengthsScalesHandle;
		private GCHandle stiffnessesHandle;
		
		public ObiTetherConstraintGroup(ObiSolver solver) : base(solver){
			tetherIndices = new int[0];
			maxLengthsScales = new Vector2[0];
			stiffnesses = new float[0];
		}
		
		protected override void CustomTeardown(){
			Oni.UnpinMemory(tetherIndicesHandle);
			Oni.UnpinMemory(maxLengthsScalesHandle);
			Oni.UnpinMemory(stiffnessesHandle);
		}
		
		protected override void CustomCommitActive(int[] activeArray){
			Oni.SetActiveTetherConstraints(solver.Solver,activeConstraintsHandle.AddrOfPinnedObject(),activeArray.Length);
		}
		
		public override void CommitToSolver(){
			
			if (tetherIndices != null && maxLengthsScales != null && stiffnesses != null){
				
				Oni.UnpinMemory(tetherIndicesHandle);
				Oni.UnpinMemory(maxLengthsScalesHandle);
				Oni.UnpinMemory(stiffnessesHandle);
				
				tetherIndicesHandle = Oni.PinMemory(tetherIndices);
				maxLengthsScalesHandle = Oni.PinMemory(maxLengthsScales);
				stiffnessesHandle = Oni.PinMemory(stiffnesses);
				
				Oni.SetTetherConstraints(solver.Solver,tetherIndicesHandle.AddrOfPinnedObject(),
				                         			   maxLengthsScalesHandle.AddrOfPinnedObject(),
				                         			   stiffnessesHandle.AddrOfPinnedObject());
				
				CommitActive();
			}
			
		}
		
	}
}


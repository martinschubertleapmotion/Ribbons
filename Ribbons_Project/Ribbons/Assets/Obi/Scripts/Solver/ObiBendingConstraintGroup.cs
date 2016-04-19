using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Obi{
	
	/**
	 * Holds all bending constraints in a solver.
	 */
	[Serializable]
	public class ObiBendingConstraintGroup : ObiSolverConstraintGroup
	{
		
		/** Bending constraints*/						
		[HideInInspector] [NonSerialized] public int[] bendingIndices;			
		[HideInInspector] [NonSerialized] public float[] restBends;			
		[HideInInspector] [NonSerialized] public Vector2[] bendingStiffnesses;
		
		private GCHandle bendingIndicesHandle;
		private GCHandle restBendsHandle;
		private GCHandle bendingStiffnessesHandle;
		
		public ObiBendingConstraintGroup(ObiSolver solver) : base(solver){
			bendingIndices = new int[0];
			restBends = new float[0];
			bendingStiffnesses = new Vector2[0];
		}
		
		protected override void CustomTeardown(){
			Oni.UnpinMemory(bendingIndicesHandle);
			Oni.UnpinMemory(restBendsHandle);
			Oni.UnpinMemory(bendingStiffnessesHandle);
		}
		
		protected override void CustomCommitActive(int[] activeArray){
			Oni.SetActiveBendingConstraints(solver.Solver,activeConstraintsHandle.AddrOfPinnedObject(),activeArray.Length);
		}
		
		public override void CommitToSolver(){
			
			if (bendingIndices != null && restBends != null && bendingStiffnesses != null){
				
				Oni.UnpinMemory(bendingIndicesHandle);
				Oni.UnpinMemory(restBendsHandle);
				Oni.UnpinMemory(bendingStiffnessesHandle);
				
				bendingIndicesHandle = Oni.PinMemory(bendingIndices);
				restBendsHandle = Oni.PinMemory(restBends);
				bendingStiffnessesHandle = Oni.PinMemory(bendingStiffnesses);
				
				Oni.SetBendingConstraints(solver.Solver,bendingIndicesHandle.AddrOfPinnedObject(),
				                          restBendsHandle.AddrOfPinnedObject(),
				                          bendingStiffnessesHandle.AddrOfPinnedObject());
				
				CommitActive();
			}
			
		}
		
	}
}




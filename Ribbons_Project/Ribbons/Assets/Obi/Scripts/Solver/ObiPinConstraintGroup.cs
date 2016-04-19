using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Obi{

/**
 * Holds all pin constraints in a solver.
 */
[Serializable]
public class ObiPinConstraintGroup : ObiSolverConstraintGroup
{

	/** Pin constraints*/					
	[HideInInspector] [NonSerialized] public int[] pinIndices;		
	[HideInInspector] [NonSerialized] public Vector3[] pinOffsets;				
	[HideInInspector] [NonSerialized] public float[] stiffnesses;			
	
	private GCHandle pinIndicesHandle;
	private GCHandle pinOffsetsHandle;
	private GCHandle stiffnessesHandle;

	public ObiPinConstraintGroup(ObiSolver solver) : base(solver){
		pinIndices = new int[0];
		pinOffsets = new Vector3[0];
		stiffnesses = new float[0];
	}
	
	protected override void CustomTeardown(){
		Oni.UnpinMemory(pinIndicesHandle);
		Oni.UnpinMemory(pinOffsetsHandle);
		Oni.UnpinMemory(stiffnessesHandle);
	}
	 
	protected override void CustomCommitActive(int[] activeArray){
		Oni.SetActivePinConstraints(solver.Solver,activeConstraintsHandle.AddrOfPinnedObject(),activeArray.Length);
	}
	
	public override void CommitToSolver(){
		
		if (pinIndices != null && pinOffsets != null && stiffnesses != null){
			
			Oni.UnpinMemory(pinIndicesHandle);
			Oni.UnpinMemory(pinOffsetsHandle);
			Oni.UnpinMemory(stiffnessesHandle);
			
			pinIndicesHandle = Oni.PinMemory(pinIndices);
			pinOffsetsHandle = Oni.PinMemory(pinOffsets);
			stiffnessesHandle = Oni.PinMemory(stiffnesses);
			
			Oni.SetPinConstraints(solver.Solver,pinIndicesHandle.AddrOfPinnedObject(),
			                           			pinOffsetsHandle.AddrOfPinnedObject(),
			                          			stiffnessesHandle.AddrOfPinnedObject());
			
			CommitActive();
		}
		
	}
	
}
}




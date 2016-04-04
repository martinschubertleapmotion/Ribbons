using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Obi{


/**
 * Holds all distance constraints in a solver.
 */
[Serializable]
public class ObiDistanceConstraintGroup : ObiSolverConstraintGroup
{

	/** Distance constraints*/					
	[HideInInspector] [NonSerialized] public int[] springIndices;			
	[HideInInspector] [NonSerialized] public float[] restLengths;			
	[HideInInspector] [NonSerialized] public Vector2[] stiffnesses;			
	[HideInInspector] [NonSerialized] public float[] stretching;
	
	private GCHandle springIndicesHandle;
	private GCHandle restLengthsHandle;
	private GCHandle stiffnessesHandle;
	private GCHandle stretchingHandle;
	
	public ObiDistanceConstraintGroup(ObiSolver solver) : base(solver){
		springIndices = new int[0];
		restLengths = new float[0];
		stiffnesses = new Vector2[0];
		stretching = new float[0];
	}
	
	protected override void CustomTeardown(){
		Oni.UnpinMemory(springIndicesHandle);
		Oni.UnpinMemory(restLengthsHandle);
		Oni.UnpinMemory(stiffnessesHandle);
		Oni.UnpinMemory(stretchingHandle);
	}
	 
	protected override void CustomCommitActive(int[] activeArray){
		Oni.SetActiveDistanceConstraints(solver.Solver,activeConstraintsHandle.AddrOfPinnedObject(),activeArray.Length);
	}
	
	public override void CommitToSolver(){
		
		if (springIndices != null && restLengths != null && stiffnesses != null && stretching != null){
			
			Oni.UnpinMemory(springIndicesHandle);
			Oni.UnpinMemory(restLengthsHandle);
			Oni.UnpinMemory(stiffnessesHandle);
			Oni.UnpinMemory(stretchingHandle);
			
			springIndicesHandle = Oni.PinMemory(springIndices);
			restLengthsHandle = Oni.PinMemory(restLengths);
			stiffnessesHandle = Oni.PinMemory(stiffnesses);
			stretchingHandle = Oni.PinMemory(stretching);
			
			Oni.SetDistanceConstraints(solver.Solver,springIndicesHandle.AddrOfPinnedObject(),
			                           restLengthsHandle.AddrOfPinnedObject(),
			                           stiffnessesHandle.AddrOfPinnedObject(),
			                           stretchingHandle.AddrOfPinnedObject());
			
			CommitActive();
		}
		
	}
	
}
}


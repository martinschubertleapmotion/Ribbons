using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Obi{

/**
  * Holds solver data for a certain kind of constraints.
  */
[Serializable]
public abstract class ObiSolverConstraintGroup
{
	
	protected ObiSolver solver;
	
	[HideInInspector] public HashSet<int> activeConstraints;
	[NonSerialized] protected GCHandle activeConstraintsHandle;	

	public ObiSolverConstraintGroup(ObiSolver solver){
		this.solver = solver;
		activeConstraints = new HashSet<int>();
	}
	
	public void Teardown(){
		Oni.UnpinMemory(activeConstraintsHandle);
		CustomTeardown ();
	}
	
	public void CommitActive(){
		int[] activeArray = new int[activeConstraints.Count];
		activeConstraints.CopyTo(activeArray);
		Oni.UnpinMemory(activeConstraintsHandle);
		activeConstraintsHandle = Oni.PinMemory(activeArray);
		CustomCommitActive(activeArray);
	}
	
	protected abstract void CustomTeardown();
	protected abstract void CustomCommitActive(int[] activeArray);
	public abstract void CommitToSolver();

}
}


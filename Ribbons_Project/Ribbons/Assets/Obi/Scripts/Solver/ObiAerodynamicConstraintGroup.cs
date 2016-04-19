using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Obi{
	
	/**
	 * Holds all aerodynamic constraints in a solver.
	 */
	[Serializable]
	public class ObiAerodynamicConstraintGroup : ObiSolverConstraintGroup
	{
		
		/** Aerodynamic constraints*/
		[HideInInspector] [NonSerialized] public int[] aerodynamicIndices;
		[HideInInspector] [NonSerialized] public Vector3[] aerodynamicNormals;
		[HideInInspector] [NonSerialized] public Vector3[] wind;
		[HideInInspector] [NonSerialized] public float[] aerodynamicCoeffs;
		
		private GCHandle aerodynamicTriangleIndicesHandle;
		private GCHandle aerodynamicTriangleNormalsHandle;
		private GCHandle windHandle;
		private GCHandle aerodynamicCoeffsHandle;
		
		public ObiAerodynamicConstraintGroup(ObiSolver solver) : base(solver){
			aerodynamicIndices = new int[0];
			aerodynamicNormals = new Vector3[0];
			wind = new Vector3[0];
			aerodynamicCoeffs = new float[0];
		}
		
		protected override void CustomTeardown(){
			Oni.UnpinMemory(aerodynamicTriangleIndicesHandle);
			Oni.UnpinMemory(aerodynamicTriangleNormalsHandle);
			Oni.UnpinMemory(windHandle);
			Oni.UnpinMemory(aerodynamicCoeffsHandle);
		}
		
		protected override void CustomCommitActive(int[] activeArray){
			Oni.SetActiveAerodynamicConstraints(solver.Solver,activeConstraintsHandle.AddrOfPinnedObject(),activeArray.Length);
		}
		
		public override void CommitToSolver(){
			
			if (aerodynamicIndices != null){
				
				Oni.UnpinMemory(aerodynamicTriangleIndicesHandle); 
				Oni.UnpinMemory(aerodynamicTriangleNormalsHandle);
				Oni.UnpinMemory(windHandle);
				Oni.UnpinMemory(aerodynamicCoeffsHandle);
				
				aerodynamicTriangleIndicesHandle = Oni.PinMemory(aerodynamicIndices);
				aerodynamicTriangleNormalsHandle = Oni.PinMemory(aerodynamicNormals);
				windHandle = Oni.PinMemory(wind);
				aerodynamicCoeffsHandle = Oni.PinMemory(aerodynamicCoeffs);
				
				Oni.SetAerodynamicConstraints(solver.Solver,
				                              aerodynamicTriangleIndicesHandle.AddrOfPinnedObject(), 
				                              aerodynamicTriangleNormalsHandle.AddrOfPinnedObject(),
				                              windHandle.AddrOfPinnedObject(),
				                              aerodynamicCoeffsHandle.AddrOfPinnedObject());
				                              
				CommitActive();
			}
			
		}
		
	}
}



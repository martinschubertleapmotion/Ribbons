using UnityEngine;
using System;
using System.Collections.Generic;


namespace Obi{

	[ExecuteInEditMode]
	public abstract class ObiEmitterShape : MonoBehaviour
	{

		protected List<Vector3> distribution = new List<Vector3>();
		protected int lastDistributionPoint = 0;

		public int DistributionPointsCount{
			get{return distribution.Count;}
		}

		public void Awake(){
			GenerateDistribution();
		}

		protected abstract void GenerateDistribution();

		public Vector3 GetDistributionPoint(){

			if (lastDistributionPoint >= distribution.Count)
				return Vector3.zero;

			Vector3 point = distribution[lastDistributionPoint];
			lastDistributionPoint = (lastDistributionPoint + 1) % distribution.Count;

			return point;
			
		}
		
	}
}


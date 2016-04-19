using UnityEngine;
using System.Collections;

namespace Obi{

	/**
	 * Small helper class that lets you assign an ObiCollisionMaterial to a regular Collider to control how
	 * it interacts with Obi particles.
	 */
	public class ObiCollider : MonoBehaviour
	{
		public ObiCollisionMaterial material;
		[HideInInspector] public int materialIndex = 0;
	}
}


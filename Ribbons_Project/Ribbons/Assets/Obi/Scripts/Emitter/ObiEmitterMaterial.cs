using UnityEngine;
using System.Collections;

namespace Obi{

/**
 * Holds information about the physical properties of the substance emitted by an emitter.
 */
public class ObiEmitterMaterial : ScriptableObject
{

	// fluid parameters:
	public bool isFluid = true;	/**< do the emitter particles generate density constraints?*/

	public float stiffness = 1f;			/**< how stiff the density corrections are.*/
	public float restDistance = 0.1f;
	public float restDensity = 1000;		/**< rest density of the fluid particles.*/
	public float viscosity = 0.01f;			/**< viscosity of the fluid particles.*/
	public float cohesion = 0.1f;
	public float surfaceTension = 0.1f;	/**< surface tension of the fluid particles.*/

	// gas parameters:
	public float buoyancy = -1.0f; 			/**< how dense is this material with respect to air?*/
	public float drag = 200;				/**< amount of drag applied by the surrounding air to particles near the surface of the material.*/
	public float vorticity = 0.2f;			/**< amount of vorticity applied to the gas*/
	
	// elastoplastic parameters:
	public float elasticRange; 		/** radius around a particle in which distance constraints are created.*/
	public float plasticCreep;		/**< rate at which a deformed plastic material regains its shape*/
	public float plasticThreshold;	/**< amount of stretching stress that a elastic material must undergo to become plastic.*/

	public Oni.FluidMaterial GetEquivalentOniMaterial()
	{
		Oni.FluidMaterial material = new Oni.FluidMaterial();
		material.stiffness = stiffness;
		material.restDistance = restDistance;
		material.restDensity = restDensity;
		material.viscosity = viscosity;
		material.cohesion = cohesion;
		material.surfaceTension = surfaceTension;
		material.buoyancy = buoyancy;
		material.drag = drag;
		material.vorticity = vorticity;
		return material;
	}
}
}


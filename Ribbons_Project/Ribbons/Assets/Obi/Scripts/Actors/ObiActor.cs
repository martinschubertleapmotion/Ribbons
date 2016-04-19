using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Obi{

/**
 * Represents a group of related particles. ObiActor does not make
 * any assumptions about the relationship between these particles, except that they get allocated 
 * and released together.
 */
public class ObiActor : MonoBehaviour, IObiSolverClient
{
	public ObiSolver solver;
	//public ObiParticleMeshRenderer particleRenderer;
	public ObiCollisionMaterial material;
	public bool selfCollisions = false;

	[HideInInspector][NonSerialized] public int actorID = -1; 						/**< actor ID in the solver..*/
	[HideInInspector][NonSerialized] public List<int> particleIndices;				/**< indices of allocated particles in the solver.*/
	
	[HideInInspector] public bool[] active;					/**< Particle activation status.*/
	[HideInInspector] public Vector3[] positions;			/**< Particle positions.*/
	[HideInInspector] public Vector3[] velocities;			/**< Particle velocities.*/
	[HideInInspector] public float[] invMasses;				/**< Particle inverse masses*/
	[HideInInspector] public float[] solidRadii;			/**< Particle solid radii (physical radius of each particle)*/
	[HideInInspector] public int[] triangles;				/**< Triangle indices in particle arrays.*/
	[HideInInspector] public Vector3[] normals;				/**< 3 normals for each triangle.*/
	[HideInInspector] public int[] phases;					/**< Particle phases.*/

	private bool inSolver = false;
	protected bool initializing = false;		
	[HideInInspector][SerializeField] protected bool initialized = false;

	private bool oldSelfCollisions = false;
	private int oldLayer = 0;
	
	public bool Initializing{
		get{return initializing;}
	}
	
	public bool Initialized{
		get{return initialized;}
	}

	public bool InSolver{
		get{return inSolver;}
	}

	public virtual void Awake(){
		oldLayer = gameObject.layer;
		oldSelfCollisions = selfCollisions;
    }

	/**
	 * Since Awake is not guaranteed to be called before OnEnable, we must add the mesh to the solver here.
	 */
	public virtual void Start(){
		if (Application.isPlaying)
			AddToSolver(null);
	}

	public virtual void OnDestroy(){
		RemoveFromSolver(null);
	}

	/**
	 * Flags all particles allocated by this actor as active or inactive depending on the "active array".
	 * The solver will then only simulate the active ones.
	 */
	public virtual void OnEnable(){

		if (!InSolver) return;

		// update active status of all particles in the actor:
		for (int i = 0; i < particleIndices.Count; ++i){
			int k = particleIndices[i];
			if (!active[i])
				solver.activeParticles.Remove(k);
            else
                solver.activeParticles.Add(k);
		}
		solver.UpdateActiveParticles();
	}

	/**
	 * Flags all particles allocated by this actor as inactive, so the solver will not include them 
	 * in the simulation. To "teleport" the actor to a new position, disable it and then pull positions
	 * and velocities from the solver. Move it to the new position, and enable it.
	 */
	public virtual void OnDisable(){

		if (!InSolver) return;

		// flag all the actor's particles as disabled:
		for (int i = 0; i < particleIndices.Count; ++i){
			int k = particleIndices[i];
			solver.activeParticles.Remove(k);
		}
		solver.UpdateActiveParticles();

		// pull current position / velocity data from solver:
		PullDataFromSolver(new ObiSolverData(ObiSolverData.ParticleData.POSITIONS | ObiSolverData.ParticleData.VELOCITIES));

	}

	/**
	 * Resets the actor to its original state.
	 */
	public virtual void ResetActor(){
	}
		
	/**
	 * Transforms the position of fixed particles from local space to world space and feeds them
	 * to the solver. This is performed just before performing particle interpolation in the solver as the
	 * last step of each frame.
	 */
	public virtual void OnSolverPreInterpolation(){
		
		for(int i = 0; i < particleIndices.Count; i++){
			if (!enabled || invMasses[i] == 0){
				solver.predictedPositions[particleIndices[i]] = 
				solver.previousPositions[particleIndices[i]] =
				solver.positions[particleIndices[i]] = transform.TransformPoint(positions[i]);
			}
		}
		
	}

	/**
	 * Updates particle phases in the solver.
	 */
	public virtual void UpdateParticlePhases(){

		if (!InSolver) return;

		for(int i = 0; i < particleIndices.Count; i++){
			phases[i] = Oni.MakePhase(gameObject.layer,selfCollisions?Oni.ParticlePhase.SelfCollide:0);
		}
		PushDataToSolver(new ObiSolverData(ObiSolverData.ParticleData.PHASES));
	}

	/**
	 * Adds this actor to a solver. No simulation will take place for this actor
 	 * unless it has been added to a solver. Returns true if the actor was succesfully added,
 	 * false if it was already added or couldn't add it for any other reason.
	 */
	public virtual bool AddToSolver(object info){
		
		if (solver != null && !InSolver){
			
			// Allocate particles in the solver:
			particleIndices = solver.AllocateParticles(positions.Length);
			if (particleIndices == null){
				Debug.LogWarning("Obi: Solver could not allocate enough particles for this actor. Please increase max particles.");
				return false;
			}

			inSolver = true;
			
			// Get an actor ID from the solver:
			actorID = solver.SetActor(actorID,this);

			// Update particle phases before sending data to the solver, as layers/flags settings might have changed.
			UpdateParticlePhases();
			
			// Send our particle data to the solver:
			PushDataToSolver(new ObiSolverData(ObiSolverData.ParticleData.ALL));

			return true;
		}
		
		return false;
	}
	
	/**
	 * Adds this actor from its current solver, if any.
	 */
	public virtual bool RemoveFromSolver(object info){
		
		if (solver != null && InSolver){
			
			solver.FreeParticles(particleIndices);
			particleIndices = null;

			inSolver = false;
			
			solver.RemoveActor(actorID);

			return true;
		}
		
		return false;
		
	}

	/**
	 * Sends local particle data to the solver.
	 */
	public virtual void PushDataToSolver(ObiSolverData data){

		if (!InSolver) return;

		for (int i = 0; i < particleIndices.Count; i++){
			int k = particleIndices[i];

			if ((data.particleData & ObiSolverData.ParticleData.ACTIVE_STATUS) != 0){
				if (!active[i])
					solver.activeParticles.Remove(k);
				else
					solver.activeParticles.Add(k);
			}

			if ((data.particleData & ObiSolverData.ParticleData.POSITIONS) != 0)
				solver.positions[k] = transform.TransformPoint(positions[i]);
			if ((data.particleData & ObiSolverData.ParticleData.PREDICTED_POSITIONS) != 0)
				solver.predictedPositions[k] = transform.TransformPoint(positions[i]);
			if ((data.particleData & ObiSolverData.ParticleData.PREVIOUS_POSITIONS) != 0)
				solver.previousPositions[k] = transform.TransformPoint(positions[i]);
			if ((data.particleData & ObiSolverData.ParticleData.VELOCITIES) != 0)
				solver.velocities[k] = transform.TransformVector(velocities[i]);
			if ((data.particleData & ObiSolverData.ParticleData.INV_MASSES) != 0)
				solver.inverseMasses[k] = invMasses[i];
			if ((data.particleData & ObiSolverData.ParticleData.SOLID_RADII) != 0)
				solver.solidRadii[k] = solidRadii[i];
			if ((data.particleData & ObiSolverData.ParticleData.PHASES) != 0)
				solver.phases[k] = phases[i];
		}
        
        if ((data.particleData & ObiSolverData.ParticleData.ACTIVE_STATUS) != 0)
			solver.UpdateActiveParticles();

	}

	/**
	 * Retrieves particle simulation data from the solver. Common uses are
	 * retrieving positions and velocities to set the initial status of the simulation,
 	 * or retrieving solver-generated data such as tensions, densities, etc.
	 */
	public virtual void PullDataFromSolver(ObiSolverData data){
		
		if (!InSolver) return;

		for (int i = 0; i < particleIndices.Count; i++){
			int k = particleIndices[i];
			if ((data.particleData & ObiSolverData.ParticleData.POSITIONS) != 0)
				positions[i] = transform.InverseTransformPoint(solver.positions[k]);
			if ((data.particleData & ObiSolverData.ParticleData.VELOCITIES) != 0)
				velocities[i] = solver.velocities[k];
		}
		
	}

	/**
	 * Returns the position of a particle in world space. 
	 * Works both when the actor is managed by a solver and when it isn't. 
	 */
	public Vector3 GetParticlePosition(int index){
		if (InSolver)
			return solver.renderablePositions[particleIndices[index]];
		else
			return transform.TransformPoint(positions[index]);
	}

	/**
	 * Returns particle reference orientation, if it can be derived. Reimplemented by subclasses. Returns
	 * Quaternion.identity by default.
	 */
	public virtual Quaternion GetParticleOrientation(int index){
		return Quaternion.identity;
	}

	public virtual bool GenerateTethers(int maxTethers){
		return true;
	}

	public virtual void OnSolverStepBegin(){
	}

	public virtual void OnSolverStepEnd(){
	}

	public virtual void OnSolverFrameBegin(){
	}

	public virtual void OnSolverFrameEnd(){	

		// If the object has changed layers, update solver particle phases.
		if (gameObject.layer != oldLayer || selfCollisions != oldSelfCollisions){
			UpdateParticlePhases();
			oldSelfCollisions = selfCollisions;
			oldLayer = gameObject.layer;
		}	

		/*if (!isActiveAndEnabled || particleRenderer == null)
			return;

		// Update particle renderer values:
		Color[] colors = new Color[particleIndices.Count];
		Vector3[] drawPos = new Vector3[particleIndices.Count];
		Vector2[] info = new Vector2[particleIndices.Count];
		for (int i = 0; i < particleIndices.Count; i++){
			drawPos[i] = particleRenderer.transform.InverseTransformPoint(solver.renderablePositions[particleIndices[i]]);
			colors[i] = particleRenderer.particleColor;
			info[i] = new Vector2(0,solidRadii[i]);
		}
		
		particleRenderer.SetParticles(drawPos,solidRadii,colors,info);*/

    }
}
}


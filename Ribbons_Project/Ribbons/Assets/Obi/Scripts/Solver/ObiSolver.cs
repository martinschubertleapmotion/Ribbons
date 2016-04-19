/**
\mainpage ObiCloth documentation
 
Introduction:
------------- 

ObiCloth is a position-based dynamics solver for cloth. It is meant to bring back and extend upon Unity's 4.x
cloth, which had two-way rigidbody coupling. 
 
Features:
-------------------

- Cloth particles can be pinned both in local space and to rigidbodies (kinematic or not).
- Cloth can be teared.
- Realistic wind forces.
- Rigidbodies react to cloth dynamics, and cloth reacts to rigidbodies too.
- Easy prefab instantiation, cloth can be translated, scaled and rotated.
- Simulation can be warm-started in the editor, then all simulation state gets serialized with the object. This means
  your cloth prefabs can be stored at any point in the simulation, and they will resume it when instantiated.

*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;

namespace Obi
{

/**
 * An ObiSolver component simulates particles and their interactions using the Oni unified physics library.
 * Several kinds of constraint types and their parameters are exposed, and several Obi components can
 * be used to feed particles and constraints to the solver.
 */
[ExecuteInEditMode]
[AddComponentMenu("Physics/Obi/Obi Solver")]
[DisallowMultipleComponent]
public sealed class ObiSolver : MonoBehaviour
{

	public enum ClothInterpolation{
		NONE,
		INTERPOLATE
	}

	public class ObiCollisionEventArgs : EventArgs{

		public int[] indices;			/**< collision indices. Even positions in the array are particle indices, odd positions are collider indices.*/
		public float[] distances;
		public Vector3[] points;
		public Vector3[] normals;

		public float[] normalImpulses;
		public float[] tangentImpulses;
		public float[] stickImpulses;

		public ObiCollisionEventArgs(int[] indices, 
									 float[] distances, 
									 Vector3[] points, 
									 Vector3[] normals,
									 float[] normalImpulses,
									 float[] tangentImpulses,
								     float[] stickImpulses){
			this.indices = indices;
			this.distances = distances;
			this.points = points;
			this.normals = normals;

			this.normalImpulses = normalImpulses;
			this.tangentImpulses = tangentImpulses;
			this.stickImpulses = stickImpulses;
		}
	}

	public event EventHandler OnFrameBegin;
	public event EventHandler OnStepBegin;
	public event EventHandler OnStepEnd;
	public event EventHandler OnBeforeActorsFrameEnd;
	public event EventHandler OnFrameEnd;
	public event EventHandler<ObiCollisionEventArgs> OnCollision;
	
	private Dictionary<Type, ObiSolverConstraintGroup> constraintGroupMap = new Dictionary<Type, ObiSolverConstraintGroup>();
	
	public int maxParticles = 5000;
	public float interactionRadius = 0.1f;
	[Tooltip("If enabled, will force the solver to keep simulating even when not visible from any camera.")]

	[HideInInspector] [NonSerialized] public bool simulate = true;
	public bool simulateWhenInvisible = true; 			/**< Whether to keep simulating the cloth when its not visible by any camera.*/
	public ObiColliderGroup colliderGroup;
	public Oni.SolverParameters parameters = new Oni.SolverParameters(Oni.SolverParameters.Interpolation.None,
	                                                                  Vector3.down*9.81f);

	[HideInInspector] [NonSerialized] public List<ObiActor> actors = new List<ObiActor>();
	[HideInInspector] [NonSerialized] public HashSet<int> allocatedParticles;
	[HideInInspector] [NonSerialized] public HashSet<int> activeParticles;

	[HideInInspector] [NonSerialized] public int[] materialIndices;
	[HideInInspector] [NonSerialized] public int[] fluidMaterialIndices;

	/** General particle data*/
	[HideInInspector] [NonSerialized] public Vector3[] renderablePositions;
	[HideInInspector] [NonSerialized] public Vector3[] positions;
	[HideInInspector] [NonSerialized] public Vector3[] predictedPositions;
	[HideInInspector] [NonSerialized] public Vector3[] previousPositions;
	[HideInInspector] [NonSerialized] public Vector3[] velocities;
	[HideInInspector] [NonSerialized] public float[] inverseMasses;
	[HideInInspector] [NonSerialized] public float[] solidRadii;
	[HideInInspector] [NonSerialized] public int[] phases;

	// constraint groups:
	[HideInInspector] public int[] constraintsOrder = new int[]{0,1,2,3,4,5,6,7,8,9};
	[HideInInspector] [NonSerialized] public ObiDistanceConstraintGroup distanceConstraints;
	[HideInInspector] [NonSerialized] public ObiBendingConstraintGroup bendingConstraints;					
	[HideInInspector] [NonSerialized] public ObiSkinConstraintGroup skinConstraints;
	[HideInInspector] [NonSerialized] public ObiAerodynamicConstraintGroup aerodynamicConstraints;
	[HideInInspector] [NonSerialized] public ObiVolumeConstraintGroup volumeConstraints;
	[HideInInspector] [NonSerialized] public ObiTetherConstraintGroup tetherConstraints;
	[HideInInspector] [NonSerialized] public ObiPinConstraintGroup pinConstraints;
	
	// constraint parameters:
	public Oni.ConstraintParameters distanceConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Sequential,3);
	public Oni.ConstraintParameters bendingConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters particleCollisionConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters collisionConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters skinConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Sequential,3);
	public Oni.ConstraintParameters volumeConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters tetherConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters pinConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters densityConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	
	private GCHandle activeHandle;
	private GCHandle materialsHandle;
	private GCHandle materialIndicesHandle;
	private GCHandle fluidMaterialsHandle;
	private GCHandle fluidMaterialIndicesHandle;
	private GCHandle renderablePositionsHandle;
	private GCHandle positionsHandle;
	private GCHandle predictedHandle;
	private GCHandle previousHandle;
	private GCHandle velocitiesHandle;
	private GCHandle inverseMassesHandle;
	private GCHandle solidRadiiHandle;
	private GCHandle phasesHandle;

	private IntPtr oniSolver;
	private ObiCollisionMaterial defaultMaterial;
	private ObiEmitterMaterial defaultFluidMaterial;
	private Oni.Bounds bounds = new Oni.Bounds();
	private GCHandle boundsHandle;
 
 	private bool initialized;
	private bool isVisible = true;
 
	public struct BodyInformation{
		public float mass;
		public Vector3 centerOfMass;
		public Vector3 centerOfMassVelocity;
	}

	public IntPtr Solver
	{
		get{return oniSolver;}
	}

	public Oni.Bounds Bounds
	{
		get{return bounds;}
	}

	public bool IsVisible
	{
		get{return isVisible;}
	}

	void Start(){
		if (colliderGroup != null)
			Oni.SetColliderGroup(oniSolver,colliderGroup.oniColliderGroup);
	}

	void Awake(){
		if (Application.isPlaying) //only during game.
			Initialize();
	}

	void OnDestroy(){
		if (Application.isPlaying) //only during game.
			Teardown();
	}

	void OnEnable(){
		if (!Application.isPlaying) //only in editor.
			Initialize();
		StartCoroutine("RunLateFixedUpdate");
	}
	
	void OnDisable(){
		if (!Application.isPlaying) //only in editor.
			Teardown();
		StopCoroutine("RunLateFixedUpdate");
	}
	
	private void RegisterConstraintGroup(Type constraintType, ObiSolverConstraintGroup group){
		constraintGroupMap[constraintType] = group;
	}

	public void Initialize(){

		// Tear everything down first:
		Teardown();
			
		try{

			// Create a default material (TODO: maybe expose this to the user?)
			defaultMaterial = ScriptableObject.CreateInstance<ObiCollisionMaterial>();
			defaultMaterial.hideFlags = HideFlags.HideAndDontSave;

			defaultFluidMaterial = ScriptableObject.CreateInstance<ObiEmitterMaterial>();
			defaultFluidMaterial.hideFlags = HideFlags.HideAndDontSave;
	
			// Create the Oni solver:
			oniSolver = Oni.CreateSolver(maxParticles,92,interactionRadius);
			
			actors = new List<ObiActor>();
			allocatedParticles = new HashSet<int>();
			activeParticles = new HashSet<int>();
			materialIndices = new int[maxParticles];
			fluidMaterialIndices = new int[maxParticles];
			renderablePositions = new Vector3[maxParticles];
			positions = new Vector3[maxParticles];
			previousPositions = new Vector3[maxParticles];
			predictedPositions = new Vector3[maxParticles];
			velocities = new Vector3[maxParticles];
			inverseMasses = new float[maxParticles];
			solidRadii = new float[maxParticles];
			phases = new int[maxParticles];		
			
			// Create constraint groups:
			distanceConstraints = new ObiDistanceConstraintGroup(this);
			bendingConstraints = new ObiBendingConstraintGroup(this);
			skinConstraints = new ObiSkinConstraintGroup(this);	
			aerodynamicConstraints = new ObiAerodynamicConstraintGroup(this);
			volumeConstraints = new ObiVolumeConstraintGroup(this);
			tetherConstraints = new ObiTetherConstraintGroup(this);
			pinConstraints = new ObiPinConstraintGroup(this);

			// Register constraint groups:
			RegisterConstraintGroup(typeof(ObiDistanceConstraints), distanceConstraints);
			RegisterConstraintGroup(typeof(ObiBendingConstraints), bendingConstraints);
			RegisterConstraintGroup(typeof(ObiSkinConstraints), skinConstraints);
			RegisterConstraintGroup(typeof(ObiAerodynamicConstraints), aerodynamicConstraints);
			RegisterConstraintGroup(typeof(ObiVolumeConstraints), volumeConstraints);
			RegisterConstraintGroup(typeof(ObiTetherConstraints), tetherConstraints);
			RegisterConstraintGroup(typeof(ObiPinConstraints), pinConstraints);
			
			// Pin all memory so that the GC won't deallocate it before we are done.
			materialIndicesHandle = Oni.PinMemory(materialIndices);
			fluidMaterialIndicesHandle = Oni.PinMemory(fluidMaterialIndices);
			renderablePositionsHandle = Oni.PinMemory(renderablePositions);
			positionsHandle = Oni.PinMemory(positions);
			predictedHandle = Oni.PinMemory(predictedPositions);
			previousHandle = Oni.PinMemory(previousPositions);
			velocitiesHandle = Oni.PinMemory(velocities);
			inverseMassesHandle = Oni.PinMemory(inverseMasses);
			solidRadiiHandle = Oni.PinMemory(solidRadii);
			phasesHandle = Oni.PinMemory(phases);
			
			// Send all pointers to pinned memory to the solver:
			Oni.SetMaterialIndices(oniSolver,materialIndicesHandle.AddrOfPinnedObject());
			Oni.SetFluidMaterialIndices(oniSolver,fluidMaterialIndicesHandle.AddrOfPinnedObject());
			Oni.SetRenderableParticlePositions(oniSolver,renderablePositionsHandle.AddrOfPinnedObject());
			Oni.SetParticlePositions(oniSolver,positionsHandle.AddrOfPinnedObject());
			Oni.SetPredictedParticlePositions(oniSolver,predictedHandle.AddrOfPinnedObject());
			Oni.SetPreviousParticlePositions(oniSolver,previousHandle.AddrOfPinnedObject());
			Oni.SetParticleVelocities(oniSolver,velocitiesHandle.AddrOfPinnedObject());
			Oni.SetParticleInverseMasses(oniSolver,inverseMassesHandle.AddrOfPinnedObject());
			Oni.SetParticleSolidRadii(oniSolver,solidRadiiHandle.AddrOfPinnedObject());
			Oni.SetParticlePhases(oniSolver,phasesHandle.AddrOfPinnedObject());
	
			// Initialize materials:
			UpdateSolverMaterials();
			UpdateFluidMaterials();
			
			// Initialize parameters:
			UpdateParameters();
				
			boundsHandle = Oni.PinMemory(bounds);
			
		}catch (Exception exception){
			Debug.LogException(exception);
		}finally{
			initialized = true;
		};

	}

	private void Teardown(){
	
		if (!initialized) return;
		
		try{

			while (actors.Count > 0){
				actors[actors.Count-1].RemoveFromSolver(null);
			}
	
			// Unpin all memory so that the GC can deallocate it.
			Oni.UnpinMemory(activeHandle);
			Oni.UnpinMemory(materialIndicesHandle);
			Oni.UnpinMemory(materialsHandle);
			Oni.UnpinMemory(fluidMaterialIndicesHandle);
			Oni.UnpinMemory(fluidMaterialsHandle);
			Oni.UnpinMemory(renderablePositionsHandle);
			Oni.UnpinMemory(positionsHandle);
			Oni.UnpinMemory(predictedHandle);
			Oni.UnpinMemory(previousHandle);
			Oni.UnpinMemory(velocitiesHandle);
			Oni.UnpinMemory(inverseMassesHandle);
			Oni.UnpinMemory(solidRadiiHandle);
			Oni.UnpinMemory(phasesHandle);

 			Oni.UnpinMemory(boundsHandle);
			
			if (distanceConstraints != null) distanceConstraints.Teardown();
			if (bendingConstraints != null) bendingConstraints.Teardown();
			if (skinConstraints != null) skinConstraints.Teardown();
			if (aerodynamicConstraints != null) aerodynamicConstraints.Teardown();
			if (volumeConstraints != null) volumeConstraints.Teardown();
			if (tetherConstraints != null) tetherConstraints.Teardown();
			if (pinConstraints != null) pinConstraints.Teardown();
				
			Oni.DestroySolver(oniSolver);
			
			GameObject.DestroyImmediate(defaultMaterial);
			GameObject.DestroyImmediate(defaultFluidMaterial);
		
		}catch (Exception exception){
			Debug.LogException(exception);
		}finally{
			initialized = false;
		}
	}

	/**
	 * Adds a new transform to the solver and returns its ID.
	 */
	public int SetActor(int ID, ObiActor actor)
	{

		// Add the transform, as its new.
		if (ID < 0 || ID >= actors.Count){
	
			int index = actors.Count;

            // Use the free slot to insert the transform:
			actors.Add(actor);

			// Update materials, in case the actor has a new one.
			UpdateSolverMaterials();
			UpdateFluidMaterials();

			// Return the transform index as its ID
			return index;

		}
		// The transform is already there.
		else{

			actors[ID] = actor;
			UpdateSolverMaterials();
			UpdateFluidMaterials();
			return ID;

		}

	}

	/**
 	 * Removes an actor from the solver and returns its ID.
	 */
	public void RemoveActor(int ID){
		
		if (ID < 0 || ID >= actors.Count) return;

		// Update actor ID for affected actors:
		for (int i = ID+1; i < actors.Count; i++){
			actors[i].actorID--;
		}

		actors.RemoveAt(ID); 

		// Update materials, in case the actor had one.
		UpdateSolverMaterials();
		UpdateFluidMaterials();
	}

	/**
	 * Reserves a certain amount of particles and returns their indices in the 
	 * solver arrays.
	 */
	public List<int> AllocateParticles(int numParticles){

		if (allocatedParticles == null)
			return null;

		List<int> allocated = new List<int>();
		for (int i = 0; i < maxParticles && allocated.Count < numParticles; i++){
			if (!allocatedParticles.Contains(i)){
				allocated.Add(i);
			}
		}

		// could not allocate enough particles.
		if (allocated.Count < numParticles){
			return null; 
		}
   
        // allocation was successful:
		allocatedParticles.UnionWith(allocated);
		activeParticles.UnionWith(allocated);
		UpdateActiveParticles();          
		return allocated;

	}

	/**
	 * Frees a list of particles.
	 */
	public void FreeParticles(List<int> indices){
		
		if (allocatedParticles == null || indices == null)
			return;
		
		allocatedParticles.ExceptWith(indices);
		activeParticles.ExceptWith(indices);

		UpdateActiveParticles(); 
		
	}

	/**
	 * Updates solver parameters, sending them to the Oni library.
	 */
	public void UpdateParameters(){

		GCHandle handle = Oni.PinMemory(parameters);
		Oni.SetSolverParameters(oniSolver,handle.AddrOfPinnedObject());
		Oni.UnpinMemory(handle);

		handle = Oni.PinMemory(distanceConstraintParameters);
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Distance,handle.AddrOfPinnedObject());
		Oni.UnpinMemory(handle);
		
		handle = Oni.PinMemory(bendingConstraintParameters);
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Bending,handle.AddrOfPinnedObject());
		Oni.UnpinMemory(handle);

		handle = Oni.PinMemory(particleCollisionConstraintParameters);
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.ParticleCollision,handle.AddrOfPinnedObject());
		Oni.UnpinMemory(handle);

		handle = Oni.PinMemory(collisionConstraintParameters);
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Collision,handle.AddrOfPinnedObject());
		Oni.UnpinMemory(handle);

		handle = Oni.PinMemory(densityConstraintParameters);
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Density,handle.AddrOfPinnedObject());
		Oni.UnpinMemory(handle);
		
		handle = Oni.PinMemory(skinConstraintParameters);
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Skin,handle.AddrOfPinnedObject());
		Oni.UnpinMemory(handle);
		
		handle = Oni.PinMemory(volumeConstraintParameters);
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Volume,handle.AddrOfPinnedObject());
		Oni.UnpinMemory(handle);
		
		handle = Oni.PinMemory(tetherConstraintParameters);
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Tether,handle.AddrOfPinnedObject());
		Oni.UnpinMemory(handle);

		handle = Oni.PinMemory(pinConstraintParameters);
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Pin,handle.AddrOfPinnedObject());
		Oni.UnpinMemory(handle);

		handle = Oni.PinMemory(constraintsOrder);
		Oni.SetConstraintsOrder(oniSolver,handle.AddrOfPinnedObject());
		Oni.UnpinMemory(handle);
    }

	/**
	 * Updates the active particles array.
	 */
	public void UpdateActiveParticles(){

		// Get allocated particles and remove the inactive ones:
		int[] activeArray = new int[activeParticles.Count];

		activeParticles.CopyTo(activeArray);
		Oni.UnpinMemory(activeHandle);
		activeHandle = Oni.PinMemory(activeArray);
		Oni.SetActiveParticles(oniSolver,activeHandle.AddrOfPinnedObject(),activeArray.Length);

	}

	public void UpdateFluidMaterials(){

		HashSet<ObiEmitterMaterial> materialsSet = new HashSet<ObiEmitterMaterial>();
		List<ObiEmitterMaterial> materials = new List<ObiEmitterMaterial>();

		// The default material must always be present.
		materialsSet.Add (defaultFluidMaterial);		
		materials.Add(defaultFluidMaterial);

		// Setup all materials used by particle actors:
		foreach (ObiActor actor in actors){
			
			ObiEmitter em = actor as ObiEmitter;
				if (em == null) continue;

			int materialIndex = 0;

			if (!materialsSet.Contains(em.emitterMaterial)){
				materialIndex = materials.Count;
				materials.Add(em.emitterMaterial);
				materialsSet.Add(em.emitterMaterial);
			}else{
				materialIndex = materials.IndexOf(em.emitterMaterial);
			}
			
			// Update material index for all actor particles:
			for(int i = 0; i < actor.particleIndices.Count; i++){
				fluidMaterialIndices[actor.particleIndices[i]] = materialIndex;
			}
		}

		Oni.UnpinMemory(fluidMaterialsHandle);
		fluidMaterialsHandle = Oni.PinMemory(materials.ConvertAll<Oni.FluidMaterial>(a => a.GetEquivalentOniMaterial()).ToArray());
		Oni.SetFluidMaterials(oniSolver,fluidMaterialsHandle.AddrOfPinnedObject(),materials.Count);
	}

	public void UpdateSolverMaterials(){

		HashSet<ObiCollisionMaterial> materialsSet = new HashSet<ObiCollisionMaterial>();
		List<ObiCollisionMaterial> materials = new List<ObiCollisionMaterial>();

		// The default material must always be present.
		materialsSet.Add (defaultMaterial);		
		materials.Add(defaultMaterial);

		// Setup all materials used by particle actors:
		foreach (ObiActor actor in actors){
			
			int materialIndex = 0;

			if (actor.material != null){
				if (!materialsSet.Contains(actor.material)){
					materialIndex = materials.Count;
					materials.Add(actor.material);
					materialsSet.Add(actor.material);
				}else{
					materialIndex = materials.IndexOf(actor.material);
				}
			}

			// Update material index for all actor particles:
			for(int i = 0; i < actor.particleIndices.Count; i++){
				materialIndices[actor.particleIndices[i]] = materialIndex;
			}
		}

		// Setup all materials used by colliders:
		if (colliderGroup != null){
			foreach (Collider c in colliderGroup.colliders){
			
				if (c == null) continue;

				ObiCollider oc = c.GetComponent<ObiCollider>();
	
				if (oc == null) continue;
					
				oc.materialIndex = 0;
				
				if (oc.material == null) continue;
	
				if (!materialsSet.Contains(oc.material)){
					oc.materialIndex = materials.Count;
					materials.Add(oc.material);
					materialsSet.Add(oc.material);
				}else{
					oc.materialIndex = materials.IndexOf(oc.material);
				}
			}
		}

		Oni.UnpinMemory(materialsHandle);
		materialsHandle = Oni.PinMemory(materials.ConvertAll<Oni.CollisionMaterial>(a => a.GetEquivalentOniMaterial()).ToArray());
		Oni.SetCollisionMaterials(oniSolver,materialsHandle.AddrOfPinnedObject());
	}

	public void AccumulateSimulationTime(float dt){

		Oni.AddSimulationTime(oniSolver,dt);

	}

	public void SimulateStep(float stepTime){

		foreach(ObiActor actor in actors)
            actor.OnSolverStepBegin();

		// Update all collider and rigidbody information, so that the solver works with up-to-date stuff:
		if (colliderGroup != null)
			colliderGroup.UpdateBodiesInfo();

		// Update the solver:
		Oni.UpdateSolver(oniSolver, stepTime);

		// Apply modified rigidbody velocities and torques properties back:
		if (colliderGroup != null)
			colliderGroup.UpdateVelocities();

		TriggerCollisionEvents();
		
		foreach(ObiActor actor in actors)
            actor.OnSolverStepEnd();

	} 

	public void EndFrame(float frameDelta){

		foreach(ObiActor actor in actors)
            actor.OnSolverPreInterpolation();

		Oni.ApplyPositionInterpolation(oniSolver, frameDelta);

		Oni.GetBounds(oniSolver,boundsHandle.AddrOfPinnedObject());
		bounds = (Oni.Bounds)boundsHandle.Target; //TODO: why is this needed, if memory is shared?

		CheckVisibility();

		if (OnBeforeActorsFrameEnd != null)
			OnBeforeActorsFrameEnd(this,null);
		
		foreach(ObiActor actor in actors)
            actor.OnSolverFrameEnd();

	}

	private void TriggerCollisionEvents(){
	
		int numCollisions = Oni.GetConstraintCount(oniSolver,7);

		if (numCollisions > 0 && OnCollision != null){

			int[] indices = new int[numCollisions*2];
			float[] distances = new float[numCollisions];
			Vector3[] points = new Vector3[numCollisions];
			Vector3[] normals = new Vector3[numCollisions];
			float[] normalImpulses = new float[numCollisions];
			float[] tangentImpulses = new float[numCollisions];
			float[] stickImpulses = new float[numCollisions];
		
			GCHandle indicesHandle = Oni.PinMemory(indices);
			GCHandle distancesHandle = Oni.PinMemory(distances);	
			GCHandle pointsHandle = Oni.PinMemory(points);
			GCHandle normalsHandle = Oni.PinMemory(normals);
			GCHandle normalImpulsesHandle = Oni.PinMemory(normalImpulses);
			GCHandle tangentImpulsesHandle = Oni.PinMemory(tangentImpulses);
			GCHandle stickImpulsesHandle = Oni.PinMemory(stickImpulses);
	
			Oni.GetCollisionIndices(oniSolver,indicesHandle.AddrOfPinnedObject(),numCollisions);
			Oni.GetCollisionDistances(oniSolver,distancesHandle.AddrOfPinnedObject(),numCollisions);
			Oni.GetCollisionPoints(oniSolver,pointsHandle.AddrOfPinnedObject(),numCollisions);
			Oni.GetCollisionNormals(oniSolver,normalsHandle.AddrOfPinnedObject(),numCollisions);
			Oni.GetCollisionNormalImpulses(oniSolver,normalImpulsesHandle.AddrOfPinnedObject(),numCollisions);
			Oni.GetCollisionTangentImpulses(oniSolver,tangentImpulsesHandle.AddrOfPinnedObject(),numCollisions);
			Oni.GetCollisionStickImpulses(oniSolver,stickImpulsesHandle.AddrOfPinnedObject(),numCollisions);
	
			OnCollision(this,new ObiCollisionEventArgs(indices,distances,points,normals,normalImpulses,tangentImpulses,stickImpulses));
		
			Oni.UnpinMemory(indicesHandle);
			Oni.UnpinMemory(distancesHandle);
			Oni.UnpinMemory(pointsHandle);
			Oni.UnpinMemory(normalsHandle);
			Oni.UnpinMemory(normalImpulsesHandle);
			Oni.UnpinMemory(tangentImpulsesHandle);
			Oni.UnpinMemory(stickImpulsesHandle);
		}
	}

	/**
	 * Checks if any particle in the solver is visible from at least one camera. If so, sets isVisible to true, false otherwise.
	 */
	private void CheckVisibility(){

		isVisible = false;

		Bounds unityBounds = new UnityEngine.Bounds(bounds.Center,bounds.Size);

		foreach (Camera cam in Camera.allCameras){
        	Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
       		if (GeometryUtility.TestPlanesAABB(planes, unityBounds)){
				isVisible = true;
				return;
			}
		}
	}
    
    void Update(){

		if (OnFrameBegin != null)
			OnFrameBegin(this,null);

		foreach(ObiActor actor in actors)
            actor.OnSolverFrameBegin();

		if (simulate && (simulateWhenInvisible || IsVisible)){
			AccumulateSimulationTime(Time.deltaTime);
		}
	}

	IEnumerator RunLateFixedUpdate() {
         while (true) {
             yield return new WaitForFixedUpdate();
             LateFixedUpdate();
         }
     }

     void LateFixedUpdate()
     {
        if (simulate && (simulateWhenInvisible || IsVisible)){

			if (OnStepBegin != null)
				OnStepBegin(this,null);

			SimulateStep(Time.fixedDeltaTime);

			if (OnStepEnd != null)
				OnStepEnd(this,null);
		}
     }

	private void LateUpdate(){
   
		EndFrame (Time.fixedDeltaTime);

		if (OnFrameEnd != null)
			OnFrameEnd(this,null);
	}

	public void CommitConstraints(Type constraintType){
		try{
			constraintGroupMap[constraintType].CommitToSolver();
		}catch{
			Debug.LogError("Constraint type is not registered.");
		}
	}

	public void AddActiveConstraint(Type constraintType, int index){
		try{
			constraintGroupMap[constraintType].activeConstraints.Add(index);
		}catch{
			Debug.LogError("Constraint type is not registered.");
		}
	}

	public void RemoveActiveConstraint(Type constraintType, int index){
		try{
			constraintGroupMap[constraintType].activeConstraints.Remove(index);
		}catch{
			Debug.LogError("Constraint type is not registered.");
		}
	}

	public void UpdateActiveConstraints(Type constraintType){
		try{
			constraintGroupMap[constraintType].CommitActive();
		}catch{
			Debug.LogError("Constraint type is not registered.");
		}
	}

}

}

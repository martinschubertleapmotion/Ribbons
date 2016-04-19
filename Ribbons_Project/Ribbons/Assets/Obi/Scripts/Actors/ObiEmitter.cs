using UnityEngine;
using System;
using System.Collections;


namespace Obi{

	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[AddComponentMenu("Physics/Obi/Obi Emitter")]
	[RequireComponent(typeof (ObiDistanceConstraints))]
	public class ObiEmitter : ObiActor {

		[Tooltip("Emitter material used. This controls the behavior of the emitted particles.")]
		public ObiEmitterMaterial emitterMaterial = null;

		[Tooltip("Shape of this emitter.")]
		public ObiEmitterShape emitterShape = null;
	
		[Tooltip("Amount of solver particles used by this emitter.")]
		public int numParticles = 1000;

		[Tooltip("Amount of particles emitted each second.")]
		public float emissionRate = 10;

		[Tooltip("Is the burst size automatically calculated from the emitter distribution?.")]
		public bool automaticBurstSize = true;

		[Tooltip("Amount of particles emitted simultaneously in a single frame.")]
		public int burstSize = 10;

		[Tooltip("Lifespan of each particle.")]
		public float lifespan = 4;

		[Tooltip("Initial velocity of particles.")]
		public Vector3 initialVelocity = Vector3.zero;

		private int activeParticleCount = 0;			/**< number of currently active particles*/
		[HideInInspector] public float[] life;			/**< per particle remaining life in seconds.*/
		[HideInInspector] public float[] mass;			/**< per particle mass.*/

		private ObiDistanceConstraints distanceConstraints;
		private float unemittedTime = 0;

		public ObiDistanceConstraints DistanceConstraints{
			get{return distanceConstraints;}
		}
	
		public override void Awake()
		{
			base.Awake();
			
			distanceConstraints = GetComponent<ObiDistanceConstraints>();
		}

		public override void OnEnable(){
			
			base.OnEnable();

			// Enable constraints affecting this emitter:
			distanceConstraints.OnEnable();

		}
		
		public override void OnDisable(){
			
			base.OnDisable();

			// Disable constraints affecting this emitter:
			distanceConstraints.OnDisable();
			
		}

		public override bool AddToSolver(object info){
			
			if (Initialized && base.AddToSolver(info)){
				distanceConstraints.AddToSolver(this);
				return true;
			}
			return false;
		}
		
		public override bool RemoveFromSolver(object info){
			
			bool removed = false;

			try{
				if (distanceConstraints != null)
					distanceConstraints.RemoveFromSolver(null);
			}catch(Exception e){
				Debug.LogException(e);
			}finally{
				removed = base.RemoveFromSolver(info);
			}
			return removed;
		}

		/**
	 	* Generates the particle based physical representation of the emitter. This is the initialization method for the rope object
		* and should not be called directly once the object has been created.
	 	*/
		public IEnumerator GeneratePhysicRepresentationForMesh()
		{		
			initialized = false;			
			initializing = true;

			RemoveFromSolver(null);

			active = new bool[numParticles];
			life = new float[numParticles];
			positions = new Vector3[numParticles];
			velocities = new Vector3[numParticles];
			invMasses  = new float[numParticles];
			solidRadii = new float[numParticles];
			phases = new int[numParticles];
			mass = new float[numParticles];

			// density = mass/(2*radius)^3
			float restDistance = (emitterMaterial != null) ? emitterMaterial.restDistance : 0.1f ;
			float restDensity = (emitterMaterial != null) ? emitterMaterial.restDensity : 1000 ;
			float pmass = Mathf.Pow(restDistance,3) * restDensity;
			
			for (int i = 0; i < numParticles; i++){

				active[i] = false;
				life[i] = 0;
				mass[i] = pmass;
				invMasses[i] = 1/mass[i];
				positions[i] = Vector3.zero;
				solidRadii[i] = restDistance*0.5f;
				phases[i] = Oni.MakePhase(gameObject.layer,(selfCollisions?Oni.ParticlePhase.SelfCollide:0) |
														   ((emitterMaterial != null && emitterMaterial.isFluid)?Oni.ParticlePhase.Fluid:0));

				if (i % 100 == 0)
					yield return new CoroutineJob.ProgressInfo("ObiEmitter: generating particles...",i/(float)numParticles);

			}

			distanceConstraints.Initialize();
			
			AddToSolver(null);

			initializing = false;
			initialized = true;
			
		}

		public override void UpdateParticlePhases(){
	
			if (!InSolver) return;
	
			for(int i = 0; i < particleIndices.Count; i++){
				phases[i] = Oni.MakePhase(gameObject.layer,(selfCollisions?Oni.ParticlePhase.SelfCollide:0) |
														   ((emitterMaterial != null && emitterMaterial.isFluid)?Oni.ParticlePhase.Fluid:0));
			}
			PushDataToSolver(new ObiSolverData(ObiSolverData.ParticleData.PHASES));
		}

		public void ResetParticlePosition(int index){	

			if (emitterShape == null){

				solver.positions[particleIndices[index]] = transform.position;
				solver.predictedPositions[particleIndices[index]] = transform.position;
				solver.previousPositions[particleIndices[index]] = transform.position;
				solver.velocities[particleIndices[index]] = transform.TransformVector(initialVelocity);

			}else{

				Vector3 spawnPosition = transform.TransformPoint(emitterShape.GetDistributionPoint());

				solver.positions[particleIndices[index]] = spawnPosition;
				solver.predictedPositions[particleIndices[index]] = spawnPosition;
				solver.previousPositions[particleIndices[index]] = spawnPosition;
				solver.velocities[particleIndices[index]] = transform.TransformVector(initialVelocity);

			}
		}

		/**
		 * Asks the emiter to emits a new particle. Returns whether the emission was succesful.
		 */
		public bool EmitParticle(){

			if (activeParticleCount == numParticles) return false;

			life[activeParticleCount] = lifespan;
			
			// move particle to its spawn position:
			ResetParticlePosition(activeParticleCount);

			// now there's one active particle more:
			active[activeParticleCount] = true;
			activeParticleCount++;

			return true;

		}

		/**
		 * Asks the emiter to kill a new particle. Returns whether the kill was succesful.
		 */
		public bool KillParticle(int index){

			if (activeParticleCount == 0 || index >= activeParticleCount) return false;

			// reduce amount of active particles:
			activeParticleCount--;
			active[activeParticleCount] = false; 

			// swap solver particle indices:
			int temp = particleIndices[activeParticleCount];
			particleIndices[activeParticleCount] = particleIndices[index];
			particleIndices[index] = temp;

			// also swap lifespans, so the swapped particle enjoys the rest of its life! :)
			float tempLife = life[activeParticleCount];
			life[activeParticleCount] = life[index];
			life[index] = tempLife;

			return true;
			
		}

		public override void OnSolverFrameEnd(){

			base.OnSolverFrameEnd();

			bool emitted = false;
			bool killed = false;

			// Update lifetime and kill dead particles:
			for (int i = activeParticleCount-1; i >= 0; --i){
				life[i] -= Time.deltaTime;

				if (life[i] <= 0){
					killed |= KillParticle(i);	
				}
			}

			// Calculate burst size:
			int effectiveBurstSize = (emitterShape != null && automaticBurstSize) ? emitterShape.DistributionPointsCount : burstSize;

			// Emit new particles:
			unemittedTime += Time.deltaTime;
			while (unemittedTime > 0){
				for (int i = 0; i < effectiveBurstSize; ++i){
					emitted |= EmitParticle();
					unemittedTime -= 1 / emissionRate;
				}
			}

			// Push active array to solver if any particle has been killed or emitted this frame.
			if (emitted || killed){
				PushDataToSolver(new ObiSolverData(ObiSolverData.ParticleData.ACTIVE_STATUS));		
			}	


			/*if (!isActiveAndEnabled || particleRenderer == null)
			return;

			// Update particle renderer values:
			Color[] colors = new Color[activeParticleCount];
			Vector3[] drawPos = new Vector3[activeParticleCount];
			Vector2[] info = new Vector2[activeParticleCount];
			for (int i = 0; i < activeParticleCount; i++){
				drawPos[i] = particleRenderer.transform.InverseTransformPoint(solver.renderablePositions[particleIndices[i]]);
				colors[i] = particleRenderer.particleColor;
				info[i] = new Vector2(0,solidRadii[i]);
			}
			
			particleRenderer.SetParticles(drawPos,solidRadii,colors,info);*/

		}
	}
}

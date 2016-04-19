using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Obi{

/**
 * ObiColliderGroup holds references to all colliders and rigidbodies that any given ObiSolver should be aware of. Multiple ObiSolvers
 * can share a single ObiColliderGroup.
 */
[ExecuteInEditMode]
public class ObiColliderGroup : MonoBehaviour
{

	public List<Collider> colliders = new List<Collider>();

	[HideInInspector]public IntPtr oniColliderGroup;

	Oni.Collider[] cols;
	Oni.Rigidbody[] bodies;
	List<Oni.Collider> oniColliders = new List<Oni.Collider>();
	List<Oni.Rigidbody> oniRigidbodies = new List<Oni.Rigidbody>();
	private List<Oni.SphereShape> oniSpheres = new List<Oni.SphereShape>();
	private List<Oni.BoxShape> oniBoxes = new List<Oni.BoxShape>();
	private List<Oni.CapsuleShape> oniCapsules = new List<Oni.CapsuleShape>();
	private List<Oni.HeightmapShape> oniHeightmaps = new List<Oni.HeightmapShape>();

	private GCHandle collidersHandle;
	private GCHandle rigidbodiesHandle;
	private GCHandle spheresHandle;
	private GCHandle boxesHandle;
	private GCHandle capsulesHandle;
	private GCHandle heightmapsHandle;

	Dictionary<int,int> rigidbodyIDs = new Dictionary<int,int>();  /**<holds pairs of <instanceid,index in oniRigidbodies>, to help with rigidbody assignment.*/
	
	[NonSerialized] public Dictionary <TerrainCollider,Oni.HeightData> heightData = new Dictionary<TerrainCollider,Oni.HeightData>();
	
	void OnEnable(){
		oniColliderGroup = Oni.CreateColliderGroup();
		UpdateBodiesInfo();
	}
	
	void OnDisable(){
		Oni.DestroyColliderGroup(oniColliderGroup);
		Oni.UnpinMemory(collidersHandle);
		Oni.UnpinMemory(spheresHandle);
		Oni.UnpinMemory(boxesHandle);
		Oni.UnpinMemory(capsulesHandle);
		Oni.UnpinMemory(heightmapsHandle);
		
		foreach(Oni.HeightData data in heightData.Values){
			data.UnpinData();
		}
		heightData.Clear();
    }

	public int GetIndexOfCollider(Collider c){
		if (colliders != null && c != null){
			return colliders.IndexOf(c);
		}
		return -1;
	}

	public void UpdateTerrainHeightInfo(TerrainCollider terrain){
		if (heightData.ContainsKey(terrain)){
			heightData[terrain].UpdateHeightData();
		}
	}	

	public void UpdateBodiesInfo(){

		oniColliders.Clear();
		oniRigidbodies.Clear();
		oniSpheres.Clear();
		oniBoxes.Clear();
		oniCapsules.Clear();
		oniHeightmaps.Clear();

		rigidbodyIDs.Clear();	

		for(int i = 0; i < colliders.Count; i++)
		{
			Collider source = colliders[i];
			if (source == null) continue;

			Rigidbody rb = colliders[i].GetComponentInParent<Rigidbody>();
			ObiCollider oc = colliders[i].GetComponent<ObiCollider>();			

			// Get the adequate rigidBodyIndex. If several colliders share a rigidbody, they'll get the same rigidBodyIndex.
			int rigidBodyIndex = -1;
			if (rb != null){

				if (!rigidbodyIDs.TryGetValue(rb.GetInstanceID(),out rigidBodyIndex)){

					rigidBodyIndex = oniRigidbodies.Count;
					oniRigidbodies.Add(new Oni.Rigidbody(rb));
					rigidbodyIDs[rb.GetInstanceID()] = rigidBodyIndex;

				}

			}
	
			if (source is SphereCollider){
				oniColliders.Add(new Oni.Collider(source,Oni.ShapeType.Sphere,oniSpheres.Count,rigidBodyIndex,(oc != null)?oc.materialIndex:0));
				oniSpheres.Add(new Oni.SphereShape(source as SphereCollider));	
			}else if (source is BoxCollider){
				oniColliders.Add(new Oni.Collider(source,Oni.ShapeType.Box,oniBoxes.Count,rigidBodyIndex,(oc != null)?oc.materialIndex:0));
				oniBoxes.Add(new Oni.BoxShape(source as BoxCollider));	
			}else if (source is CapsuleCollider){
				oniColliders.Add(new Oni.Collider(source,Oni.ShapeType.Capsule,oniCapsules.Count,rigidBodyIndex,(oc != null)?oc.materialIndex:0));
				oniCapsules.Add(new Oni.CapsuleShape(source as CapsuleCollider));	
			}else if (source is CharacterController){
				oniColliders.Add(new Oni.Collider(source,Oni.ShapeType.Capsule,oniCapsules.Count,rigidBodyIndex,(oc != null)?oc.materialIndex:0));
				oniCapsules.Add(new Oni.CapsuleShape(source as CharacterController));	
			}else if (source is TerrainCollider){

				TerrainCollider tc = source as TerrainCollider;
				if (!heightData.ContainsKey(tc)){
					heightData[tc] = new Oni.HeightData(source as TerrainCollider);
				}

				oniColliders.Add(new Oni.Collider(source,Oni.ShapeType.Heightmap,oniHeightmaps.Count,rigidBodyIndex,(oc != null)?oc.materialIndex:0));
				oniHeightmaps.Add(new Oni.HeightmapShape(source as TerrainCollider,heightData[tc].AddrOfHeightData()));
				
			}else{
				Debug.LogWarning(source.GetType()+" not supported by Obi. Ignoring it.");
			}
			
		}

		UpdateColliders();
		UpdateRigidbodies();
		UpdateSpheres();
		UpdateBoxes();
		UpdateCapsules();
		UpdateHeightmaps();
	}

	/**
	 * Injects back into the rigidbodies the new velocities calculated by the solver in response to patrticle interactions:
 	 */
	public void UpdateVelocities(){ 

		if (bodies != null)
		{	
			for (int i = 0; i < colliders.Count; ++i){

				if (colliders[i] == null) continue;

				int rigidBodyIndex = -1;
				Rigidbody rb = colliders[i].GetComponentInParent<Rigidbody>();

				if (rb != null && rigidbodyIDs.TryGetValue(rb.GetInstanceID(),out rigidBodyIndex)){
					
					rb.velocity = bodies[rigidBodyIndex].linearVelocity;
					rb.angularVelocity = bodies[rigidBodyIndex].angularVelocity;
					rigidBodyIndex++;
				}

			}
		}

	}

	void UpdateColliders(){
		Oni.UnpinMemory(collidersHandle);
		cols = oniColliders.ToArray();
		collidersHandle = Oni.PinMemory(cols);
		Oni.SetColliders(oniColliderGroup,collidersHandle.AddrOfPinnedObject(),cols.Length);
	}

	void UpdateRigidbodies(){
		Oni.UnpinMemory(rigidbodiesHandle);
		bodies = oniRigidbodies.ToArray();
		rigidbodiesHandle = Oni.PinMemory(bodies);
		Oni.SetRigidbodies(oniColliderGroup,rigidbodiesHandle.AddrOfPinnedObject(), bodies.Length);
	}

	void UpdateSpheres(){
		Oni.UnpinMemory(spheresHandle);
		spheresHandle = Oni.PinMemory(oniSpheres.ToArray());
		Oni.SetSphereShapes(oniColliderGroup,spheresHandle.AddrOfPinnedObject());
	}
	void UpdateBoxes(){
		Oni.UnpinMemory(boxesHandle);
		boxesHandle = Oni.PinMemory(oniBoxes.ToArray());
		Oni.SetBoxShapes(oniColliderGroup,boxesHandle.AddrOfPinnedObject());
    }
	void UpdateCapsules(){
		Oni.UnpinMemory(capsulesHandle);
		capsulesHandle = Oni.PinMemory(oniCapsules.ToArray());
		Oni.SetCapsuleShapes(oniColliderGroup,capsulesHandle.AddrOfPinnedObject());
	}
	void UpdateHeightmaps(){
		Oni.UnpinMemory(heightmapsHandle);
		heightmapsHandle = Oni.PinMemory(oniHeightmaps.ToArray());
		Oni.SetHeightmapShapes(oniColliderGroup,heightmapsHandle.AddrOfPinnedObject());
	}
}
}


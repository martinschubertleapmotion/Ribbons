using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{

/**
 * Half-Edge data structure. Used to simplify and accelerate adjacency queries for
 * a triangular mesh. You can check out http://www.flipcode.com/archives/The_Half-Edge_Data_Structure.shtml
 * for more information on the half-edge mesh representation.
 *
 * This particular implementation does not use pointers, in order to benefit from Unity's serialization system.
 * Instead it uses arrays and indices, which makes some operations more cumbersome due to the need of updating
 * indices across the whole structure when removing faces, edges, or vertices.
 */

public class ObiMeshTopology : ScriptableObject
{
	
	public Mesh input = null;
    public Vector3 scale = Vector3.one;
	[HideInInspector] public bool initialized = false;

	[NonSerialized] public EditorCoroutine generationRoutine = null;

	public class HEEdge{

		public int halfEdgeIndex;		  /**< Index to one of the half-edges in this edge. This is always the lower-index half-edge of the two.*/

		public HEEdge(int halfEdgeIndex){
			this.halfEdgeIndex = halfEdgeIndex;
		}

	}

	/**
	 * Represents a half-edge of the mesh.
	 */
	[Serializable]
	public class HEHalfEdge{

		public int index;			      /**<edge index in edge list.*/
		public int indexOnFace = -1;      /**<edge index on its face, -1 if this is a border edge.*/
		public int faceIndex = -1;	  	  /**<face index on face list, -1 if this is a border edge.*/
		public int nextEdgeIndex;    	  /**<next face edge index on edge list.*/
		public int pair = -1;		 	  /**<pair edge index.*/
		public bool torn = false;		  /**<whether this edge has been torn apart or not.*/

		public int endVertex;   		  /**<index of vertex at the end of the half-edge*/
		public int startVertex; 		  /**<index of vertex at the start of the half-edge*/

	}

	/**
	 * Represents a vertex of the mesh. Each HEVertex can correspond to more than one "physical" vertex in the mesh,
	 * since some vertices can be split at uv/normal/other attributes discontinuities. You can get all physical vertices
	 * shared by a single HEVertex using the physicalIDs list, which holds indices for the mesh.vertices array.
	 */
	[Serializable]
	public class HEVertex{	

		public Vector3 position;		/**<Position of this vertex in object space*/
		public List<int> physicalIDs;	/**<IDs of the physical mesh vertices associated to this vertex.*/
		public int index;				/**<vertex index on vertex list.*/
		public int halfEdgeIndex;		/**<index of outgoing half edge. In case of a border vertex, this is always a border edge.*/
		
		public HEVertex(Vector3 position, int physicalIndex){
			this.position = position;
			physicalIDs = new List<int>(){physicalIndex};
		}

	}

	/**
	 * Represents a face in the mesh.
	 */
	[Serializable]
	public class HEFace{
		public int index;							/**< face index on face list.*/
		public float area;							/**< area of the face*/
		public int[] edges = new int[3]; 	 		/**< indices of edges on the face.*/
	}
	
    [HideInInspector] public List<HEFace> heFaces = null;				/**<faces list*/
    [HideInInspector] public List<HEHalfEdge> heHalfEdges = null;		/**<half edges list*/
    [HideInInspector] public List<HEVertex> heVertices = null;			/**<vertices list*/
	
	[HideInInspector] public List<Quaternion> vertexOrientation = null; /**< per vertex orientation, based on tangent space.*/

	[HideInInspector][SerializeField] protected float _volume = 0;   			/**< mesh volume*/
	[HideInInspector][SerializeField] protected float _area = 0;	  				/**< mesh area*/
	[HideInInspector][SerializeField] protected float _avgEdgeLength = 0;		/**< average edge lenght*/
	[HideInInspector][SerializeField] protected float _minEdgeLength = 0;		/**< min edge lenght*/
	[HideInInspector][SerializeField] protected float _maxEdgeLength = 0;		/**< max edge lenght*/
	[HideInInspector][SerializeField] protected int _borderEdgeCount = 0;		/**< max edge lenght*/
	[HideInInspector][SerializeField] protected bool _closed = true;		
	[HideInInspector][SerializeField] protected bool _modified = false;
	[HideInInspector][SerializeField] protected bool _nonManifold = false;

	public bool Initialized{
		get{return initialized;}
	}

	public Mesh InputMesh{
		set{input = value;}
		get{return input;}
	}

	/**
	 * Returns volume for a closed mesh (readonly)
	 */
	public float MeshVolume{
		get{return _volume;}
	}

	public float MeshArea{
		get{return _area;}
	}

	public float AvgEdgeLength{
		get{return _avgEdgeLength;}
	}

	public float MinEdgeLength{
		get{return _minEdgeLength;}
	}

	public float MaxEdgeLength{
		get{return _maxEdgeLength;}
	}

	public int BorderEdgeCount{
		get{return _borderEdgeCount;}
	}

	public bool IsClosed{
		get{return _closed;}
	}

	public bool IsModified{
		get{return _modified;}
	} 

	public bool IsNonManifold{
		get{return _nonManifold;}
	}

    public void OnEnable(){
        // Initialize variables after serialization:
        if (heFaces == null)
            heFaces = new List<HEFace>();
        if (heVertices == null)
            heVertices = new List<HEVertex>();
        if (heHalfEdges == null)
            heHalfEdges = new List<HEHalfEdge>();
		if (vertexOrientation == null)
			vertexOrientation = new List<Quaternion>();
        if (scale == Vector3.zero)
            scale = Vector3.one;
    }
		 
	/**
	 * Analyzes the input mesh and populates the half-edge structure. Can be called as many times you want (for examples if the original mesh is modified).
	 */
	public IEnumerator Generate(){

		initialized = false;

		heFaces.Clear();
		heVertices.Clear();
		heHalfEdges.Clear();

		vertexOrientation.Clear();

		_area = 0;
		_volume = 0;
		_modified = false;
		_nonManifold = false;

		bool nonManifoldEdges = false;

		if (input != null){

			Dictionary<Vector3, HEVertex> vertexBuffer = new Dictionary<Vector3, HEVertex>();
			Dictionary<KeyValuePair<int,int>,HEHalfEdge> edgeBuffer = new Dictionary<KeyValuePair<int,int>,HEHalfEdge>();
			
			// Get copies of vertex and triangle buffers:
			Vector3[] vertices = input.vertices;
			int[] triangles = input.triangles;
			Vector3[] normals = input.normals;

			// first, create vertices:
			for(int i = 0; i < vertices.Length; i++){

				//if the vertex already exists, add physical vertex index to it.
				HEVertex vertex;
				if (vertexBuffer.TryGetValue(vertices[i], out vertex)){
					vertex.physicalIDs.Add(i);
				}else{
					vertex = new HEVertex(Vector3.Scale(vertices[i],scale),i);
				}

				vertexBuffer[vertices[i]] = vertex;

				if (i % 200 == 0)
					yield return new CoroutineJob.ProgressInfo("Half-edge: analyzing vertices...",i/(float)vertices.Length);
			}
			
			// assign unique indices to vertices:
			int index = 0;
			foreach(KeyValuePair<Vector3,HEVertex> pair in vertexBuffer){
				((HEVertex)pair.Value).index = index;
				heVertices.Add(pair.Value);
				if (index % 200 == 0)
					yield return new CoroutineJob.ProgressInfo("Half-edge: assigning indices...",index/(float)vertices.Length);
				index++;
			}
			
			// build half edge structure:
			for(int i = 0; i<triangles.Length;i+=3){

				Vector3 pos1 = vertices[triangles[i]];
				Vector3 pos2 = vertices[triangles[i+1]];
				Vector3 pos3 = vertices[triangles[i+2]];

				HEVertex v1 = vertexBuffer[pos1];
				HEVertex v2 = vertexBuffer[pos2];
				HEVertex v3 = vertexBuffer[pos3];

                pos1.Scale(scale);
                pos2.Scale(scale);
                pos3.Scale(scale);

				// create half edges:
				HEHalfEdge e1 = new HEHalfEdge();
				e1.index = heHalfEdges.Count;
				e1.indexOnFace = 0;
				e1.faceIndex = heFaces.Count;
				e1.endVertex = v1.index;
				e1.startVertex = v2.index;
				
				HEHalfEdge e2 = new HEHalfEdge();
				e2.index = heHalfEdges.Count+1;
				e2.indexOnFace = 1;
				e2.faceIndex = heFaces.Count;
				e2.endVertex = v2.index;
				e2.startVertex = v3.index;
				
				HEHalfEdge e3 = new HEHalfEdge();
				e3.index = heHalfEdges.Count+2;
				e3.indexOnFace = 2;
				e3.faceIndex = heFaces.Count;
				e3.endVertex = v3.index;
				e3.startVertex = v1.index;

				// link half edges together:
				e1.nextEdgeIndex = e3.index;
				e2.nextEdgeIndex = e1.index;
				e3.nextEdgeIndex = e2.index;

				// vertex outgoing half edge indices:
				v1.halfEdgeIndex = e3.index;
				v2.halfEdgeIndex = e1.index;
				v3.halfEdgeIndex = e2.index;

				KeyValuePair<int,int> e1Key = new KeyValuePair<int,int>(v1.index,v2.index);
				KeyValuePair<int,int> e2Key = new KeyValuePair<int,int>(v2.index,v3.index);
				KeyValuePair<int,int> e3Key = new KeyValuePair<int,int>(v3.index,v1.index);

				// Check if vertex winding order is consistent with existing triangles. If not, ignore this one.
				if (edgeBuffer.ContainsKey(e1Key) || edgeBuffer.ContainsKey(e2Key) || edgeBuffer.ContainsKey(e3Key))
				{
					nonManifoldEdges = true;
					continue;
				}else{
					edgeBuffer.Add(e1Key,e1);
					edgeBuffer.Add(e2Key,e2);
					edgeBuffer.Add(e3Key,e3);
				}

				// add edges:
				heHalfEdges.Add(e1);
				heHalfEdges.Add(e2);
				heHalfEdges.Add(e3);
				
				// populate and add face:
				HEFace face = new HEFace();
				face.edges[0] = e1.index;
				face.edges[1] = e2.index;
				face.edges[2] = e3.index;
				face.area = ObiUtils.TriangleArea(pos1,pos2,pos3);
				_area += face.area;
				_volume += Vector3.Dot(Vector3.Cross(pos1,pos2),pos3)/6f;
				face.index = heFaces.Count;
				heFaces.Add(face);

				if (i % 500 == 0)
					yield return new CoroutineJob.ProgressInfo("Half-edge: generating edges and faces...",i/(float)triangles.Length);

			}
			
			// Calculate average/min/max edge length:
			_avgEdgeLength = 0;
			_minEdgeLength = float.MaxValue;
			_maxEdgeLength = float.MinValue;
			for (int i = 0; i < heHalfEdges.Count; i++){
				float edgeLength = Vector3.Distance(heVertices[heHalfEdges[i].startVertex].position,
				                                    heVertices[heHalfEdges[i].endVertex].position);
				_avgEdgeLength += edgeLength;
				_minEdgeLength = Mathf.Min(_minEdgeLength,edgeLength);
				_maxEdgeLength = Mathf.Max(_maxEdgeLength,edgeLength);
			}
			_avgEdgeLength /= heHalfEdges.Count;

			List<HEHalfEdge> borderEdges = new List<HEHalfEdge>();		//edges belonging to a mesh border.
			
			// stitch half edge pairs together:
			index = 0;
			foreach(KeyValuePair<KeyValuePair<int,int>,HEHalfEdge> pair in edgeBuffer){

				KeyValuePair<int,int> edgeKey = new KeyValuePair<int,int>(pair.Key.Value,pair.Key.Key);

				HEHalfEdge edge = null;
				if (edgeBuffer.TryGetValue(edgeKey, out edge)){
					((HEHalfEdge)pair.Value).pair = edge.index;
				}else{

					//create border edge:
					HEHalfEdge e = new HEHalfEdge();
					e.index = heHalfEdges.Count;
					e.endVertex = ((HEHalfEdge)pair.Value).startVertex;
					e.startVertex = ((HEHalfEdge)pair.Value).endVertex;
					heVertices[e.startVertex].halfEdgeIndex = e.index;
					e.pair = ((HEHalfEdge)pair.Value).index;
					((HEHalfEdge)pair.Value).pair = e.index;
					heHalfEdges.Add(e);

					borderEdges.Add(e);
				}

				if (index % 1000 == 0)
					yield return new CoroutineJob.ProgressInfo("Half-edge: stitching half-edges...",index/(float)edgeBuffer.Count);
				
				index++;

			}

			_closed = (borderEdges.Count == 0);
			_borderEdgeCount = borderEdges.Count;

			// link together border edges:
			foreach(HEHalfEdge edge in borderEdges){
				edge.nextEdgeIndex = heVertices[edge.endVertex].halfEdgeIndex;
			}

			if (nonManifoldEdges)
				Debug.LogWarning("Non-manifold edges found (vertex winding is not consistent, and/or there are more than 2 faces sharing an edge). Affected faces/edges will be ignored.");

			_nonManifold = nonManifoldEdges;

			// Calculate vertex orientations:
			for(int i = 0; i < heVertices.Count; i++){
				vertexOrientation.Add(CalculateVertexOrientation(i,normals, vertices));
			}

			initialized = true;

		}else{
			Debug.LogWarning("Tried to generate adjacency info for an empty mesh.");
		}
		
	}

	/**
	 * Generates a quaternion for a given vertex, which encodes a reference orientation which can be used to skin
	 * other meshes to this one, or to update tangent space.
	 */
	public Quaternion CalculateVertexOrientation(int index, Vector3[] normals, Vector3[] vertices){

		HEVertex vertex = heVertices[index];
		HEVertex oppositeVertex = heVertices[heHalfEdges[vertex.halfEdgeIndex].endVertex];

		Vector3 normal = normals[vertex.physicalIDs[0]];
		Vector3 forward = Vector3.Cross(normal,vertices[oppositeVertex.physicalIDs[0]] - vertices[vertex.physicalIDs[0]]).normalized;

		if (forward == Vector3.zero) return Quaternion.identity;

		return Quaternion.LookRotation(forward, normal);

	}

	public bool AreLinked(HEVertex v1, HEVertex v2){
		
		HEHalfEdge startEdge = heHalfEdges[v1.halfEdgeIndex];
		HEHalfEdge edge = startEdge;
		
		do{
			edge = heHalfEdges[edge.pair];
			if (edge.startVertex == v2.index)
				return true;
			edge = heHalfEdges[edge.nextEdgeIndex];
			
		} while (edge != startEdge);

		return false;
	}

	public IEnumerable<HEVertex> GetNeighbourVerticesEnumerator(HEVertex vertex)
	{
		
		HEHalfEdge startEdge = heHalfEdges[vertex.halfEdgeIndex];
		HEHalfEdge edge = startEdge;
		
		do{
			edge = heHalfEdges[edge.pair];
			yield return heVertices[edge.startVertex];
			edge = heHalfEdges[edge.nextEdgeIndex];
			
		} while (edge != startEdge);
		
	}

	public IEnumerable<HEHalfEdge> GetNeighbourEdgesEnumerator(HEVertex vertex)
	{
		
		HEHalfEdge startEdge = heHalfEdges[vertex.halfEdgeIndex];
		HEHalfEdge edge = startEdge;
		
		do{
			edge = heHalfEdges[edge.pair];
			yield return edge;
			edge = heHalfEdges[edge.nextEdgeIndex];
			yield return edge;
			
		} while (edge != startEdge);
		
	}

	public IEnumerable<HEFace> GetNeighbourFacesEnumerator(HEVertex vertex)
	{

		HEHalfEdge startEdge = heHalfEdges[vertex.halfEdgeIndex];
		HEHalfEdge edge = startEdge;

		do{

			edge = heHalfEdges[edge.pair];
			if (edge.faceIndex > -1)
				yield return heFaces[edge.faceIndex];
			edge = heHalfEdges[edge.nextEdgeIndex];

		} while (edge != startEdge);

	}

	/**
	 * Calculates and returns a list of all edges (note: not half-edges, but regular edges) in the mesh.
	 * This is O(2N) in both time and space, with N = number of edges.
	 */
	public List<HEEdge> GetEdgeList(){

		List<HEEdge> edges = new List<HEEdge>();
		bool[] listed = new bool[heHalfEdges.Count];

		for (int i = 0; i < heHalfEdges.Count; i++)
		{
			if (!listed[heHalfEdges[i].pair])
			{
				edges.Add(new HEEdge(i));
				listed[heHalfEdges[i].pair] = true;
				listed[i] = true;
			}
		}

		return edges;
	}

	/**
	 * Calculates area-weighted normals for the input normals/vertices array, taking into account shared vertices. Will only
	 * modify those normals known by the half-edge structure, in case it does not represent the whole mesh.
	 */
	public Vector3[] AreaWeightedNormals(Vector3[] normals, Vector3[] vertices){
		
		// array of bools to store if the normal has been modified or not.
		bool[] modified = new bool[normals.Length];
		
		Vector3 v1,v2,v3,n;
		for(int f = 0; f < heFaces.Count; f++){

			HEFace face = heFaces[f];
			
			HEVertex hv1 = heVertices[heHalfEdges[face.edges[0]].endVertex];
			HEVertex hv2 = heVertices[heHalfEdges[face.edges[1]].endVertex];
			HEVertex hv3 = heVertices[heHalfEdges[face.edges[2]].endVertex];
			
			v1 = vertices[hv1.physicalIDs[0]];
			v2 = vertices[hv2.physicalIDs[0]];
			v3 = vertices[hv3.physicalIDs[0]];
			
			n = Vector3.Cross(v2-v1,v3-v1);

			int j = 0;
			for(int i = 0; i < hv1.physicalIDs.Count; i++){
				j = hv1.physicalIDs[i];
				normals[j] = modified[j] ? normals[j]+n : n;
				modified[j] = true;
			}
			for(int i = 0; i < hv2.physicalIDs.Count; i++){
				j = hv2.physicalIDs[i];
				normals[j] = modified[j] ? normals[j]+n : n;
				modified[j] = true;
			}
			for(int i = 0; i < hv3.physicalIDs.Count; i++){
				j = hv3.physicalIDs[i];
				normals[j] = modified[j] ? normals[j]+n : n;
				modified[j] = true;
			}

		}

		// No need to normalize normals, as Unity 5 doesn't anyway.
		// They arrive unnormalized at the vertex shader.
		
		return normals;
		
	}
		

	/**
	 * Use this to remove big amounts of vertices from the half-edge. It will also remove faces and edges connecting those
	 * vertices.
	 */
	public void BatchRemoveVertices(HashSet<int> vertexIndices){
	
		// We need to keep a list of all old edges and faces:
		List<HEHalfEdge> oldEdges = new List<HEHalfEdge>(heHalfEdges);
		List<HEFace> oldFaces = new List<HEFace>(heFaces);
		List<HEVertex> oldVertices = new List<HEVertex>(heVertices);
		
		// Remove all half-edge faces that reference at least one optimized-away particle. 
		heFaces.RemoveAll((ObiMeshTopology.HEFace face)=>{
			foreach(int edgeIndex in face.edges){
				if (vertexIndices.Contains(heHalfEdges[edgeIndex].endVertex)){
					// iterate over all edges and set their face index and index on face to -1. This allows to preserve border edges.
					foreach(int i in face.edges){
						heHalfEdges[i].faceIndex = heHalfEdges[i].indexOnFace = -1;
					}
					return true;
				}
			}
			return false;
		});
		
		// Remove all half-edge edges that reference at least one optimized-away particle.
		heHalfEdges.RemoveAll((ObiMeshTopology.HEHalfEdge edge)=>{
			return vertexIndices.Contains(edge.endVertex) || vertexIndices.Contains(edge.startVertex);
		});
		
		// Remove all half-edge vertices that have been optimized-away.
		heVertices.RemoveAll(v => vertexIndices.Contains(v.index));
		
		// Update face indices:
		for (int i = 0; i < heFaces.Count; i++){
			heFaces[i].index = i;
		}
		
		// Update vertex indices:
		for (int i = 0; i < heVertices.Count; i++){
			heVertices[i].index = i;
		}
		
		//Re-stitch edge indices for faces and face indices for edges:
		for (int i = 0; i < heHalfEdges.Count; i++){
			
			ObiMeshTopology.HEHalfEdge edge = heHalfEdges[i];
			
			//Update edge start and end vertex indices:
			edge.startVertex = oldVertices[edge.startVertex].index;
			edge.endVertex = oldVertices[edge.endVertex].index;
			
			if (edge.faceIndex >= 0){
				
				ObiMeshTopology.HEFace face = oldFaces[edge.faceIndex];
				
				//update face index.
				edge.faceIndex = face.index; 
				
				// update face edge indices:
				face.edges[edge.indexOnFace] = i;
				
			}
			
		}
		
		// Update edge indices:
		for (int i = 0; i < heHalfEdges.Count; i++){
			heHalfEdges[i].index = i;
		}

		// Update half-edge indices. 
		for (int i = 0; i < heFaces.Count; i++){
			heVertices[heHalfEdges[heFaces[i].edges[0]].startVertex].halfEdgeIndex = heFaces[i].edges[0];
			heVertices[heHalfEdges[heFaces[i].edges[1]].startVertex].halfEdgeIndex = heFaces[i].edges[1];
			heVertices[heHalfEdges[heFaces[i].edges[2]].startVertex].halfEdgeIndex = heFaces[i].edges[2];
		}	
		
		// Re-stitch edge pair indices and next edge indices:
		for (int i = 0; i < heHalfEdges.Count; i++){
			ObiMeshTopology.HEHalfEdge edge = heHalfEdges[i];
			oldEdges[edge.pair].pair = edge.index;
			edge.pair = oldEdges[edge.pair].index;

			if (edge.faceIndex == -1){ //in case of a border edge, the start vertex should reference it.
				heVertices[edge.startVertex].halfEdgeIndex = edge.index;
			}

			edge.nextEdgeIndex = oldEdges[edge.nextEdgeIndex].index;
		}

		// Link border edges together:
		for (int i = 0; i < heHalfEdges.Count; i++){
			ObiMeshTopology.HEHalfEdge edge = heHalfEdges[i];
			if (edge.faceIndex == -1){
				edge.nextEdgeIndex = heVertices[edge.endVertex].halfEdgeIndex;
			}
		}

		//TODO: update total area.
		_closed = false;
		
	}

	/**
	 * Calculates angle-weighted normals for the input mesh, taking into account shared vertices.
	 */
	public Vector3[] AngleWeightedNormals(){
		
		if (input == null) return null;

		Vector3[] normals = input.normals;
		Vector3[] vertices = input.vertices;

		for(int i = 0; i < normals.Length; i++)
			normals[i] = Vector3.zero;

		int i1,i2,i3;
		Vector3 e1, e2;
		foreach(HEFace face in heFaces){
			
			HEVertex hv1 = heVertices[heHalfEdges[face.edges[0]].endVertex];
			HEVertex hv2 = heVertices[heHalfEdges[face.edges[1]].endVertex];
			HEVertex hv3 = heVertices[heHalfEdges[face.edges[2]].endVertex];

			i1 = hv1.physicalIDs[0];
			i2 = hv2.physicalIDs[0];
			i3 = hv3.physicalIDs[0];
			
			e1 = vertices[i2]-vertices[i1];
			e2 = vertices[i3]-vertices[i1];
			foreach(int pi in hv1.physicalIDs)
				normals[pi] += Vector3.Cross(e1,e2) * Mathf.Acos(Vector3.Dot(e1.normalized,e2.normalized));
			
			e1 = vertices[i3]-vertices[i2];
			e2 = vertices[i1]-vertices[i2];
			foreach(int pi in hv2.physicalIDs)
				normals[pi] += Vector3.Cross(e1,e2) * Mathf.Acos(Vector3.Dot(e1.normalized,e2.normalized));
			
			e1 = vertices[i1]-vertices[i3];
			e2 = vertices[i2]-vertices[i3];
			foreach(int pi in hv3.physicalIDs)
				normals[pi] += Vector3.Cross(e1,e2) * Mathf.Acos(Vector3.Dot(e1.normalized,e2.normalized));
			
		}

		for(int i = 0; i < normals.Length; i++)
			normals[i].Normalize();
		
		return normals;
	}

	/**
	 * Splits a vertex in two along a plane. Returns true if the vertex can be split, false otherwise. Does not create new border
	 * edges inside the tear in order to prevent non-manifold vertices emerging, so it is only suitable for realtime cloth tearing.
	 * \param vertex the vertex to split.
     * \param splitPlane plane to split the vertex at.
     * \param newVertex the newly created vertex after the split operation has been performed.
     * \param vertices new mesh vertices list after the split operation.
     * \param updatedEdges indices of half-edges that need some kind of constraint update.
	 */
	public bool SplitVertex(HEVertex vertex, Plane splitPlane, MeshBuffer meshBuffer, out HEVertex newVertex, out HashSet<int> updatedEdges){
		
		// initialize return values:
		updatedEdges = new HashSet<int>();
		newVertex = null;
		
		// initialize face lists for each side of the split plane.
		List<HEFace> side1Faces = new List<HEFace>();
		List<HEFace> side2Faces = new List<HEFace>();
		HashSet<int> side2Edges = new HashSet<int>();
		
		// classify adjacent faces depending on which side of the cut plane they reside in:
		foreach(HEFace face in GetNeighbourFacesEnumerator(vertex)){

			int v1 = heHalfEdges[face.edges[0]].startVertex;
			int v2 = heHalfEdges[face.edges[1]].startVertex;
			int v3 = heHalfEdges[face.edges[2]].startVertex;
			
			// Skip this face if it doesnt contain the splitted vertex. 
			// This ca happen because edge pair links are not updated, and so a vertex in the cut stil "sees"
			// the faces at the other side like neighbour faces.
			if (v1 != vertex.index && v2 != vertex.index && v3 != vertex.index) continue;
			
			// Average positions to get the center of the face:
			Vector3 faceCenter = (heVertices[v1].position +
			                      heVertices[v2].position +
			                      heVertices[v3].position) / 3.0f;
			
			if (splitPlane.GetSide(faceCenter)){
				side1Faces.Add(face);
			}else{
				side2Faces.Add(face);
				foreach(int e in face.edges)
					side2Edges.Add(e);
			}
		}
		
		// If the vertex cant be split, return false.
		if (side1Faces.Count == 0 || side2Faces.Count == 0) return false;
		
		// create a new vertex:
		newVertex = new HEVertex(vertex.position,meshBuffer != null ? meshBuffer.vertexCount : -1);
		newVertex.index = heVertices.Count;
		heVertices.Add(newVertex);

		// add a new vertex to the mesh too, if needed.
		if (meshBuffer != null){
			meshBuffer.AddVertex(vertex.physicalIDs[0]);
		}
		
		// rearrange edges at side 1:
		foreach(HEFace face in side1Faces){ 
			
			// find half edges that start or end at the split vertex:
			HEHalfEdge edgeIn = heHalfEdges[Array.Find<int>(face.edges,e => heHalfEdges[e].endVertex == vertex.index)];
			HEHalfEdge edgeOut = heHalfEdges[Array.Find<int>(face.edges,e => heHalfEdges[e].startVertex == vertex.index)];

			// If the edge pair is on the other side of the cut, it will spawn a new constraint, and update other.
			// If not, we only need to update an existing constraint, which will be the minimum of the edge index and its pair index.
			if (side2Edges.Contains(edgeIn.pair)){
				edgeIn.torn = true;
				heHalfEdges[edgeIn.pair].torn = true;
			}

			if (side2Edges.Contains(edgeOut.pair)){
				edgeOut.torn = true;
				heHalfEdges[edgeOut.pair].torn = true;
			}

			if (edgeIn.torn){
				updatedEdges.Add(edgeIn.index);
				updatedEdges.Add(edgeIn.pair);
			}else{
				updatedEdges.Add(Mathf.Min(edgeIn.index,edgeIn.pair));
			}

			if (edgeOut.torn){
				updatedEdges.Add(edgeOut.index);
				updatedEdges.Add(edgeOut.pair);
			}else{
				updatedEdges.Add(Mathf.Min(edgeOut.index,edgeOut.pair));
			}
			
			// stitch half edges to new vertex
			edgeIn.endVertex = newVertex.index;
			edgeOut.startVertex = newVertex.index;
			newVertex.halfEdgeIndex = edgeOut.index;
            
            // update mesh triangle buffer to point at new vertex where needed:
			if (meshBuffer != null){
				if (meshBuffer.triangles[face.index*3] == vertex.physicalIDs[0]) meshBuffer.triangles[face.index*3] = newVertex.physicalIDs[0];
				if (meshBuffer.triangles[face.index*3+1] == vertex.physicalIDs[0]) meshBuffer.triangles[face.index*3+1] = newVertex.physicalIDs[0];
				if (meshBuffer.triangles[face.index*3+2] == vertex.physicalIDs[0]) meshBuffer.triangles[face.index*3+2] = newVertex.physicalIDs[0];
			}
            
        }

        _closed = false;
        _modified = true;
        
        return true;
        
    }

}
}



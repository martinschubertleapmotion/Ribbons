using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Obi{

	[CustomEditor(typeof(ObiMeshTopology))] 
	public class ObiMeshTopologyEditor : Editor
	{
		
		ObiMeshTopology halfEdge;
		PreviewHelpers previewHelper;
		Vector2 previewDir;
		Material previewMaterial;
		Material previewMaterialBorders;

		Mesh previewMesh;
		bool drawBordersOnly = false;

		[MenuItem("Assets/Create/Obi/Obi Mesh Topology")]
		public static void CreateObiMesh ()
		{
			ObiEditorUtils.CreateAsset<ObiMeshTopology> ();
		}

		[MenuItem("Assets/Create/Obi/Obi Collision Material")]
		public static void CreateObiCollisionMaterial ()
		{
			ObiEditorUtils.CreateAsset<ObiCollisionMaterial> ();
		}

		private void UpdatePreview(){

			CleanupPreview();

			drawBordersOnly = false;

			previewHelper = new PreviewHelpers();
			previewMaterial = EditorGUIUtility.LoadRequired("TopologyPreview.mat") as Material;
			previewMaterialBorders = EditorGUIUtility.LoadRequired("TopologyPreviewBorder.mat") as Material;

			previewMesh = new Mesh();
			previewMesh.hideFlags = HideFlags.HideAndDontSave;

			UpdateTopologyMesh();

		}
		
		private void CleanupPreview(){
			GameObject.DestroyImmediate(previewMesh);
		}
		
		public void OnEnable(){
			halfEdge = (ObiMeshTopology) target;
			UpdatePreview();
		}
		
		public void OnDisable(){
			EditorUtility.ClearProgressBar();
			previewHelper.Cleanup();
			CleanupPreview();
		}
		
		public override void OnInspectorGUI() {

			halfEdge.InputMesh = EditorGUILayout.ObjectField("Input mesh",halfEdge.InputMesh, typeof(Mesh), false) as Mesh;
			halfEdge.scale = EditorGUILayout.Vector3Field("Scale",halfEdge.scale);

			if (GUILayout.Button("Generate")){
				// Start a coroutine job in the editor.
				CoroutineJob job = new CoroutineJob();
				halfEdge.generationRoutine = EditorCoroutine.StartCoroutine( job.Start( halfEdge.Generate()));
			}
			
			// Show job progress:
			EditorCoroutine.ShowCoroutineProgressBar("Analyzing mesh",halfEdge.generationRoutine);
			
			//If the generation routine has been completed, release it and update volumetric preview:
			if (halfEdge.generationRoutine != null && halfEdge.generationRoutine.IsDone){
				halfEdge.generationRoutine = null;
				EditorUtility.SetDirty(target);
				UpdatePreview();
			}
			
			// Print topology info:
			if (halfEdge.Initialized){
				EditorGUILayout.HelpBox("Vertices:"+ halfEdge.heVertices.Count + " "+
					                    "Edges:"+ halfEdge.heHalfEdges.Count/2 + " "+
										"Faces:"+halfEdge.heFaces.Count + "\n"+
										"Total Area:"+halfEdge.MeshArea + " " +
				                        "Total Volume:"+halfEdge.MeshVolume +"\n"+
				                        "Closed:"+halfEdge.IsClosed,MessageType.Info);
			}

			if (halfEdge.IsNonManifold){
				EditorGUILayout.HelpBox("Your mesh is non-manifold. Obi will still try to make things work, but consider fixing it.", MessageType.Warning);
			}
			
			GUI.enabled = (halfEdge.BorderEdgeCount > 0);
				drawBordersOnly = EditorGUILayout.Toggle("Draw borders only",drawBordersOnly);
			GUI.enabled = true;
			
			if (GUI.changed)
				serializedObject.ApplyModifiedProperties();
			
		}
		
		public override bool HasPreviewGUI(){
			return true;
		}
		
		public override bool RequiresConstantRepaint(){
			return false;	
		}
		
		public override void OnPreviewGUI(Rect region, GUIStyle background)
		{
			previewDir = PreviewHelpers.Drag2D(previewDir, region);
			
			if (Event.current.type != EventType.Repaint || halfEdge.InputMesh == null)
			{
				return;
			}
			
			Quaternion quaternion = Quaternion.Euler(this.previewDir.y, 0f, 0f) * Quaternion.Euler(0f, this.previewDir.x, 0f) * Quaternion.Euler(0, 120, -20f);
			
			previewHelper.BeginPreview(region, background);
			
			Bounds bounds = halfEdge.InputMesh.bounds;
			float magnitude = bounds.extents.magnitude;
			float num = 4f * magnitude;
			previewHelper.m_Camera.transform.position = -Vector3.forward * num;
			previewHelper.m_Camera.transform.rotation = Quaternion.identity;
			previewHelper.m_Camera.nearClipPlane = num - magnitude * 1.1f;
			previewHelper.m_Camera.farClipPlane = num + magnitude * 1.1f;
			
			// Compute matrix to rotate the mesh around the center of its bounds:
			Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero,quaternion,Vector3.one) * Matrix4x4.TRS(-bounds.center,Quaternion.identity,Vector3.one);
			
			if (previewMesh.subMeshCount == 2){
				if (!drawBordersOnly) Graphics.DrawMesh(previewMesh, matrix, previewMaterial,1, previewHelper.m_Camera, 0);
				Graphics.DrawMesh(previewMesh, matrix, previewMaterialBorders,1, previewHelper.m_Camera, 1);
			}
			
			Texture texture = previewHelper.EndPreview();
			GUI.DrawTexture(region, texture, ScaleMode.StretchToFill, true);
			
		}

		private void UpdateTopologyMesh(){
	
			if (previewMesh == null || halfEdge.InputMesh == null || !halfEdge.Initialized) return;

			previewMesh.Clear();
			previewMesh.subMeshCount = 2;

			Vector3[] vertices = new Vector3[halfEdge.heFaces.Count*3 + halfEdge.BorderEdgeCount*2];
			Vector3[] normals = new Vector3[halfEdge.heFaces.Count*3 + halfEdge.BorderEdgeCount*2];
			int[] faceIndices = new int[halfEdge.heFaces.Count*3];
			int[] edgeIndices = new int[halfEdge.BorderEdgeCount*2];

			// iterate over all faces.
			int borderIndex = 0;
            for (int i = 0; i < halfEdge.heFaces.Count; i++){

				ObiMeshTopology.HEHalfEdge edge1 = halfEdge.heHalfEdges[halfEdge.heFaces[i].edges[0]];
				ObiMeshTopology.HEHalfEdge edge2 = halfEdge.heHalfEdges[halfEdge.heFaces[i].edges[1]];
				ObiMeshTopology.HEHalfEdge edge3 = halfEdge.heHalfEdges[halfEdge.heFaces[i].edges[2]];

                ObiMeshTopology.HEVertex vertex1 = halfEdge.heVertices[edge1.endVertex];
				ObiMeshTopology.HEVertex vertex2 = halfEdge.heVertices[edge2.endVertex];
				ObiMeshTopology.HEVertex vertex3 = halfEdge.heVertices[edge3.endVertex];
                
                Vector3 centroid = (vertex1.position + vertex2.position + vertex3.position) * 0.33f;
				Vector3 n = Vector3.Cross(vertex2.position-vertex1.position,vertex3.position-vertex1.position);

				normals[i*3] += n;
				normals[i*3+1] += n;
				normals[i*3+2] += n;
				
				vertices[i*3] = centroid+(vertex1.position-centroid)*0.8f;
				vertices[i*3+1] = centroid+(vertex2.position-centroid)*0.8f;
                vertices[i*3+2] = centroid+(vertex3.position-centroid)*0.8f;

				faceIndices[i*3] = i*3;
				faceIndices[i*3+1] = i*3+1;
				faceIndices[i*3+2] = i*3+2;

				// Add border edge data:

				if (halfEdge.heHalfEdges[edge1.pair].faceIndex == -1){
					vertices[halfEdge.heFaces.Count*3 + borderIndex*2] = vertex1.position;
					vertices[halfEdge.heFaces.Count*3 + borderIndex*2+1] = vertex2.position;
					edgeIndices[borderIndex*2] = halfEdge.heFaces.Count*3 + borderIndex*2;
					edgeIndices[borderIndex*2+1] = halfEdge.heFaces.Count*3 + borderIndex*2+1;
					borderIndex++;
				}
				if (halfEdge.heHalfEdges[edge2.pair].faceIndex == -1){
					vertices[halfEdge.heFaces.Count*3 + borderIndex*2] = vertex2.position;
					vertices[halfEdge.heFaces.Count*3 + borderIndex*2+1] = vertex3.position;
					edgeIndices[borderIndex*2] = halfEdge.heFaces.Count*3 + borderIndex*2;
					edgeIndices[borderIndex*2+1] = halfEdge.heFaces.Count*3 + borderIndex*2+1;
                    borderIndex++;
                }
				if (halfEdge.heHalfEdges[edge3.pair].faceIndex == -1){
					vertices[halfEdge.heFaces.Count*3 + borderIndex*2] = vertex3.position;
					vertices[halfEdge.heFaces.Count*3 + borderIndex*2+1] = vertex1.position;
					edgeIndices[borderIndex*2] = halfEdge.heFaces.Count*3 + borderIndex*2;
					edgeIndices[borderIndex*2+1] = halfEdge.heFaces.Count*3 + borderIndex*2+1;
                    borderIndex++;
                }
			}
 
			previewMesh.vertices = vertices;
			previewMesh.normals = normals;
			previewMesh.SetIndices(faceIndices,UnityEngine.MeshTopology.Triangles,0);

			previewMesh.SetIndices(edgeIndices,UnityEngine.MeshTopology.Lines,1);

		}
	
	}
}


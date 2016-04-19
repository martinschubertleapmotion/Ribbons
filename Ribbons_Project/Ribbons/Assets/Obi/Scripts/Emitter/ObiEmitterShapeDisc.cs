using UnityEngine;
using System;
using System.Collections.Generic;

namespace Obi
{

	[ExecuteInEditMode]
	public class ObiEmitterShapeDisc : ObiEmitterShape
	{
		
		public enum DiscSamplingMethod{
			REGULAR,
			POISSON,
		}

		public DiscSamplingMethod samplingMethod = DiscSamplingMethod.REGULAR;
		public float radius = 0.5f;
		public float density = 10;	

		private Vector2 PointToGrid(Vector3 point, float cellSize){
			return new Vector2(Mathf.FloorToInt(point.x/cellSize),Mathf.FloorToInt(point.y/cellSize));
		}

		private bool PointInGrid(Vector3 point,float gridWidth,float gridHeight){
			return point.x > 0 && point.x < gridWidth && point.y > 0 && point.y < gridHeight;
		}

		private Vector3 GenerateRandomPointAround(Vector3 point, float mindist)
		{ //non-uniform, favours points closer to the inner ring, leads to denser packings
		  float r1 = UnityEngine.Random.Range(0f,1f); //random point between 0 and 1
		  float r2 = UnityEngine.Random.Range(0f,1f);
		  //random radius between mindist and 2 * mindist
		  float radius = mindist * (r1 + 1);
		  //random angle
		  float angle = 2 * Mathf.PI * r2;
		  //the new point is generated around the point (x, y)
		  float newX = point.x + radius * Mathf.Cos(angle);
		  float newY = point.y + radius * Mathf.Sin(angle);
		  return new Vector3(newX, newY,0);
		}

		private List<Vector3> SquareAroundPoint(Vector3[,] grid, Vector2 gridPoint, int gridSize,  float cellSize, float cells)
		{
			List<Vector3> neighborhood = new List<Vector3>();
			for (int x = 0; x < cells; x++){
				for (int y = 0; y < cells; y++){
					int px = (int)gridPoint.x-(int)(cells*0.5f) + x;
					int py = (int)gridPoint.y-(int)(cells*0.5f) + y;
					if (px >= 0 && py >= 0 && px < gridSize && py < gridSize){
						if (grid[px,py] != Vector3.zero) //empty cell
							neighborhood.Add(grid[px,py]);
					}
				}
			}
			return neighborhood;
		}

		private bool InNeighbourhood(Vector3[,] grid, Vector3 point, float mindist, int gridSize, float cellSize)
		{
		  Vector2 gridPoint = PointToGrid(point, cellSize);
		  //get the neighbourhood if the point in the grid
		  List<Vector3> neighbourhood = SquareAroundPoint(grid, gridPoint,gridSize, cellSize, 5);
		  foreach(Vector3 neighbour in neighbourhood){
			if (Vector3.Distance(neighbour, point) < mindist)
		        return true;
		  }
		  return false;
		}

		protected override void GenerateDistribution(){

			distribution.Clear(); 

			if (samplingMethod == DiscSamplingMethod.REGULAR){
		
				int num = Mathf.FloorToInt(radius * density);
	
				for (int x = -num; x < num; ++x){
					for (int y = -num; y < num; ++y){
	
						Vector3 pos = new Vector3(x/density,y/density,0);
	
						if (pos.magnitude <= radius)
							distribution.Add(pos);
	
					}
				}

			}else{

				int num = (int)Mathf.Pow(Mathf.FloorToInt(radius * density),2);
				float minDistance = 1/density;

				float cellSize = minDistance/ Mathf.Sqrt(2);
				int gridSize = Mathf.CeilToInt(radius*2/cellSize);
				
				// declare containers:
				Vector3[,] grid = new Vector3[gridSize,gridSize];
				List<Vector3> processList = new List<Vector3>();

				// select first point:
				Vector3 firstPoint;
				do{
				 firstPoint = new Vector3(UnityEngine.Random.Range(0, radius*2),
										  UnityEngine.Random.Range(0, radius*2),0);

				}while((firstPoint - new Vector3(radius,radius,0)).magnitude > radius);

				//update containers
			    processList.Add(firstPoint);
			   	distribution.Add(firstPoint - new Vector3(radius,radius,0));
				Vector2 gridCell = PointToGrid(firstPoint, cellSize);
				grid[(int)gridCell.x,(int)gridCell.y] = firstPoint;
			
			  	//generate other points from points in queue.
			  	while (processList.Count > 0)
			  	{
			
				  	// remove element at random index:
				  	int index = UnityEngine.Random.Range(0,processList.Count-1);
			   	  	Vector3 point = processList[index];
				  	processList.RemoveAt(index);
		
		   	    	for (int i = 0; i < num; ++i)
		    		{
				      	Vector3 newPoint = GenerateRandomPointAround(point, minDistance);
				      	//check that the point is in the image region
				      	//and no points exists in the point's neighbourhood
						if (PointInGrid(newPoint,radius*2,radius*2) && !InNeighbourhood(grid, newPoint, minDistance, gridSize,cellSize) &&
							(newPoint - new Vector3(radius,radius,0)).magnitude <= radius )
				     	{
					      	 //update containers
					      	 processList.Add(newPoint);
							 distribution.Add(newPoint - new Vector3(radius,radius,0));
							 gridCell = PointToGrid(newPoint, cellSize);
							 grid[(int)gridCell.x,(int)gridCell.y] =  newPoint;
				      	}
			    	}
			  	}

			}

		}

		public void OnDrawGizmosSelected(){

			/*Handles.matrix = transform.localToWorldMatrix;
			Handles.color  = Color.cyan;

			Handles.DrawWireDisc(Vector3.zero,Vector3.forward,radius);

			foreach (Vector3 point in distribution)
				Handles.DrawLine(point,point + Vector3.forward * 0.1f);*/

		}

	}
}


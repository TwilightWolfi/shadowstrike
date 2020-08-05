using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunayVoronoi;

public class WorldStructureLoad : MonoBehaviour
{
	public float minX, maxX, minY, maxY;
	public int minWidth, maxWidth, minHeight, maxHeight;
	public int minHubWidth, minHubHeight;
	public List<Rigidbody2D> spawnedBodies;
	public int numRooms;
	public GameObject roomSolver;
	public enum worldGenState {End, PhysicsSolve, Triangle, Paths};
	public worldGenState genState = worldGenState.PhysicsSolve;
	public float shapeWidth;
	public float shapeHeight;
	public Vector3 locationRandom;
	public Vector2 sizeRandom;
	public int j = 0;
	public List<Vector3> roomPositions = new List<Vector3>();
	public List<Vector2> points;
	public DelaunayTriangulator triangulator;
	public List<Vector2> scales = new List<Vector2>();
	List<Vector3> oldPos = new List<Vector3>();
	List<int> hubRoomIndexes = new List<int>();
	Edge edgeMatch;
	
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numRooms; i++)
		{
			shapeWidth = Mathf.Round(Random.Range(minWidth,maxWidth));
			shapeWidth += shapeWidth%2;
			shapeHeight = Mathf.Round(Random.Range(minHeight,maxHeight));
			shapeHeight += shapeHeight%2;
			
			locationRandom = new Vector3(Mathf.Round(Random.Range(minX,maxX)), Mathf.Round(Random.Range(minY,maxY)), -1);
			sizeRandom = new Vector2(shapeWidth, shapeHeight);
			GameObject spawned = (GameObject)Instantiate(roomSolver, locationRandom, Quaternion.identity);
			spawned.transform.localScale = sizeRandom;
			
			spawnedBodies.Add(spawned.GetComponent<Rigidbody2D>());
			oldPos.Add(spawned.transform.position);
		}
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		
		j++;
		if (genState == worldGenState.PhysicsSolve)
		{
			bool allStill = true;
			
			for(int i = 0; i<spawnedBodies.Count; i++)
			{
			  Vector3 deltaPos = oldPos[i] - spawnedBodies[i].transform.position;
			  oldPos[i] = spawnedBodies[i].transform.position;
			  
			  if(deltaPos.magnitude > 0)
			  {
				 allStill = false;
			  }
			}
			if (allStill)
			{
				if (j > 20) {
					j = 0;
					foreach (Rigidbody2D rb in spawnedBodies)
					{
						Vector3 position = new Vector3(Mathf.Round(rb.transform.position.x), Mathf.Round(rb.transform.position.y), -1);
						roomPositions.Add(position);
						scales.Add(rb.transform.localScale);
						if (rb.transform.localScale.x > 20 && rb.transform.localScale.y > 15)
						{
							hubRoomIndexes.Add(j);
						}
						rb.isKinematic = true;
						//Destroy(rb.gameObject);
						j++;
					}
					genState = worldGenState.Triangle;
				}
			}
		}
		
		if (genState == worldGenState.Triangle)
		{
			
			List<Point> pointList = new List<Point>();
			foreach(int index in hubRoomIndexes)
			{
				Vector3 vec = roomPositions[index];
				print("Hub Location: (" + vec.x + ", " + vec.y + ")");
				pointList.Add(new Point(vec.x, vec.y));
			}
			
			List<Edge> edges = new List<Edge>(DelaunayToEdges(GenerateTriangles(pointList)));
			
			DrawEdges(edges);
			genState = worldGenState.Paths;
		}
		
		if (genState == worldGenState.Paths)
		{
			
		}
		
		if (genState == worldGenState.End)
		{
			Debug.Log("WE DID IT REDDIT");
		}
    }
	
	public List<Triangle> GenerateTriangles(List<Point> pointList)
	{
		if(triangulator == null)
		{
			triangulator = new DelaunayTriangulator((double)maxX, (double)maxY);
		}
		
		return new List<Triangle>(triangulator.BowyerWatson(pointList));
	}
	
	public List<Edge> KruskalMST(List<Edge> edges, List<Vector3> points)
	{
		return null;
	}
	
	public List<Edge> DelaunayToEdges (List<Triangle> triList)
	{
		List<Edge> edges = new List<Edge>();
		print(triList.Count + " Triangles");
		
		foreach(Triangle tri in triList)
		{
			List<Point> verts = new List<Point>(tri.Vertices);
			List<Edge> triEdges = new List<Edge>(){new Edge(verts[0], verts[1]), new Edge(verts[1], verts[2]), new Edge(verts[2], verts[0])};
			
			foreach(Edge edge in triEdges)
			{
				if (!edges.Contains(edge) || !edges.Contains(new Edge(edge.point2, edge.point1)))
				{
					edges.Add(edge);
				}
			}
		}
		
		return edges;
	}
	
	public void DrawEdges(List<Edge> edges)
	{
		foreach(Edge edge in edges)
		{
			Debug.DrawLine(new Vector3((float)edge.point1.x, (float)edge.point1.y, 0), new Vector3((float)edge.point2.x, (float)edge.point2.y, 0), Color.white, 100f);
			
			print("Edge: (" + edge.point1.x + ", " + edge.point1.y + ") to (" + edge.point2.x + ", " + edge.point2.y + ")");
		}
		if (edges.Count == 0)
		{
			print("No edges to draw.");
		}
		
	}
}


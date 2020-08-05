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
	public GameObject roomSolver, wallTemporary, backgroundTemporary;
	public enum worldGenState {End, PhysicsSolve, Triangle, Paths, Rooms, DoneForReal};
	public worldGenState genState = worldGenState.PhysicsSolve;
	public float shapeWidth;
	public float shapeHeight;
	public Vector3 locationRandom;
	public Vector3 sizeRandom;
	public int j = 0;
	public List<Vector3> roomPositions = new List<Vector3>();
	public List<Vector2> points;
	public DelaunayTriangulator triangulator;
	public List<Vector2> scales = new List<Vector2>();
	List<Vector3> oldPos = new List<Vector3>();
	List<int> hubRoomIndexes = new List<int>();
	Edge edgeMatch;
	public Transform playerCharacter;
	//scale up the world once generation is over
	public Vector3 worldScaleEnd = new Vector3(5, 5, 5);
	
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numRooms; i++)
		{
			shapeWidth = Mathf.Round(Random.Range(minWidth,maxWidth));
			shapeWidth += shapeWidth%2;
			shapeHeight = Mathf.Round(Random.Range(minHeight,maxHeight));
			shapeHeight += shapeHeight%2;
			
			locationRandom = new Vector3(Mathf.Round(Random.Range(minX,maxX)), Mathf.Round(Random.Range(minY,maxY)), 0);
			sizeRandom = new Vector3(shapeWidth, shapeHeight, 1);
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
				spawnedBodies[i].transform.position = Vector3.Lerp(spawnedBodies[i].transform.position, new Vector3(Mathf.Round(spawnedBodies[i].transform.position.x), Mathf.Round(spawnedBodies[i].transform.position.y), 0), Time.deltaTime);
			  oldPos[i] = spawnedBodies[i].transform.position;
			  if(deltaPos.magnitude > 0.01f)
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
						Vector3 position = new Vector3(Mathf.Round(rb.transform.position.x), Mathf.Round(rb.transform.position.y), 0);
						roomPositions.Add(position);
						scales.Add(rb.transform.localScale);
				    rb.GetComponent<MeshRenderer>().material.color = new Color(0.1f, 0.1f, 0.1f);
						if (rb.transform.localScale.x > minHubWidth && rb.transform.localScale.y > minHubHeight)
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
				spawnedBodies[index].GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0f, 0.5f), Random.Range(0f, 0.5f), Random.Range(0f, 0.5f));
				pointList.Add(new Point(vec.x, vec.y));
			}
			
			edges = new List<Edge>(DelaunayToEdges(GenerateTriangles(pointList)));
			
			DrawEdges(edges);
			genState = worldGenState.Paths;
		}
		
		if (genState == worldGenState.Paths)
		{
				List<int> indexesToSpare = new List<int>();
			foreach(Edge edge in edges)
			{
				Vector2 dir = new Vector2((float)edge.point2.x - (float)edge.point1.x, (float)edge.point2.y - (float)edge.point1.y);
				float dist = dir.magnitude;
				dir.Normalize();
				Vector2 origin = new Vector3((float)edge.point1.x, (float)edge.point1.y);
				//Ray ray = new Ray(new Vector3((float)edge.point1.x, (float)edge.point1.y, -1), dir);
				print("Firing ray from: "+origin+" on direction "+dir+" with length "+dist);
				print("Position comparison: "+origin+" to "+(origin+(dir*dist))+" vs "+edge.point1.x+","+edge.point1.y+" to "+edge.point2.x+","+edge.point2.y);
				RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, dist);
				print("Hit #: "+hits.Length);
				print("Index list set up");
				for(int i = 0; i<hits.Length; i++)
				{
					print("hit: "+hits[i].collider.name);
					if(hits[i].collider.GetComponent<Rigidbody2D>() != null)
					{
						Rigidbody2D rb = hits[i].collider.GetComponent<Rigidbody2D>();
						if(spawnedBodies.Contains(rb))
						{
							int index = spawnedBodies.IndexOf(rb);
							if(!indexesToSpare.Contains(index))
							{
								indexesToSpare.Add(index);
							}
						}
					}
				}
			}
				for(int i = 0; i<spawnedBodies.Count; i++)
				{
					if(indexesToSpare.Contains(i) == false)
					{
						spawnedBodies[i].gameObject.SetActive(false);
					}
				}
			genState = worldGenState.Rooms;
		}

		if (genState == worldGenState.Rooms)
		{
			for(int i = 0; i<spawnedBodies.Count; i++)
			{
				Rigidbody2D rb = spawnedBodies[i];
				if(rb.gameObject.activeSelf)
				{
					Color rbCol = rb.GetComponent<MeshRenderer>().material.color;
					Vector2 position = new Vector2(Mathf.Round(rb.transform.position.x), Mathf.Round(rb.transform.position.y));
					Vector2 scale = new Vector2(Mathf.Round(rb.transform.localScale.x), Mathf.Round(rb.transform.localScale.y));
					GameObject go = null;
					for(int x = ((int)(-scale.x/2))-1; x<=scale.x/2; x++)
					{
						//if(!Physics2D.Raycast(new Vector2(position.x + x, position.y - (scale.y/2)), Vector2.down, 0.25f))
						{
							go = (GameObject)Instantiate(wallTemporary, new Vector3(position.x + x, position.y - (scale.y/2), 0), Quaternion.identity, transform);
							go.GetComponent<MeshRenderer>().material.color = rbCol;
						}
						//if(!Physics2D.Raycast(new Vector2(position.x + x, position.y + (scale.y/2)), Vector2.down, 0.25f))
						{
							go = (GameObject)Instantiate(wallTemporary, new Vector3(position.x + x, position.y + (scale.y/2), 0), Quaternion.identity, transform);
							go.GetComponent<MeshRenderer>().material.color = rbCol;
						}
					}
					for(int y = ((int)(-scale.y/2)); y<=scale.y/2; y++)
					{
						//if(!Physics2D.Raycast(new Vector2(position.x - (scale.x/2), position.y + y), Vector2.down, 0.25f))
						{
							go = (GameObject)Instantiate(wallTemporary, new Vector3(position.x - (scale.x/2), position.y + y, 0), Quaternion.identity, transform);
							go.GetComponent<MeshRenderer>().material.color = rbCol;
						}
						//if(!Physics2D.Raycast(new Vector2(position.x + (scale.x/2), position.y + y), Vector2.down, 0.25f))
						{
							go = (GameObject)Instantiate(wallTemporary, new Vector3(position.x + (scale.x/2), position.y + y, 0), Quaternion.identity, transform);
							go.GetComponent<MeshRenderer>().material.color = rbCol;
						}
					}
					for(int x = ((int)(-scale.x/2))-1; x<=scale.x/2; x++)
					{
						for(int y = ((int)(-scale.y/2))-1; y<=scale.y/2; y++)
						{
							//if(!Physics2D.Raycast(new Vector2(position.x + x, position.y - (scale.y/2)), Vector2.down, 0.25f))
							{
								go = (GameObject)Instantiate(backgroundTemporary, new Vector3(position.x + x, position.y - (scale.y/2), 1), Quaternion.identity, transform);
								go.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = rbCol;
							}
						}
					}
				}
				rb.isKinematic = true;
			}
			foreach(Edge edge in edges)
			{
				Vector2 dir = new Vector2((float)edge.point2.x - (float)edge.point1.x, (float)edge.point2.y - (float)edge.point1.y);
				float dist = dir.magnitude;
				dir.Normalize();
				Vector2 origin = new Vector3((float)edge.point1.x, (float)edge.point1.y);
				//Ray ray = new Ray(new Vector3((float)edge.point1.x, (float)edge.point1.y, -1), dir);
				print("Firing ray from: "+origin+" on direction "+dir+" with length "+dist);
				print("Position comparison: "+origin+" to "+(origin+(dir*dist))+" vs "+edge.point1.x+","+edge.point1.y+" to "+edge.point2.x+","+edge.point2.y);
				RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, dist);
				print("Hit #: "+hits.Length);
				print("Index list set up");
				for(int i = 0; i<hits.Length; i++)
				{
					print("hit: "+hits[i].collider.name);
					Destroy(hits[i].collider.gameObject);
				}
			}
			genState = worldGenState.End;
		}
		
		if (genState == worldGenState.End)
		{
			transform.localScale = worldScaleEnd;
			Vector3 offset = Vector3.zero;
			int numBodies = 0;
			foreach(Rigidbody2D rb in spawnedBodies)
			{
				if(rb.gameObject.activeSelf)
				{
					offset = offset + rb.transform.position;
					numBodies += 1;
					rb.gameObject.SetActive(false);
				}
			}
			offset /= numBodies;
			offset = new Vector3(offset.x * worldScaleEnd.x, offset.y * worldScaleEnd.y, offset.z * worldScaleEnd.z);
			//transform.position = -offset;
			Debug.Log("WE DID IT REDDIT");
			CharacterController2D rbb = playerCharacter.GetComponent<CharacterController2D>();
			rbb.SetVelocity(Vector2.zero);
			Vector3 pos = spawnedBodies[hubRoomIndexes[0]].transform.position;
			pos = new Vector3(pos.x * worldScaleEnd.x, pos.y * worldScaleEnd.y, pos.z * worldScaleEnd.z);
			playerCharacter.position = pos + transform.position;
			rbb.SetVelocity(Vector2.zero);
			genState = worldGenState.DoneForReal;
		}
    }
	
	List<Edge> edges = null;
	
	public List<Triangle> GenerateTriangles(List<Point> pointList)
	{
		if(triangulator == null)
		{
			triangulator = new DelaunayTriangulator((double)maxX*100, (double)maxY*100);
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
			Debug.DrawLine(new Vector3((float)edge.point1.x, (float)edge.point1.y, 0), new Vector3((float)edge.point2.x, (float)edge.point2.y, 0), new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)), 100f);
			
			print("Edge: (" + edge.point1.x + ", " + edge.point1.y + ") to (" + edge.point2.x + ", " + edge.point2.y + ")");
		}
		if (edges.Count == 0)
		{
			print("No edges to draw.");
		}
		
	}
}


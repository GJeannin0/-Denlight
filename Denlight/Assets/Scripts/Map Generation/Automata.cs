using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class Node
{
	public List<Node> neighbors;
	public Vector2 pos;

	public bool isFree;

	public bool hasBeenVisited = false;
	public bool isPath = false;
	public Node cameFrom = null;

	public float cost;
	public float currentCost = -1;
}

public class Automata : MonoBehaviour
{
	public struct PathCell
	{
		public bool explored;
		public bool isEnd;
		public int typeWeight;
		public int currentWeight;
		public int totalWeight;

		public List<PathCell> neighbors;
		public Vector2 ID;
		public Vector2 cameFromID;
	}

	public struct Cell
	{
		public bool alive;
		public bool willLive;
		public uint room;
		public uint typeIndex;          // 1 wall, 2 void

		public Vector2 pos;
	}

	public struct Room
	{
		public int index;
		public int typeIndex;               // 1 start, 2 End, 3 void, 4 freeloot, 5 danger
		public List<int> directNeigbors;                  
	}

	[SerializeField] private GameObject player;
	private GameObject startPos;
	private float infiniteCheck = 0;

	private Manager manager;


	[SerializeField] private GameObject softWall;
	[SerializeField] private GameObject wall;

	[SerializeField] private GameObject start;
	[SerializeField] private GameObject end;
	[SerializeField] private GameObject danger;
	[SerializeField] private GameObject loot;

	[SerializeField] private GameObject pathedCell;
	[SerializeField] private GameObject cellaimed;

	[SerializeField] private int stepIterations = 14;
	[SerializeField] private int seed;
	[SerializeField] private float amountOfLiving;
	[SerializeField] private int height;
	[SerializeField] private int length;

	[SerializeField] private float dangerZoneRatio = 0.50f;

	[SerializeField] private float hoveringLightsPerCell = 0.2f;
	[SerializeField] private float extremeHoveringLightsPerCell = 0.2f;
	[SerializeField] private float enemyPerCell = 0.080f;
	[SerializeField] private float extremeEnemyPerCell = 0.080f;

	[SerializeField] private float minimumHoveringLightPerLootCell;
	[SerializeField] private float hardMinimumHoveringLightPerLootCell;
	[SerializeField] private float extremeMinimumHoveringLightPerLootCell;

	[SerializeField] private float minimumEnemyPerDangerCell;
	[SerializeField] private float hardMinimumEnemyPerDangerCell;


	[SerializeField] private float enemyLevel1Weight = 7.0f;
	[SerializeField] private float enemyLevel2Weight = 3.0f;
	[SerializeField] private float enemyLevel3Weight = 1.0f;

	[SerializeField] private float hardEnemyLevel2Weight = 3.0f;
	[SerializeField] private float hardEnemyLevel3Weight = 1.0f;

	[SerializeField] private float extremeEnemyLevel2Weight = 3.0f;
	[SerializeField] private float extremeEnemyLevel3Weight = 1.0f;

	[SerializeField] private GameObject circleLevel1;
	[SerializeField] private GameObject circleLevel2;
	[SerializeField] private GameObject circleLevel3;
	[SerializeField] private GameObject hoveringLight;

	private Cell[,] map;
	private Cell[,] reRoomingMap;
	private Cell[,] originalRoomsMap;
	private Room[] rooms;
	private Room[] originalRooms;
	private uint currentRoomIndex = 1;

	private float minimumRange = 0.00001f;

	[SerializeField] private int neighboursToSpawn;
	[SerializeField] private int neighboursToLive;
	[SerializeField] private int neighboursToDie;


	[SerializeField] private int voidWeight;
	[SerializeField] private int wallWeight;
	[SerializeField] private int softWallWeight;

	[SerializeField] private float heuristicTargetingPlayer = 300;

	[SerializeField] private int endWeight = 300;

	private PathCell[,] pathingGraph;

	private int difficulty = 0;

	[SerializeField] private int borderThickness = 6;

	private void Awake()
	{
		manager = FindObjectOfType<Manager>();
		manager.FindGenerator();
	}

	void Start()
	{
		difficulty = manager.GetDifficulty();
		if (difficulty == 2) // Hard
		{
			minimumEnemyPerDangerCell = hardMinimumEnemyPerDangerCell;
			minimumHoveringLightPerLootCell = hardMinimumHoveringLightPerLootCell;
			enemyLevel2Weight = hardEnemyLevel2Weight;
			enemyLevel3Weight = hardEnemyLevel3Weight;
		}

		if (difficulty == 3) // Extreme
		{
			minimumEnemyPerDangerCell = hardMinimumEnemyPerDangerCell;
			minimumHoveringLightPerLootCell = extremeMinimumHoveringLightPerLootCell;
			enemyLevel2Weight = extremeEnemyLevel2Weight;
			enemyLevel3Weight = extremeEnemyLevel3Weight;
			enemyPerCell = extremeEnemyPerCell;
			hoveringLightsPerCell = extremeHoveringLightsPerCell;
		}
		seed = manager.GetSeed();
		GenerateMap(seed);
		SetPathingGraph(map);
	}

	public void GenerateMap(int seed)
	{
		map = new Cell[height, length];
		Random.InitState(seed);
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < length; j++)
			{
				map[i, j].room = 0;
				map[i, j] = new Cell();
				map[i, j].pos = new Vector2(i, j);
				if (Random.value <= amountOfLiving)
				{
					map[i, j].alive = true;
				}
				else
				{
					map[i, j].alive = false;
				}
			}
		}

		for (int i = 0; i < stepIterations; i++)
		{
			Step();
		}
		FindRooms(map);
		
		originalRoomsMap = map;
		DigTunnels(map);
		originalRooms = rooms;
		SetRoomTypes();
		SpawnEntities(originalRoomsMap);

		if (rooms.Length > 1)
		{
			ConnectEveryRoom(map);
		}
		DrawRooms(map);
		SpawnBorders();
		SetPathingGraph(map);
	}

	private void DrawRoomTypes(Cell[,] map)
	{
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < length; j++)
			{
				if (map[i, j].room == 0)
					continue;

				if (originalRooms[map[i, j].room - 1].typeIndex == 1)
				{
					Instantiate(start, new Vector3(j, i, -2.0f), transform.rotation);
				}

				if (originalRooms[map[i, j].room - 1].typeIndex == 2)
				{
					Instantiate(end, new Vector3(j, i, -2.0f), transform.rotation);
				}

				if (originalRooms[map[i, j].room - 1].typeIndex == 4)
				{
					Instantiate(loot, new Vector3(j, i, -2.0f), transform.rotation);
				}

				if (originalRooms[map[i, j].room - 1].typeIndex == 5)
				{
					Instantiate(danger, new Vector3(j, i, -2.0f), transform.rotation);
				}
			}
		}
	}

	private void Step()
	{
		BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < length; j++)
			{
				int neighbours = 0;

				foreach (Vector2Int b in bounds.allPositionsWithin)
				{
					if (b.x == 0 && b.y == 0) continue;
					if (j + b.x < 0 || j + b.x >= length || i + b.y < 0 || i + b.y >= height) continue;

					if (map[i + b.y, j + b.x].alive)
					{
						neighbours++;
					}
				}

				if (map[i, j].alive && (neighbours >= neighboursToLive && neighbours < neighboursToDie))
				{
					map[i, j].willLive = true;
				}
				else if (!map[i, j].alive && neighbours >= neighboursToSpawn)
				{
					map[i, j].willLive = true;
				}
				else
				{
					map[i, j].willLive = false;
				}
			}
		}

		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < length; j++)
			{
				map[i, j].alive = map[i, j].willLive;
			}
		}
	}

	private void FindRooms(Cell[,] map)
	{
		currentRoomIndex = 0;
		BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < length; j++)
			{
				if (!map[i, j].alive) continue;
				if (map[i, j].room != 0) continue;
				List<Vector2Int> openList = new List<Vector2Int>();
				List<Vector2Int> closedList = new List<Vector2Int>();
				openList.Add(new Vector2Int(i, j));
				currentRoomIndex++;

				while (openList.Count > 0)
				{
					map[openList[0].x, openList[0].y].room = currentRoomIndex;
					closedList.Add(openList[0]);

					foreach (Vector2Int b in bounds.allPositionsWithin)
					{
						if (b.x == 0 && b.y == 0) continue;

						if (b.x != 0 && b.y != 0) continue;

						Vector2Int pos = new Vector2Int(openList[0].x + b.x, openList[0].y + b.y);
						if (pos.x < 0 || pos.x >= length || pos.y < 0 || pos.y >= height) continue;

						if (!map[pos.x, pos.y].alive)
						{
							map[pos.x, pos.y].typeIndex = 1;
							map[pos.x, pos.y].room = currentRoomIndex;
							continue;
						}
		
						if (map[pos.x, pos.y].room != 0)
							continue;
		
						if (closedList.Contains(pos))
							continue;

						if (openList.Contains(pos))
							continue;

						openList.Add(new Vector2Int(pos.x, pos.y));
					}
					openList.RemoveAt(0);
				}
			}
		}
		rooms = new Room[currentRoomIndex];

		for (int i = 0; i < currentRoomIndex; i++)
		{
			rooms[i].index = i + 1;
			rooms[i].directNeigbors = new List<int>();
		}
	}

	private void DigTunnels(Cell[,] map)
	{
		List<Cell>[] walls = new List<Cell>[currentRoomIndex];

		for (int i = 0; i < currentRoomIndex; i++)
		{
			walls[i] = new List<Cell>();
		}

		for (int i = 0; i < currentRoomIndex; i++)
		{
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < length; k++)
				{
					if (map[j, k].typeIndex == 1 && map[j, k].room == i + 1)
					{
						walls[i].Add(map[j, k]);
					}
				}
			}
		}

		for (int i = 0; i < currentRoomIndex; i++)
		{
			Cell[] closestWalls = new Cell[2];
			float minimumDistance = height + length;
			int connectedRoomIndex = 0;

			for (int j = 0; j < currentRoomIndex; j++)	// Digs the smalest tunnel to connect room i to room j
			{
				if (j == i) continue;		

				if (rooms[j].directNeigbors.Count > 0)
				{
					bool alreadyConnected = false;
					for (int l = 0; l < rooms[j].directNeigbors.Count; l++)
					{
						if (rooms[j].directNeigbors[l] == i + 1)
						{
							alreadyConnected = true;
							break;
						}
					}
					if (alreadyConnected) continue;
				}

				for (int k = 0; k < walls[i].Count; k++)
				{
					for (int m = 0; m < walls[j].Count; m++)
					{
						if (minimumDistance > (walls[i][k].pos - walls[j][m].pos).magnitude)
						{
							minimumDistance = (walls[i][k].pos - walls[j][m].pos).magnitude;
							closestWalls[0] = walls[i][k];
							closestWalls[1] = walls[j][m];
							connectedRoomIndex = j + 1;
						}
					}
				}
			}

			if (connectedRoomIndex != 0)
			{
				DigFromTo(map, closestWalls[0].pos, closestWalls[1].pos);

				rooms[i].directNeigbors.Add(connectedRoomIndex);
				rooms[connectedRoomIndex - 1].directNeigbors.Add(i + 1);
			}
		}
	}

	private void DigFromTo(Cell[,] map, Vector2 from, Vector2 to)
	{
		Vector2 direction = to - from;
		int xSign = 0;
		int ySign = 0;

		if (direction.x >= 0)
		{
			xSign = 1;
		}
		else
		{
			xSign = -1;
		}

		if (direction.y >= 0)
		{
			ySign = 1;
		}
		else
		{
			ySign = -1;
		}

		direction = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
		Vector2 lastCell = from;
		map[(int)lastCell.x, (int)lastCell.y].alive = true;
		map[(int)lastCell.x + 1, (int)lastCell.y].alive = true;
		map[(int)lastCell.x - 1, (int)lastCell.y].alive = true;
		map[(int)lastCell.x, (int)lastCell.y + 1].alive = true;
		map[(int)lastCell.x, (int)lastCell.y - 1].alive = true;

		while (direction.x + direction.y > 0)
		{
			if (Random.Range(minimumRange, 1) * direction.x >= Random.Range(minimumRange, 1) * direction.y)
			{
				lastCell += new Vector2(xSign, 0);
				direction.x--;
				map[(int)lastCell.x, (int)lastCell.y + 1].alive = true;
				map[(int)lastCell.x, (int)lastCell.y - 1].alive = true;
			}
			else
			{
				lastCell += new Vector2(0, ySign);
				direction.y--;
				map[(int)lastCell.x + 1, (int)lastCell.y].alive = true;
				map[(int)lastCell.x - 1, (int)lastCell.y].alive = true;
			}
			map[(int)lastCell.x, (int)lastCell.y].alive = true;
		}
	}

	private void ConnectEveryRoom(Cell[,] mapToReroom)
	{
		infiniteCheck++;

		if (infiniteCheck >= 10)
		{
			map = reRoomingMap;
			return;
		}
		reRoomingMap = mapToReroom;

		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < length; j++)
			{
				reRoomingMap[i, j].room = 0;
				if (reRoomingMap[i, j].alive)
				{
					reRoomingMap[i, j].typeIndex = 3;
				}
				else
				{
					reRoomingMap[i, j].typeIndex = 2;
				}
			}
		}
		FindRooms(reRoomingMap);

		if (rooms.Length > 1)
		{
			DigTunnels(reRoomingMap);
			ConnectEveryRoom(reRoomingMap);
		}
		else
		{
			map = reRoomingMap;
		}
		map = reRoomingMap;
	}

	private void DrawRooms(Cell[,] map)
	{
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < length; j++)
			{
				if (map[i, j].typeIndex == 1)
				{
					Instantiate(softWall, new Vector3(j, i, 0.0f), transform.rotation);
				}
				else
				{
					if (map[i, j].typeIndex == 2)
					{
						Instantiate(wall, new Vector3(j, i, 0.0f), transform.rotation);
					}
				}
			}
		}
	}

	private void SpawnBorders()
	{
		for (int i = -1; i <= height; i++)
		{
			Instantiate(softWall, new Vector3(-1.0f, i, 0.0f), transform.rotation);
			Instantiate(softWall, new Vector3(length, i, 0.0f), transform.rotation);
		}
		for (int i = 0; i < length; i++)
		{
			Instantiate(softWall, new Vector3(i, -1.0f, 0.0f), transform.rotation);
			Instantiate(softWall, new Vector3(i, height, 0.0f), transform.rotation);
		}

		for (int j = 0; j < borderThickness; j++)
		{
			for (int i = -2 - j; i <= height + 1 + j; i++)
			{
				Instantiate(wall, new Vector3(-2.0f - j, i, 0.0f), transform.rotation);
				Instantiate(wall, new Vector3(length + 1.0f + j, i, 0.0f), transform.rotation);
			}
			for (int i = -1 - j; i <= length + j; i++)
			{
				Instantiate(wall, new Vector3(i, -2.0f - j, 0.0f), transform.rotation);
				Instantiate(wall, new Vector3(i, height + 1.0f + j, 0.0f), transform.rotation);
			}
		}
	}

	private void SetRoomTypes()
	{
		if (originalRooms.Length < 3)
		{
			manager.Reload();
			return;
		}
		int mostConnectedRoom = 0;
		int lessConnectedRoom = 0;

		for (int i = 0; i < originalRooms.Length; i++)
		{
			if (originalRooms[i].directNeigbors.Count > originalRooms[mostConnectedRoom].directNeigbors.Count)
			{
				mostConnectedRoom = i;
			}

			if (originalRooms[i].directNeigbors.Count < originalRooms[lessConnectedRoom].directNeigbors.Count)
			{
				lessConnectedRoom = i;
			}
			else
			{
				if (originalRooms[i].directNeigbors.Count == originalRooms[lessConnectedRoom].directNeigbors.Count && (Random.value <= 0.70f))
				{
					lessConnectedRoom = i;
				}
			}
		}

		originalRooms[mostConnectedRoom].typeIndex = 1;
		originalRooms[lessConnectedRoom].typeIndex = 2;

		for (int i = 0; i < originalRooms.Length; i++)
		{
			if (originalRooms[i].typeIndex != 0)
				continue;

			if (originalRooms[i].directNeigbors.Count == 1)
			{
				originalRooms[i].typeIndex = 4;
				continue;
			}

			if (Random.value <= dangerZoneRatio)
			{
				originalRooms[i].typeIndex = 5;
			}
			else
			{
				originalRooms[i].typeIndex = 4;
			}
		}
	}

	private void SpawnEntities(Cell[,] map)
	{
		int startCells = 0;
		int endCells = 0;
		float dangerCells = 0;
		float lootCells = 0;

		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < length; j++)
			{
				if (map[i, j].typeIndex == 1 && !(originalRooms[map[i, j].room - 1].typeIndex == 5))
				{
					map[i, j].room = 0;
					continue;
				}

				if (map[i, j].room == 0)
					continue;

				if (originalRooms[map[i, j].room - 1].typeIndex == 0)
				{
					continue;
				}

				if (originalRooms[map[i, j].room - 1].typeIndex == 1) // if start
				{
					startCells++;
				}

				if (originalRooms[map[i, j].room - 1].typeIndex == 2) // if end
				{
					endCells++;
				}

				if (originalRooms[map[i, j].room - 1].typeIndex == 4 || originalRooms[map[i, j].room - 1].typeIndex == 1) // if freeloot or start
				{
					lootCells += minimumHoveringLightPerLootCell;

					if (lootCells >= 1)
					{
						lootCells = 0;
						Instantiate(hoveringLight, new Vector3(j, i, 0.0f), transform.rotation);
					}
					else
					{
						if (Random.Range(0.0f, 1.0f) <= hoveringLightsPerCell)
						{
							Instantiate(hoveringLight, new Vector3(j, i, 0.0f), transform.rotation);
						}
					}
					continue;
				}

				if (originalRooms[map[i, j].room - 1].typeIndex == 5 || originalRooms[map[i, j].room - 1].typeIndex == 2) // if danger
				{
					dangerCells += minimumEnemyPerDangerCell;
					if (dangerCells >= 1)
					{
						dangerCells = 0;
						SpawnEnemy(j, i, true);
					}
					else
					{
						SpawnEnemy(j, i, false);
					}
					continue;
				}
			}
		}
		bool startSpawned = false;
		bool endSpawned = false;
		int startCell = (int)Random.Range(0.0f, startCells);
		int endCell = (int)Random.Range(0.0f, endCells);

		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < length; j++)
			{
				if (map[i, j].room == 0)
					continue;

				if (originalRooms[map[i, j].room - 1].typeIndex > 2 || originalRooms[map[i, j].room - 1].typeIndex == 0)
				{
					continue;
				}

				if (originalRooms[map[i, j].room - 1].typeIndex == 1) // if start
				{
					startCells--;

					if (!startSpawned && startCells == startCell)
					{
						startPos = Instantiate(start, new Vector3(j, i, 0.0f), transform.rotation);
						startSpawned = true;
					}
					continue;
				}

				if (originalRooms[map[i, j].room - 1].typeIndex == 2) // if end
				{
					endCells--;
					if (!endSpawned && endCells == endCell)
					{
						Instantiate(end, new Vector3(j, i, 0.0f), transform.rotation);
						endSpawned = true;
					}
					continue;
				}
			}
		}

		player.transform.position = startPos.transform.position;
	}

	private bool SpawnEnemy(int x, int y, bool forceSpawn)
	{
		if (Random.Range(0.0f, 1.0f) <= enemyPerCell || forceSpawn)
		{
			float randomValue = Random.Range(0.0f, (enemyLevel1Weight + enemyLevel2Weight + enemyLevel3Weight));

			if (randomValue < enemyLevel1Weight)
			{
				Instantiate(circleLevel1, new Vector3(x, y, 0.0f), transform.rotation);
				return true;
			}
			else
			{
				if (randomValue - enemyLevel1Weight < enemyLevel2Weight)
				{
					Instantiate(circleLevel2, new Vector3(x, y, 0.0f), transform.rotation);
					return true;
				}
				else
				{
					if (randomValue - enemyLevel1Weight - enemyLevel2Weight <= enemyLevel3Weight)
					{
						Instantiate(circleLevel3, new Vector3(x, y, 0.0f), transform.rotation);

						return true;
					}
					else // only to avoid an error
						return false;
				}
			}
		}
		else
			return false;
	}

	private void SetPathingGraph(Cell[,] map)
	{
		pathingGraph = new PathCell[height, length];

		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < length; j++)
			{
				pathingGraph[i, j] = new PathCell();
				pathingGraph[i, j].neighbors = new List<PathCell>();
				pathingGraph[i, j].isEnd = false;
				pathingGraph[i, j].explored = false;
				pathingGraph[i, j].ID = new Vector2(i, j);
			}
		}

		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < length; j++)
			{
				if (map[i, j].typeIndex == 0)
					pathingGraph[i, j].typeWeight = wallWeight;
				if (map[i, j].typeIndex == 1)
					pathingGraph[i, j].typeWeight = softWallWeight;
				if (map[i, j].alive)
					pathingGraph[i, j].typeWeight = voidWeight;

				BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

				foreach (Vector2Int b in bounds.allPositionsWithin)
				{
					if (b.x == 0 && b.y == 0) continue;
					if (j + b.x < 0 || j + b.x >= length || i + b.y < 0 || i + b.y >= height) continue;

					pathingGraph[i, j].neighbors.Add(pathingGraph[i + b.y, j + b.x]);
				}
			}
		}
	}

	public PathCell[,] GetPathingGraph()
	{
		return pathingGraph;
	}

	public int GetLength()
	{
		return length;
	}

	public int GetHeight()
	{
		return height;
	}

	public List<Vector2> FindPathFromTo(Vector2 from, Vector2 to)
	{
		List<Vector2> path = new List<Vector2>();

		if (from == to || from.x >= height || from.x < 0 || from.y >= height || from.y < 0 || to.x >= height || to.x < 0 || to.y >= height || to.y < 0) //Checks if within Borders
		{
			path.Add(to);
			return path;
		}
		BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

		foreach (Vector2Int b in bounds.allPositionsWithin)
		{
			if (b.x == 0 && b.y == 0) continue;
			if (from.x + b.x < 0 || from.x + b.x >= length || from.y + b.y < 0 || from.y + b.y >= height) continue;

			if (to == b + from)
			{
				path.Add(to);
				return path;
			}
		}
		Instantiate(cellaimed, new Vector3(to.x, to.y, 0.0f), transform.rotation);
		PathCell[,] usagePathGraph = pathingGraph;

		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < length; j++)
			{
				usagePathGraph[i, j].explored = false;
				usagePathGraph[i, j].isEnd = false;
				usagePathGraph[i, j].totalWeight = -usagePathGraph[i, j].typeWeight - (int)(heuristicTargetingPlayer / Mathf.Sqrt((i - to.x) * (i - to.x) + (j - to.y) * (j - to.y)));
			}
		}
		bool lookingForPath = true;
		bool pathLoading = true;
		List<PathCell> toExplore = new List<PathCell>();
		PathCell start = usagePathGraph[(int)from.x, (int)from.y];
		PathCell end = usagePathGraph[(int)to.x, (int)to.y];
		usagePathGraph[(int)to.x, (int)to.y].isEnd = true;
		usagePathGraph[(int)to.x, (int)to.y].totalWeight -= endWeight;
		PathCell cellToCircle;
		toExplore.Add(start);

		while (lookingForPath)
		{
			toExplore = toExplore.OrderBy(x => x.totalWeight).ToList();
			cellToCircle = toExplore[0];
			toExplore.RemoveAt(0);

			for (int i = cellToCircle.neighbors.Count - 1; i >= 0; i--)
			{
				PathCell currentlyExplored = cellToCircle.neighbors[i];
				if (usagePathGraph[(int)currentlyExplored.ID.x, (int)currentlyExplored.ID.y].explored)
				{
					continue;
				}

				if (currentlyExplored.cameFromID == Vector2.zero)
				{
					currentlyExplored.cameFromID = cellToCircle.ID;
					cellToCircle.neighbors[i] = currentlyExplored;
				}
				usagePathGraph[(int)currentlyExplored.ID.x, (int)currentlyExplored.ID.y].cameFromID = currentlyExplored.cameFromID;

				if (usagePathGraph[(int)currentlyExplored.ID.x, (int)currentlyExplored.ID.y].isEnd) //returns the path
				{
					lookingForPath = false;
					end.cameFromID = currentlyExplored.cameFromID;
					break;
				}
				else
				{

					Instantiate(pathedCell, new Vector3(currentlyExplored.ID.x, currentlyExplored.ID.y, -9.0f), transform.rotation);
				}
				toExplore.Add(usagePathGraph[(int)currentlyExplored.ID.x, (int)currentlyExplored.ID.y]);
				usagePathGraph[(int)currentlyExplored.ID.x, (int)currentlyExplored.ID.y].explored = true;
			}
		}

		while (pathLoading)
		{
			if (end.cameFromID != start.ID)
			{
				path.Add(end.ID);
				end = usagePathGraph[(int)end.cameFromID.x, (int)end.cameFromID.y];
			}
			else
			{
				pathLoading = false;
			}
		}
		return path;
	}
}

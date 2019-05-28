using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class LevelGenerator : MonoBehaviour
{
    enum LevelTile {empty, floor, wall, bottomWall};
    LevelTile[,] grid;
    struct RandomWalker {
		public Vector2 dir;
		public Vector2 pos;
	}
    List<RandomWalker> walkers; 

    public GameObject[] floorTiles;
    public GameObject[] wallTiles;
    public GameObject[] bottomWallTiles;
    public GameObject exit;
    public GameObject player;

    public GameObject virtualCamera;

    public int levelWidth;
    public int levelHeight;
	public float percentToFill = 0.2f; 
	public float chanceWalkerChangeDir = 0.5f;
    public float chanceWalkerSpawn = 0.05f;
    public float chanceWalkerDestoy = 0.05f;
    public int maxWalkers = 10;
    public int iterationSteps = 100000;

    void Awake() {
        Setup();
        CreateFloors();    
        CreateWalls();
        CreateBottomWalls();
        SpawnLevel();
        SpawnPlayer();
        SpawnExit();
    }

    void Setup() {
        // prepare grid
		grid = new LevelTile[levelWidth, levelHeight];
		for (int x = 0; x < levelWidth - 1; x++){
			for (int y = 0; y < levelHeight - 1; y++){ 
				grid[x, y] = LevelTile.empty;
			}
		}

        //generate first walker
		walkers = new List<RandomWalker>();
		RandomWalker walker = new RandomWalker();
		walker.dir = RandomDirection();
		Vector2 pos = new Vector2(Mathf.RoundToInt(levelWidth/ 2.0f), Mathf.RoundToInt(levelHeight/ 2.0f));
		walker.pos = pos;
		walkers.Add(walker);
    }

	void CreateFloors() {
		int iterations = 0;
		do{
			//create floor at position of every Walker
			foreach (RandomWalker walker in walkers){
				grid[(int)walker.pos.x,(int)walker.pos.y] = LevelTile.floor;
			}

			//chance: destroy Walker
			int numberChecks = walkers.Count;
			for (int i = 0; i < numberChecks; i++) {
				if (Random.value < chanceWalkerDestoy && walkers.Count > 1){
					walkers.RemoveAt(i);
					break;
				}
			}

			//chance: Walker pick new direction
			for (int i = 0; i < walkers.Count; i++) {
				if (Random.value < chanceWalkerChangeDir){
					RandomWalker thisWalker = walkers[i];
					thisWalker.dir = RandomDirection();
					walkers[i] = thisWalker;
				}
			}

			//chance: spawn new Walker
			numberChecks = walkers.Count;
			for (int i = 0; i < numberChecks; i++){
				if (Random.value < chanceWalkerSpawn && walkers.Count < maxWalkers) {
					RandomWalker walker = new RandomWalker();
					walker.dir = RandomDirection();
					walker.pos = walkers[i].pos;
					walkers.Add(walker);
				}
			}

			//move Walkers
			for (int i = 0; i < walkers.Count; i++){
				RandomWalker walker = walkers[i];
				walker.pos += walker.dir;
				walkers[i] = walker;				
			}

			//avoid boarder of grid
			for (int i =0; i < walkers.Count; i++){
				RandomWalker walker = walkers[i];
				walker.pos.x = Mathf.Clamp(walker.pos.x, 1, levelWidth-2);
				walker.pos.y = Mathf.Clamp(walker.pos.y, 1, levelHeight-2);
				walkers[i] = walker;
			}

			//check to exit loop
			if ((float)NumberOfFloors() / (float)grid.Length > percentToFill){
				break;
			}
			iterations++;
		} while(iterations < iterationSteps);
	}

	void CreateWalls(){
		for (int x = 0; x < levelWidth-1; x++) {
			for (int y = 0; y < levelHeight-1; y++) {
				if (grid[x,y] == LevelTile.floor) {
					if (grid[x,y+1] == LevelTile.empty) {
						grid[x,y+1] = LevelTile.wall;
					}

					if (grid[x,y-1] == LevelTile.empty) {
						grid[x,y-1] = LevelTile.wall;
					}
					if (grid[x+1,y] == LevelTile.empty) {
						grid[x+1,y] = LevelTile.wall;
					}
					if (grid[x-1,y] == LevelTile.empty) {
						grid[x-1,y] = LevelTile.wall;
					}

                    if (grid[x - 1, y - 1] == LevelTile.empty) {
                        grid[x - 1, y - 1] = LevelTile.wall;
                    }
                    if (grid[x - 1, y + 1] == LevelTile.empty) {
                        grid[x - 1, y + 1] = LevelTile.wall;
                    }
                    if (grid[x + 1, y + 1] == LevelTile.empty) {
                        grid[x + 1, y + 1] = LevelTile.wall;
                    }
                    if (grid[x + 1, y - 1] == LevelTile.empty) {
                        grid[x + 1, y - 1] = LevelTile.wall;
                    }
                }
            }
		}
	}



	void CreateBottomWalls() {
		for (int x = 0; x < levelWidth; x++) {
			for (int y = 1; y < levelHeight; y++) {
				if (grid[x, y] == LevelTile.wall && grid[x, y - 1] == LevelTile.floor) {
                    grid[x, y] = LevelTile.bottomWall;
                }
            }
		}
	}

	void SpawnLevel(){
		for (int x = 0; x < levelWidth; x++) {
			for (int y = 0; y < levelHeight; y++) {
				switch(grid[x, y]) {
					case LevelTile.empty:
						break;
					case LevelTile.floor:
						Spawn(x, y, floorTiles[Random.Range(0, floorTiles.Length)]);
						break;
					case LevelTile.wall:
						Spawn(x, y, wallTiles[Random.Range(0, wallTiles.Length)]);
						break;
					case LevelTile.bottomWall:
						Spawn(x, y, bottomWallTiles[Random.Range(0, bottomWallTiles.Length)]);
						break;
				}
			}
		}
	}    

	Vector2 RandomDirection(){
		int choice = Mathf.FloorToInt(Random.value * 3.99f);
		switch (choice){
			case 0:
				return Vector2.down;
			case 1:
				return Vector2.left;
			case 2:
				return Vector2.up;
			default:
				return Vector2.right;
		}
	}

	int NumberOfFloors() {
		int count = 0;
		foreach (LevelTile space in grid){
			if (space == LevelTile.floor){
				count++;
			}
		}
		return count;
	}

    void SpawnPlayer() {
		Vector3 pos = new Vector3(Mathf.RoundToInt(levelWidth / 2.0f),
										Mathf.RoundToInt(levelHeight / 2.0f), 0);
		GameObject playerObj = Instantiate(player, pos, Quaternion.identity) as GameObject;
        CinemachineVirtualCamera vCam = virtualCamera.GetComponent<CinemachineVirtualCamera>();
        vCam.m_Follow = playerObj.transform;
    }

    public void SpawnExit() {

        Vector2 playerPos = new Vector2(Mathf.RoundToInt(levelWidth/ 2.0f),
                                Mathf.RoundToInt(levelHeight/ 2.0f));
        Vector2 exitPos = playerPos;
        float exitDistance = 0f;

		for (int x = 0; x < levelWidth - 1; x++){
			for (int y = 0; y < levelHeight - 1; y++){
				if (grid[x,y] == LevelTile.floor){
                    Vector2 nextPos = new Vector2(x, y);
                    float distance = Vector2.Distance(playerPos, nextPos);
                    if (distance > exitDistance) {
                        exitDistance = distance;
                        exitPos = nextPos;
                    }
                }
            }
        }

        Spawn(exitPos.x, exitPos.y, exit);
    }       

	void Spawn(float x, float y, GameObject toSpawn) {
		Instantiate(toSpawn, new Vector3(x, y, 0), Quaternion.identity);
	}
}

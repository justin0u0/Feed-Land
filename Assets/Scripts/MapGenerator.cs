﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject mapPrefab;
    public GameObject redTreePrefab;
    public GameObject blueTreePrefab;
    public GameObject redMinePrefab;
    public GameObject blueMinePrefab;
    public GameObject pathEffectPrefab;
    public GameObject cannonPlacePrefab;
    public GameObject unicorn;
    public int Level1ElementNum;
    public int Level2ElementNum;
    // public int minLevel1ElementNum;
    // public int maxLevel1ElementNum;
    // public int minLevel2ElementNum;
    // public int maxLevel2ElementNum;
    public int initMapNum;
    public bool isInitialized { get; private set; }
    private int currentMapIdx;

    private int cannonPlaceNum;
    private int redTreesNum;
    private int redMinesNum;
    private int blueTreesNum;
    private int blueMinesNum;
    private int mapWidth;
    private int mapHeight;
    private int prevZPoint;
    private List<List<Vector3>> paths;
    // private List<Vector3> elementPositions;
    private List<System.Tuple<Vector3, GameObject>> elementPositions;
    

    void Start()
    {
        // initialization
        mapWidth = 28;
        mapHeight = 14;
        currentMapIdx = 0;
        cannonPlaceNum = 6;
        prevZPoint = 0;
        paths = new List<List<Vector3>>();

        for (int i = 0; i < initMapNum; i++)
        {
            GenerateMap();
        }
        isInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (unicorn != null)
        {
            int unicornMapIdx = FindUnicornPosition();
            if (unicornMapIdx == currentMapIdx - 1) GenerateMap();
        }
    }

    public int GetCurrentMapIdx()
    {
        return currentMapIdx;
    }

    private void GenerateMap()
    {
        // elementPositions = new List<Vector3>();
        elementPositions = new List<System.Tuple<Vector3, GameObject>>();
        // create ground first
        Vector3 mapPos = new Vector3(currentMapIdx * mapWidth, 0f, 0f);
        Instantiate(mapPrefab, mapPos, Quaternion.identity);
        // generate path on the map
        GeneratePath(new Vector3(-13.5f + currentMapIdx * mapWidth, 0.5f, -6.5f));
        // generate elements on the map
        // redTreesNum = Random.Range(minLevel1ElementNum, maxLevel1ElementNum);
        // redMinesNum = Random.Range(minLevel1ElementNum, maxLevel1ElementNum);
        // level1
        // redTreesNum = Level1ElementNum;
        // redMinesNum = Level1ElementNum;
        // GenerateElement(redTreesNum, new Vector3(-13.5f + currentMapIdx * mapWidth, 0.5f, 6.5f), redTreePrefab);
        // GenerateElement(redMinesNum, new Vector3(-13.5f + currentMapIdx * mapWidth, 0.5f, 6.5f), redMinePrefab);
        // // level2
        // blueTreesNum = Level2ElementNum;
        // blueMinesNum = Level2ElementNum;
        // GenerateElement(blueTreesNum, new Vector3(-13.5f + currentMapIdx * mapWidth, 0.5f, 6.5f), blueTreePrefab);
        // GenerateElement(blueMinesNum, new Vector3(-13.5f + currentMapIdx * mapWidth, 0.5f, 6.5f), blueMinePrefab);
        // // generate cannonPlace on the map
        // GenerateCannonPlace(new Vector3(-13.5f + currentMapIdx * mapWidth, 0f, -1.5f));
        currentMapIdx++;
    }

    private void Shuffle(List<Vector3> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Vector3 currentIndex = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = currentIndex;
        }
    }

    private void GenerateElementPosition(int total, Vector3 offset, GameObject prefab)
    {
        HashSet<Vector3> topLeftPositions = new HashSet<Vector3>();

        // first, random generate the position
        for (int i = 0; i < total; i++)
        {
            Vector3 point;
            do
            {
                point = new Vector3(Random.Range(0, mapWidth - 2), 0f, (-1) * Random.Range(0, mapHeight - 2));
            } while (CheckIfElementOverlay(point));
            topLeftPositions.Add(point);
        }

        // next, place the element according to the column position
        // notice that we will reserved 3*3 place for that kind of elements
        // random the number of each 3*3 place for diversity
        List<Vector3> box = new List<Vector3>();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                box.Add(new Vector3(i, 0f, -j));
            }
        }

        foreach (Vector3 tfpos in topLeftPositions)
        {
            int num = Random.Range(3, 7);
            Vector3 position = offset + tfpos;
            Shuffle(box);

            for (int i = 0; i < num; i++)
            {
                System.Tuple<Vector3, GameObject> tp = new System.Tuple<Vector3, GameObject> (position + box[i], prefab);
                elementPositions.Add(tp);
            }
        }
    }

    private bool CheckIfElementOverlay(Vector3 position)
    {
        for (int x=0; x<3; x++) {
            for (int z=0; z<3; z++) {
                bool contians = elementPositions.Any(m => m.Item1 == new Vector3(x, 0f, z));
                if (contians) return true;
                // if (elementPositions.Contains(position + new Vector3(x, 0f, z))) return true;
            }
        }
        return false;
    }

    private void GeneratePath(Vector3 offset)
    {
        // First, random each column position
        List<int> zPositions = new List<int>();
        int pathWidth = 3;
        zPositions.Add(prevZPoint);
        for (int i = 0; i < mapWidth - 1; i++)
        {
            int zPos = Random.Range(0, mapHeight);
            zPositions.Add(zPos);
            for (int k = 1; k < pathWidth; k++)
            {
                zPositions.Add(zPos);
                if (i == mapWidth - 2) prevZPoint = zPos;
                if (i < mapWidth - 1) i++;
                else break;
            }
            // old version with pathWidth = 1
            // zPositions.Add(Random.Range(0, lines));
        }
        

        // direction
        // 1 -> up
        // 0 -> right
        // -1 -> down
        // Next, generate the path according to each column position
        int currentZPosition = 0;
        List<Vector3> path = new List<Vector3>();
        path.Add(offset);
        foreach (int p in zPositions)
        {
            Vector3 point = new Vector3(1f, 0f, 0f);
            offset += point;
            path.Add(offset);

            if (currentZPosition != p)
            {
                int step = (currentZPosition > p) ? -1 : 1;
                for (int i = 0; i < Mathf.Abs(currentZPosition - p); i++)
                {
                    Vector3 vec = (currentZPosition > p) ? new Vector3(0f, 0f, -1) : new Vector3(0f, 0f, 1);
                    offset += vec;
                    path.Add(offset);
                }
            }
            currentZPosition = p;
        }

        // Finally, load the effect on the determined path
        LoadEffect(path);
        paths.Add(path);
    }
    private void LoadEffect(List<Vector3> path)
    {
        foreach (Vector3 p in path)
        {
            Instantiate(pathEffectPrefab, p, Quaternion.identity);
        }
    }

    public List<Vector3> GetPath(int idx)
    {
        return paths[idx];
    }

    private void GenerateCannonPlace(Vector3 offset)
    {
        int lines = 3;
        // first, random the column position
        List<int> xPositions = new List<int>();
        for (int i = 0; i < cannonPlaceNum; i++)
        {
            int xPos;
            do
            {
                xPos = Random.Range(1, mapWidth - 1);
            } while (CheckIfCannonPlaceOverlay(xPositions, xPos));
            xPositions.Add(xPos);
        }

        // next, place the cannonPlace according to the column position 
        // (same as generate element)
        // notice that there are 3 available z positions to place
        // so we need to decide it first (random)
        // Vector3 baseY = new Vector3(0f, 1f, 0f);
        foreach (int xPosition in xPositions)
        {
            int zPos = Random.Range(0, lines - 1);
            Vector3 position = offset + new Vector3(xPosition, 1f, -zPos);
            Instantiate(cannonPlacePrefab, position, Quaternion.identity);
        }
    }

    private bool CheckIfCannonPlaceOverlay(List<int> xPositions, int xPos)
    {
        // If there's already a cannonPlace in xPos-2, xPos-1, xPos, xPos+1, xPos+2 -> overlay
        // Else -> valid position to place the cannonPlace
        for (int i = -2; i <= 2; i++)
        {
            if (xPositions.Contains(xPos + i)) return true;
        }
        return false;
    }

    private void GeneratePrefab(Vector3 position, GameObject prefab) {
        Instantiate(prefab, position, Quaternion.identity);
    }

    public Vector3 ResetPlayerPosition()
    {
        // first, find out which map unicorn is at
        int mapIdx = FindUnicornPosition();
        // Debug.Log(mapIdx);

        // next, set the player's position in the middle of the map
        Vector3 resetPosition = new Vector3(-0.5f + mapIdx * mapWidth, 2.0f, 1.5f);
        return resetPosition;
    }

    private int FindUnicornPosition()
    {
        // Debug.Log(unicorn.name);
        int idx = Mathf.FloorToInt((14f + unicorn.transform.position.x) / mapWidth);
        idx = Mathf.Max(idx, 0);
        return idx;
    }
}

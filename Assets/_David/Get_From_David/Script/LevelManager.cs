using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int timeForChangeElement = 10;
    public GameObject tilePrefab;
    public GameObject pathPrefab;
    public GameObject startPrefab;
    private GameObject endPath; 
    private int width = 5;
    private int height = 5;
    // none:0 path:1 end:2 nearPath:3 start:4
    private int[][] map = 
    {
        new int[] {4,1,0,0,0},
        new int[] {0,1,1,0,0},
        new int[] {0,0,1,0,0},
        new int[] {0,0,1,1,0},
        new int[] {0,0,0,2,0}
    };
    private List<TileProperty> tilePropertyList = new List<TileProperty>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeMap();
        for(int row=0; row < height ; row++)
        {
            for(int col=0; col < width ; col++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(col,0,row), Quaternion.identity);
                tilePropertyList.Add(tile.GetComponent<TileProperty>());
                switch(map[row][col])
                {
                case 1:
                    Instantiate(pathPrefab, new Vector3(col,0,row), Quaternion.identity);
                    tile.GetComponent<TileProperty>().isPath = true;
                    break;
                case 2:
                    endPath = Instantiate(pathPrefab, new Vector3(col,0,row), Quaternion.identity);
                    tile.GetComponent<TileProperty>().isPath = true;
                    break;
                case 3:
                    tile.GetComponent<TileProperty>().isNearPath = true;
                    break;
                case 4:
                    Instantiate(pathPrefab, new Vector3(col,0,row), Quaternion.identity);
                    tile.GetComponent<TileProperty>().isPath = true;
                    Instantiate(startPrefab, new Vector3(col, 1, row), Quaternion.identity);
                    break;
                }
            }
        }
        StartCoroutine(ElementChangeRoutine());
        findPath(endPath);
    }

    public void findPath(GameObject endPath)
    {
        PathDetactor endPathScript = endPath.GetComponent<PathDetactor>();
        endPathScript.findPrevPath(MoveDirection.End);
    }

    private IEnumerator ElementChangeRoutine()
    {
        yield return new WaitForSeconds(2f);
        while (true)
        {
            // 隨機挑選 n 個方塊並變色
            int randCount = Random.Range(3,6);
            PickAndChangeRandomBlocks(randCount);
            ResolveElementChains();
            // 等待時間
            yield return new WaitForSeconds(timeForChangeElement);
        }
    }

    private void PickAndChangeRandomBlocks(int n)
    {
        if (tilePropertyList.Count == 0) return;

        // 安全機制：如果想要改變的數量大於總數，就限制它
        n = Mathf.Min(n, tilePropertyList.Count);

        // 複製一份暫存清單，用來進行不重複抽樣
        List<TileProperty> pool = new List<TileProperty>(tilePropertyList);

        for (int i = 0; i < n; i++)
        {
            // 隨機從 pool 裡面挑一個索引
            int randomIndex = Random.Range(0, pool.Count);
            TileProperty selectedTile = pool[randomIndex];

            if(selectedTile.element != Element.None)
            {
                i--;
                continue;
            }

            selectedTile.ResetElementTimer();

            int randElement = Random.Range(0,2);

            if(randElement == 0)
            {
                selectedTile.element = Element.Fire;
            }
            if(randElement == 1)
            {
                selectedTile.element = Element.Ice;
            }


            // 從暫存清單移除，確保下次循環不會重複抽到同一個
            pool.RemoveAt(randomIndex);
        }
    }

    public void ResolveElementChains()
    {
        foreach(TileProperty tileScript in tilePropertyList)
        {
            tileScript.DetactActive();
        }
        foreach(TileProperty tileScript in tilePropertyList)
        {
            while(tileScript.chainCount > 0)
            {
                tileScript.chainCount--;
                int randCount = Random.Range(3,6);
                PickAndChangeRandomBlocks(randCount);
                ResolveElementChains();
            }
        }
    }

    void InitializeMap()
    {
        for(int i=0 ; i < map.Length ; i++)
        {
            for(int j=0 ; j < map[0].Length ; j++)
            {
                if(map[i][j] == 1)
                {
                    for(int r = -1 ; r <= 1 ; r++)
                    {
                        if((i+r) < 0 || (i+r) >= map.Length)
                        {
                            continue;
                        }
                        if(map[i+r][j] == 0)
                        {
                            map[i+r][j] = 3;
                        }
                    }
                    for(int c = -1 ; c <= 1 ; c++)
                    {
                        if((j+c) < 0  || (j+c) >= map[0].Length)
                        {
                            continue;
                        }
                        if(map[i][j+c] == 0)
                        {
                            map[i][j+c] = 3;
                        }
                    }
                }
            }
        }
    }
}

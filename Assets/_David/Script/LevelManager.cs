using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Initialize Tiles")]
    [SerializeField]public GameObject tilePrefab;
    [SerializeField]public GameObject pathPrefab;
    [SerializeField]public GameObject egg;
    [SerializeField]private GameObject endPath; 
    [SerializeField]private int width = 5;
    [SerializeField]private int height = 5;
    // none:0 path:1 end:2 nearPath:3 start:4
    [SerializeField]private int[][] map;
    [SerializeField]private int[][] level1 = 
    {
        new int[] {0,0,4,0,0,0,0,0,0,0},
        new int[] {0,0,1,0,0,1,1,1,1,0},
        new int[] {0,0,1,0,0,1,0,0,1,0},
        new int[] {0,0,1,0,0,1,0,0,1,0},
        new int[] {0,0,1,0,0,1,0,0,1,0},
        new int[] {0,0,1,0,0,1,0,0,1,0},
        new int[] {0,0,1,0,0,1,0,0,1,0},
        new int[] {0,0,1,0,0,1,0,0,1,0},
        new int[] {0,0,1,1,1,1,0,0,1,0},
        new int[] {0,0,0,0,0,0,0,0,2,0}
    };

    [Header("Element")]
    [SerializeField]public int startElementCount = 10;
    [SerializeField]public int minChange = 10;
    [SerializeField]public int maxChange = 15;
    [SerializeField]public float elementTime = 30f;

    public int timeForChangeElement = 10;
    public int level = 1;
    
    public List<TileProperty> tilePropertyList = new List<TileProperty>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        StartCoroutine(ElementChangeRoutine());
        // ConstructTiles();   //remember to delete
        // findPath(endPath);  //remember to delete
    }

    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.Space))
        // {
        //     ResolveElementChains();
        // }
    }

    private IEnumerator ElementChangeRoutine()
    {
        yield return new WaitForSeconds(0.01f);
        PickAndChangeRandomBlocks(startElementCount);
        ResolveElementChains();
        yield return new WaitForSeconds(2f);
        while (true)
        {
            // 隨機挑選 n 個方塊並變色
            int randCount = Random.Range(minChange,maxChange+1);
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

            selectedTile.elementTimer = elementTime;

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
            tileScript.isChainProgress = false;
        }
        foreach(TileProperty tileScript in tilePropertyList)
        {
            while(tileScript.chainCount > 0)
            {
                // Debug.Log(tileScript.chainCount);
                tileScript.chainCount--;
                int randCount = Random.Range(minChange,maxChange+1);
                PickAndChangeRandomBlocks(randCount);
                ResolveElementChains();
            }
        }
    }

    // void ConstructTiles()
    // {
    //     switch(level)
    //     {
    //     case 1:
    //         map = level1;
    //         break;
    //     case 2:
    //         break;
    //     case 3:
    //         break;
    //     }
    //     width = map[0].Length;
    //     height = map.Length;
    //     InitializeMap();
    //     for(int row=0; row < height ; row++)
    //     {
    //         for(int col=0; col < width ; col++)
    //         {
    //             GameObject tile = Instantiate(tilePrefab, new Vector3(col,0,row), Quaternion.identity);
    //             tilePropertyList.Add(tile.GetComponent<TileProperty>());
    //             switch(map[row][col])
    //             {
    //             case 1:
    //                 Instantiate(pathPrefab, new Vector3(col,0,row), Quaternion.identity);
    //                 tile.GetComponent<TileProperty>().isPath = true;
    //                 break;
    //             case 2:
    //                 endPath = Instantiate(pathPrefab, new Vector3(col,0,row), Quaternion.identity);
    //                 Instantiate(egg, new Vector3(col, 1, row), Quaternion.identity);
    //                 tile.GetComponent<TileProperty>().isPath = true;
    //                 break;
    //             case 3:
    //                 tile.GetComponent<TileProperty>().isNearPath = true;
    //                 break;
    //             case 4:
    //                 Instantiate(pathPrefab, new Vector3(col,0,row), Quaternion.identity);
    //                 tile.GetComponent<TileProperty>().isPath = true;
    //                 break;
    //             }
    //         }
    //     }
    // }

    // void InitializeMap()
    // {
    //     for(int i=0 ; i < height ; i++)
    //     {
    //         for(int j=0 ; j < width ; j++)
    //         {
    //             if(map[i][j] == 1)
    //             {
    //                 for(int r = -1 ; r <= 1 ; r++)
    //                 {
    //                     if((i+r) < 0 || (i+r) >= height)
    //                     {
    //                         continue;
    //                     }
    //                     if(map[i+r][j] == 0)
    //                     {
    //                         map[i+r][j] = 3;
    //                     }
    //                 }
    //                 for(int c = -1 ; c <= 1 ; c++)
    //                 {
    //                     if((j+c) < 0  || (j+c) >= width)
    //                     {
    //                         continue;
    //                     }
    //                     if(map[i][j+c] == 0)
    //                     {
    //                         map[i][j+c] = 3;
    //                     }
    //                 }
    //             }
    //         }
    //     }
    // }

    // public void findPath(GameObject endPath)
    // {
    //     PathDetactor endPathScript = endPath.GetComponent<PathDetactor>();
    //     endPathScript.findPrevPath(MoveDirection.End);
    // }
}

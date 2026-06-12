using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject pathPrefab;
    private GameObject endPath; 
    private int width = 5;
    private int height = 5;
    private int[,] pathList = 
    {
        {1,1,0,0,0},
        {0,1,1,0,0},
        {0,0,1,0,0},
        {0,0,1,1,0},
        {0,0,0,2,0}
    };
    private List<TileProperty> tilePropertyList = new List<TileProperty>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int row=0; row < height ; row++)
        {
            for(int col=0; col < width ; col++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(col,0,row), Quaternion.identity);
                tilePropertyList.Add(tile.GetComponent<TileProperty>());
                if(pathList[row,col] == 1)
                {
                    Instantiate(pathPrefab, new Vector3(col,0,row), Quaternion.identity);
                }
                else if(pathList[row,col] == 2)
                {
                    endPath = Instantiate(pathPrefab, new Vector3(col,0,row), Quaternion.identity);
                }
            }
        }
        StartCoroutine(ColorChangeRoutine());
        findPath(endPath);
    }

    public void findPath(GameObject endPath)
    {
        PathDetactor endPathScript = endPath.GetComponent<PathDetactor>();
        endPathScript.findPrevPath(MoveDirection.End);
    }

    private IEnumerator ColorChangeRoutine()
    {
        while (true)
        {
            float randTime = Random.Range(5f,10f);
            // 1. 等待時間
            yield return new WaitForSeconds(2f);

            // 3. 隨機挑選 n 個方塊並變色
            int randCount = Random.Range(3,5);
            PickAndChangeRandomBlocks(randCount);
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

            int randElement = Random.Range(0,3);

            if(randElement == 0)
            {
                selectedTile.element = Element.None;
            }
            if(randElement == 1)
            {
                selectedTile.element = Element.Fire;
            }
            if(randElement == 2)
            {
                selectedTile.element = Element.Ice;
            }


            // // 改變顏色並記錄起來
            // selectedBlock.ChangeColor(targetColor);
            // currentlyChangedBlocks.Add(selectedBlock);

            // 從暫存清單移除，確保下次循環不會重複抽到同一個
            pool.RemoveAt(randomIndex);
        }
    }
}

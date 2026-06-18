using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int timeForChangeElement = 10;
    public int level = 1;
    
    public List<TileProperty> tilePropertyList = new List<TileProperty>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ElementChangeRoutine());
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
}

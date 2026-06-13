using System.Collections;
using UnityEngine;

public class GenerateEnemy : MonoBehaviour
{
    public GameObject[] monsters;
    void Start()
    {

        StartCoroutine(GenerateMonster());
    }


    IEnumerator GenerateMonster()
    {
        while(true)
        {
            yield return new WaitForSeconds(5);
            if (monsters != null && monsters.Length > 0)
            {
                // 3. 取得隨機索引 (左閉右開區間，包含 0，但不包含陣列長度)
                int randomIndex = Random.Range(0, monsters.Length);
                GameObject monster = monsters[randomIndex];
                Debug.Log("Generate Enemy At " + transform.position);
                Instantiate(monster, transform.position, transform.rotation);
                
                // 4. 印出或使用隨機抽到的元素
                // Debug.Log("隨機選擇的項目是：" + monster);
            }
        }
        
    }
}

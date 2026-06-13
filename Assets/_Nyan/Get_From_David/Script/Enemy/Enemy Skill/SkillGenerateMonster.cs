using UnityEngine;

public class SkillGenerateMonster : MonoBehaviour
{
    public GameObject[] monsters;
    public float CD = 5;
    private float timer = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = CD;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            timer = CD;
            GenerateMonster();
        }
    }

    void GenerateMonster()
    {
        if (monsters != null && monsters.Length > 0)
        {
            // 3. 取得隨機索引 (左閉右開區間，包含 0，但不包含陣列長度)
            int randomIndex = Random.Range(0, monsters.Length);
            GameObject monster = monsters[randomIndex];
            Instantiate(monster, transform.position, transform.rotation);
            
            // 4. 印出或使用隨機抽到的元素
            // Debug.Log("隨機選擇的項目是：" + monster);
        }
    }
}

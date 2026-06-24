using System.Collections.Generic;
using UnityEngine;

public class SkillGenerateMonster : MonoBehaviour
{
    private List<GameObject> aliveMobs = new List<GameObject>();
    private LevelFlowController levelFlowController;
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

    private void TrackSpawnedEnemy(GameObject enemy)
    {
        aliveMobs.Add(enemy);
        levelFlowController?.RegisterSpawnedEnemy(enemy);
    }

    void GenerateMonster()
    {
        if (monsters != null && monsters.Length > 0)
        {
            // 3. 取得隨機索引 (左閉右開區間，包含 0，但不包含陣列長度)
            int randomIndex = Random.Range(0, monsters.Length);
            GameObject monster = monsters[randomIndex];
            GameObject mob = Instantiate(monster, transform.position, transform.rotation);
            EnemyController selfScript = GetComponent<EnemyController>();
            EnemyController mobScript = mob.GetComponent<EnemyController>();
            if(!mobScript)
            {
                Debug.Log("not found enemy controller");
            }
            float hpRatio = (float)selfScript.HP / selfScript.maxHP;
            mobScript.maxHP = Mathf.RoundToInt(monster.GetComponent<EnemyController>().maxHP * selfScript.hpRatio * (hpRatio>0.5f ? (hpRatio * 1.5f) - 0.5f : 0.25f ));
            mobScript.HP = mobScript.maxHP;
            Debug.Log((hpRatio>0.5f ? (hpRatio * 1.5f) - 0.5f : 0.25f ));
            TrackSpawnedEnemy(mob);
            // 4. 印出或使用隨機抽到的元素
            // Debug.Log("隨機選擇的項目是：" + monster);
        }
    }

    void OnDestroy()
    {
        foreach(GameObject mob in aliveMobs)
        {
            Destroy(mob);
        }
    }
}

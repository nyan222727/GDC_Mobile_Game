using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waves : MonoBehaviour
{
    private LevelFlowController levelFlowController;
    public bool isEnd = false;
    public int wave = 1;
    private bool isEndGenerate = false;
    private List<GameObject> onFieldEnemys = new List<GameObject>();
    public GameObject[] monsters;

    void Awake()
    {
        levelFlowController = FindAnyObjectByType<LevelFlowController>();
    }

    void Start()
    {
        switch(wave)
        {
        case 1:
            StartCoroutine(Wave1());
            break;
        case 2:
            StartCoroutine(Wave2());
            break;
        case 3:
            StartCoroutine(Wave3());
            break;
        case 4:
            StartCoroutine(Wave4());
            break;
        case 5:
            StartCoroutine(Wave5());
            break;
        case 6:
            StartCoroutine(Wave6());
            break;
        case 7:
            StartCoroutine(Wave7());
            break;
        case 8:
            StartCoroutine(Wave8());
            break;
        }
        

    }

    private void TrackSpawnedEnemy(GameObject enemy)
    {
        onFieldEnemys.Add(enemy);
        levelFlowController?.RegisterSpawnedEnemy(enemy);
    }

    void Update()
    {
        onFieldEnemys.RemoveAll(item => item == null);
        if(isEndGenerate && onFieldEnemys.Count == 0)
        {
            isEnd = true;
        }
    }

    //==================== Wave 1 ====================
    IEnumerator Wave1()
    {
        yield return StartCoroutine(GenerateNormal1());
        isEndGenerate=true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal1()
    {
        yield return new WaitForSeconds(2);
        for(int i=0 ; i<20 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[0], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(0.5f * monsterScript.maxHP);
            yield return new WaitForSeconds(2);
        }
    }

    //==================== Wave 2 ====================
    IEnumerator Wave2()
    {
        yield return StartCoroutine(GenerateNormal2());
        isEndGenerate=true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal2()
    {
        yield return new WaitForSeconds(2);
        for(int i=0 ; i<30 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[0], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(0.5f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;

            yield return new WaitForSeconds(0.5f);
        }
    }

    //==================== Wave 3 ====================
    IEnumerator Wave3()
    {
        Coroutine task1 = StartCoroutine(GenerateNormal3());
        Coroutine task2 = StartCoroutine(GenerateSpeed3());
        yield return task1;
        yield return task2;
        isEndGenerate=true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal3()
    {
        yield return new WaitForSeconds(2);
        for(int i=0 ; i<10 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[0], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(1.25f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;

            yield return new WaitForSeconds(3);
        }
    }

    IEnumerator GenerateSpeed3()
    {
        yield return new WaitForSeconds(4);
        for(int i=0 ; i<10 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[2], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(1.25f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;

            yield return new WaitForSeconds(3);
        }
    }

    //==================== Wave 4 ====================
    IEnumerator Wave4()
    {
        yield return GenerateStrong4();
        isEndGenerate=true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateStrong4()
    {
        yield return new WaitForSeconds(2);
        for(int i=0 ; i<20 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[4], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(1 * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;

            yield return new WaitForSeconds(1);
        }
    }

    //==================== Wave 5 ====================
    IEnumerator Wave5()
    {
        Coroutine task1 = StartCoroutine(GenerateNormal5());
        Coroutine task2 = StartCoroutine(GenerateStrong5());
        yield return task1;
        yield return task2;
        isEndGenerate=true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal5()
    {
        yield return new WaitForSeconds(2);
        for(int i=0 ; i<10 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[0], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(1.5f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;

            yield return new WaitForSeconds(3);
        }
    }

    IEnumerator GenerateStrong5()
    {
        yield return new WaitForSeconds(3);
        for(int i=0 ; i<10 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[4], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(1.5f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;

            yield return new WaitForSeconds(3);
        }
    }

    //==================== Wave 6 ====================
    IEnumerator Wave6()
    {
        Coroutine task1 = StartCoroutine(GenerateSpeed6());
        Coroutine task2 = StartCoroutine(GenerateStrong6());
        yield return task1;
        yield return task2;
        isEndGenerate=true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateSpeed6()
    {
        yield return new WaitForSeconds(4);
        for(int i=0 ; i<10 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[2], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(1.5f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;

            yield return new WaitForSeconds(3);
        }
    }

    IEnumerator GenerateStrong6()
    {
        yield return new WaitForSeconds(2);
        for(int i=0 ; i<10 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[4], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(1.5f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;

            yield return new WaitForSeconds(3);
        }
    }

    //==================== Wave 7 ====================
    IEnumerator Wave7()
    {
        Coroutine task1 = StartCoroutine(GenerateNormal7());
        Coroutine task2 = StartCoroutine(GenerateSpeed7());
        Coroutine task3 = StartCoroutine(GenerateStrong7());
        yield return task1;
        yield return task2;
        yield return task3;
        isEndGenerate=true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal7()
    {
        yield return new WaitForSeconds(2);
        for(int i=0 ; i<15 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[0], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(2 * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;

            yield return new WaitForSeconds(4);
        }
    }

    IEnumerator GenerateSpeed7()
    {
        yield return new WaitForSeconds(10);
        for(int i=0 ; i<20 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[2], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(2 * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;

            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator GenerateStrong7()
    {
        yield return new WaitForSeconds(4);
        for(int i=0 ; i<20 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[4], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(2 * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;

            yield return new WaitForSeconds(3);
        }
    }

    //===== Wave 8 =====
    IEnumerator Wave8()
    {
        Coroutine task1 = StartCoroutine(GenerateNormal8());
        Coroutine task2 = StartCoroutine(GenerateSpeed8());
        Coroutine task3 = StartCoroutine(GenerateStrong8());
        Coroutine task4 = StartCoroutine(GenerateStun8());
        yield return task1;
        yield return task2;
        yield return task3;
        yield return task4;
        isEndGenerate=true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal8()
    {
        yield return new WaitForSeconds(2);
        Debug.Log("Start Genearte Normal");
        for(int i=0 ; i<20 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[0], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(2 * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;

            yield return new WaitForSeconds(3);
        }
    }

    IEnumerator GenerateSpeed8()
    {
        yield return new WaitForSeconds(20);
        Debug.Log("Start Genearte Speed");
        for(int i=0 ; i<10 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[2], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(2 * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;

            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator GenerateStrong8()
    {
        yield return new WaitForSeconds(4);
        Debug.Log("Start Genearte Strong");
        for(int i=0 ; i<10 ; i++)
        {
            GameObject newMonster = Instantiate(monsters[4], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(2 * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;

            yield return new WaitForSeconds(3);
        }
    }

    IEnumerator GenerateStun8()
    {
        yield return new WaitForSeconds(5);

        GameObject newMonster = Instantiate(monsters[6], this.transform.position, Quaternion.identity);
        TrackSpawnedEnemy(newMonster);
        EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

        monsterScript.maxHP = Mathf.RoundToInt(1 * monsterScript.maxHP);
        monsterScript.HP = monsterScript.maxHP;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waves3A : MonoBehaviour
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
        case 9:
            StartCoroutine(Wave9());
            break;
        case 10:
            StartCoroutine(Wave10());
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
            GetComponent<Waves>().isEnd = true;
        }
    }

    //==================== Wave 1 ====================
    IEnumerator Wave1()
    {
        yield return StartCoroutine(GenerateNormal1());
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal1()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 20; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[0+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(1f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(1);
        }
    }

    //==================== Wave 2 ====================
    IEnumerator Wave2()
    {
        yield return StartCoroutine(GenerateSpeed3());
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateSpeed3()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 40; i++)
        {
            GameObject newMonster = Instantiate(monsters[2], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(1f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(0.5f);
        }
    }

    //==================== Wave 3 ====================
    IEnumerator Wave3()
    {
        Coroutine task1 = StartCoroutine(GenerateNormal5());
        Coroutine task2 = StartCoroutine(GenerateSpeed5());
        Coroutine task3 = StartCoroutine(GenerateStrong5());
        Coroutine task4 = StartCoroutine(GenerateFreeze5());
        yield return task1;
        yield return task2;
        yield return task3;
        yield return task4;
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal5()
    {
        yield return new WaitForSeconds(10);
        for (int i = 0; i < 20; i++)
        {
            GameObject newMonster = Instantiate(monsters[0], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(2f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator GenerateSpeed5()
    {
        yield return new WaitForSeconds(20);
        for (int i = 0; i < 20; i++)
        {
            GameObject newMonster = Instantiate(monsters[2], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(2f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator GenerateStrong5()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 10; i++)
        {
            GameObject newMonster = Instantiate(monsters[4], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(2f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(3);
        }
    }

    IEnumerator GenerateFreeze5()
    {
        yield return new WaitForSeconds(9);
        GameObject newMonster = Instantiate(monsters[8], this.transform.position, Quaternion.identity);
        TrackSpawnedEnemy(newMonster);
        EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

        monsterScript.maxHP = Mathf.RoundToInt(1f * monsterScript.maxHP);
        monsterScript.HP = monsterScript.maxHP;
    }

    //==================== Wave 4 ====================
    IEnumerator Wave4()
    {
        Coroutine task1 = StartCoroutine(GenerateNormal7());
        Coroutine task2 = StartCoroutine(GenerateSpeed7());
        yield return task1;
        yield return task2;
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal7()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 20; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[0+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(3.5f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator GenerateSpeed7()
    {
        yield return new WaitForSeconds(5);
        for (int i = 0; i < 20; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[2+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(4.5f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    //==================== Wave 5 ====================
    IEnumerator Wave5()
    {
        Coroutine task1 = StartCoroutine(GenerateSpeed9());
        Coroutine task2 = StartCoroutine(GenerateStrong9());
        yield return task1;
        yield return task2;
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateSpeed9()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 10; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[2+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(7f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerator GenerateStrong9()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 15; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[4+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(7f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    //==================== Wave 6 ====================
    IEnumerator Wave6()
    {
        yield return StartCoroutine(GenerateNormal11());
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal11()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 50; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[0+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(3.5f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(0.5f);
        }
    }

    //==================== Wave 7 ====================
    IEnumerator Wave7()
    {
        Coroutine task1 = StartCoroutine(GenerateSpeed13());
        Coroutine task2 = StartCoroutine(GenerateStun13());
        Coroutine task3 = StartCoroutine(GenerateNormal13());
        yield return task1;
        yield return task2;
        yield return task3;
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal13()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 10; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[0+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(9f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator GenerateSpeed13()
    {
        yield return new WaitForSeconds(15);
        for (int i = 0; i < 50; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[2+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(7f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator GenerateStun13()
    {
        yield return new WaitForSeconds(3);
        for (int i = 0; i < 2; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[6+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(5f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(25);
        }
    }

    //==================== Wave 8 ====================
    IEnumerator Wave8()
    {
        yield return StartCoroutine(GenerateSpeed15());
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateSpeed15()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 80; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[2+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(5f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(0.5f);
        }
    }

    //==================== Wave 9 ====================
    IEnumerator Wave9()
    {
        Coroutine task1 = StartCoroutine(GenerateNormal17());
        Coroutine task2 = StartCoroutine(GenerateGenerate17());
        Coroutine task3 = StartCoroutine(GenerateStun17());
        yield return task1;
        yield return task2;
        yield return task3;
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal17()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 40; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[0+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(10f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator GenerateGenerate17()
    {
        yield return new WaitForSeconds(20);
        GameObject newMonster = Instantiate(monsters[11], this.transform.position, Quaternion.identity);
        TrackSpawnedEnemy(newMonster);
        EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

        monsterScript.maxHP = Mathf.RoundToInt(5f * monsterScript.maxHP);
        monsterScript.HP = monsterScript.maxHP;
        yield return null;
    }

    IEnumerator GenerateStun17()
    {
        yield return new WaitForSeconds(3);
        GameObject newMonster = Instantiate(monsters[6], this.transform.position, Quaternion.identity);
        TrackSpawnedEnemy(newMonster);
        EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

        monsterScript.maxHP = Mathf.RoundToInt(7f * monsterScript.maxHP);
        monsterScript.HP = monsterScript.maxHP;
        yield return null;
    }

    //==================== Wave 10 ====================
    IEnumerator Wave10()
    {
        Coroutine task1 = StartCoroutine(GenerateStrong19());
        Coroutine task2 = StartCoroutine(GenerateFreeze19());
        Coroutine task3 = StartCoroutine(GenerateStun19());
        yield return task1;
        yield return task2;
        yield return task3;
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateStrong19()
    {
        yield return new WaitForSeconds(1);
        for (int i = 0; i < 40; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[4+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(4f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator GenerateFreeze19()
    {
        yield return new WaitForSeconds(20);
        int result = (Random.value < 0.5f) ? 0 : 1;
        GameObject newMonster = Instantiate(monsters[8+result], this.transform.position, Quaternion.identity);
        TrackSpawnedEnemy(newMonster);
        EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

        monsterScript.maxHP = Mathf.RoundToInt(3f * monsterScript.maxHP);
        monsterScript.HP = monsterScript.maxHP;
        yield return null;
    }

    IEnumerator GenerateStun19()
    {
        yield return new WaitForSeconds(20);
        int result = (Random.value < 0.5f) ? 0 : 1;
        GameObject newMonster = Instantiate(monsters[6+result], this.transform.position, Quaternion.identity);
        TrackSpawnedEnemy(newMonster);
        EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

        monsterScript.maxHP = Mathf.RoundToInt(3f * monsterScript.maxHP);
        monsterScript.HP = monsterScript.maxHP;
        yield return null;
    }
}
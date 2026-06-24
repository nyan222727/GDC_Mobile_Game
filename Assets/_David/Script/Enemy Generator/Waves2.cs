using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waves2 : MonoBehaviour
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
        case 11:
            StartCoroutine(Wave11());
            break;
        case 12:
            StartCoroutine(Wave12());
            break;
        case 13:
            StartCoroutine(Wave13());
            break;
        case 14:
            StartCoroutine(Wave14());
            break;
        case 15:
            StartCoroutine(Wave15());
            break;
        case 16:
            StartCoroutine(Wave16());
            break;
        case 17:
            StartCoroutine(Wave17());
            break;
        case 18:
            StartCoroutine(Wave18());
            break;
        case 19:
            StartCoroutine(Wave19());
            break;
        case 20:
            StartCoroutine(Wave20());
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

    // 請記得在 Start() 的 switch 語句中補上 case 9 到 20 的 StartCoroutine 呼叫

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
        Coroutine task1 = StartCoroutine(GenerateSpeed2());
        Coroutine task2 = StartCoroutine(GenerateStrong2());
        yield return task1;
        yield return task2;
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateSpeed2()
    {
        yield return new WaitForSeconds(5);
        for (int i = 0; i < 20; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[2+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(1f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator GenerateStrong2()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 20; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[4+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(1f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    //==================== Wave 3 ====================
    IEnumerator Wave3()
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

    //==================== Wave 4 ====================
    IEnumerator Wave4()
    {
        Coroutine task1 = StartCoroutine(GenerateNormal4());
        Coroutine task2 = StartCoroutine(GenerateSpeed4());
        Coroutine task3 = StartCoroutine(GenerateStrong4());
        yield return task1;
        yield return task2;
        yield return task3;
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal4()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 30; i++)
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

    IEnumerator GenerateSpeed4()
    {
        yield return new WaitForSeconds(4);
        for (int i = 0; i < 20; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[2+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(1f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(1.5f);
        }
    }

    IEnumerator GenerateStrong4()
    {
        yield return new WaitForSeconds(3);
        for (int i = 0; i < 10; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[4+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(1f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    //==================== Wave 5 ====================
    IEnumerator Wave5()
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

    //==================== Wave 6 ====================
    IEnumerator Wave6()
    {
        Coroutine task1 = StartCoroutine(GenerateNormal6());
        Coroutine task2 = StartCoroutine(GenerateStrong6());
        yield return task1;
        yield return task2;
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal6()
    {
        yield return new WaitForSeconds(7);
        for (int i = 0; i < 20; i++)
        {
            GameObject newMonster = Instantiate(monsters[0], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(3f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator GenerateStrong6()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 20; i++)
        {
            GameObject newMonster = Instantiate(monsters[4], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(3f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    //==================== Wave 7 ====================
    IEnumerator Wave7()
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

    //==================== Wave 8 ====================
    IEnumerator Wave8()
    {
        yield return StartCoroutine(GenerateStrong8());
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateStrong8()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 10; i++)
        {
            GameObject newMonster = Instantiate(monsters[5], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(10f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(1.5f);
        }
    }

    //==================== Wave 9 ====================
    IEnumerator Wave9()
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

    //==================== Wave 10 ====================
    IEnumerator Wave10()
    {
        Coroutine task1 = StartCoroutine(GenerateNormal10());
        Coroutine task2 = StartCoroutine(GenerateSpeed10());
        Coroutine task3 = StartCoroutine(GenerateGenerate10());
        yield return task1;
        yield return task2;
        yield return task3;
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal10()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 20; i++)
        {
            GameObject newMonster = Instantiate(monsters[1], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(3f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator GenerateSpeed10()
    {
        yield return new WaitForSeconds(3);
        for (int i = 0; i < 20; i++)
        {
            GameObject newMonster = Instantiate(monsters[3], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(3f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator GenerateGenerate10()
    {
        yield return new WaitForSeconds(5);
        GameObject newMonster = Instantiate(monsters[11], this.transform.position, Quaternion.identity);
        TrackSpawnedEnemy(newMonster);
        EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

        monsterScript.maxHP = Mathf.RoundToInt(2.5f * monsterScript.maxHP);
        monsterScript.HP = monsterScript.maxHP;
        monsterScript.hpRatio = 2.5f;
    }

    //==================== Wave 11 ====================
    IEnumerator Wave11()
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

    //==================== Wave 12 ====================
    IEnumerator Wave12()
    {
        Coroutine task1 = StartCoroutine(GenerateSpeed12());
        Coroutine task2 = StartCoroutine(GenerateStrong12());
        yield return task1;
        yield return task2;
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateSpeed12()
    {
        yield return new WaitForSeconds(7);
        for (int i = 0; i < 15; i++)
        {
            GameObject newMonster = Instantiate(monsters[3], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(8f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator GenerateStrong12()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 10; i++)
        {
            GameObject newMonster = Instantiate(monsters[4], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(8f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(4);
        }
    }

    //==================== Wave 13 ====================
    IEnumerator Wave13()
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

    //==================== Wave 14 ====================
    IEnumerator Wave14()
    {
        yield return StartCoroutine(GenerateStrong14());
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateStrong14()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 10; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[4+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(14f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(4);
        }
    }

    //==================== Wave 15 ====================
    IEnumerator Wave15()
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

    //==================== Wave 16 ====================
    IEnumerator Wave16()
    {
        Coroutine task1 = StartCoroutine(GenerateNormal16());
        Coroutine task2 = StartCoroutine(GenerateStrong16());
        yield return task1;
        yield return task2;
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal16()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 15; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[0+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(15f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(3);
        }
    }

    IEnumerator GenerateStrong16()
    {
        yield return new WaitForSeconds(3);
        for (int i = 0; i < 15; i++)
        {
            GameObject newMonster = Instantiate(monsters[5], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(15f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(3);
        }
    }

    //==================== Wave 17 ====================
    IEnumerator Wave17()
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
        monsterScript.hpRatio = 5f;
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

    //==================== Wave 18 ====================
    IEnumerator Wave18()
    {
        Coroutine task1 = StartCoroutine(GenerateNormal18());
        Coroutine task2 = StartCoroutine(GenerateSpeed18());
        Coroutine task3 = StartCoroutine(GenerateStrong18());
        yield return task1;
        yield return task2;
        yield return task3;
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal18()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 80; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[0+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(10f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator GenerateSpeed18()
    {
        yield return new WaitForSeconds(3);
        for (int i = 0; i < 80; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[2+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(6f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator GenerateStrong18()
    {
        yield return new WaitForSeconds(5);
        for (int i = 0; i < 40; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[4+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(8f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(1);
        }
    }

    //==================== Wave 19 ====================
    IEnumerator Wave19()
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

    //==================== Wave 20 ====================
    IEnumerator Wave20()
    {
        Coroutine task1 = StartCoroutine(GenerateNormal20());
        Coroutine task2 = StartCoroutine(GenerateSpeed20());
        Coroutine task3 = StartCoroutine(GenerateStrong20());
        Coroutine task4 = StartCoroutine(GenerateFreeze20());
        Coroutine task5 = StartCoroutine(GenerateStun20());
        Coroutine task6 = StartCoroutine(GenerateGenerate20());
        yield return task1;
        yield return task2;
        yield return task3;
        yield return task4;
        yield return task5;
        yield return task6;
        isEndGenerate = true;
        Debug.Log("end Generate");
    }

    IEnumerator GenerateNormal20()
    {
        yield return new WaitForSeconds(7);
        for (int i = 0; i < 40; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[0+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(6f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator GenerateSpeed20()
    {
        yield return new WaitForSeconds(12);
        for (int i = 0; i < 40; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[2+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(6f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator GenerateStrong20()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < 20; i++)
        {
            int result = (Random.value < 0.5f) ? 0 : 1;
            GameObject newMonster = Instantiate(monsters[4+result], this.transform.position, Quaternion.identity);
            TrackSpawnedEnemy(newMonster);
            EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

            monsterScript.maxHP = Mathf.RoundToInt(6f * monsterScript.maxHP);
            monsterScript.HP = monsterScript.maxHP;
            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator GenerateFreeze20()
    {
        yield return new WaitForSeconds(7);
        int result = (Random.value < 0.5f) ? 0 : 1;
        GameObject newMonster = Instantiate(monsters[8+result], this.transform.position, Quaternion.identity);
        TrackSpawnedEnemy(newMonster);
        EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

        monsterScript.maxHP = Mathf.RoundToInt(3f * monsterScript.maxHP);
        monsterScript.HP = monsterScript.maxHP;
        yield return null;
    }

    IEnumerator GenerateStun20()
    {
        yield return new WaitForSeconds(5);
        int result = (Random.value < 0.5f) ? 0 : 1;
        GameObject newMonster = Instantiate(monsters[6+result], this.transform.position, Quaternion.identity);
        TrackSpawnedEnemy(newMonster);
        EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

        monsterScript.maxHP = Mathf.RoundToInt(3f * monsterScript.maxHP);
        monsterScript.HP = monsterScript.maxHP;
        yield return null;
    }

    IEnumerator GenerateGenerate20()
    {
        yield return new WaitForSeconds(10);
        int result = (Random.value < 0.5f) ? 0 : 1;
        GameObject newMonster = Instantiate(monsters[10+result], this.transform.position, Quaternion.identity);
        TrackSpawnedEnemy(newMonster);
        EnemyController monsterScript = newMonster.GetComponent<EnemyController>();

        monsterScript.maxHP = Mathf.RoundToInt(3f * monsterScript.maxHP);
        monsterScript.HP = monsterScript.maxHP;
        monsterScript.hpRatio = 3f;
        yield return null;
    }
}

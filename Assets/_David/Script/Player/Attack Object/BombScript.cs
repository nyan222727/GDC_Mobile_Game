using System.Collections;
using UnityEngine;

public class BombScript : MonoBehaviour
{
    public float forwardSpeed = 5f;   // 水平移動速度
    public float waveHeight = 1f;     // 震盪的高度（振幅）
    public float inAirTime = 1f;

    private float timer = 0f;
    private Transform target;
    public int damage = 0;
    public float damageRange = 2;
    private float initHeight;

    public void Seek(Transform _target)
    {
        target = _target;
        forwardSpeed = Vector3.Distance(transform.position, target.position) / inAirTime;
        initHeight = transform.position.y;

        // 子彈一生成，就啟動協程 (Coroutine)
        // 在 0.1 秒後觸發扣血
        StartCoroutine(DamageTimer());
    }

    void Update()
    {
        timer += Time.deltaTime;



        // 簡單的移動邏輯
        if (timer < inAirTime)
        {
            float currentHeight = initHeight + Mathf.Sin(timer * Mathf.PI / inAirTime) * waveHeight;
            transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z) + transform.forward * forwardSpeed * Time.deltaTime;
        }
    }

    IEnumerator DamageTimer()
    {
        yield return new WaitForSeconds(inAirTime);
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, damageRange, LayerMask.GetMask("Enemy"));

        foreach (Collider enemy in enemiesInRange)
        {
            // 尋找怪物身上的腳本（假設腳本名稱叫 EnemyHealth）
            // 請將 EnemyHealth 替換成你實際寫 LoseHP 的那個腳本名稱
            var enemyScript = enemy.GetComponent<EnemyController>();

            if (enemyScript != null)
            {
                enemyScript.LoseHP(damage);
                // Debug.Log("已爆炸，怪物扣血！");
            }
        }

        yield return new WaitForSeconds(0.5f);
        // 傷害造成後銷毀子彈，或是你希望等子彈碰到怪物再銷毀
        Destroy(gameObject);
    }

    // 開發時顯示範圍
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRange);
    }
}

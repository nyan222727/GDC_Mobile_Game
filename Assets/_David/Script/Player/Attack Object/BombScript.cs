using System.Collections;
using UnityEngine;

public class BombScript : MonoBehaviour
{
    public Element element = Element.None;
    private float sameElementDamage=0.8f;
    public float forwardSpeed = 5f;   // 水平移動速度
    public float waveHeight = 1f;     // 震盪的高度（振幅）
    public float inAirTime = 1f;

    private float timer = 0f;
    private Transform target;
    public int damage = 0;
    public float damageRange = 1;
    private float initHeight;

    public GameObject bombRender;
    public GameObject explosionEffect;


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
                if(enemyScript.element == this.element)
                {
                    enemyScript.LoseHP(Mathf.RoundToInt(damage*sameElementDamage));
                }
                else
                {
                    enemyScript.LoseHP(damage);
                }
                // Debug.Log("已爆炸，怪物扣血！");
            }
        }
        
        if (bombRender != null)
        {
            bombRender.SetActive(false); // 啟動子物件，特效會自動 Play On Awake
        }
        if (explosionEffect != null)
        {
            explosionEffect.transform.localScale = new Vector3(damageRange,damageRange,damageRange);
            explosionEffect.SetActive(true); // 啟動子物件，特效會自動 Play On Awake
        }

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    // 開發時顯示範圍
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRange);
    }
}
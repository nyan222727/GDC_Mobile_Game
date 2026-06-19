using System.Collections;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public Element element = Element.None;
    private float sameElementDamage=0.8f;
    private float distance;
    private Transform target;
    public float speed = 2f;
    public int damage = 0;

    public void Seek(Transform _target) {
        target = _target;
        distance = Vector3.Distance( transform.position, target.position);
        
        // 子彈一生成，就啟動協程 (Coroutine)
        // 在 0.1 秒後觸發扣血
        StartCoroutine(DamageTimer());
    }

    void Update() {
        if (target == null) {
            Destroy(gameObject);
            return;
        }

        // 簡單的移動邏輯
        transform.position = Vector3.MoveTowards(
            transform.position, 
            target.position, 
            speed * Time.deltaTime
        );
    }

    IEnumerator DamageTimer() {
        // 等待 0.1 秒
        yield return new WaitForSeconds(distance / speed);

        if (target != null) {
            // 尋找怪物身上的腳本（假設腳本名稱叫 EnemyHealth）
            // 請將 EnemyHealth 替換成你實際寫 LoseHP 的那個腳本名稱
            var enemyScript = target.GetComponent<EnemyController>(); 
            
            if (enemyScript != null) {
                if(enemyScript.element == this.element)
                {
                    enemyScript.LoseHP(Mathf.RoundToInt(damage*sameElementDamage));
                }
                else
                {
                    enemyScript.LoseHP(damage);
                }
                // Debug.Log("已過 n 秒，怪物扣血！");
            }
        }

        // 傷害造成後銷毀子彈，或是你希望等子彈碰到怪物再銷毀
        Destroy(gameObject); 
    }
}

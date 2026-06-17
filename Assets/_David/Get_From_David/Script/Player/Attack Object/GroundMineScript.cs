using UnityEngine;

public class GroundMineScript : MonoBehaviour
{

    public int damage = 0;
    public float damageRange = 0;

    private void OnTriggerEnter(Collider other) 
    {
        // 1. 抓取對方的 Layer 數字，並轉換為字串名稱
        string layerName = LayerMask.LayerToName(other.gameObject.layer);

        // 2. 直接用字串進行比對
        if (layerName == "Enemy")
        {
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
            Destroy(gameObject);
        }
    }
}

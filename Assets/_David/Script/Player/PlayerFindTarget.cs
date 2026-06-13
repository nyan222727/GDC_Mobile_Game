using UnityEngine;

public class PlayerFindTarget : MonoBehaviour
{
    public PlayerBuffManager buffScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buffScript = this.gameObject.GetComponent<PlayerBuffManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Transform FindTarget(float range, int filter = 0) {
        if(buffScript.stunTimer>0)return null;
        Transform target;          // 當前鎖定的目標

        // 取得範圍內所有怪物的碰撞體
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("Enemy"));
        GameObject targetEnemy = null;

        switch(filter)
        {
        case 0:
            float shortestDistance = Mathf.Infinity;
            foreach (Collider enemy in enemiesInRange) {
                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
                if (distanceToEnemy < shortestDistance) {
                    shortestDistance = distanceToEnemy;
                    targetEnemy = enemy.gameObject;
                }
            }
            break;
        case 1:
            float highestHP = 0f;
            foreach (Collider enemy in enemiesInRange) {
                EnemyController enemyScript = enemy.gameObject.GetComponent<EnemyController>();
                if (enemyScript.HP > highestHP) {
                    highestHP = enemyScript.HP;
                    targetEnemy = enemy.gameObject;
                }
            }
            break;
        }
        

        if (targetEnemy != null) {
            target = targetEnemy.transform;
        } else {
            target = null;
        }

        return target;
    }
}

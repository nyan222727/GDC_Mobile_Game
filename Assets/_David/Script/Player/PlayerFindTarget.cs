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

    public Transform FindTarget(float range) {
        if(buffScript.stunTimer>0)return null;
        Transform target;          // 當前鎖定的目標

        // 取得範圍內所有怪物的碰撞體
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("Enemy"));
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (Collider enemy in enemiesInRange) {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance) {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy.gameObject;
            }
        }

        if (nearestEnemy != null) {
            target = nearestEnemy.transform;
        } else {
            target = null;
        }

        return target;
    }
}

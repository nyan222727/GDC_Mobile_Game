using UnityEngine;

public class ArcherAI : MonoBehaviour
{
    public float range = 5f;          // 攻擊範圍
    public Transform target;          // 當前鎖定的目標
    public float rawFireCD = 1f;
    private float fireCD = 100f;       // 每秒攻擊次數
    private float fireCDTimer = 0f;

    public GameObject bulletPrefab; // 拖入你的子彈 Prefab
    public Vector3 firePoint;     // 子彈發射的起始點

    void Start()
    {
        fireCD = rawFireCD;
    }

    void Update() {
        
        FindTarget(); // 尋找目標
        

        if (target)
        {
            // 鎖定邏輯：讓塔轉向目標
            Vector3 dir = target.position - transform.position;
            dir.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * 20f);

            // 攻擊計時
            if (fireCDTimer <= 0f) {
                Attack();
                fireCDTimer = fireCD;
            }
            fireCDTimer -= Time.deltaTime;  
        }
    }

    void FindTarget() {
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
    }

    void Attack() {
        Debug.Log("發射子彈！攻擊 " + target.name);
        firePoint = transform.position + 1f*transform.forward;
        // 1. 生成子彈
        GameObject bullet = Instantiate(bulletPrefab, firePoint, transform.rotation);
    
        // 2. 取得子彈腳本並初始化
        BulletScript bulletScript = bullet.GetComponent<BulletScript>();
        if (bulletScript != null) {
            // 將當前的目標傳給子彈
            bulletScript.Seek(target);
            bulletScript.damage = 2;
        }
        // 在這裡實例化 (Instantiate) 子彈，並給予目標資訊
    }

    // 在編輯器畫出範圍，方便除錯
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

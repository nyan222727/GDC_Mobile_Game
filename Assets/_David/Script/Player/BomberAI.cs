using UnityEngine;

public class BomberAI : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField]public float range = 3f;          // 攻擊範圍
    [SerializeField]public float defaultDamage = 10f;
    [SerializeField]public float elementBuffDamage = 1.2f;
    [SerializeField]private float damage; 
    [SerializeField]public float defaultFireCD = 1.5f;
    [SerializeField]public float elementBuffCD = 1f;
    [SerializeField]private float fireCD = 100f;       // 每秒攻擊次數
    [SerializeField]private float fireCDTimer = 0f;

    public Transform target;          // 當前鎖定的目標
    private Vector3 targetDir;

    [Header("Bomb")]
    [SerializeField]public GameObject bombPrefab; // 拖入你的子彈 Prefab
    [SerializeField]public Vector3 firePoint;     // 子彈發射的起始點
    
    [Header("Buff Manager")]
    [SerializeField]public PlayerFindTarget searchScript;
    [SerializeField]public PlayerBuffManager buffScript;

    void Start()
    {
        buffScript = this.gameObject.GetComponent<PlayerBuffManager>();
        searchScript = this.gameObject.GetComponent<PlayerFindTarget>();
        fireCD = defaultFireCD;
        damage = defaultDamage;
    }

    void Update() {
        target = searchScript.FindTarget(range); // 尋找目標

        if (fireCDTimer > 0)
        {
            fireCDTimer -= Time.deltaTime;
        }
        

        if (target)
        {
            // 鎖定邏輯：讓塔轉向目標
            targetDir = target.position - transform.position;
            targetDir.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * 20f);

            // 攻擊計時
            if (fireCDTimer <= 0f) {
                Attack();
                fireCDTimer = fireCD * buffScript.slowDownRatio;
            }
        }
    }



    void Attack() {

        if(buffScript.isSameElement)  // on buff element
        {
            damage = elementBuffDamage * defaultDamage;
            fireCD = elementBuffCD * defaultFireCD;
        }
        else  // reset buff
        {
            damage = defaultDamage;
            fireCD = defaultFireCD;
        }

        // Debug.Log("投擲炸彈！攻擊 " + target.name);
        firePoint = transform.position + transform.forward;
        // 1. 生成子彈
        Quaternion bulletRotation = Quaternion.LookRotation(targetDir);
        GameObject bomb = Instantiate(bombPrefab, firePoint, bulletRotation);
    
        // 2. 取得子彈腳本並初始化
        BombScript bulletScript = bomb.GetComponent<BombScript>();
        if (bulletScript != null) {
            // 將當前的目標傳給子彈
            bulletScript.Seek(target);
            bulletScript.damage = Mathf.RoundToInt(damage);
        }
        // 在這裡實例化 (Instantiate) 子彈，並給予目標資訊
    }

    // 在編輯器畫出範圍，方便除錯
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

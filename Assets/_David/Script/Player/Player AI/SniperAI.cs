using UnityEngine;

public class SniperAI : MonoBehaviour
{
    [Header("Range")]
    [SerializeField]public float defaultRange = 5f;
    [SerializeField]public float elementRange = 1;
    [SerializeField]public float activeRange = 1.5f;
    [SerializeField]private float range;

    [Header("Damage")]
    [SerializeField]public float defaultDamage = 20f;
    [SerializeField]public float elementBuffDamage = 1.2f;
    [SerializeField]public float activeBuffDamage = 1.5f;
    [SerializeField]private float damage; 
    
    [Header("Fire CD")]
    [SerializeField]public float defaultFireCD = 1f;
    [SerializeField]public float elementFireCD = 0.8f;
    [SerializeField]public float activeFireCD = 0.3f;
    [SerializeField]private float fireCD = 100f;       // 每秒攻擊次數
    [SerializeField]private float fireCDTimer = 0f;
    

    private Transform target;          // 當前鎖定的目標

    [Header("Bullet")]
    [SerializeField]public GameObject bulletPrefab; // 拖入你的子彈 Prefab
    [SerializeField]public Vector3 firePoint;     // 子彈發射的起始點

    [Header("Buff Manager")]
    [SerializeField]public PlayerFindTarget searchScript;
    [SerializeField]public PlayerBuffManager buffScript;

    [Header("Audio")]
    [SerializeField] private AudioClip attackSound; // 在 Inspector 把你的音效檔案（.mp3/.wav）拉進來
    
    private AudioSource audioSource;

    // 1. 先在類別（Class）的最上方宣告一個新的計時器變數（跟 fireCDTimer 放一起）
    private float searchTimer = 0f;
    private const float searchInterval = 0.1f; // 每 0.15 秒才允許搜尋一次目標（極度省效能）

    void Start()
    {
        // 取得身上的 AudioSource 組件
        audioSource = GetComponent<AudioSource>();

        buffScript = this.gameObject.GetComponent<PlayerBuffManager>();
        searchScript = this.gameObject.GetComponent<PlayerFindTarget>();
        fireCD = defaultFireCD;
        damage = defaultDamage;
    }

    void Update() {

        
        // 💡 只有時間到了，才去重新計算攻擊範圍與搜尋目標
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            searchTimer = searchInterval; // 重置搜尋計時器

            range = defaultRange;
            if(buffScript.isSameElement)
            {
                range *= elementRange;
            }
            if(buffScript.isActive)
            {
                range *= activeRange;
            }
            
            // 🔒 只有這時候才執行昂貴的物理與 GetComponent 搜尋！
            target = searchScript.FindTarget(range, 1); 
        }
        
        if(fireCDTimer > 0)
        {
            if(buffScript.stunTimer<=0)
            {
                fireCDTimer -= Time.deltaTime; 
            }
        }
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
                fireCDTimer = fireCD * buffScript.slowDownRatio;
                // Debug.Log("fireCD: "+fireCD);
                // Debug.Log("ratio: "+buffScript.slowDownRatio);
            } 
        }
    }


    void Attack() {
        // Debug.Log("發射子彈！攻擊 " + target.name);
        firePoint = transform.position + 1f*transform.forward;

        damage = defaultDamage;
        fireCD = defaultFireCD;
        if(buffScript.isSameElement)  // on buff element
        {
            damage *= elementBuffDamage;
            fireCD *= elementFireCD;
        }
        if(buffScript.isActive)
        {
            damage *= activeBuffDamage;
            fireCD *= activeFireCD;
        }

        // 1. 生成子彈
        GameObject bullet = Instantiate(bulletPrefab, firePoint, transform.rotation);
        // 2. 取得子彈腳本並初始化
        BulletScript bulletScript = bullet.GetComponent<BulletScript>();
        if (bulletScript != null) {
            // 將當前的目標傳給子彈
            bulletScript.Seek(target);
            bulletScript.damage = Mathf.RoundToInt(damage);
            bulletScript.element = this.buffScript.playerElement;
        }
        // 在這裡實例化 (Instantiate) 子彈，並給予目標資訊
        if (audioSource != null && attackSound != null)
        {
            // PlayOneShot 適合這種短促的特效音，後面的 1.0f 是音量大小（0.0 ~ 1.0）
            audioSource.PlayOneShot(attackSound, 1.0f); 
        }
    }

    // 在編輯器畫出範圍，方便除錯
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

using UnityEngine;

public class SkillStun : MonoBehaviour
{
    public float timePerTrigger = 2f;
    public float skillRange = 5f;
    public int restrict = 5;
    private EnemyController infoScript;

    [Header("Audio")]
    [SerializeField] private AudioClip attackSound; // 在 Inspector 把你的音效檔案（.mp3/.wav）拉進來
    
    private AudioSource audioSource;

    [Header("effect")]
    [SerializeField] private GameObject skillEffect;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 取得身上的 AudioSource 組件
        audioSource = GetComponent<AudioSource>();

        infoScript = this.GetComponent<EnemyController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(infoScript.hurtCount>=restrict)
        {
            // Debug.Log("freeze skill 檢測到已扣血！");
            StunSkill(timePerTrigger);
            CastSkill();
            infoScript.hurtCount-=restrict;
        }
    }

    private void StunSkill(float stunTime)
    {
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, skillRange, LayerMask.GetMask("Player"));
        foreach(Collider player in playersInRange)
        {
            PlayerBuffManager debuffScript = player.GetComponent<PlayerBuffManager>(); 

            if (debuffScript != null) {
                debuffScript.stunTimer += stunTime;
                // Debug.Log("觸發範圍緩速技能！");
            }
        }
    }

    void CastSkill()
    {
        if (audioSource != null && attackSound != null)
        {
            // PlayOneShot 適合這種短促的特效音，後面的 1.0f 是音量大小（0.0 ~ 1.0）
            audioSource.PlayOneShot(attackSound, 0.3f); 
        }
        if (skillEffect != null)
        {
            // 1. 如果特效本來就是開著的，先關掉它（重置記憶體）
            skillEffect.SetActive(false);
            
            // 2. 移動到玩家當前的位置
            skillEffect.transform.position = this.transform.position;
            
            // 3. 重新打開它！因為 Looping 是關閉的且 OnEnable 會觸發，
            // 它就會完美地「從頭開始」瞬間往外擴散一次！
            skillEffect.SetActive(true);
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, skillRange);
    }
}

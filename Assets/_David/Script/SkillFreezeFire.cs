using UnityEngine;

public class SkillFreezeFire : MonoBehaviour
{
    private int levelPerHit = 1;
    public float skillRange = 5f;
    private EnemyController infoScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        infoScript = this.GetComponent<EnemyController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(infoScript.hurtCount>0)
        {
            Debug.Log("已扣血觸發範圍緩速技能！");
            freezeSkill(levelPerHit);
            infoScript.hurtCount--;
        }
    }

    private void freezeSkill(int level)
    {
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, skillRange, LayerMask.GetMask("Player"));
        foreach(Collider player in playersInRange)
        {
            // 尋找怪物身上的腳本（假設腳本名稱叫 EnemyHealth）
            // 請將 EnemyHealth 替換成你實際寫 LoseHP 的那個腳本名稱
            PlayerBuffManager debuffScript = player.GetComponent<PlayerBuffManager>(); 
            
            if (debuffScript != null) {
                debuffScript.freezeDebuff += level;
                Debug.Log("已扣血觸發範圍緩速技能！");
            }
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, skillRange);
    }
}

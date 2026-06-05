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
            // Debug.Log("freeze skill 檢測到已扣血！");
            FreezeSkill(levelPerHit);
            infoScript.hurtCount--;
        }
    }

    private void FreezeSkill(int level)
    {
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, skillRange, LayerMask.GetMask("Player"));
        foreach(Collider player in playersInRange)
        {
            PlayerBuffManager debuffScript = player.GetComponent<PlayerBuffManager>(); 

            if (debuffScript != null) {
                debuffScript.freezeDebuff += level;
                // Debug.Log("觸發範圍緩速技能！");
            }
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, skillRange);
    }
}

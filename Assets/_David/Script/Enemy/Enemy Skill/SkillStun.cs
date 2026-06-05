using UnityEngine;

public class SkillStun : MonoBehaviour
{
    private float timePerTrigger = 2f;
    public float skillRange = 5f;
    public int restrict = 5;
    private EnemyController infoScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        infoScript = this.GetComponent<EnemyController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(infoScript.hurtCount>=restrict)
        {
            // Debug.Log("freeze skill 檢測到已扣血！");
            StunSkill(timePerTrigger);
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

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, skillRange);
    }
}

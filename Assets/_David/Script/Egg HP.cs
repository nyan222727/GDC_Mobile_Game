using UnityEngine;

public class EggHP : MonoBehaviour
{

    public int HP = 200;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void OnTriggerEnter(Collider other)
    {
        string layerName = LayerMask.LayerToName(other.gameObject.layer);

        // 2. 直接用字串進行比對
        if (layerName == "Enemy")
        {
            EnemyController enemyScript = other.GetComponent<EnemyController>();
            HP -= enemyScript.damage;
            Destroy(other.gameObject);
        }
    }

    void Update()
    {
        if(HP<0)
        {
            HP = 0;
        }
    }
}

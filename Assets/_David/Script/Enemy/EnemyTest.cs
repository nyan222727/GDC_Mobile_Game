using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    public float moveSpeed = 10;
    public int maxHP = 20;
    public int HP;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HP = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.A))
        {
            transform.position += new Vector3(-moveSpeed*Time.deltaTime,0,0);
        }
        if(Input.GetKey(KeyCode.D))
        {
            transform.position += new Vector3(moveSpeed*Time.deltaTime,0,0);
        }
        if(Input.GetKey(KeyCode.W))
        {
            transform.position += new Vector3(0,0,moveSpeed*Time.deltaTime);
        }
        if(Input.GetKey(KeyCode.S))
        {
            transform.position += new Vector3(0,0,-moveSpeed*Time.deltaTime);
        }

        if(HP <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void LoseHP(int damage)
    {
        HP -= damage;
    }


}

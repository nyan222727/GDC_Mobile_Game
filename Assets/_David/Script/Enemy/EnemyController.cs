using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Element element = Element.None;
    public int hurtCount = 0;
    public Vector3 targetPosition=new Vector3(0,1,0);
    public bool isMoving = true;
    public int damage;

    [Header("Speed")]
    [SerializeField]public float defaultSpeed = 1;
    [SerializeField]private float speed;

    [Header("HP")]
    [SerializeField]public float hpRatio = 1f;
    [SerializeField]public int maxHP = 20;
    [SerializeField]public int HP;
    
    [Header("Element Buff")]
    [SerializeField]private Element onTileElement = Element.None;
    [SerializeField]public float tileDamageRatio = 0.02f;
    [SerializeField]private int tileDamageBound = 40;
    [SerializeField]private float tileDamageTimer = 0;
    [SerializeField]public float tileBuffSpeedRatio = 1.2f;

    [Header("Active")]
    [SerializeField]public bool isActive = false;
    [SerializeField]private float activeTImer = 0;
    [SerializeField]public float activeTime = 5f;
    [SerializeField]private float activeDamageIgnore = 0;
    [SerializeField]public float activeDamageRatio = 0.1f;
    [SerializeField]private int activeDamageBound = 400;
    [SerializeField]public float activateSpeedRatio = 1.5f;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxHP = Mathf.RoundToInt(maxHP*hpRatio);
        if (DifficultyManager.IsEasyMode)
        {
            Debug.Log("生成easy monster");
            maxHP = Mathf.RoundToInt(maxHP * 0.5f); // 簡單模式下，怪物的血量增幅比例變為 0.5 倍
        }
        HP = maxHP;
        speed = defaultSpeed;
        // 初始對齊格子中心
        SnapToGrid();
        FindNextTarget();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if(activeDamageIgnore > 0)
        {
            activeDamageIgnore -= Time.deltaTime;
        }
        // active
        if(isActive)
        {
            if(activeTImer >= 0)
            {
                activeTImer -= Time.deltaTime;
            }
            else
            {
                isActive = false;
            }
        }
        

        // 對角下方塊無敵時間
        if(tileDamageTimer > 0)
        {
            tileDamageTimer -= Time.deltaTime;
        }

        // 死亡偵測
        if(HP <= 0)
        {
            Destroy(gameObject);
        }

        // 可行動偵測
        if (isMoving)
        {
            // 向目標點移動
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // 到達目標點後，找下一個目標
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                FindNextTarget();
            }    
        }

        // 確認腳下元素
        speed = defaultSpeed;
        DetactTileElement();
        if(onTileElement == this.element)
        {
            speed *= tileBuffSpeedRatio;
        }
        if(isActive)
        {
            speed *= activateSpeedRatio;
        }


        if(this.element != Element.None && onTileElement != Element.None && onTileElement != this.element)
        {
            if(tileDamageTimer<=0)
            {
               if(tileDamageRatio * maxHP >= tileDamageBound)
                {
                    HP -= tileDamageBound;
                }
                else
                {
                    HP -= Mathf.RoundToInt(tileDamageRatio * maxHP);
                } 
                tileDamageTimer = 1f;
            }
        }
    }
    void FindNextTarget()
    {
        // 透過 Raycast 往下偵測踩到的方塊
        RaycastHit hitPath;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hitPath, 2f, LayerMask.GetMask("Path")))
        {
            PathDetactor detactor = hitPath.collider.GetComponent<PathDetactor>();
            if (detactor != null && detactor.nextDirection != MoveDirection.End)
            {
                Vector3 dir = detactor.GetVectorFromDirection(detactor.nextDirection);
                targetPosition = transform.position + dir; // 目標是下一格的中心
                isMoving = true;
            }
        }

        
    }

    void DetactTileElement()
    {
        RaycastHit hitTile;
        if (Physics.Raycast(transform.position, Vector3.down, out hitTile, 2f, LayerMask.GetMask("Tile")))
        {
            TileProperty detactor = hitTile.collider.GetComponent<TileProperty>();
            // Debug.Log("detact element:"+detactor.element);
            if (detactor != null)
            {
                onTileElement = detactor.element;
            }
        }
    }

    public void DamageByActive()
    {
        if(activeDamageIgnore > 0)
        {
            return;
        }

        if(activeDamageRatio * maxHP >= activeDamageBound)
        {
            HP -= activeDamageBound;
        }
        else
        {
            HP -= Mathf.RoundToInt(activeDamageRatio * maxHP);
        }
        activeDamageIgnore = 0.001f;
    }
    public void ResetActiveTimer()
    {
        activeTImer = activeTime;
        isActive = true;
    }

    void SnapToGrid()
    {
        transform.position = new Vector3(Mathf.Round(transform.position.x), transform.position.y, Mathf.Round(transform.position.z));
    }

    public void LoseHP(int damage)
    {
        HP -= damage;
        hurtCount ++;
    }
}

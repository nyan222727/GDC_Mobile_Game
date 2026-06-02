using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int hurtCount = 0;
    public float speed = 1;
    public Vector3 targetPosition=new Vector3(0,1,0);
    public bool isMoving = true;
    private bool isNewTile = true;
    public int maxHP = 20;
    public int HP;
    private Element onTileElement = Element.None;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HP = maxHP;
        // 初始對齊格子中心
        SnapToGrid();
        FindNextTarget();
        DetactTileElement();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
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
                DetactTileElement();
                isNewTile = true;
            }
            if (isNewTile && Vector3.Distance(transform.position, targetPosition) - 0.5f < 0)
            {
                DetactTileElement();
                isNewTile = false;
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
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hitTile, 2f, LayerMask.GetMask("Tile")))
        {
            TileProperty detactor = hitTile.collider.GetComponent<TileProperty>();
            if (detactor != null)
            {
                onTileElement = detactor.element;
            }
        }
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

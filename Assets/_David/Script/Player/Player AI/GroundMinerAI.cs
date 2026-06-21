using UnityEngine;

public class GroundMinerAI : MonoBehaviour
{
    [Header("Range")]
    [SerializeField]public float mineRange;

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

    [Header("Mine")]
    [SerializeField]public GameObject minePrefab; // 拖入你的子彈 Prefab
    [SerializeField]public GameObject flowManager;
    [SerializeField]private LevelFlowController flowScript;

    [Header("Buff Manager")]
    [SerializeField]public PlayerBuffManager buffScript;

    void Start()
    {
        flowManager = GameObject.FindGameObjectWithTag("GameController");
        flowScript = flowManager.GetComponent<LevelFlowController>();
        buffScript = this.gameObject.GetComponent<PlayerBuffManager>();
        fireCD = defaultFireCD;
        damage = defaultDamage;
    }

    void Update() {

        if(buffScript.isActive)
        {
            ActivePlace();
            buffScript.activeTimer = 0;
        }
        Debug.Log(flowScript.currentState);
        if(fireCDTimer > 0)
        {
            fireCDTimer -= Time.deltaTime;
        }
        else if(flowScript.currentState == LevelFlowController.LevelState.Combat)
        {
            Vector3 emptyPathPos = FindNeighborTile();
            if(emptyPathPos.y != -1)
            {
                PlaceMine(emptyPathPos);
                fireCDTimer = fireCD * buffScript.slowDownRatio;
            }
        }
    }

    void PlaceMine(Vector3 pos)
    {
        damage = defaultDamage;
        fireCD = defaultFireCD;
        if(buffScript.isSameElement)
        {
            damage *= elementBuffDamage;
            fireCD *= elementFireCD;
        }
        if(buffScript.isActive)
        {
            damage *= activeBuffDamage;
            fireCD *= activeFireCD;
        } 
        GameObject mine = Instantiate(minePrefab, pos, Quaternion.identity);
        GroundMineScript mineScript = mine.GetComponent<GroundMineScript>();
        mineScript.damage = Mathf.RoundToInt(damage);
        mineScript.element = this.buffScript.playerElement;
    }

    private Vector3 FindNeighborTile()
    {
        Vector3 pos= new Vector3(0,-1,0);

        for(int r = -1 ; r <= 1 ; r++)
        {
            Vector3 detactPos = new Vector3(this.transform.position.x, 1, this.transform.position.z + r);
            Collider[] tile = Physics.OverlapBox(detactPos - new Vector3(0,1,0), new Vector3(0.1f,0.1f,0.1f), Quaternion.identity, LayerMask.GetMask("Tile"));
            if(tile.Length != 0)
            {
                TileProperty tileScript = tile[0].gameObject.GetComponent<TileProperty>();
                if(tileScript.isPath)
                {
                    Collider[] placement = Physics.OverlapBox(detactPos, new Vector3(0.1f,0.1f,0.1f), Quaternion.identity, LayerMask.GetMask("Placement"));
                    if(placement.Length == 0)
                    {
                        pos = detactPos;
                    }
                }
            }
        }
        for(int c = -1 ; c <= 1 ; c++)
        {
            Vector3 detactPos = new Vector3(this.transform.position.x + c, 1, this.transform.position.z);
            Collider[] tile = Physics.OverlapBox(detactPos - new Vector3(0,1,0), new Vector3(0.1f,0.1f,0.1f), Quaternion.identity, LayerMask.GetMask("Tile"));
            if(tile.Length != 0)
            {
                TileProperty tileScript = tile[0].gameObject.GetComponent<TileProperty>();
                if(tileScript.isPath)
                {
                    Collider[] placement = Physics.OverlapBox(detactPos, new Vector3(0.1f,0.1f,0.1f), Quaternion.identity, LayerMask.GetMask("Placement"));
                    if(placement.Length == 0)
                    {
                        pos = detactPos;
                    }
                }
            }
        }

        return pos;
    }

    void ActivePlace()
    {
        for(int r = -1 ; r <= 1 ; r++)
        {
            for(int c = -1 ; c <= 1 ; c++)
            {
                Vector3 detactPos = new Vector3(this.transform.position.x + c, 1, this.transform.position.z + r);
                Collider[] tile = Physics.OverlapBox(detactPos - new Vector3(0,1,0), new Vector3(0.1f,0.1f,0.1f), Quaternion.identity, LayerMask.GetMask("Tile"));
                if(tile.Length != 0)
                {
                    TileProperty tileScript = tile[0].gameObject.GetComponent<TileProperty>();
                    if(tileScript.isPath)
                    {
                        Collider[] placement = Physics.OverlapBox(detactPos, new Vector3(0.1f,0.1f,0.1f), Quaternion.identity, LayerMask.GetMask("Placement"));
                        if(placement.Length == 0)
                        {
                            PlaceMine(detactPos);
                        }
                    }
                }
            }
            
        }
    }
}

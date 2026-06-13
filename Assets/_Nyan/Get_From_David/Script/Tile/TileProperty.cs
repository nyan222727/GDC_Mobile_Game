using System;
using UnityEngine;

public enum Element { Fire, Ice, None }

public enum Direction { None, Forward, Back, Left, Right}

public class TileProperty : MonoBehaviour
{
    public Element element;
    public int chainCount;
    private bool isChainProgress = false;
    public bool isPath = false;
    public bool isNearPath = false;
    public Material defaultMaterial;
    public Material fireMaterial;
    public Material iceMaterial; 
    private Renderer myRenderer;
    public float elementTime = 30f;
    private float elementTimer;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        chainCount = 0;
        element = Element.None;
        myRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(element != Element.None)
        {
            if(elementTimer > 0)
            {
                elementTimer -= Time.deltaTime;
            }
            else
            {
                element = Element.None;
            }
        }
        switch(element){
            case Element.Fire:
                if (fireMaterial != null)
                {
                    myRenderer.material = fireMaterial;
                }
                break;
            case Element.Ice:
                if (iceMaterial != null)
                {
                    myRenderer.material = iceMaterial;
                }
                break;
            case Element.None:
                if (defaultMaterial != null)
                {
                    myRenderer.material = defaultMaterial;
                }
                break; 
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            DetactActive();
        }
    }

    public void DetactActive()
    {
        if(this.element == Element.None)
        {
            return;
        }
        RaycastHit forwardTile;
        if (Physics.Raycast(transform.position, Vector3.forward, out forwardTile, 2f, LayerMask.GetMask("Tile")))
        {
            TileProperty forwardTileScript = forwardTile.collider.GetComponent<TileProperty>();
            if(forwardTileScript.element == this.element)
            {
                RaycastHit backTile;
                if (Physics.Raycast(transform.position, Vector3.back, out backTile, 2f, LayerMask.GetMask("Tile")))
                {
                    TileProperty backTileScript = backTile.collider.GetComponent<TileProperty>();
                    if(backTileScript.element == this.element)
                    {
                        ActiveChain(Direction.None);
                        chainCount += 1;
                    }
                }  
            }
        }
        RaycastHit rightTile;
        if (Physics.Raycast(transform.position, Vector3.right, out rightTile, 2f, LayerMask.GetMask("Tile")))
        {
            TileProperty rightTileScript = rightTile.collider.GetComponent<TileProperty>();
            if(rightTileScript.element == this.element)
            {
                RaycastHit leftTile;
                if (Physics.Raycast(transform.position, Vector3.left, out leftTile, 2f, LayerMask.GetMask("Tile")))
                {
                    TileProperty leftTileScript = leftTile.collider.GetComponent<TileProperty>();
                    if(leftTileScript.element == this.element)
                    {
                        ActiveChain(Direction.None);
                        chainCount += 1;
                    }
                }  
            }
        }
    
    }

    public void ActiveChain(Direction lastDir= Direction.None)
    {
        if(isChainProgress)
        {
            return;
        }

        isChainProgress = true;
        if(lastDir != Direction.Left) // 向右檢索
        {
            RaycastHit rightTile;
            if (Physics.Raycast(transform.position, Vector3.right, out rightTile, 2f, LayerMask.GetMask("Tile")))
            {
                TileProperty rightTileScript = rightTile.collider.GetComponent<TileProperty>();
                if(rightTileScript.element == this.element)
                {
                    rightTileScript.ActiveChain(Direction.Right);
                }
            }
        }

        if(lastDir != Direction.Right) // 向左檢索
        {
            RaycastHit leftTile;
            if (Physics.Raycast(transform.position, Vector3.left, out leftTile, 2f, LayerMask.GetMask("Tile")))
            {
                TileProperty leftTileScript = leftTile.collider.GetComponent<TileProperty>();
                if(leftTileScript.element == this.element)
                {
                    leftTileScript.ActiveChain(Direction.Left);
                }
            }
        }

        if(lastDir != Direction.Forward)
        {
            RaycastHit backTile;
            if (Physics.Raycast(transform.position, Vector3.back, out backTile, 2f, LayerMask.GetMask("Tile")))
            {
                TileProperty backTileScript = backTile.collider.GetComponent<TileProperty>();
                if(backTileScript.element == this.element)
                {
                    backTileScript.ActiveChain(Direction.Back);
                }
            }
        }

        if(lastDir != Direction.Back)
        {
            RaycastHit forwardTile;
            if (Physics.Raycast(transform.position, Vector3.forward, out forwardTile, 2f, LayerMask.GetMask("Tile")))
            {
                TileProperty forwardTileScript = forwardTile.collider.GetComponent<TileProperty>();
                if(forwardTileScript.element == this.element)
                {
                    forwardTileScript.ActiveChain(Direction.Forward);
                }
            }
        }

        // 對敵人觸發
        Collider[] enemiesOnActive = Physics.OverlapBox(transform.position + new Vector3(0,1,0), new Vector3(0.5f,0.5f,0.5f), Quaternion.identity, LayerMask.GetMask("Enemy"));
        if(enemiesOnActive.Length != 0)
        {
            foreach(Collider enemy in enemiesOnActive)
            {
                Debug.Log("find enemy");
                EnemyController enemyScript = enemy.gameObject.GetComponent<EnemyController>();
                if(enemyScript.element == this.element)
                {
                    enemyScript.ResetActiveTimer();
                }
                if(enemyScript.element != Element.None && enemyScript.element != this.element)
                {
                    enemyScript.DamageByActive();
                }
            }
        }

        // 對角色觸發
        Collider[] playersOnActive = Physics.OverlapBox(transform.position + new Vector3(0,1,0), new Vector3(0.5f,0.5f,0.5f), Quaternion.identity, LayerMask.GetMask("Player"));
        if(playersOnActive.Length != 0)
        {
            foreach(Collider player in playersOnActive)
            {
                // Debug.Log(player);
                PlayerBuffManager playerScript = player.gameObject.GetComponent<PlayerBuffManager>();
                if(playerScript.playerElement == this.element)
                {
                    playerScript.ResetActiveTimer();
                }
            }
        }
        Debug.Log("reset element");
        this.element = Element.None;
        isChainProgress = false;
    }

    public void ResetElementTimer()
    {
        elementTimer = elementTime;
    }

}

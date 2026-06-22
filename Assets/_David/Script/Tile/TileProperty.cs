using System;
using UnityEngine;

public enum Element { Fire, Ice, None }

public enum Direction { None, Forward, Back, Left, Right}

public class TileProperty : MonoBehaviour
{
    public Element element;
    public int chainCount;
    public bool isChainProgress = false;
    public bool isPath = false;
    public bool isNearPath = false;
    public Material defaultMaterial;
    public Material fireMaterial;
    public Material iceMaterial; 
    private Renderer myRenderer;
    public float elementTimer;

    public float RemainingElementTime => Mathf.Max(0f, elementTimer);

    [Header("Audio")]
    [SerializeField] private AudioClip attackSound; // 在 Inspector 把你的音效檔案（.mp3/.wav）拉進來
    
    private AudioSource audioSource;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 取得身上的 AudioSource 組件
        audioSource = GetComponent<AudioSource>();

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
        // if(Input.GetKeyDown(KeyCode.Space))
        // {
        //     DetactActive();
        // }
    }

    public void DetactActive()
    {
        // Debug.Log("detact chain");
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
                        if (audioSource != null && attackSound != null)
                        {
                            // PlayOneShot 適合這種短促的特效音，後面的 1.0f 是音量大小（0.0 ~ 1.0）
                            audioSource.PlayOneShot(attackSound, 0.3f); 
                        }
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
                        if (audioSource != null && attackSound != null)
                        {
                            // PlayOneShot 適合這種短促的特效音，後面的 1.0f 是音量大小（0.0 ~ 1.0）
                            audioSource.PlayOneShot(attackSound, 0.3f); 
                        }
                        ActiveChain(Direction.None);
                        chainCount += 1;
                    }
                }  
            }
        }
    
    }

    public void ActiveChain(Direction lastDir= Direction.None)
    {
        // Debug.Log("Find Chain");
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
                // Debug.Log("find enemy");
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
        // Debug.Log("reset element");
        this.element = Element.None;
        // isChainProgress = false;
    }

    public void SetElementState(Element newElement, float remainingTime)
    {
        element = newElement;
        elementTimer = newElement == Element.None ? 0f : Mathf.Max(0f, remainingTime);
    }

}

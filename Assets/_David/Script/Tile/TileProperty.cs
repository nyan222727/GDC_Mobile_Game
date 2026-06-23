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
    public float elementTime;

    public float RemainingElementTime => Mathf.Max(0f, elementTimer);
    [SerializeField]public GameObject flowManager;
    [SerializeField]private LevelFlowController flowScript;

    [Header("Audio")]
    [SerializeField] private AudioClip attackSound; // 在 Inspector 把你的音效檔案（.mp3/.wav）拉進來
    
    private AudioSource audioSource;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        flowManager = GameObject.FindGameObjectWithTag("GameController");
        flowScript = flowManager.GetComponent<LevelFlowController>();
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
                if(flowScript.currentState == LevelFlowController.LevelState.Combat)
                {
                    elementTimer -= Time.deltaTime;
                }
                if(flowScript.currentState == LevelFlowController.LevelState.Placement)
                {
                    elementTimer = elementTime;
                }
            }
            else
            {
                element = Element.None;
            }
        }

        // 🎨 處理材質球顏色切換
        Material targetMaterial = defaultMaterial;
        switch(element)
        {
            case Element.Fire: targetMaterial = fireMaterial; break;
            case Element.Ice:  targetMaterial = iceMaterial;  break;
            case Element.None: targetMaterial = defaultMaterial; break; 
        }

        if (myRenderer != null && targetMaterial != null)
        {
            // 💡 關鍵：先將材質球指定過去
            myRenderer.material = targetMaterial;

            // 🌗 核心暗化邏輯：如果剩下不到 10 秒，且目前有屬性加成
            if (element != Element.None && elementTimer <= 20f)
            {
                // 計算變暗的比例 (t 會從 1.0 漸變到 0.0)
                // 當 elementTimer = 10 時，t = 1.0 (原本亮度)
                // 當 elementTimer = 0  時，t = 0.0 (最暗)
                float t = Mathf.Clamp01(elementTimer / 20f);

                // 調整最低亮度（例如 0.2f），防止方塊全黑到看不見
                float brightness = Mathf.Lerp(0.2f, 1.0f, t);

                // 取得原本材質球的顏色，並乘以亮度係數
                Color originalColor = targetMaterial.color;
                Color darkerColor = new Color(
                    originalColor.r * brightness, 
                    originalColor.g * brightness, 
                    originalColor.b * brightness, 
                    originalColor.a
                );

                // 把變暗後的顏色指定給當前渲染器
                myRenderer.material.color = darkerColor;
            }
        }
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

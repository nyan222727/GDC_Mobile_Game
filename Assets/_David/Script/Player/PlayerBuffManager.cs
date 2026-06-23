using UnityEngine;

public class PlayerBuffManager : MonoBehaviour
{
    [Header("Element")]
    [SerializeField]public Element playerElement = Element.None;
    [SerializeField]public bool isSameElement = false;
    [SerializeField]private GetTileElement tileScript;

    [Header("Active")]
    [SerializeField]public bool isActive;
    [SerializeField]private float activeTime = 10;
    [SerializeField]public float activeTimer; // 測試用public，記得改回來
    [SerializeField]public GameObject activeEffect;


    [Header("Freeze")]
    [SerializeField]public int freezeDebuff = 0;
    [SerializeField]public float slowDownRatio=1f;
    private float freezeTimer = 0;
    private float freezeFadeTime = 5f;
    private float ratioPerFreeze = 0.02f;

    [Header("Stun")]
    [SerializeField]public float stunTimer = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tileScript = this.gameObject.GetComponent<GetTileElement>();
        freezeTimer = freezeFadeTime;
        isActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Element
        if(playerElement == tileScript.tileElement)
        {
            isSameElement = true;
        }
        else
        {
            isSameElement = false;
        }

        // Active
        if(isActive)
        {
            if(activeEffect != null)
            {
                activeEffect.SetActive(true);
            }
            if(activeTimer > 0)
            {
                activeTimer -= Time.deltaTime;
            }
            else
            {
                isActive = false;
                if(activeEffect != null)
                {
                    activeEffect.SetActive(false);
                }
            }
        }

        // Freeze
        if(freezeDebuff>0)
        {
            
            if(freezeTimer>0)
            {
                freezeTimer -= Time.deltaTime;
            }
            else
            {
                freezeTimer = freezeFadeTime;
                freezeDebuff --;
            }
            slowDownRatio = 1f + (ratioPerFreeze * freezeDebuff);
        }
        else
        {
            slowDownRatio = 1f;
        }

        // Stun
        if(stunTimer>0)
        {
            stunTimer -= Time.deltaTime;
        }
    }

    public void ResetActiveTimer()
    {
        activeTimer = activeTime;
        isActive = true;
    }
}

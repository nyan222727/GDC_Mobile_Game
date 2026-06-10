using UnityEngine;

public class PlayerBuffManager : MonoBehaviour
{
    [Header("Element")]
    [SerializeField]public Element playerElement = Element.None;
    [SerializeField]public bool isSameElement = false;
    [SerializeField]private GetTileElement tileScript;

    [Header("Freeze")]
    [SerializeField]public int freezeDebuff = 0;
    [SerializeField]public float slowDownRatio=1f;
    [SerializeField]private float freezeTimer = 0;
    [SerializeField]private float freezeFadeTime = 5f;
    [SerializeField]private float ratioPerFreeze = 0.2f;

    [Header("Stun")]
    [SerializeField]public float stunTimer = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tileScript = this.gameObject.GetComponent<GetTileElement>();
        freezeTimer = freezeFadeTime;
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
}

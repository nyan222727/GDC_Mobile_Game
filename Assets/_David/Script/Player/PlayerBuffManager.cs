using UnityEngine;

public class PlayerBuffManager : MonoBehaviour
{
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
        freezeTimer = freezeFadeTime;
    }

    // Update is called once per frame
    void Update()
    {
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

        if(stunTimer>0)
        {
            stunTimer -= Time.deltaTime;
        }
    }
}

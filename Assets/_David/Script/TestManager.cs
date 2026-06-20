using UnityEngine;

public class TestManager : MonoBehaviour
{
    void Update()
    {
        // 按下鍵盤的 T 鍵（Time），切換為 3 倍速
        if (Input.GetKey(KeyCode.T))
        {
            Time.timeScale = 3f; // 變成 3 倍速
            
            // 💡 調整 Time.fixedDeltaTime 是為了讓物理偵測（如你的 Raycast/Rigidbody）
            // 在加速時依然保持精準，不會因為太快而穿牆或漏偵測！
            Time.fixedDeltaTime = 0.02f * Time.timeScale; 
        }
        else
        {
            // 放開鍵盤，或是預設狀態下，回復 1 倍正常速度
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f; // 恢復預設物理步長
        }
    }
}

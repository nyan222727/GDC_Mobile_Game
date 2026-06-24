using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    // 靜態變數，方便其他場景的怪獸、防禦塔直接讀取難度
    public static bool IsEasyMode = false;

    // 💡 關鍵：這個方法必須是 public，且參數必須帶有一個 bool
    public void SetEasyMode(bool isStatusOn)
    {
        IsEasyMode = isStatusOn;

        if (IsEasyMode)
        {
            Debug.Log("🔌 已切換至：簡單模式 (怪獸血量將會減少)");
            // 在這裡可以順便調整全域參數，例如：
            // EnemyController.hpRatio = 0.5f; 
        }
        else
        {
            Debug.Log("⚔️ 已切換至：普通模式");
            // EnemyController.hpRatio = 1.0f;
        }
    }
}

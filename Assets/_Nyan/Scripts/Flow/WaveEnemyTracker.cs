using UnityEngine;

[DisallowMultipleComponent]
public sealed class WaveEnemyTracker : MonoBehaviour
{
    private LevelFlowController owner;
    private int waveIndex = -1;
    private bool isArmed;

    public void Arm(LevelFlowController levelFlowController, int ownerWaveIndex)
    {
        owner = levelFlowController;
        waveIndex = ownerWaveIndex;
        isArmed = true;
    }

    public void Disarm()
    {
        isArmed = false;
        owner = null;
        waveIndex = -1;
    }

    public void NotifyDefeated(bool grantsCoinReward = true)
    {
        if (!isArmed || owner == null)
        {
            return;
        }

        LevelFlowController targetOwner = owner;
        int targetWaveIndex = waveIndex;
        isArmed = false;
        owner = null;
        waveIndex = -1;

        targetOwner.NotifyEnemyDefeated(this, targetWaveIndex, grantsCoinReward);
    }

    private void OnDestroy()
    {
        NotifyDefeated(true);
    }
}

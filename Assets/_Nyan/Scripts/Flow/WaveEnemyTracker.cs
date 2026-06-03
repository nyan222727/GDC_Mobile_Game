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

    private void OnDestroy()
    {
        if (!isArmed || owner == null)
        {
            return;
        }

        owner.NotifyEnemyDefeated(this, waveIndex);
        isArmed = false;
    }
}

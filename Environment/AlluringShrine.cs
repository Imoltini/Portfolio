using System.Collections;
using UnityEngine;

public class AlluringShrine : ShrineBase
{
    float rewardPitch = 1;
    float rewardVolume = 1;
    string alluringAchievement = "ACH_SHRINE_ALLURING";
    WaitForSeconds dropDelay = new WaitForSeconds(0.4f);

    //

    public override void StartTask() => StartCoroutine(SpawnEnemies());
    public override void HandOutReward() => StartCoroutine(GlyphRoutine());
    IEnumerator GlyphRoutine()
    {
        if (claimedReward) yield break;
        //
        shrine.PlaySoundEffect(rewardSFX, rewardPitch, rewardVolume);
        CheckForAchievement();
        claimedReward = true;
        //
        int rewardsToSpawn = 1;
        int spawnChance = Random.Range(0, 100);
        //
        if (spawnChance < 5) rewardsToSpawn = 3;
        else if (spawnChance < 20) rewardsToSpawn = 2;
        //
        if (trackedTasks > 20 && rewardsToSpawn != 3) rewardsToSpawn *= 2;
        for (int i = 0; i < rewardsToSpawn; i++)
        {
            GM.i.lootManager.DropRandomCosmetics(shrine.obeliskHolder.position);
            yield return dropDelay;
        }
    }
    //
    void CheckForAchievement() => GM.i.steamManager.UnlockAchievement(alluringAchievement);
}

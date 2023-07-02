using System.Collections;
using UnityEngine;
using System;

public abstract class ShrineBase : MonoBehaviour
{
    int minEnemies = 5;
    int maxDepthCutoff = 7;
    int minDepthCutoff = 15;
    int minEnemiesCutoff = 25;
    int maxEnemiesCutoff = 35;
    float depthMultiplier = 0.4f;
    string completionText = "CLAIM REWARD";
    //
    public AudioClip rewardSFX;
    protected int trackedTasks;
    [HideInInspector] public bool claimedReward;
    [HideInInspector] public bool triggeredEvent;
    WaitForSeconds spawnTimer = new WaitForSeconds(0.25f);
    WaitForSeconds trackerTickRate = new WaitForSeconds(0.5f);

    //

    public Shrine shrine;
    public abstract void StartTask();
    public abstract void HandOutReward();
    protected IEnumerator TrackTaskCompletion(int tasksValue)
    {
        while (trackedTasks < tasksValue) yield return trackerTickRate;
        GM.i.globalEnemyManager.OnEnemyKilled -= HandleEnemyKilled;
        ChangeDuty();
        //
        shrine.taskIsActive = false;
        shrine.taskCompleted = true;
        shrine.promptDescription.text = completionText;
    }
    //
    protected IEnumerator SpawnEnemies()
    {
        if (triggeredEvent) yield break;
        shrine.taskIsActive = true;
        //
        GM.i.globalEnemyManager.OnEnemyKilled += HandleEnemyKilled;
        int depth = GM.i.GetCurrentChunk();
        trackedTasks = 0;
        //
        int minEnemiesToSpawn = depth < minDepthCutoff ? minEnemies : Mathf.CeilToInt(depth * depthMultiplier);
        if (minEnemiesToSpawn > minEnemiesCutoff) minEnemiesToSpawn = minEnemiesCutoff;
        //
        int maxEnemiesToSpawn = depth < maxDepthCutoff ? maxDepthCutoff : depth;
        if (maxEnemiesToSpawn > maxEnemiesCutoff) maxEnemiesToSpawn = maxEnemiesCutoff;
        //
        int enemiesToSpawn = UnityEngine.Random.Range(minEnemiesToSpawn, maxEnemiesToSpawn);
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GM.i.spawnManager.SpawnSingleEnemy();
            yield return spawnTimer;
        }
        //
        triggeredEvent = true;
        GM.i.enemyFinder.StartSearching();
        StartCoroutine(TrackTaskCompletion(enemiesToSpawn));
        GM.i.tutorialManager.ChangeToSpecificDuty(DutyType.ShrineTask);
    }
    //
    void HandleEnemyKilled() => trackedTasks++;
    void ChangeDuty()
    {
        if ((int)shrine.shrineType < 4) GM.i.tutorialManager.ChangeToSpecificDuty(DutyType.ShrineCompleted);
        else GM.i.tutorialManager.ReturnToCampaignDuties();
    }
}

using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Visyde;
using System;
using Photon.Pun;

public class GameManagerJQ : GameManager
{
    public static new GameManagerJQ instance;

    // Map elements
    const string defaultBreakingPlatformPath = "UI/Misc/Christmas/platform_base_cracked.png";
    const string breakingPlatformPath = "UI/Misc/Christmas/platform_icicle.png";
    const string movingPlatformPath = "UI/Misc/Christmas/platform_wooden.png";
    const string defaultPlatformPath = "UI/Misc/Christmas/platform_";
    const string decorationPath = "UI/Misc/Christmas/deco_";

    readonly string[] decoNames = new string[6]
    { "snow_small", "snow_big", "sapling", "signage", "stump", "rock" };

    readonly string[] platformNames = new string[4]
    { "base", "grass", "snow", "bridge" };

    [HideInInspector] public Sprite defaultBreakingPlatformSprite;
    [HideInInspector] public Sprite[] defaultPlatformSprites;
    [HideInInspector] public Sprite breakingPlatformSprite;
    [HideInInspector] public Sprite movingPlatformSprite;
    [HideInInspector] public Sprite[] decorations;

    const string shootingDragonPrefabPath = "WorldElements/JumpQuestShootingDragon.prefab";
    const string flyingDragonPrefabPath = "WorldElements/JumpQuestFlyingDragon.prefab";
    const string explosionPrefabPath = "WorldElements/JumpQuestExplosion.prefab";
    const string iceShardPrefabPath = "WorldElements/JumpQuestIceShard.prefab";
    const string platformPrefabPath = "WorldElements/JumpQuestPlatform.prefab";
    const string blizzardPrefabPath = "WorldElements/JumpQuestBlizzard.prefab";
    const string boulderPrefabPath = "WorldElements/JumpQuestBoulder.prefab";
    const string fireworkPrefabPath = "VFX/FireworksParticles.prefab";
    JumpQuestShootingDragon[] shootingDragons;
    JumpQuestFlyingDragon[] flyingDragons;
    JumpQuestPlatform[] fakePlatforms;
    JumpQuestBlizzard[] blizzards;
    JumpQuestPlatform[] platforms;
    JumpQuestIceShard[] iceShards;
    JumpQuestBoulder[] boulders;
    GameObject[] explosions;

    // GameOver screen
    const string playAgainPrefabPath = "UI/Menu/JumpQuestPlayAgainUI.prefab";
    GameObject playAgainUI;

    // Achievement Flags
    const string personalBestFlagPrefabPath = "WorldElements/JumpQuestPersonalBestFlag.prefab";
    const string worldAverageFlagPrefabPath = "WorldElements/JumpQuestWorldAverageFlag.prefab";
    [HideInInspector] public GameObject personalBestFlag;
    [HideInInspector] public GameObject worldAverageFlag;

    // Platform variables
    [HideInInspector] public const float maxPlatformScale = 1.4f;
    [HideInInspector] public const float maxHeightDiff = 2.6f;
    [HideInInspector] public const float minHeightDiff = 1.5f;
    [HideInInspector] public const float minX = -10;
    [HideInInspector] public const float maxX = 10;

    [HideInInspector] public const float maxHorizontalDistance = 6.55f;
    [HideInInspector] public const float minHorizontalDistance = 3.5f;
    [HideInInspector] public const float minMoveParam = 1.45f;
    [HideInInspector] public const float maxMoveParam = 1.85f;

    [HideInInspector] public float currentPlatformHorizontalMult;
    [HideInInspector] public float currentPlatformVerticalMult;

    [HideInInspector] public const int platformBreakDelay = 1000;
    [HideInInspector] public int platformBreakChance;
    [HideInInspector] public int platformMoveChance;

    const int platformBreakChanceIncreaseTrigger = 11;
    const int platformMoveChanceIncreaseTrigger = 8;
    const int maxBreakChance = 35;
    const int maxMoveChance = 40;

    const int fakePlatformSpawnChance = 10;
    const int fakePlatformsToSpawn = 5;
    const int platformsToSpawn = 12;
    int currentHeightIndex;

    // Ice shard variables
    [HideInInspector] public const int iceShardDamage = 300;
    const int iceShardAcceleration = 2;
    const int maxIceShardSpeed = 7;
    const int minIceShardSpeed = 3;
    const int iceShardLifetime = 2;

    const int iceShardSpawnIncreaseTrigger = 7;
    const int maxIceShardSpawnChance = 50;
    const int iceShardSpawnDelay = 400;
    const int iceShardsToSpawn = 10;
    int iceShardSpawnChance;

    int currentMaxIceShardSpeedMult;
    bool triggeredIceShardSpawn;
    int currentIceShardIndex;
    bool initializedShards;

    // Explosion variables
    const int explosionsToSpawn = 3;
    int currentExplosionIndex;

    // Dynamic obstacles
    [HideInInspector] public int shootingDragonChance;
    [HideInInspector] public int flyingDragonChance;
    [HideInInspector] public int blizzardChance;
    [HideInInspector] public int boulderChance;

    const int blizzardChanceIncreaseTrigger = 15;
    const int dragonChanceIncreaseTrigger = 12;
    const int maxBlizzardChance = 12;
    const int maxDragonChance = 15;
    const int maxBoulderChance = 5;

    [HideInInspector] public const int blizzardStartTrigger = 225;
    const int shootingDragonStartTrigger = 100;
    const int flyingDragonStartTrigger = 150;
    const int boulderStartTrigger = 175;

    const int shootingDragonsToSpawn = 5;
    const int flyingDragonsToSpawn = 5;
    const int blizzardsToSpawn = 3;
    const int bouldersToSpawn = 3;

    int currentShootingDragonIndex;
    int currentFlyingDragonIndex;
    int currentBlizzardIndex;
    int currentBoulderIndex;

    // Misc Variables
    const int initObstacleChance = 3;
    const int initChance = 5;

    public int timesPlayed = 1;
    public float scoreMultiplier = 1f;
    const float scoreMultiplierIncrement = 0.2f;

    [HideInInspector] public Fireworks fireworkParticles;
    [HideInInspector] public int highScoreForSession;
    [HideInInspector] public int personalBestScore;
    [HideInInspector] public int worldAverageScore;
    [HideInInspector] public bool doNotExecute;
    [HideInInspector] public int highScore;
    [HideInInspector] public int cumulativeHighScore;

    Vector2 topPlatformPosition;
    bool alreadyStartedClimbing;
    int lowestFakePlatformIndex;
    int lowestPlatformIndex;


    void Awake()
    {
        CopyBaseGameManagerProperties(GameManager.instance);
        gameCam.followOffset = new Vector3(0, 1, 0);
        GameManager.instance = instance = this;

        if (EventManager.IsEventInActivationTime(ConstDefine.XMAS_EVENT_ID))
        {
            int daysSinceEventStarted = EventManager.GetDaysSinceEventStarted(ConstDefine.XMAS_EVENT_ID);
            var averageScores = ServerController.Instance.GetGameData().eventTimings[ConstDefine.XMAS_EVENT_ID].param[0].Split(";");

            if (daysSinceEventStarted < averageScores.Length)
                worldAverageScore = int.Parse(averageScores[daysSinceEventStarted]) + 1;
            else
                worldAverageScore = int.Parse(averageScores[^1]);

            EventStatsObj eventStats = EventManager.GetEventStats(ConstDefine.XMAS_EVENT_ID);

            if (eventStats.additional_data.Count > 0)
            {
                int personalBestTarget = int.Parse(eventStats.additional_data[0]);
                if ((eventStats.event_high_score + 1) > personalBestTarget)
                    personalBestScore = eventStats.event_high_score + 1;
                else
                    personalBestScore = personalBestTarget;
            }
            else
                personalBestScore = eventStats.event_high_score + 1;
        }
    }

    public override async UniTask Initialize()
    {
        List<UniTask> waitTaskList = new()
        {
            SpawnPlatformObjects(),
            SpawnIceShardObjects(),
            SpawnExplosionObjects(),
            SpawnPlayAgainUI(),
            SpawnObstacles()
        };
        await UniTask.WhenAll(waitTaskList);

        controlsManager.jumpButton.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-280, 245);
        controlsManager.jumpButton.gameObject.transform.localScale = Vector3.one * 1.5f;

        controlsManager.grenadeButton.gameObject.SetActive(false);
        controlsManager.shootButton.gameObject.SetActive(false);

        await base.Initialize();
        ourPlayer.onDead += HandlePlayerDied;
    }

    protected override void Update()
    {
        base.Update();

        if (!initializedShards)
            return;

        for (int i = 0; i < iceShardsToSpawn; i++)
        {
            iceShards[i].UpdateShard();
        }
    }

    public void IncreaseHighScore(int newScore)
    {
        if (newScore <= highScore) return;

        if (highScore > 6)
        {
            IncreaseIceShardSpawnChance();
            IncreaseSpecialPlatformChance(platformMoveChanceIncreaseTrigger, maxMoveChance, ref platformMoveChance);
            IncreaseSpecialPlatformChance(platformBreakChanceIncreaseTrigger, maxBreakChance, ref platformBreakChance);

            IncreaseObstacleChance(blizzardStartTrigger, blizzardChanceIncreaseTrigger, maxBlizzardChance, ref blizzardChance);
            IncreaseObstacleChance(flyingDragonStartTrigger, dragonChanceIncreaseTrigger, maxDragonChance, ref flyingDragonChance);
            IncreaseObstacleChance(shootingDragonStartTrigger, dragonChanceIncreaseTrigger, maxDragonChance, ref shootingDragonChance);
            IncreaseObstacleChance(boulderStartTrigger, dragonChanceIncreaseTrigger, maxBoulderChance, ref boulderChance);

            var scoreDifference = newScore - highScore;
            if (scoreDifference > 1)
            {
                for (int i = 0; i < scoreDifference; i++)
                {
                    if (!ReuseShootingDragon()) ReuseFlyingDragon();
                    ReuseLowestPlatform();
                    ReuseBlizzard();
                    ReuseBoulder();
                }
            }
            else
            {
                if (!ReuseShootingDragon()) ReuseFlyingDragon();
                ReuseLowestPlatform();
                ReuseBlizzard();
                ReuseBoulder();
            }
        }

        highScore = newScore;

        if (highScore > highScoreForSession)
            highScoreForSession = highScore;

        if (
            highScore > worldAverageScore
            && EventManager.IsEventInActivationTime(ConstDefine.XMAS_EVENT_ID)
            && (
                !ServerController.Instance.GetUserData().resetItemDict.TryGetValue(ConstDefine.XMAS_WORLD_AVG_RESET_ITEM + ConstDefine.XMAS_EVENT_ID, out var resetItemObj)
                || (resetItemObj != null && resetItemObj.count != EventManager.GetDaysSinceEventStarted(ConstDefine.XMAS_EVENT_ID))
            )
        )
        {
            ServerController.Instance.UpdateResetItem(ConstDefine.XMAS_WORLD_AVG_RESET_ITEM + ConstDefine.XMAS_EVENT_ID, "0:0", EventManager.GetDaysSinceEventStarted(ConstDefine.XMAS_EVENT_ID));
        }
        UIManagerJQ.Instance.UpdateHeightText(highScore);
    }

    public void HandleStartOfClimb()
    {
        if (alreadyStartedClimbing) return;
        alreadyStartedClimbing = true;
        OnStartedPlaying();
    }

    void IncreaseSpecialPlatformChance(int increaseTrigger, int maxChance, ref int chance)
    {
        if (highScore % increaseTrigger != 0 || chance >= maxChance) return;
        chance++;
    }

    void IncreaseIceShardSpawnChance()
    {
        if (highScore % iceShardSpawnIncreaseTrigger != 0 || iceShardSpawnChance >= maxIceShardSpawnChance) return;

        if (!triggeredIceShardSpawn)
            StartSpawningIceShards();

        iceShardSpawnChance++;
    }

    void IncreaseObstacleChance(int startTrigger, int increaseTrigger, int maxChance, ref int chance)
    {
        if (highScore < startTrigger || highScore % increaseTrigger != 0 || chance >= maxChance) return;
        chance++;
    }

    async void StartSpawningIceShards()
    {
        triggeredIceShardSpawn = true;

        while (!isGameOver && !doNotExecute)
        {
            await UniTask.Delay(iceShardSpawnDelay);
            if (UnityEngine.Random.Range(0, 100) < iceShardSpawnChance && !doNotExecute)
            {
                iceShards[currentIceShardIndex].SetIceShard(GetIceShardSpeed(), iceShardLifetime, iceShardAcceleration, iceShardDamage);
                currentIceShardIndex = (currentIceShardIndex + 1) % iceShardsToSpawn;
            }
        }
    }

    int GetIceShardSpeed()
    {
        if (currentMaxIceShardSpeedMult < maxIceShardSpeed && highScore % 30 == 0)
            currentMaxIceShardSpeedMult++;

        int maxSpeed = currentMaxIceShardSpeedMult + minIceShardSpeed;
        return UnityEngine.Random.Range(minIceShardSpeed, maxSpeed);
    }

    void ReuseFlyingDragon()
    {
        if (highScore < flyingDragonStartTrigger || UnityEngine.Random.Range(0, 100) > flyingDragonChance) return;

        flyingDragons[currentFlyingDragonIndex].SetDragon(topPlatformPosition);
        currentFlyingDragonIndex = (currentFlyingDragonIndex + 1) % flyingDragonsToSpawn;
    }

    void ReuseBlizzard()
    {
        if (highScore < blizzardStartTrigger || UnityEngine.Random.Range(0, 100) > blizzardChance) return;

        blizzards[currentBlizzardIndex].SetBlizzard(topPlatformPosition);
        currentBlizzardIndex = (currentBlizzardIndex + 1) % blizzardsToSpawn;
    }

    void ReuseBoulder()
    {
        if (highScore < boulderStartTrigger || UnityEngine.Random.Range(0, 100) > boulderChance) return;

        boulders[currentBoulderIndex].SetPosition(topPlatformPosition);
        currentBoulderIndex = (currentBoulderIndex + 1) % bouldersToSpawn;
    }

    public bool ReuseShootingDragon()
    {
        if (highScore < shootingDragonStartTrigger || UnityEngine.Random.Range(0, 100) > shootingDragonChance)
            return false;

        shootingDragons[currentShootingDragonIndex].SetDragon(topPlatformPosition);
        currentShootingDragonIndex = (currentShootingDragonIndex + 1) % shootingDragonsToSpawn;

        return true;
    }

    void ReuseLowestPlatform()
    {
        currentHeightIndex++;

        topPlatformPosition = platforms[lowestPlatformIndex].SetPlatformPosition(topPlatformPosition, currentHeightIndex);

        if (currentHeightIndex == personalBestScore)
            SetPersonalBestFlag(platforms[lowestPlatformIndex]);

        if (currentHeightIndex == worldAverageScore)
            SetWorldAverageFlag(platforms[lowestPlatformIndex]);

        if (UnityEngine.Random.Range(0, 100) < fakePlatformSpawnChance)
            ReuseFakePlatform();

        lowestPlatformIndex = (lowestPlatformIndex + 1) % platformsToSpawn;
    }

    void ReuseFakePlatform()
    {
        fakePlatforms[lowestFakePlatformIndex].gameObject.SetActive(false);
        fakePlatforms[lowestFakePlatformIndex].SetFakePlatform(topPlatformPosition, currentHeightIndex);
        fakePlatforms[lowestFakePlatformIndex].gameObject.SetActive(true);

        lowestFakePlatformIndex = (lowestFakePlatformIndex + 1) % fakePlatformsToSpawn;
    }

    async UniTask SpawnPlatformObjects()
    {
        await LoadPlatformVisuals();

        GameObject platformPrefab = await AddressablesManager.LoadAssetAsync<GameObject>(platformPrefabPath);
        fakePlatforms = new JumpQuestPlatform[fakePlatformsToSpawn];
        platforms = new JumpQuestPlatform[platformsToSpawn];

        platformBreakChance = initChance;
        platformMoveChance = initChance;

        shootingDragonChance = initObstacleChance;
        flyingDragonChance = initObstacleChance;
        blizzardChance = initObstacleChance;

        await SpawnAchievementFlags();

        for (int i = 0; i < platformsToSpawn; i++)
        {
            platforms[i] = Instantiate(platformPrefab, Vector2.zero, Quaternion.identity).GetComponent<JumpQuestPlatform>();

            if (i == 0)
                topPlatformPosition = platforms[i].SetPlatformPosition(Vector2.zero, i + 1);
            else
                topPlatformPosition = platforms[i].SetPlatformPosition(topPlatformPosition, i + 1);

            currentHeightIndex = i + 1;
            if (currentHeightIndex == personalBestScore)
                SetPersonalBestFlag(platforms[i]);
        }

        for (int j = 0; j < fakePlatformsToSpawn; j++)
        {
            fakePlatforms[j] = Instantiate(platformPrefab, Vector2.zero, Quaternion.identity).GetComponent<JumpQuestPlatform>();
            fakePlatforms[j].gameObject.SetActive(false);
        }
    }

    async UniTask LoadPlatformVisuals()
    {
        decorations = new Sprite[decoNames.Length];
        for (int i = 0; i < decorations.Length; i++)
        {
            decorations[i] = await AddressablesManager.LoadAssetAsync<Sprite>($"{decorationPath}{decoNames[i]}.png");
        }

        defaultBreakingPlatformSprite = await AddressablesManager.LoadAssetAsync<Sprite>(defaultBreakingPlatformPath);
        breakingPlatformSprite = await AddressablesManager.LoadAssetAsync<Sprite>(breakingPlatformPath);
        movingPlatformSprite = await AddressablesManager.LoadAssetAsync<Sprite>(movingPlatformPath);

        defaultPlatformSprites = new Sprite[platformNames.Length];

        for (int i = 0; i < defaultPlatformSprites.Length; i++)
        {
            defaultPlatformSprites[i] = await AddressablesManager.LoadAssetAsync<Sprite>($"{defaultPlatformPath}{platformNames[i]}.png");
        }

        await UniTask.Yield();
    }

    async UniTask SpawnIceShardObjects()
    {
        GameObject iceShardPrefab = await AddressablesManager.LoadAssetAsync<GameObject>(iceShardPrefabPath);
        iceShards = new JumpQuestIceShard[iceShardsToSpawn];
        iceShardSpawnChance = 5;

        for (int i = 0; i < iceShardsToSpawn; i++)
        {
            iceShards[i] = Instantiate(iceShardPrefab, Vector2.zero, Quaternion.identity).GetComponent<JumpQuestIceShard>();
        }

        initializedShards = true;
    }

    async UniTask SpawnExplosionObjects()
    {
        GameObject explosionPrefab = await AddressablesManager.LoadAssetAsync<GameObject>(explosionPrefabPath);
        explosions = new GameObject[explosionsToSpawn];

        for (int i = 0; i < explosionsToSpawn; i++)
        {
            GameObject newExplosion = Instantiate(explosionPrefab, Vector2.zero, Quaternion.identity);
            newExplosion.SetActive(false);
            explosions[i] = newExplosion;
        }

        GameObject fireWorksPrefab = await AddressablesManager.LoadAssetAsync<GameObject>(fireworkPrefabPath);
        GameObject fireworks = Instantiate(fireWorksPrefab, Vector2.zero, Quaternion.identity);
        fireworkParticles = fireworks.GetComponent<Fireworks>();
        fireworkParticles.StopFireworks();
    }

    async UniTask SpawnObstacles()
    {
        GameObject flyingDragonPrefab = await AddressablesManager.LoadAssetAsync<GameObject>(flyingDragonPrefabPath);
        GameObject shootingDragonPrefab = await AddressablesManager.LoadAssetAsync<GameObject>(shootingDragonPrefabPath);
        GameObject blizzardPrefab = await AddressablesManager.LoadAssetAsync<GameObject>(blizzardPrefabPath);
        GameObject bounderPrefab = await AddressablesManager.LoadAssetAsync<GameObject>(boulderPrefabPath);

        flyingDragons = new JumpQuestFlyingDragon[flyingDragonsToSpawn];
        for (int i = 0; i < flyingDragonsToSpawn; i++)
        {
            GameObject flyingDragon = Instantiate(flyingDragonPrefab, Vector2.zero, Quaternion.identity);
            flyingDragons[i] = flyingDragon.GetComponent<JumpQuestFlyingDragon>();
        }

        shootingDragons = new JumpQuestShootingDragon[shootingDragonsToSpawn];
        for (int j = 0; j < shootingDragonsToSpawn; j++)
        {
            GameObject shootingDragon = Instantiate(shootingDragonPrefab, Vector2.zero, Quaternion.identity);
            shootingDragons[j] = shootingDragon.GetComponent<JumpQuestShootingDragon>();
        }

        blizzards = new JumpQuestBlizzard[blizzardsToSpawn];
        for (int k = 0; k < blizzardsToSpawn; k++)
        {
            GameObject blizzard = Instantiate(blizzardPrefab, Vector2.zero, Quaternion.identity);
            blizzard.SetActive(false);
            blizzards[k] = blizzard.GetComponent<JumpQuestBlizzard>();
        }

        boulders = new JumpQuestBoulder[bouldersToSpawn];
        for (int i = 0; i < bouldersToSpawn; i++)
        {
            GameObject boulder = Instantiate(bounderPrefab, Vector2.zero, Quaternion.identity);
            boulder.SetActive(false);
            boulders[i] = boulder.GetComponent<JumpQuestBoulder>();
        }
    }

    async UniTask SpawnPlayAgainUI()
    {
        GameObject uiPanel = await AddressablesManager.LoadAssetAsync<GameObject>(playAgainPrefabPath);
        playAgainUI = Instantiate(uiPanel, UIManager.Instance.menuUiParentTrans);
        playAgainUI.SetActive(false);
    }

    async UniTask SpawnAchievementFlags()
    {
        GameObject pbFlag = await AddressablesManager.LoadAssetAsync<GameObject>(personalBestFlagPrefabPath);
        personalBestFlag = Instantiate(pbFlag, Vector2.zero, Quaternion.identity);
        personalBestFlag.SetActive(false);

        GameObject waFlag = await AddressablesManager.LoadAssetAsync<GameObject>(worldAverageFlagPrefabPath);
        worldAverageFlag = Instantiate(waFlag, Vector2.zero, Quaternion.identity);
        worldAverageFlag.SetActive(false);
    }

    public void SpawnIceShardExplosion(Vector2 spawnPosition)
    {
        explosions[currentExplosionIndex].transform.position = spawnPosition;
        explosions[currentExplosionIndex].SetActive(true);

        currentExplosionIndex = (currentExplosionIndex + 1) % explosionsToSpawn;
    }

    void SetPersonalBestFlag(JumpQuestPlatform parent)
    {
        var offset = new Vector2(-1f, 0.15f);
        personalBestFlag.transform.position = topPlatformPosition + offset;
        personalBestFlag.transform.parent = parent.transform;
        personalBestFlag.SetActive(true);
        parent.SetHasPersonalBestFlag();
    }

    void SetWorldAverageFlag(JumpQuestPlatform parent)
    {
        var offset = new Vector2(1f, 0.15f);
        worldAverageFlag.transform.position = topPlatformPosition + offset;
        worldAverageFlag.transform.parent = parent.transform;
        worldAverageFlag.SetActive(true);
        parent.SetHasWorldAverageFlag();
    }

    public void ResetGame()
    {
        AnalyticsManager.Instance.EndQuickMatch(DataCarrier.characters[DataCarrier.chosenCharacter].charName, ourPlayer.playerInstance.secondaryItem, (int)PhotonNetwork.CurrentRoom.CustomProperties["map"], (int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"], (bool)PhotonNetwork.CurrentRoom.CustomProperties["teams"]);

        OnResetGame();

        timesPlayed++;
        cumulativeHighScore += highScore;
        if (highScore > 35)
        {
            scoreMultiplier += scoreMultiplierIncrement;
        }

        if (EventManager.IsEventInActivationTime(ConstDefine.XMAS_EVENT_ID))
        {
            EventStatsObj eventStats = EventManager.GetEventStats(ConstDefine.XMAS_EVENT_ID);
            personalBestScore = eventStats.event_high_score + 1;
        }

        personalBestFlag.SetActive(false);
        worldAverageFlag.SetActive(false);
        alreadyStartedClimbing = false;

        UIManagerJQ.Instance.UpdateHeightText(0);
        currentPlatformHorizontalMult = 0;
        currentPlatformVerticalMult = 0;
        currentMaxIceShardSpeedMult = 0;

        shootingDragonChance = initObstacleChance;
        flyingDragonChance = initObstacleChance;
        blizzardChance = initObstacleChance;

        platformBreakChance = initChance;
        iceShardSpawnChance = initChance;
        platformMoveChance = initChance;

        lowestFakePlatformIndex = 0;
        lowestPlatformIndex = 0;
        currentHeightIndex = 0;
        highScore = 0;

        for (int i = 0; i < platformsToSpawn; i++)
        {
            if (i == 0)
                platforms[i].SetPlatformPosition(Vector2.zero, i + 1);
            else
                topPlatformPosition = platforms[i].SetPlatformPosition(platforms[i - 1].transform.position, i + 1);

            currentHeightIndex = i + 1;
            if (currentHeightIndex == personalBestScore) SetPersonalBestFlag(platforms[i]);
        }

        for (int j = 0; j < fakePlatformsToSpawn; j++)
        {
            fakePlatforms[j].gameObject.SetActive(false);
            fakePlatforms[j].transform.position = Vector2.one * -3;
        }

        for (int i = 0; i < flyingDragons.Length; i++)
        {
            flyingDragons[i].gameObject.SetActive(false);
        }
        currentFlyingDragonIndex = 0;

        for (int i = 0; i < shootingDragons.Length; i++)
        {
            shootingDragons[i].gameObject.SetActive(false);
        }
        currentShootingDragonIndex = 0;

        for (int i = 0; i < blizzards.Length; i++)
        {
            blizzards[i].gameObject.SetActive(false);
        }
        currentBlizzardIndex = 0;

        for (int i = 0; i < boulders.Length; i++)
        {
            boulders[i].gameObject.SetActive(false);
        }
        currentBoulderIndex = 0;
        UIManagerJQ.Instance.HideBoulderWarning();

        deathTime = 0;
        dead = false;

        Spawn();
        ourPlayer.ResetPlayer();
        AnalyticsManager.Instance.StartQuickMatch(DataCarrier.characters[DataCarrier.chosenCharacter].charName, ourPlayer.playerInstance.secondaryItem,
            (int)PhotonNetwork.CurrentRoom.CustomProperties["map"], (int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"], (bool)PhotonNetwork.CurrentRoom.CustomProperties["teams"]);
    }

    void HandlePlayerDied(PlayerController cont)
    {
        if (EventManager.IsEventInActivationTime(ConstDefine.XMAS_EVENT_ID))
        {
            EventStatsObj eventStats = EventManager.GetEventStats(ConstDefine.XMAS_EVENT_ID);
            if (highScore > eventStats.event_high_score)
            {
                EventManager.Instance.UpdateEventHighScore(ConstDefine.XMAS_EVENT_ID, highScore);
            }
        }

        playAgainUI.SetActive(true);
        OnOurPlayerDied();
    }

    void OnDestroy() => doNotExecute = true;
}

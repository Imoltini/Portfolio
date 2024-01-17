using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using Visyde;

public class JumpQuestPlatform : MonoBehaviour
{
    SpriteRenderer visual;
    bool alreadyScored;
    int heightIndex;
    Transform form;

    bool isHorizontal;
    bool isBreaking;
    bool isMoving;

    bool hasWorldAverageFlag;
    bool hasPersonalBestFlag;

    PlayerController playerOnPlatform;
    Transform playerTransform;
    Vector2 previousPosition;
    bool isPlayerOnPlatform;

    void Awake()
    {
        form = transform;
        visual = form.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
    }

    void Update() => MovePlayerWithPlatform();
    void LateUpdate() => previousPosition = form.position;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || !GameManager.instance.gameStarted) return;

        playerOnPlatform = GameManagerJQ.instance.ourPlayer;
        playerTransform = playerOnPlatform.transform;
        isPlayerOnPlatform = true;

        if (alreadyScored) return;
        if (isBreaking) StartBreakingPlatform();

        HandleFlagCheck();
        alreadyScored = true;
        GameManagerJQ.instance.IncreaseHighScore(heightIndex);
        if (heightIndex < 3) GameManagerJQ.instance.HandleStartOfClimb();
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        isPlayerOnPlatform = false;
        playerOnPlatform = null;
        playerTransform = null;
    }

    void HandleFlagCheck()
    {
        if (!(hasPersonalBestFlag || hasWorldAverageFlag)) return;

        if (hasPersonalBestFlag)
            ToastManager.Instance.ActivateToast("New Personal Best!", 0.2f);
        if (hasWorldAverageFlag)
            ToastManager.Instance.ActivateToast("You beat Today's World Average!", 0.2f);

        AudioManager.Instance.Play(AudioConsts.CosmeticUnlock, AudioType.SFX);
        StartFireworks();
    }

    public Vector2 SetPlatformPosition(Vector2 previousPlatformPosition, int newHeightIndex)
    {
        float randScale = Random.Range(1, GameManagerJQ.maxPlatformScale);
        transform.localScale = new Vector2(randScale, randScale);
        heightIndex = newHeightIndex;
        alreadyScored = false;

        StopMovingPlatform();
        isBreaking = false;

        if (hasPersonalBestFlag) RemovePersonalFlag();
        if (hasWorldAverageFlag) RemoveWorldAverageFlag();

        return CalculatePlatformPosition(previousPlatformPosition);
    }

    Vector2 CalculatePlatformPosition(Vector2 previousPos)
    {
        if (previousPos == Vector2.zero) return SetFirstPlatform();

        var newPosition = new Vector2(SetNewHorizontalPosition(previousPos.x), SetNewVerticalPosition(previousPos.y));
        form.position = newPosition;

        if (heightIndex > 10)
        {
            if (Random.Range(0, 100) < GameManagerJQ.instance.platformMoveChance)
                StartMovingPlatform(newPosition);

            if (!isMoving && Random.Range(0, 100) < GameManagerJQ.instance.platformBreakChance)
                isBreaking = true;
        }

        SetPlatformVisuals();
        return newPosition;
    }

    public void SetFakePlatform(Vector2 otherPlatform, int newHeightIndex)
    {
        isBreaking = false;
        heightIndex = newHeightIndex;

        float randX = Random.Range(GameManagerJQ.minHorizontalDistance + 1, GameManagerJQ.maxHorizontalDistance + 1);
        float newXPos = Random.Range(0, 4) > 1 ? otherPlatform.x + randX : otherPlatform.x - randX;

        if (newXPos > GameManagerJQ.maxX)
            newXPos = otherPlatform.x - randX;
        else if (newXPos < GameManagerJQ.minX)
            newXPos = otherPlatform.x + randX;

        if (Random.Range(0, 100) < GameManagerJQ.instance.platformBreakChance)
            isBreaking = true;

        SetPlatformVisuals();

        float randHeight = Random.Range(-0.15f, 0.2f);
        form.position = new Vector2(newXPos, otherPlatform.y + randHeight);
    }

    Vector2 SetFirstPlatform()
    {
        float randX = Random.Range(-2f, 2f);
        var randPos = new Vector2(randX, -1.15f);

        visual.sprite = GameManagerJQ.instance.defaultPlatformSprites[0];
        transform.position = randPos;
        return randPos;
    }

    float SetNewVerticalPosition(float previousPos)
    {
        if (GameManagerJQ.instance.currentPlatformVerticalMult < GameManagerJQ.maxHeightDiff && heightIndex % 40 == 0)
            GameManagerJQ.instance.currentPlatformVerticalMult += 0.35f;

        float maxHeightDiff = GameManagerJQ.minHeightDiff + GameManagerJQ.instance.currentPlatformVerticalMult;

        float newVerticalPosition = previousPos + Random.Range(GameManagerJQ.minHeightDiff, maxHeightDiff);
        return newVerticalPosition;
    }

    float SetNewHorizontalPosition(float previousPos)
    {
        if (GameManagerJQ.instance.currentPlatformHorizontalMult < GameManagerJQ.maxHorizontalDistance && heightIndex % 35 == 0)
            GameManagerJQ.instance.currentPlatformHorizontalMult += 0.4f;

        float maxHorizontalDistance = GameManagerJQ.minHorizontalDistance + GameManagerJQ.instance.currentPlatformHorizontalMult;

        float randX = Random.Range(GameManagerJQ.minHorizontalDistance, maxHorizontalDistance);
        float newXPos = Random.Range(0, 4) > 1 ? previousPos + randX : previousPos - randX;

        if (newXPos > GameManagerJQ.maxX)
            newXPos = previousPos - randX;
        else if (newXPos < GameManagerJQ.minX)
            newXPos = previousPos + randX;

        return newXPos;
    }

    void SetPlatformVisuals()
    {
        var gm = GameManagerJQ.instance;
        visual.enabled = true;

        if (isBreaking)
        {
            if (gm.highScore < 100)
                visual.sprite = gm.defaultBreakingPlatformSprite;
            else
                visual.sprite = gm.breakingPlatformSprite;
            return;
        }

        if (isMoving)
        {
            visual.sprite = gm.movingPlatformSprite;
            return;
        }

        if (heightIndex > 100)
        {
            visual.sprite = gm.defaultPlatformSprites[Random.Range(2, gm.defaultPlatformSprites.Length)];
            return;
        }

        visual.sprite = gm.defaultPlatformSprites[Random.Range(0, 2)];
    }

    void StartMovingPlatform(Vector2 currentPosition)
    {
        float moveDuration = Random.Range(GameManagerJQ.minMoveParam, GameManagerJQ.maxMoveParam);
        float moveAmount = Random.Range(GameManagerJQ.minMoveParam, GameManagerJQ.maxMoveParam);

        if (Random.Range(0, 10) < 5)
        {
            form.DOMoveX(currentPosition.x + moveAmount, moveDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            isHorizontal = true;
        }
        else
        {
            form.DOMoveY(currentPosition.y + moveAmount, moveDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            isHorizontal = false;
        }

        isMoving = true;
    }

    async void StartBreakingPlatform()
    {
        await UniTask.Delay(GameManagerJQ.platformBreakDelay);

        visual.transform.DOShakePosition(1, 0.02f, 30, 90, false, false);
        await UniTask.Delay(GameManagerJQ.platformBreakDelay);

        visual.transform.DOShakePosition(1, 0.03f, 35, 90, false, false);
        await UniTask.Delay(GameManagerJQ.platformBreakDelay);

        visual.transform.DOShakePosition(1, 0.04f, 40, 90, false, false);
        await UniTask.Delay(GameManagerJQ.platformBreakDelay);

        GameManagerJQ.instance.SpawnIceShardExplosion(form.position);

        isBreaking = false;
        visual.enabled = false;
        form.position = Vector2.one * -50;
    }

    void StopMovingPlatform()
    {
        isMoving = false;
        form.DOComplete();
        form.DOKill();
    }

    void MovePlayerWithPlatform()
    {
        if (!isMoving || !isPlayerOnPlatform || !isHorizontal)
            return;

        if (!playerOnPlatform.moving && playerOnPlatform.playerRigidBody.velocity.y < 0.1f)
            playerTransform.position += new Vector3(form.position.x - previousPosition.x, 0f);
    }

    void RemovePersonalFlag()
    {
        GameManagerJQ.instance.personalBestFlag.transform.parent = null;
        GameManagerJQ.instance.personalBestFlag.SetActive(false);
        hasPersonalBestFlag = false;
    }

    void RemoveWorldAverageFlag()
    {
        GameManagerJQ.instance.worldAverageFlag.transform.parent = null;
        GameManagerJQ.instance.worldAverageFlag.SetActive(false);
        hasWorldAverageFlag = false;
    }

    void StartFireworks()
    {
        var fireworks = GameManagerJQ.instance.fireworkParticles;
        fireworks.transform.position = form.position;
        fireworks.StartFireworks();
    }

    public void SetHasPersonalBestFlag() => hasPersonalBestFlag = true;
    public void SetHasWorldAverageFlag() => hasWorldAverageFlag = true;

    void OnDisable() => StopMovingPlatform();
}

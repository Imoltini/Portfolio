using Cysharp.Threading.Tasks;
using UnityEngine;
using Photon.Pun;
using Visyde;

public class GameManagerBB : GameManager
{
    [HideInInspector] public Vector2 spawnPoint { get; private set; }
    public static new GameManagerBB instance;
    public int[] teamScores = new int[2];

    const int explosionDisableDelay = 2500;
    const int explosionDamage = 3000;
    const int explosionRadius = 5;
    const int pointsDelay = 500;
    const int holderBonus = 15;

    Vector2 pointOffset = new(0, 0.5f);
    const string pointStr = "+1";

    GameObject explosionEffect;
    public BattleBall ball;

    void Awake()
    {
        CopyBaseGameManagerProperties(GameManager.instance);
        GameManager.instance = instance = this;
    }

    public override async UniTask Initialize()
    {
        await base.Initialize();
        await ObjectPooler.Instance.CreateProjectileObjectPool(ConstDefine.PUMPKIN_EVENT_PROJECTILENAME, $"Projectiles/{ConstDefine.PUMPKIN_EVENT_PROJECTILENAME}.prefab");
        RegisterBallEvents();
        controlsManager.grenadeButton.gameObject.SetActive(false);
    }

    async void RegisterBallEvents()
    {
        await UniTask.WaitUntil(() => gameMap != null);
        ball = FindObjectOfType<BattleBall>();
        spawnPoint = ball.transform.position;

        explosionEffect = ball.explosionGameObject;
        explosionEffect.transform.parent = null;

        ball.OnBallGrabbed += HandleBallGrabbed;
        ball.OnBallDropped += HandleBallDropped;
    }

    public override void SomeoneDied(int dying, int killer, bool isTeamKill)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        base.SomeoneDied(dying, killer, isTeamKill);
        ball.CheckBallDrop(dying);
    }

    async void StartGivingPoints()
    {
        await UniTask.WaitUntil(() => ball != null);
        while (ball.isOnPlayer)
        {
            if (ball.ownerTeam == BattleBall.missing)
                break;

            if (isGameOver)
            {
                if (ball.ownerTeam != BattleBall.missing)
                {
                    teamScores[ball.ownerTeam - 1] += holderBonus;
                    GetPlayerInstance(ball.ownerPlayerId).AddStats(0, 15, 0, true);
                    photonView.RPC(nameof(RPC_UpdateScore), RpcTarget.All, teamScores[0], teamScores[1], true);
                }

                break;
            }

            await UniTask.Delay(pointsDelay);

            if (ball.ownerTeam != BattleBall.missing)
                teamScores[ball.ownerTeam - 1]++;

            if (ball.ownerPlayerId != BattleBall.missing)
                GetPlayerInstance(ball.ownerPlayerId).AddStats(0, 1, 0, true);

            photonView.RPC(nameof(RPC_UpdateScore), RpcTarget.All, teamScores[0], teamScores[1], false);
        }
    }

    [PunRPC]
    private void RPC_UpdateScore(int team1Score, int team2Score, bool isBonus)
    {
        teamScores[0] = team1Score;
        teamScores[1] = team2Score;

        UIManager.Instance.UpdateBoards();

        Color pointColor = (ball.ownerTeam) == 1 ? UIManager.Instance.color_team1 : UIManager.Instance.color_team2;

        UIManager.Instance.UpdateBoards();

        if (isBonus)
        {
            pooler.Spawn("DamagePopup", ball.GetPosition() + pointOffset).GetComponent<DamagePopup>().Set("+15", Color.white);
            return;
        }

        pooler.Spawn("DamagePopup", ball.GetPosition() + pointOffset).GetComponent<DamagePopup>().Set(pointStr, pointColor);

        if (ball.ownerPlayerId == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            QuestManager.Instance?.ScorePointsInBattleBall();
        }
    }

    void HandleBallGrabbed()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        StartGivingPoints();
    }

    async void HandleBallDropped(Vector2 pos, int playerID, bool allowHurtingSelf)
    {
        if (isGameOver)
            return;

        explosionEffect.transform.position = pos;
        explosionEffect.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
            HandleExplosionDamage(playerID, allowHurtingSelf);

        await UniTask.Delay(explosionDisableDelay);
        explosionEffect.SetActive(false);
    }

    void HandleExplosionDamage(int playerID, bool allowHurtingSelf = true)
    {
        if (isGameOver)
            return;

        Collider2D[] cols = Physics2D.OverlapCircleAll(ball.transform.position, explosionRadius);
        for (int i = 0; i < cols.Length; i++)
        {
            if (!cols[i].CompareTag("Player"))
                continue;

            PlayerController p = cols[i].GetComponent<PlayerController>();
            if (((p.playerInstance.playerID == playerID && allowHurtingSelf) || p.playerInstance.playerID != playerID) && !p.invulnerable)
            {
                if (!p.isDead)
                {
                    Vector2 grPos = new(ball.transform.position.x, ball.transform.position.y);
                    RaycastHit2D[] hits = Physics2D.RaycastAll(grPos, new Vector2(cols[i].transform.position.x, cols[i].transform.position.y) - grPos, explosionRadius);
                    RaycastHit2D hit;

                    for (int h = 0; h < hits.Length; h++)
                    {
                        if (hits[h].collider.gameObject == cols[i].gameObject)
                        {
                            hit = hits[h];
                            // Calculate the damage based on distance:
                            int finalDamage = Mathf.RoundToInt(explosionDamage * (1 - ((ball.transform.position - new Vector3(hit.point.x, hit.point.y)).magnitude / explosionRadius)));
                            // Apply damage:
                            p.ApplyDamage(playerID, finalDamage, secondaryWeaponId: 99);
                            break;
                        }
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        ball.OnBallGrabbed -= HandleBallGrabbed;
        ball.OnBallDropped -= HandleBallDropped;
    }
}

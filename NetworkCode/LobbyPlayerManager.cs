using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class LobbyPlayerManager : MonoBehaviour
{
    // Colours
    [HideInInspector] public readonly Color defaultBackgroundColour = new(0.64f, 0.76f, 1f, 1f);
    [HideInInspector] public readonly Color ownerBackgroundColour = new(1f, 0.91f, 0.76f, 1f);
    [HideInInspector] public readonly Color emptyPlayerColour = new(0.25f, 0.25f, 0.25f, 1);
    [HideInInspector] public readonly Color orangeTeamColour = new(0.91f, 0.68f, 0.1f, 1f);
    [HideInInspector] public readonly Color purpleTeamColour = new(0.74f, 0.47f, 1f, 1f);
    [HideInInspector] public readonly Color ownerTextColour = new(0.67f, 1f, 0.56f, 1f);

    //Positions
    readonly Vector2 defaultPurplePosition = new(-400, 62);
    readonly Vector2 defaultOrangePosition = new(400, 62);
    readonly Vector2 teamPurplePosition = new(-500, 62);
    readonly Vector2 teamOrangePosition = new(500, 62);
    readonly Vector2 purpleLowPosition = new(-400, -62);
    readonly Vector2 orangeLowPosition = new(400, -62);

    //References
    public Button homeButton;
    public RectTransform purpleTeamHolder;
    public RectTransform orangeTeamHolder;
    public LobbyPlayerItem[] playerItems;
    public Sprite emptyPlayerSprite;
    public Sprite orangeTeamSwapSprite;
    public Sprite purpleTeamSwapSprite;
    public GameObject versusObject;

    const string arrayKey = "uiPosArray";
    Dictionary<int, int> actorByIndex = new();
    public List<LobbyPlayerItem> activePlayerList = new();

    [HideInInspector] public bool isSpectateMode = false;

    void Start()
    {
        homeButton.onClick.RemoveAllListeners();
        homeButton.onClick.AddListener(() => Leave());

        for (int i = 0; i < playerItems.Length; i++)
        {
            playerItems[i].ResetActorNumber();
        }
    }

    public void SetLobby()
    {
        if (!Visyde.Connector.isInCustomGame)
            return;

        int[] uiArray = (int[])PhotonNetwork.CurrentRoom.CustomProperties[arrayKey];
        if (uiArray.Length == 0)
            return;

        isSpectateMode = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("customSpectate") && (bool)PhotonNetwork.CurrentRoom.CustomProperties["customSpectate"];

        SetDefaultLobby();
        HandleTeamPositions();
        SetActivePlayerList();
    }

    void SetDefaultLobby()
    {
        for (int i = 0; i < playerItems.Length; i++)
        {
            playerItems[i].gameObject.SetActive(true);
            playerItems[i].SetDefaultPlayerItem();
            playerItems[i].actorNumber = -1;
        }
    }

    void HandleTeamPositions()
    {
        if (Visyde.Connector.instance.loadNow) return;

        if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["teams"])
        {
            purpleTeamHolder.anchoredPosition = teamPurplePosition;
            orangeTeamHolder.anchoredPosition = teamOrangePosition;
            versusObject.SetActive(true);
        }
        else
        {
            int maxPlayers = isSpectateMode ? PhotonNetwork.CurrentRoom.MaxPlayers - 1 : PhotonNetwork.CurrentRoom.MaxPlayers;
            if (maxPlayers <= 4)
            {
                purpleTeamHolder.anchoredPosition = purpleLowPosition;
                orangeTeamHolder.anchoredPosition = orangeLowPosition;
            }
            else
            {
                purpleTeamHolder.anchoredPosition = defaultPurplePosition;
                orangeTeamHolder.anchoredPosition = defaultOrangePosition;
            }
            versusObject.SetActive(false);
        }
    }

    void SetActivePlayerList()
    {
        activePlayerList.Clear();

        int maxPlayers = isSpectateMode ? PhotonNetwork.CurrentRoom.MaxPlayers - 1 : PhotonNetwork.CurrentRoom.MaxPlayers;

        if (maxPlayers >= playerItems.Length)
        {
            HandleFullRoom();
        }
        else
        {
            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["teams"])
                HandleTeamRoom();
            else
                HandleSoloRoom();
        }
    }

    void HandleFullRoom()
    {
        // Get current room's ui position array
        int[] uiArray = (int[])PhotonNetwork.CurrentRoom.CustomProperties[arrayKey];
        int playersSet = 0;

        for (int i = 0; i < uiArray.Length; i++)
        {
            // copy the original lobby player item into the active list
            AddPlayerItemToActiveList(i);
            activePlayerList[i].index = i;

            // if the ui array contains a player's actor number
            if (uiArray[i] != -1)
            {
                // Get the correct player from the current room's player list
                Player playerToSet = PhotonNetwork.CurrentRoom.GetPlayer(uiArray[i]);
                if (playerToSet != null)
                {
                    SetAndCachePlayerUIPosition(ref uiArray, i, i, playerToSet);
                    playersSet++;
                }
            }
        }

        int allPlayers = isSpectateMode ? PhotonNetwork.CurrentRoom.PlayerCount - 1 : PhotonNetwork.CurrentRoom.PlayerCount;
        if (playersSet < allPlayers)
            SetRemainingPlayer();
    }

    void HandleTeamRoom()
    {
        int[] uiArray = (int[])PhotonNetwork.CurrentRoom.CustomProperties[arrayKey];
        int adjustedPlayerCount = isSpectateMode ? PhotonNetwork.CurrentRoom.MaxPlayers - 1 : PhotonNetwork.CurrentRoom.MaxPlayers;
        int playersSet = 0;
        int uiArrayIndex = 0;

        for (int i = 0; i < playerItems.Length; i++)
        {
            if (i == 2 || i == 5)
            {
                DisablePlayerItem(i);
                continue;
            }

            if (i == 3 || i == 4)
            {
                if (adjustedPlayerCount != 6)
                {
                    DisablePlayerItem(i);
                    continue;
                }
            }

            AddPlayerItemToActiveList(i);
            int activePlayerIndex = activePlayerList.Count - 1;
            activePlayerList[activePlayerIndex].index = activePlayerIndex;

            if (uiArrayIndex < uiArray.Length)
            {
                if (uiArray[uiArrayIndex] != -1)
                {
                    Player playerToSet = PhotonNetwork.CurrentRoom.GetPlayer(uiArray[uiArrayIndex]);
                    if (playerToSet != null)
                    {
                        SetAndCachePlayerUIPosition(ref uiArray, activePlayerIndex, uiArrayIndex, playerToSet);
                        playersSet++;
                    }
                }
                ++uiArrayIndex;
            }
        }

        int allPlayers = isSpectateMode ? PhotonNetwork.CurrentRoom.PlayerCount - 1 : PhotonNetwork.CurrentRoom.PlayerCount;
        if (playersSet < allPlayers)
            SetRemainingPlayer();
    }

    void HandleSoloRoom()
    {
        int[] uiArray = (int[])PhotonNetwork.CurrentRoom.CustomProperties[arrayKey];
        int playersSet = 0;

        for (int i = 0; i < playerItems.Length; i++)
        {
            // check if we are outside the bounds of the uiArray
            // and if we are, disable the playerItem
            if (i >= uiArray.Length)
            {
                DisablePlayerItem(i);
                continue;
            }

            // copy the original lobby player item into the active list
            AddPlayerItemToActiveList(i);
            activePlayerList[i].index = i;

            // if the ui array contains a player's actor number
            if (uiArray[i] != -1)
            {
                // Get the correct player from the current room's player list
                Player playerToSet = PhotonNetwork.CurrentRoom.GetPlayer(uiArray[i]);
                if (playerToSet != null)
                {
                    SetAndCachePlayerUIPosition(ref uiArray, i, i, playerToSet);
                    playersSet++;
                }
            }
        }

        int allPlayers = isSpectateMode ? PhotonNetwork.CurrentRoom.PlayerCount - 1 : PhotonNetwork.CurrentRoom.PlayerCount;
        if (playersSet < allPlayers)
            SetRemainingPlayer();
    }

    void SetRemainingPlayer()
    {
        int[] uiArray = (int[])PhotonNetwork.CurrentRoom.CustomProperties[arrayKey];

        for (int i = 0; i < activePlayerList.Count; i++)
        {
            if (!activePlayerList[i].hasPlayer)
                uiArray[i] = -1;
                
        }

        for (int i = 0; i < uiArray.Length; i++)
        {
            // get the first empty UI slot
            if (uiArray[i] == -1)
            {
                Player playerToSet = GetRemainingPlayerToSet();
                if (playerToSet != null)
                {
                    SetAndCachePlayerUIPosition(ref uiArray, i, i, playerToSet);
                }
            }
        }

        // Set custom properties of room to update everyone's UI
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable p = new();
            p.Add(arrayKey, uiArray);
            PhotonNetwork.CurrentRoom.SetCustomProperties(p);
        }
    }

    void SetAndCachePlayerUIPosition(ref int[] array, int index, int arrayIndex, Player playerToSet)
    {
        // Set the lobby player item UI
        activePlayerList[index].Set(playerToSet);

        // Cache the player's ui position in the ui array
        array[arrayIndex] = playerToSet.ActorNumber;

        if (!actorByIndex.ContainsKey(playerToSet.ActorNumber))
            actorByIndex.Add(playerToSet.ActorNumber, index);
    }

    // returns the first player that is not in the actorByIndex dictionary or null
    Player GetRemainingPlayerToSet()
    {
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if (!actorByIndex.ContainsKey(players[i].ActorNumber))
            {
                if (players[i].IsMasterClient && isSpectateMode)
                    continue;
                return players[i];
            }
        }
        return null;
    }

    public int GetIndexFromActor(int actor) => actorByIndex[actor];
    public void RemoveActorFromDictionary(int actor) => actorByIndex.Remove(actor);

    public void SwapPlayerTeam(int originalIndex, int teamIndex)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        MainMenuUI.Instance.lobbyBrowserUI.masterCalls = 0;
        bool foundEmptySpot = false;

        var list = activePlayerList;
        int swapIndex = teamIndex == 1 ? 2 : 1;

        Player originalPlayer = activePlayerList[originalIndex].owner;

        int[] uiArray = (int[])PhotonNetwork.CurrentRoom.CustomProperties[arrayKey];
        for (int i = 0; i < uiArray.Length; i++)
        {
            if (list[i].teamIndex != teamIndex && uiArray[i] == -1)
            {
                foundEmptySpot = true;
                swapIndex = i;
                break;
            }
        }

        if (!foundEmptySpot && activePlayerList[swapIndex].hasPlayer)
        {
            Player swapPlayer = activePlayerList[swapIndex].owner;
            uiArray[originalIndex] = swapPlayer.ActorNumber;

            if (actorByIndex.ContainsKey(swapPlayer.ActorNumber))
                actorByIndex[swapPlayer.ActorNumber] = originalIndex;
        }
        else
        {
            uiArray[originalIndex] = -1;
        }

        uiArray[swapIndex] = originalPlayer.ActorNumber;
        if (actorByIndex.ContainsKey(originalPlayer.ActorNumber))
            actorByIndex[originalPlayer.ActorNumber] = swapIndex;

        ExitGames.Client.Photon.Hashtable roomProperties = new();
        roomProperties.Add(arrayKey, uiArray);
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
    }

    public void ClearLists()
    {
        actorByIndex.Clear();
        activePlayerList.Clear();
    }

    void DisablePlayerItem(int index) => playerItems[index].gameObject.SetActive(false);
    void AddPlayerItemToActiveList(int index)
    {
        activePlayerList.Add(playerItems[index]);
        playerItems[index].gameObject.SetActive(true);
    }

    public void Leave()
    {
        if (PhotonNetwork.InRoom)
        {
            int[] uiArray = (int[])PhotonNetwork.CurrentRoom.CustomProperties["uiPosArray"];
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            int index;

            if (isSpectateMode)
            {
                if (!PhotonNetwork.LocalPlayer.IsMasterClient)
                {
                    index = GetIndexFromActor(actorNumber);

                    uiArray[index] = -1;

                    ExitGames.Client.Photon.Hashtable p = new();
                    p.Add("uiPosArray", uiArray);
                    PhotonNetwork.CurrentRoom.SetCustomProperties(p);
                }
            }
            else
            {
                index = GetIndexFromActor(actorNumber);

                uiArray[index] = -1;

                ExitGames.Client.Photon.Hashtable p = new();
                p.Add("uiPosArray", uiArray);
                PhotonNetwork.CurrentRoom.SetCustomProperties(p);
            }

            if (PhotonNetwork.IsMasterClient)
                RemoveAllPlayersFromLobby();

            SetDefaultLobby();
            PhotonNetwork.LeaveRoom(false);
            PhotonNetwork.Disconnect();

            MainMenuUI.Instance.lobbyBrowserUI.ResetCreateRoomWindow();
            MainMenuUI.Instance.ShowLoadingPanel();
        }
    }

    void RemoveAllPlayersFromLobby()
    { 
        PhotonNetwork.CurrentRoom.PlayerTtl = 0;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.EmptyRoomTtl = 0;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        var list = activePlayerList;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].hasPlayer)
            {
                if (!list[i].owner.IsMasterClient)
                {
                    list[i].RemoveFromLobby();
                    PhotonNetwork.SendAllOutgoingCommands();
                }
            }
        }

        int[] uiArray = new int[0];
        ExitGames.Client.Photon.Hashtable p = new();
        p.Add(arrayKey, uiArray);

        PhotonNetwork.CurrentRoom.SetCustomProperties(p);
    }
}

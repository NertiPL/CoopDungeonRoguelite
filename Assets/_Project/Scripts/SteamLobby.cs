using UnityEngine;
using Mirror;
using Steamworks;

public class SteamLobby : MonoBehaviour
{
    public static CSteamID CurrentLobbyID;

    Callback<LobbyCreated_t> lobbyCreated;
    Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;
    Callback<LobbyEnter_t> lobbyEntered;

    const string LOBBY_HOST_KEY = "host";

    void Start()
    {
        if (!SteamManager.Initialized) { Debug.LogError("Steam nie jest zainicjowany"); return; }
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 8);
    }

    public void JoinLobby()
    {
        SteamFriends.ActivateGameOverlay("Friends");
    }

    void OnLobbyCreated(LobbyCreated_t cb)
    {
        if (cb.m_eResult != EResult.k_EResultOK) { Debug.Log("CreateLobby failed"); return; }

        CurrentLobbyID = new CSteamID(cb.m_ulSteamIDLobby);

        SteamMatchmaking.SetLobbyData(CurrentLobbyID, LOBBY_HOST_KEY, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(CurrentLobbyID, "name", SteamFriends.GetPersonaName() + " lobby");

        NetworkManager.singleton.StartHost();

        SteamRehostManager.Instance.BindLobby(CurrentLobbyID);
        SteamRehostManager.Instance.AdvertiseMeAsHostIfNeeded();

        SteamFriends.ActivateGameOverlayInviteDialog(CurrentLobbyID);
        Debug.Log("Lobby stworzone: " + CurrentLobbyID);
    }

    void OnJoinRequest(GameLobbyJoinRequested_t cb)
    {
        SteamMatchmaking.JoinLobby(cb.m_steamIDLobby);
    }

    void OnLobbyEntered(LobbyEnter_t cb)
    {
        CurrentLobbyID = new CSteamID(cb.m_ulSteamIDLobby);

        SteamRehostManager.Instance.BindLobby(CurrentLobbyID);

        if (!NetworkServer.active && !NetworkClient.isConnected)
        {
            string hostAddr = SteamMatchmaking.GetLobbyData(CurrentLobbyID, LOBBY_HOST_KEY);
            if (string.IsNullOrEmpty(hostAddr))
                hostAddr = SteamMatchmaking.GetLobbyOwner(CurrentLobbyID).m_SteamID.ToString();

            NetworkManager.singleton.networkAddress = hostAddr;
            Debug.Log($"Connecting to host SteamID: {hostAddr}");
            NetworkManager.singleton.StartClient();
        }

        Debug.Log("Do³¹czono do lobby: " + CurrentLobbyID);
    }

}

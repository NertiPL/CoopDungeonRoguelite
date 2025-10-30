#if STEAMWORKS_NET
using Mirror;
using Steamworks;
using UnityEngine;
using System.Collections;

public class SteamRehostManager : MonoBehaviour
{
    public static SteamRehostManager Instance { get; private set; }

    [Header("Klucz w danych lobby")]
    public string lobbyHostKey = "host";

    [Header("Czy automatycznie zosta� hostem, gdy stajesz si� w�a�cicielem lobby")]
    public bool autoHostIfOwner = true;

    CSteamID _lobbyId;
    CSteamID _currentOwner;
    string _advertisedHost; // SteamID hosta jako string
    float _pollInterval = 1.0f;
    bool _bound;

    Callback<LobbyDataUpdate_t> _onLobbyDataUpdate;
    Callback<LobbyChatUpdate_t> _onLobbyChatUpdate;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("[Rehost] Steam nie zainicjalizowany");
            enabled = false;
            return;
        }

        _onLobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdated);
        _onLobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
    }

    // Wywo�aj po utworzeniu lub do��czeniu do lobby
    public void BindLobby(CSteamID lobbyId)
    {
        _lobbyId = lobbyId;
        _currentOwner = SteamMatchmaking.GetLobbyOwner(_lobbyId);
        _advertisedHost = SteamMatchmaking.GetLobbyData(_lobbyId, lobbyHostKey);
        if (!_bound)
        {
            _bound = true;
            StartCoroutine(PollOwnerRoutine());
        }

        // Je�eli jeste� W�A�CICIELEM lobby i jeszcze nie ma hosta w data � og�o� siebie
        if (IsMe(_currentOwner))
            AdvertiseMeAsHostIfNeeded();
    }

    IEnumerator PollOwnerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_pollInterval);
            if (_lobbyId.m_SteamID == 0) continue;

            var owner = SteamMatchmaking.GetLobbyOwner(_lobbyId);
            if (owner != _currentOwner)
            {
                _currentOwner = owner;
                Debug.Log($"[Rehost] Nowy w�a�ciciel lobby: {_currentOwner}");
                OnOwnerChanged(owner);
            }
        }
    }

    void OnOwnerChanged(CSteamID newOwner)
    {
        // odczytaj og�oszonego hosta
        _advertisedHost = SteamMatchmaking.GetLobbyData(_lobbyId, lobbyHostKey);

        if (IsMe(newOwner))
        {
            // my zostali�my w�a�cicielem
            if (autoHostIfOwner && !NetworkServer.active)
            {
                // wr�� do lobby scenicznie, je�eli jeste� w grze
                if (NetworkClient.isConnected) NetworkManager.singleton.StopClient();

                // start hosta
                NetworkManager.singleton.StartHost();

                // og�o� sw�j SteamID jako nowy host
                AdvertiseMeAsHostIfNeeded();
            }
        }
        else
        {
            // nie jeste�my w�a�cicielem: ��czymy si� do og�oszonego hosta
            var hostStr = SteamMatchmaking.GetLobbyData(_lobbyId, lobbyHostKey);
            if (!string.IsNullOrEmpty(hostStr))
            {
                // Fizzy Steamworks u�ywa networkAddress = SteamID hosta jako string
                NetworkManager.singleton.StopHost(); // na wszelki wypadek
                NetworkManager.singleton.StopClient();

                NetworkManager.singleton.networkAddress = hostStr;
                NetworkManager.singleton.StartClient();
            }
        }
    }

    void OnLobbyDataUpdated(LobbyDataUpdate_t data)
    {
        if ((CSteamID)data.m_ulSteamIDLobby != _lobbyId) return;

        var newHost = SteamMatchmaking.GetLobbyData(_lobbyId, lobbyHostKey);
        if (newHost != _advertisedHost)
        {
            _advertisedHost = newHost;
            Debug.Log($"[Rehost] Zmiana hosta w danych lobby: {newHost}");
            if (!IsMe(_currentOwner) && !string.IsNullOrEmpty(newHost))
            {
                NetworkManager.singleton.StopClient();
                NetworkManager.singleton.networkAddress = newHost;
                NetworkManager.singleton.StartClient();
            }
        }
    }

    void OnLobbyChatUpdate(LobbyChatUpdate_t data)
    {
        // kto� wyszed� lub do��czy�; Steam sam przeniesie w�asno��
        // obs�u�y to PollOwnerRoutine oraz OnLobbyDataUpdated
    }

    public void AdvertiseMeAsHostIfNeeded()
    {
        var myId = SteamUser.GetSteamID();
        var current = SteamMatchmaking.GetLobbyData(_lobbyId, lobbyHostKey);
        var myStr = myId.m_SteamID.ToString();

        if (current != myStr)
        {
            SteamMatchmaking.SetLobbyData(_lobbyId, lobbyHostKey, myStr);
            _advertisedHost = myStr;
            Debug.Log($"[Rehost] Ustawiam hosta w danych lobby: {myStr}");
        }
    }

    bool IsMe(CSteamID id) => id == SteamUser.GetSteamID();
}
#endif

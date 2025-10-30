using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameNetworkManager : NetworkManager
{
    [System.Serializable]
    public class ClassEntry
    {
        public ClassId id;
        public GameObject gameplayPrefab;   // prefab gracza w scenie gry
        public PlayerClassSO classSO;       // na póŸniej
    }

    [Header("Mapowanie klas na prefaby")]
    public List<ClassEntry> classPrefabs = new List<ClassEntry>();

    [Header("Nazwa sceny gry")]
    public string gameplaySceneName = "Level";

    Dictionary<ClassId, ClassEntry> _map;
    // zapamiêtujemy wybór klasy dla ka¿dego po³¹czenia
    readonly Dictionary<int, ClassId> _chosenByConnId = new Dictionary<int, ClassId>();

    public static GameNetworkManager Instance { get; private set; }

    public override void Awake()
    {
        base.Awake();
        Instance = this;

        _map = new Dictionary<ClassId, ClassEntry>();
        foreach (var e in classPrefabs)
        {
            if (e == null || e.gameplayPrefab == null) continue;
            _map[e.id] = e;

            // zarejestruj prefaby, ¿eby klient móg³ je spawnowaæ
            if (!spawnPrefabs.Contains(e.gameplayPrefab))
                spawnPrefabs.Add(e.gameplayPrefab);
        }
    }

    // wywo³ywane z PlayerSelection.CmdSelectClass na serwerze
    [Server]
    public void ServerSetSelection(NetworkConnectionToClient conn, ClassId id)
    {
        if (conn == null) return;
        _chosenByConnId[conn.connectionId] = id;
        // Debug.Log($"[GNM] Zapisano wybór: conn {conn.connectionId} -> {id}");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Mirror wo³a to:
        // - przy pierwszym do³¹czeniu do lobby (spawn lobbyplayera)
        // - po ServerChangeScene, kiedy stara to¿samoœæ zosta³a zniszczona

        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == gameplaySceneName)
        {
            // jesteœmy ju¿ w scenie gry – spawnuje PREFAB KLASY
            ClassId id = ClassId.Scout;
            if (_chosenByConnId.TryGetValue(conn.connectionId, out var chosen) && chosen != ClassId.None)
                id = chosen;

            if (!_map.TryGetValue(id, out var entry) || entry.gameplayPrefab == null)
            {
                Debug.LogError($"[GNM] Brak prefaba dla klasy {id} – spawnujê playerPrefab fallback.");
                base.OnServerAddPlayer(conn);
                return;
            }

            Transform start = GetStartPosition();
            Vector3 pos = start ? start.position : Vector3.zero;
            Quaternion rot = start ? start.rotation : Quaternion.identity;

            var player = Instantiate(entry.gameplayPrefab, pos, rot);
            if (player.TryGetComponent(out PlayerClassHolder holder))
                holder.classId = id;

            // teraz poprawny spawn: AddPlayerForConnection
            NetworkServer.AddPlayerForConnection(conn, player);
            // Debug.Log($"[GNM] Spawn klasy {id} dla conn {conn.connectionId}");
            return;
        }

        // scena lobby – u¿yj domyœlnego playerPrefab (LobbyPlayer)
        base.OnServerAddPlayer(conn);
        // Debug.Log($"[GNM] Spawn lobbyplayer dla conn {conn.connectionId}");
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        if (NetworkServer.active) return; // jeœli to host, nie rób nic

        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
    }

}

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
        public PlayerClassSO classSO;       // na p�niej
    }

    [Header("Mapowanie klas na prefaby")]
    public List<ClassEntry> classPrefabs = new List<ClassEntry>();

    [Header("Nazwa sceny gry")]
    public string gameplaySceneName = "Level";

    Dictionary<ClassId, ClassEntry> _map;
    // zapami�tujemy wyb�r klasy dla ka�dego po��czenia
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

            // zarejestruj prefaby, �eby klient m�g� je spawnowa�
            if (!spawnPrefabs.Contains(e.gameplayPrefab))
                spawnPrefabs.Add(e.gameplayPrefab);
        }
    }

    // wywo�ywane z PlayerSelection.CmdSelectClass na serwerze
    [Server]
    public void ServerSetSelection(NetworkConnectionToClient conn, ClassId id)
    {
        if (conn == null) return;
        _chosenByConnId[conn.connectionId] = id;
        // Debug.Log($"[GNM] Zapisano wyb�r: conn {conn.connectionId} -> {id}");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Mirror wo�a to:
        // - przy pierwszym do��czeniu do lobby (spawn lobbyplayera)
        // - po ServerChangeScene, kiedy stara to�samo�� zosta�a zniszczona

        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == gameplaySceneName)
        {
            // jeste�my ju� w scenie gry � spawnuje PREFAB KLASY
            ClassId id = ClassId.Scout;
            if (_chosenByConnId.TryGetValue(conn.connectionId, out var chosen) && chosen != ClassId.None)
                id = chosen;

            if (!_map.TryGetValue(id, out var entry) || entry.gameplayPrefab == null)
            {
                Debug.LogError($"[GNM] Brak prefaba dla klasy {id} � spawnuj� playerPrefab fallback.");
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

        // scena lobby � u�yj domy�lnego playerPrefab (LobbyPlayer)
        base.OnServerAddPlayer(conn);
        // Debug.Log($"[GNM] Spawn lobbyplayer dla conn {conn.connectionId}");
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        if (NetworkServer.active) return; // je�li to host, nie r�b nic

        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
    }

}

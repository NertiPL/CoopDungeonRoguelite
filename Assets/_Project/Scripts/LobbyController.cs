using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NetworkIdentity))]
public class LobbyController : NetworkBehaviour
{
    [Header("Ustaw w inspectorze")]
    Collider startZone;         
    public string gameSceneName = "Level";

    readonly HashSet<NetworkIdentity> playersInZone = new HashSet<NetworkIdentity>();

    float countdown;
    bool counting;

    void Awake()
    {
        startZone = GetComponent<Collider>();
    }

    void OnEnable()
    {
        playersInZone.Clear();
    }

    void Update()
    {
        if (!isServer) return;

        int total = NetworkServer.connections.Count; // ilu graczy jest po³¹czonych
        int inside = playersInZone.Count;            // ilu stoi w strefie

        bool allInside = total > 0 && inside == total;

        if (allInside && AllPlayersChoseClass())
        {
            if (!counting)
            {
                counting = true;
                countdown = 5f;
            }

            countdown -= Time.deltaTime;
            RpcUpdateHud(inside, total, Mathf.CeilToInt(countdown));

            if (countdown <= 0f)
            {
                counting = false;
                NetworkManager.singleton.ServerChangeScene(gameSceneName);
            }
        }
        else
        {
            if (counting) counting = false;
            RpcUpdateHud(inside, total, 0);

            // prosty feedback, jeœli ktoœ nie ma klasy
            if (allInside && !AllPlayersChoseClass())
                LobbyUI.ShowText("Ka¿dy musi wybraæ klasê, zanim zaczniemy.");
        }
    }

        bool AllPlayersChoseClass()
    {
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn?.identity == null) continue;
            var sel = conn.identity.GetComponent<PlayerSelection>();
            if (sel == null || !sel.HasChosen()) return false;
        }
        return true;
    }


    void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        var ni = other.attachedRigidbody
            ? other.attachedRigidbody.GetComponentInParent<NetworkIdentity>()
            : other.GetComponentInParent<NetworkIdentity>();

        if (ni != null && ni.connectionToClient != null) // tylko prawdziwi gracze
            playersInZone.Add(ni);
    }

    void OnTriggerExit(Collider other)
    {
        if (!isServer) return;

        var ni = other.attachedRigidbody
            ? other.attachedRigidbody.GetComponentInParent<NetworkIdentity>()
            : other.GetComponentInParent<NetworkIdentity>();

        if (ni != null) playersInZone.Remove(ni);
    }

    [ClientRpc]
    void RpcUpdateHud(int inside, int total, int sec)
    {
        LobbyUI.instance?.UpdateHud(inside, total, sec);
    }
}

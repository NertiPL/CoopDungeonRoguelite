using Mirror;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [SyncVar] public int hp = 100;
    public int maxHp = 100;

    [Server]
    public void ServerDamage(int dmg)
    {
        if (hp <= 0) return;
        hp -= Mathf.Max(0, dmg);
        if (hp <= 0) RpcDieAndRespawn();
    }

    [ClientRpc]
    void RpcDieAndRespawn()
    {
        // bardzo prosto: natychmiastowe „wstanie”
        if (isServer) hp = maxHp;
    }
}

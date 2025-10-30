using UnityEngine;
using Mirror;

[RequireComponent(typeof(Collider))]
public class ClassSelectZone : MonoBehaviour
{
    public ClassId classId;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var ni = other.GetComponentInParent<NetworkIdentity>();
        if (!ni || !ni.isLocalPlayer) return;

        var sel = ni.GetComponent<PlayerSelection>();
        if (sel != null)
        {
            sel.CmdSelectClass(classId);
            LobbyUI.ShowText($"Wybrano klasê: {classId}");
        }
    }
}

using Mirror;
using UnityEngine;

public class PlayerSelection : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnClassChanged))]
    public ClassId selectedClass = ClassId.None;

    public System.Action<ClassId> onLocalChanged;

    [Command]
    public void CmdSelectClass(ClassId id)
    {
        selectedClass = id;

        // zapisz wybór w NetworkManagerze – kluczowe!
        var mgr = GameNetworkManager.Instance ?? (GameNetworkManager)NetworkManager.singleton;
        mgr?.ServerSetSelection(connectionToClient, id);
    }

    void OnClassChanged(ClassId oldV, ClassId newV)
    {
        if (isLocalPlayer) onLocalChanged?.Invoke(newV);
    }

    public bool HasChosen() => selectedClass != ClassId.None;
}

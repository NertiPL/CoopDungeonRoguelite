using Mirror;

public class PlayerClassHolder : NetworkBehaviour
{
    [SyncVar] public ClassId classId = ClassId.None;

    // na p�niej: mo�esz w Start() wczyta� SO i ustawi� HP, pr�dko�ci itd.
}

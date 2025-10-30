using Mirror;

public class PlayerClassHolder : NetworkBehaviour
{
    [SyncVar] public ClassId classId = ClassId.None;

    // na póŸniej: mo¿esz w Start() wczytaæ SO i ustawiæ HP, prêdkoœci itd.
}

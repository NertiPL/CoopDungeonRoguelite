using Mirror;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class NetPlayerGate : NetworkBehaviour
{
    public Behaviour[] disableForRemote; // np. FirstPersonController, StarterAssetsInputs
#if ENABLE_INPUT_SYSTEM
    public PlayerInput playerInput;
#endif
    public GameObject localOnlyObjects;  // np. Twoja kamera/rek̨ce/HUD

    public override void OnStartAuthority()
    {
        // nasz lokalny gracz
#if ENABLE_INPUT_SYSTEM
        if (playerInput) playerInput.enabled = true;
#endif
        if (localOnlyObjects) localOnlyObjects.SetActive(true);
        SetScriptsEnabled(true);
    }

    public override void OnStartClient()
    {
        // zdalny gracz na tym kliencie
        if (!isLocalPlayer)
        {
#if ENABLE_INPUT_SYSTEM
            if (playerInput) playerInput.enabled = false;
#endif
            if (localOnlyObjects) localOnlyObjects.SetActive(false);
            SetScriptsEnabled(false);
        }
    }

    public override void OnStopClient()
    {
        // sprzątanie na kliencie lokalnym
        if (isLocalPlayer)
        {
#if ENABLE_INPUT_SYSTEM
            if (playerInput) playerInput.enabled = false;
#endif
            if (localOnlyObjects) localOnlyObjects.SetActive(false);
            SetScriptsEnabled(false);
        }
    }

    void SetScriptsEnabled(bool enabled)
    {
        if (disableForRemote == null) return;
        foreach (var b in disableForRemote)
            if (b) b.enabled = enabled;
    }
}

using UnityEngine;
using Mirror;
using System.Collections;
using Unity.Cinemachine;

public class VCamAutoFollow : MonoBehaviour
{
    CinemachineCamera vcam;

    [Tooltip("Nazwa obiektu w prefabie gracza bêd¹cego anchor’em kamery")]
    public string playerAnchorName = "PlayerCameraRoot";

    IEnumerator Start()
    {
        // czekaj, a¿ Mirror utworzy lokalnego gracza
        while (NetworkClient.localPlayer == null)
            yield return null;

        var localPlayer = NetworkClient.localPlayer.gameObject;
        var anchor = FindChildRecursive(localPlayer.transform, playerAnchorName);
        if (anchor == null)
        {
            Debug.LogError($"VCamAutoFollow: nie znalaz³em {playerAnchorName} u lokalnego gracza");
            yield break;
        }

        vcam = GetComponent<CinemachineCamera>();

        if (vcam != null)
        {
            vcam.Follow = anchor;
            vcam.LookAt = anchor;
        }
        else
        {
            Debug.LogError("VCamAutoFollow: na obiekcie nie znaleziono komponentu Cinemachine.");
        }
    }

    Transform FindChildRecursive(Transform root, string name)
    {
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var found = FindChildRecursive(root.GetChild(i), name);
            if (found) return found;
        }
        return null;
    }
}

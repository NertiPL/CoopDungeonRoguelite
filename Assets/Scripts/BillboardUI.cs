using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    Transform _cam;

    void LateUpdate()
    {
        if (_cam == null)
        {
            var cam = Camera.main;
            if (cam) _cam = cam.transform;
            else return;
        }

        // patrz w stronê kamery; obracamy **Canvas**, nie sam TMP
        Vector3 dir = transform.position - _cam.position;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}

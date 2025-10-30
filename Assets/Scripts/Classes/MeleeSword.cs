using Mirror;
using UnityEngine;
using StarterAssets;

public class MeleeSword : NetworkBehaviour
{
    public int damage = 30;
    public float range = 2f;
    public float radius = 1f;
    public float attackRate = 1.2f;
    public LayerMask hitMask;

    StarterAssetsInputs _inputs;
    float _next;

    void Start()
    {
        if (!isLocalPlayer) enabled = false;
        _inputs = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (_inputs.shoot && Time.time >= _next)
        {
            _next = Time.time + 1f / attackRate;
            _inputs.shoot = false;

            Vector3 origin = Camera.main ? Camera.main.transform.position : transform.position + Vector3.up * 1.5f;
            Vector3 dir = Camera.main ? Camera.main.transform.forward : transform.forward;

            CmdSwing(origin + dir * range * 0.5f, radius);
        }
    }

    [Command]
    void CmdSwing(Vector3 center, float r)
    {
        var hits = Physics.OverlapSphere(center, r, hitMask, QueryTriggerInteraction.Collide);
        foreach (var h in hits)
        {
            var id = h.GetComponentInParent<NetworkIdentity>();
            if (id && id.TryGetComponent(out EnemyHealth eh))
                eh.ServerTakeDamage(damage);
        }
        // tu mo¿esz dodaæ Rpc efektu uderzenia
    }

    void OnDrawGizmosSelected()
    {
        if (!isLocalPlayer) return;
        var cam = Camera.main;
        Vector3 origin = cam ? cam.transform.position : transform.position + Vector3.up * 1.5f;
        Vector3 dir = cam ? cam.transform.forward : transform.forward;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin + dir * range * 0.5f, radius);
    }
}

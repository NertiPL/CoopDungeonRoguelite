using Mirror;
using UnityEngine;
using StarterAssets;

[RequireComponent(typeof(StarterAssetsInputs))]
public class HitscanGun : NetworkBehaviour
{
    public int damage = 15;
    public float fireRate = 8f;   // strza³ów/s
    public float range = 80f;
    public LayerMask hitMask;     // jeœli zostawisz 0, u¿yjemy ~0 (wszystko)

    Camera _cam;
    StarterAssetsInputs _inputs;
    float _nextShootTime;

    void Awake()
    {
        _inputs = GetComponent<StarterAssetsInputs>();
    }

    void Start()
    {
        if (!isLocalPlayer) { enabled = false; return; }

        // spróbuj znaleŸæ kamerê gracza
        _cam = GetComponentInChildren<Camera>(true);
        if (_cam == null) _cam = Camera.main; // awaryjnie
    }

    void Update()
    {
        if (_inputs == null) return;

        if (_inputs.shoot && Time.time >= _nextShootTime)
        {
            _nextShootTime = Time.time + 1f / fireRate;
            _inputs.shoot = false;
            TryShoot();
        }
    }

    void TryShoot()
    {
        if (_cam == null) return;

        // jeœli maska pusta w inspectorze – strzelaj we wszystko
        int mask = hitMask.value != 0 ? hitMask.value : ~0;

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * range, Color.cyan, 0.15f);

        if (Physics.Raycast(ray, out var hit, range, mask, QueryTriggerInteraction.Collide))
        {
            // szybka diagnostyka
            // Debug.Log($"Hitscan hit: {hit.collider.name} (layer {hit.collider.gameObject.layer})");

            var id = hit.collider.GetComponentInParent<NetworkIdentity>();
            if (id != null)
                CmdHit(id, damage);
        }
    }

    [Command]
    void CmdHit(NetworkIdentity target, int dmg)
    {
        if (!target) return;

        if (target.TryGetComponent(out EnemyHealth eh))
        {
            eh.ServerTakeDamage(dmg);
            // tu mo¿esz dodaæ efekt trafienia przez Rpc
        }
    }
}

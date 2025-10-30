using Mirror;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [Header("Ruch i �ycie")]
    public float speed = 25f;
    public int damage = 25;
    public float lifeTime = 5f;

    [Header("Maski kolizji")]
    public LayerMask damageMask;   // np. Enemy
    public LayerMask destroyMask;  // np. Default | Environment | Enemy (je�li chcesz niszczy� na wszystkim)

    Collider _col;
    NetworkIdentity _owner;        // kto wystrzeli�

    void Awake()
    {
        _col = GetComponent<Collider>();
    }

    public override void OnStartServer()
    {
        Invoke(nameof(ServerSelfDestruct), lifeTime);
    }

    // wo�aj to zaraz po Instantiate na serwerze
    [Server]
    public void ServerInit(NetworkIdentity owner)
    {
        _owner = owner;

        // ignoruj w�asne kolizje
        if (_owner && _col)
        {
            foreach (var c in _owner.GetComponentsInChildren<Collider>())
                if (c) Physics.IgnoreCollision(_col, c, true);
        }
    }

    [ServerCallback]
    void Update()
    {
        // kinematic + trigger, wi�c przesuwamy transformem
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        // ignoruj w�a�ciciela
        if (_owner && other.GetComponentInParent<NetworkIdentity>() == _owner) return;

        int otherLayerMask = 1 << other.gameObject.layer;

        // 1) trafienie wroga -> zadaj dmg
        if ((damageMask.value & otherLayerMask) != 0)
        {
            if (other.GetComponentInParent<EnemyHealth>() is EnemyHealth eh)
                eh.ServerTakeDamage(damage);

            ServerSelfDestruct();
            return;
        }

        // 2) cokolwiek innego z destroyMask -> zniszcz pocisk
        if ((destroyMask.value & otherLayerMask) != 0)
        {
            ServerSelfDestruct();
        }
        // je�li chcesz, aby pocisk �przelatywa�� przez niekt�re warstwy, po prostu nie dodawaj ich do destroyMask
    }

    [Server]
    void ServerSelfDestruct()
    {
        if (isServer) NetworkServer.Destroy(gameObject);
    }
}

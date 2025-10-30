using Mirror;
using UnityEngine;
using StarterAssets;

public class MageFireballCaster : NetworkBehaviour
{
    public GameObject projectilePrefab; // ma mieæ NetworkIdentity + Collider isTrigger + Rigidbody kinematic
    public Transform spawnPoint;        // np. przed kamer¹
    public float fireRate = 1.5f;

    StarterAssetsInputs _inputs;
    float _nextTime;

    void Start()
    {
        if (!isLocalPlayer) enabled = false;
        _inputs = GetComponent<StarterAssetsInputs>();
        if (!spawnPoint) spawnPoint = Camera.main.transform;
    }

    void Update()
    {
        if (_inputs.shoot && Time.time >= _nextTime)
        {
            _nextTime = Time.time + 1f / fireRate;
            _inputs.shoot = false;
            CmdShoot(spawnPoint.position, spawnPoint.forward);
        }
    }

    [Command]
    void CmdShoot(Vector3 pos, Vector3 dir)
    {
        var go = Instantiate(projectilePrefab, pos, Quaternion.LookRotation(dir));
        var proj = go.GetComponent<Projectile>();
        if (proj) proj.ServerInit(connectionToClient.identity); // wa¿ne: ignoruj kolizjê z w³asnym graczem
        NetworkServer.Spawn(go);
    }

}

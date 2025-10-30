using Mirror;
using TMPro;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour
{
    [Header("HP")]
    public int maxHp = 100;

    [SyncVar(hook = nameof(OnHpChanged))]
    private int hp;

    [Header("UI")]
    [SerializeField] TMP_Text hpText;        // Text (TMP) na Canvasie (World Space)

    void Start()
    {
        // na serwerze ustaw startowe HP
        if (isServer) hp = maxHp;
        UpdateHpText(); // klienci te¿ odœwie¿¹ z bie¿¹cej wartoœci
    }

    void OnHpChanged(int oldValue, int newValue) => UpdateHpText();

    void UpdateHpText()
    {
        if (hpText) hpText.text = $"HP: {hp}";
    }

    [Server]
    public void ServerTakeDamage(int dmg)
    {
        if (hp <= 0) return;
        hp = Mathf.Max(0, hp - Mathf.Max(0, dmg));
        if (hp == 0) NetworkServer.Destroy(gameObject);
    }
}

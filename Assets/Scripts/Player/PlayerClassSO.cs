using UnityEngine;

[CreateAssetMenu(menuName = "Coop/Player Class")]
public class PlayerClassSO : ScriptableObject
{
    public ClassId id;
    public string displayName;

    [Header("Baza")]
    public int baseHP = 100;
    public float moveSpeed = 4.5f;
    public float sprintSpeed = 6.5f;
}

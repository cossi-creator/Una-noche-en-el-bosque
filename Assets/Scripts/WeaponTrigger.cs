using UnityEngine;

public class WeaponTrigger : MonoBehaviour
{
    public PlayerControllerFPS player;

    void Reset()
    {
        if (player == null) player = GetComponentInParent<PlayerControllerFPS>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (player == null) player = GetComponentInParent<PlayerControllerFPS>();
        if (player == null) return;

        // Delegar al player para procesar enemigos u objetos
        player.OnWeaponHitColliderEnter(other);

        // Opcional: debug rápido
        Debug.Log($"WeaponTrigger hit: {other.gameObject.name}");
    }
}

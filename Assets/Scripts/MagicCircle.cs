using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MagicCircle : MonoBehaviour
{
    [Header("Efecto visual y audio")]
    public GameObject effectPrefab;
    public Transform effectSpawnPoint;
    public AudioClip activateSfx;
    public float effectDuration = 4f;

    [Header("Buff")]
    [Tooltip("Porcentaje por stack (0.25 = +25%)")]
    public float buffPercent = 0.25f;
    [Tooltip("Máximo de stacks que puede dar esta ruina (si quieres limitar por ruina)")]
    public int maxStacksPerPlayer = 3;

    [Header("Cooldown")]
    public float playerCooldown = 1.0f;

    private Collider triggerCollider;
    private Dictionary<GameObject, float> lastActivated = new Dictionary<GameObject, float>();

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other, out PlayerControllerFPS player)) return;
        if (player == null) return;

        if (lastActivated.TryGetValue(player.gameObject, out float lastTime))
        {
            if (Time.time - lastTime < playerCooldown) return;
        }

        // Curar al máximo
        player.HealToFull();

        // Aplicar buff permanente, pasando el límite de esta ruina
        player.ApplyPermanentStrengthBuff(buffPercent, maxStacksPerPlayer);

        // VFX
        if (effectPrefab != null)
        {
            Vector3 pos = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
            GameObject fx = Instantiate(effectPrefab, pos, Quaternion.identity);
            Destroy(fx, effectDuration + 0.5f);
        }

        if (activateSfx != null)
        {
            AudioSource.PlayClipAtPoint(activateSfx, transform.position);
        }

        lastActivated[player.gameObject] = Time.time;
    }

    bool IsPlayer(Collider col, out PlayerControllerFPS player)
    {
        player = col.GetComponentInParent<PlayerControllerFPS>();
        if (player != null) return true;

        if (col.CompareTag("Player"))
        {
            player = col.GetComponentInParent<PlayerControllerFPS>();
            return player != null;
        }

        return false;
    }

    public void ActivateFor(PlayerControllerFPS player)
    {
        if (player == null) return;
        player.HealToFull();
        player.ApplyPermanentStrengthBuff(buffPercent, maxStacksPerPlayer);

        if (effectPrefab != null)
        {
            Vector3 pos = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
            GameObject fx = Instantiate(effectPrefab, pos, Quaternion.identity);
            Destroy(fx, effectDuration + 0.5f);
        }

        if (activateSfx != null)
        {
            AudioSource.PlayClipAtPoint(activateSfx, transform.position);
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EstatuaMaldita : MonoBehaviour
{
    [Header("Efecto al destruir")]
    [Tooltip("Prefab del efecto especial (particulas) que aparece al destruir la estatua")]
    public GameObject efectoDestruccion;

    [Header("Configuracion")]
    [Tooltip("Vida de la estatua. Usar RecibirDaño() si tenes un sistema de combate con daño")]
    public int vida = 1;

    [Tooltip("Tag opcional para identificar el arma del jugador (no obligatorio si usas WeaponTrigger)")]
    public string tagArmaJugador = "Arma";

    private bool destruida = false;

    void Reset()
    {
        // Asegurar que el collider sea trigger si queremos detección por trigger
        var c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    // Llamar este metodo desde tu sistema de combate cuando el jugador golpee la estatua
    public void RecibirDaño(int cantidad)
    {
        if (destruida) return;

        vida -= cantidad;
        if (vida <= 0)
        {
            DestruirEstatua();
        }
    }

    // Tambien podes llamar a este metodo directamente desde otro script o evento
    public void DestruirEstatua()
    {
        if (destruida) return;
        destruida = true;

        // Instanciar el efecto especial en la posicion de la estatua
        if (efectoDestruccion != null)
        {
            Instantiate(efectoDestruccion, transform.position, transform.rotation);
        }

        // Avisar al GameManager que se destruyo una estatua mas (llamada directa, sin SendMessage)
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.EstatuaDestruida();
        }
        else
        {
            Debug.LogWarning("EstatuaMaldita: no se encontro GameManager.Instancia al destruir estatua.");
        }

        Destroy(gameObject);
    }

    // Detección por trigger: detecta tanto colisiones por tag como colisiones con el WeaponTrigger del jugador.
    // Esto permite que la estatua se destruya si la maza (WeaponTrigger) la golpea, aunque la maza no tenga tag "Arma".
    private void OnTriggerEnter(Collider other)
    {
        if (destruida) return;

        // 1) Si el otro collider tiene un componente WeaponTrigger (script auxiliar del arma), asumimos que es un golpe del jugador
        var weaponTrigger = other.GetComponentInParent<WeaponTrigger>();
        if (weaponTrigger != null)
        {
            // Si querés que la estatua reciba daño variable, podrías leer un valor desde WeaponTrigger o PlayerControllerFPS.
            // Por simplicidad asumimos 1 de daño por golpe (o podés pasar la cantidad desde WeaponTrigger).
            RecibirDaño(1);
            return;
        }

        // 2) Si el arma del jugador tiene un tag específico (legacy), respetarlo
        if (!string.IsNullOrEmpty(tagArmaJugador) && other.CompareTag(tagArmaJugador))
        {
            RecibirDaño(1);
            return;
        }

        // 3) Si el collider pertenece al jugador (por ejemplo un empujón), ignorar
        var player = other.GetComponentInParent<PlayerControllerFPS>();
        if (player != null) return;

        // 4) Fallback: si el objeto entrante tiene un componente que expone RecibirDaño por SendMessage (compatibilidad),
        // intentar invocar RecibirDaño en el otro (no obligatorio).
    }
}

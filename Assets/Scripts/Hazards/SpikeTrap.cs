using UnityEngine;

/// <summary>
/// Trampa de pinchos. Cuando el jugador la toca, le quita vidas llamando a
/// PlayerHealth.TakeDamage(). Sigue el mismo patrón que EnemyPatrol.
///
/// Mientras el jugador siga ENCIMA de los pinchos, vuelve a hacerle daño cada
/// vez que se le acabe la invulnerabilidad (esa pausa la controla PlayerHealth,
/// así que no hay que temporizar nada aquí: TakeDamage() ignora el golpe si el
/// jugador está invulnerable).
///
/// Requisitos en el GameObject de los pinchos:
///   - Un Collider (Box, Mesh, etc.) con "Is Trigger" ACTIVADO.
///   - El jugador debe tener el tag "Player" y el componente PlayerHealth.
/// Pensado para Unity 6 (6000.0.x) + URP, igual que el resto del proyecto.
/// </summary>
public class SpikeTrap : MonoBehaviour
{
    [Header("Daño")]
    [Tooltip("ACTIVO: los pinchos siguen dañando mientras el jugador esté encima " +
             "(respetando la invulnerabilidad de PlayerHealth).\n" +
             "INACTIVO: solo dañan en el primer contacto.")]
    [SerializeField] private bool continuousDamage = true;

    /// <summary>Primer contacto con los pinchos.</summary>
    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    /// <summary>
    /// Mientras el jugador siga dentro del trigger. Solo se usa si el daño es
    /// continuo. TakeDamage() se "auto-pausa" gracias a la invulnerabilidad,
    /// así que aunque esto se llame cada frame, solo cuenta cuando toca.
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (continuousDamage)
            TryDamage(other);
    }

    /// <summary>
    /// Comprueba que sea el jugador y le aplica un golpe.
    /// GetComponentInParent por si el collider está en un hijo del jugador.
    /// </summary>
    private void TryDamage(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(); // quita una vida (y respawnea si está configurado)
        }
        else
        {
            Debug.LogWarning("[SpikeTrap] El objeto con tag 'Player' no tiene el componente PlayerHealth.", this);
        }
    }
}

using UnityEngine;

/// <summary>
/// Tipos de power-up disponibles.
/// </summary>
public enum PowerUpType { Velocidad, VidaExtra, Escudo }

/// <summary>
/// Va en CADA objeto de power-up del nivel.
/// Funciona igual que tu CollectibleItem: necesita un Collider con
/// "Is Trigger" activado. Al tocarlo el jugador, le avisa a PlayerPowerUps
/// qué tipo de power-up activar y se desactiva.
///
/// Para 2D: cambia "Collider" por "Collider2D" y
/// "OnTriggerEnter" por "OnTriggerEnter2D".
/// </summary>
[RequireComponent(typeof(Collider))]
public class PowerUpPickup : MonoBehaviour
{
    [Header("Tipo de power-up")]
    [Tooltip("Qué efecto da este objeto al recogerlo.")]
    [SerializeField] private PowerUpType type = PowerUpType.Velocidad;

    [Header("Config")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Opcional: partículas/sonido a instanciar al recoger.")]
    [SerializeField] private GameObject pickupVFX;

    [Tooltip("Si lo activas, este objeto también cuenta para el PuzzleManager " +
             "(por si quieres reutilizar el contador del acertijo).")]
    [SerializeField] private bool countInPuzzle = false;

    private bool collected = false;

    private void Reset()
    {
        // Al añadir el script, deja el collider como trigger automáticamente.
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag(playerTag)) return;

        // GetComponentInParent por si el collider del jugador está en un hijo,
        // igual que hace tu EnemyPatrol con PlayerHealth.
        PlayerPowerUps powerUps = other.GetComponentInParent<PlayerPowerUps>();
        if (powerUps == null)
        {
            Debug.LogWarning("[PowerUpPickup] El jugador no tiene el componente PlayerPowerUps.", this);
            return;
        }

        collected = true;

        // 1) Activar el power-up correspondiente.
        powerUps.Activate(type);

        // 2) (Opcional) contar también en el acertijo.
        if (countInPuzzle && PuzzleManager.Instance != null)
            PuzzleManager.Instance.CollectItem();

        // 3) Efecto visual opcional.
        if (pickupVFX != null)
            Instantiate(pickupVFX, transform.position, Quaternion.identity);

        // 4) Quitar el objeto del mundo.
        gameObject.SetActive(false);
        // Si prefieres destruirlo: Destroy(gameObject);
    }
}

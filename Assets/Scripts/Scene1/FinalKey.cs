using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// La LLAVE final del juego. Va en el objeto llave (con un Collider en
/// "Is Trigger"). Cuando el jugador la toca, muestra el mensaje de victoria
/// ("¡Felicidades, lo lograste!") y termina el juego.
///
/// Usa WinUI si existe en la escena (pantalla persistente).
/// Si no hay WinUI, hace fallback a tu ClueUI para que igual funcione.
///
/// Para 2D: cambia "Collider" por "Collider2D" y
/// "OnTriggerEnter" por "OnTriggerEnter2D".
/// </summary>
[RequireComponent(typeof(Collider))]
public class FinalKey : MonoBehaviour
{
    [Header("Mensaje de victoria")]
    [SerializeField, TextArea(2, 4)] private string winMessage = "¡Felicidades, lo lograste!";

    [Header("Config")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Si está activo, la llave SOLO funciona cuando ya se resolvió el " +
             "puzzle (todos los objetos recogidos / puerta abierta). " +
             "Déjalo en falso si la llave debe funcionar siempre.")]
    [SerializeField] private bool requirePuzzleComplete = false;

    [Tooltip("Opcional: efecto/partículas/sonido a instanciar al recoger la llave.")]
    [SerializeField] private GameObject pickupVFX;

    [Header("Extra (opcional)")]
    [Tooltip("Se dispara al recoger la llave. Útil para sonidos, animaciones, etc.")]
    [SerializeField] private UnityEvent onCollected;

    private bool collected = false;

    private void Reset()
    {
        // Se asegura de que el collider sea trigger al añadir el script.
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag(playerTag)) return;

        // Si se exige tener el puzzle completo y aún no lo está, no hace nada.
        if (requirePuzzleComplete &&
            PuzzleManager.Instance != null &&
            !PuzzleManager.Instance.IsComplete)
        {
            return;
        }

        collected = true;

        // 1) Efecto visual opcional.
        if (pickupVFX != null)
            Instantiate(pickupVFX, transform.position, Quaternion.identity);

        // 2) Evento opcional.
        onCollected?.Invoke();

        // 3) Mostrar la victoria. WinUI primero; si no hay, usa ClueUI.
        if (WinUI.Instance != null)
            WinUI.Instance.ShowWin(winMessage);
        else if (ClueUI.Instance != null)
            ClueUI.Instance.ShowClue(winMessage);
        else
            Debug.LogWarning("[FinalKey] No hay WinUI ni ClueUI en la escena.", this);

        // 4) Quitar la llave del mundo.
        gameObject.SetActive(false);
    }
}

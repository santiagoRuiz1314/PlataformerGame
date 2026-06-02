using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// El JARRÓN que el jugador toca para activar el mecanismo de la fuente.
/// Requiere un Collider con "Is Trigger" activado.
///
/// Cuando el jugador entra al trigger:
///   1) Le dice a la fuente que se mueva (fountain.Activate()).
///   2) (Opcional) muestra una pista en el cuadro de diálogo (ClueUI).
///   3) (Opcional) dispara un evento e instancia un efecto.
///
/// Para 2D: cambia "Collider" por "Collider2D" y
/// "OnTriggerEnter" por "OnTriggerEnter2D".
/// </summary>
[RequireComponent(typeof(Collider))]
public class VaseTrigger : MonoBehaviour
{
    [Header("Mecanismo")]
    [Tooltip("La fuente que se debe mover al tocar este jarrón. Arrástrala aquí.")]
    [SerializeField] private MovingFountain fountain;

    [Header("Config")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Si está activo, el jarrón desaparece tras activarse. " +
             "Desactívalo si quieres que el jarrón siga visible (ej. roto).")]
    [SerializeField] private bool hideOnTrigger = false;

    [Tooltip("Opcional: efecto/partículas/sonido a instanciar al tocarlo " +
             "(ej. el jarrón rompiéndose).")]
    [SerializeField] private GameObject triggerVFX;

    [Header("Pista (opcional)")]
    [Tooltip("Texto que aparece al tocar el jarrón. Déjalo vacío para no mostrar nada.")]
    [SerializeField, TextArea(2, 4)] private string clueText = "";
    [SerializeField] private string speakerName = "";
    [SerializeField] private Sprite portraitOverride;

    [Header("Extra (opcional)")]
    [Tooltip("Se dispara al activarse. Útil para enganchar sonidos, animaciones, etc.")]
    [SerializeField] private UnityEvent onActivated;

    private bool triggered = false;

    private void Reset()
    {
        // Se asegura de que el collider sea trigger al añadir el script.
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;

        // 1) Mover la fuente.
        if (fountain != null)
            fountain.Activate();
        else
            Debug.LogWarning("[VaseTrigger] No hay fuente asignada en el inspector.", this);

        // 2) Mostrar la pista (si hay).
        if (!string.IsNullOrEmpty(clueText) && ClueUI.Instance != null)
            ClueUI.Instance.ShowClue(clueText, speakerName, portraitOverride);

        // 3) Efecto visual opcional.
        if (triggerVFX != null)
            Instantiate(triggerVFX, transform.position, Quaternion.identity);

        // 4) Evento opcional.
        onActivated?.Invoke();

        // 5) Esconder el jarrón si así se configuró.
        if (hideOnTrigger)
            gameObject.SetActive(false);
    }
}

using UnityEngine;

/// <summary>
/// Va en CADA objeto que el jugador debe encontrar.
/// Requiere un Collider con "Is Trigger" activado.
/// Al tocarlo el jugador: muestra una pista (en el cuadro de diálogo),
/// avisa al PuzzleManager y se desactiva.
///
/// Para 2D: cambia "Collider" por "Collider2D" y
/// "OnTriggerEnter" por "OnTriggerEnter2D".
/// </summary>
[RequireComponent(typeof(Collider))]
public class CollectibleItem : MonoBehaviour
{
    [Header("Pista")]
    [Tooltip("Texto que aparece al recoger este objeto. " +
             "Escribe aquí la pista que ayuda a encontrar otro objeto.")]
    [SerializeField, TextArea(2, 4)] private string clueText = "Encontraste algo...";

    [Header("Quién da la pista (opcional)")]
    [Tooltip("Si lo dejas vacío, usa el personaje por defecto del ClueUI.")]
    [SerializeField] private string speakerName = "";
    [Tooltip("Si lo dejas vacío, usa el retrato por defecto del ClueUI.")]
    [SerializeField] private Sprite portraitOverride;

    [Header("Config")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Opcional: efecto/partículas/sonido a instanciar al recoger.")]
    [SerializeField] private GameObject pickupVFX;

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

        collected = true;

        // 1) Mostrar la pista con el personaje (cuadro de diálogo).
        if (ClueUI.Instance != null)
            ClueUI.Instance.ShowClue(clueText, speakerName, portraitOverride);

        // 2) Avisar al manager para que lleve la cuenta.
        if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.CollectItem();

        // 3) Efecto visual opcional.
        if (pickupVFX != null)
            Instantiate(pickupVFX, transform.position, Quaternion.identity);

        // 4) Quitar el objeto del mundo.
        gameObject.SetActive(false);
        // Si prefieres destruirlo del todo: Destroy(gameObject);
    }
}

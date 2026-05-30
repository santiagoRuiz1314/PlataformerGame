using System;
using UnityEngine;

/// <summary>
/// Cerebro del acertijo del primer escenario.
/// Lleva la cuenta de objetos recolectados y avisa (por eventos)
/// cuando se recoge uno y cuando ya se recogieron todos.
/// Colócalo en un GameObject vacío en la escena (ej: "PuzzleManager").
/// </summary>
public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Config")]
    [Tooltip("Cuántos objetos hay que recoger para abrir la puerta.")]
    [SerializeField] private int totalItems = 3;

    private int collectedItems = 0;

    // ---- Eventos a los que se suscriben otros scripts ----
    // (recogidos, total)  -> útil para actualizar un HUD tipo "1/3"
    public event Action<int, int> OnItemCollected;
    // se dispara una sola vez, cuando ya están todos
    public event Action OnAllItemsCollected;

    public bool IsComplete => collectedItems >= totalItems;

    private void Awake()
    {
        // Singleton simple: solo debe existir uno
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Lo llama cada CollectibleItem cuando el jugador lo recoge.
    /// </summary>
    public void CollectItem()
    {
        if (IsComplete) return; // ya estaba completo, ignorar

        collectedItems++;
        OnItemCollected?.Invoke(collectedItems, totalItems);

        Debug.Log($"[Puzzle] Objeto recogido: {collectedItems}/{totalItems}");

        if (collectedItems >= totalItems)
        {
            Debug.Log("[Puzzle] ¡Todos los objetos recogidos! Abriendo puerta...");
            OnAllItemsCollected?.Invoke();
        }
    }
}

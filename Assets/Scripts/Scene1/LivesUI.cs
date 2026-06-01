using UnityEngine;

/// <summary>
/// HUD de vidas: enciende/apaga los corazones según las vidas actuales.
/// Colócalo en el Canvas y arrastra los corazones (Vida 0, Vida 1, Vida 2)
/// al arreglo 'hearts', de izquierda a derecha.
/// </summary>
public class LivesUI : MonoBehaviour
{
    [Header("Corazones (de izquierda a derecha)")]
    [Tooltip("Arrastra aquí los objetos Vida 0, Vida 1 y Vida 2 en orden.")]
    [SerializeField] private GameObject[] hearts;

    private void Start()
    {
        // Awake de PlayerHealth ya corrió antes que cualquier Start,
        // así que aquí Instance ya existe.
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnLivesChanged += UpdateHearts;

            // Pintamos el estado inicial.
            UpdateHearts(PlayerHealth.Instance.CurrentLives, PlayerHealth.Instance.MaxLives);
        }
        else
        {
            Debug.LogWarning("[LivesUI] No encontré ningún PlayerHealth en la escena.");
        }
    }

    private void OnDestroy()
    {
        // Buena práctica: desuscribirse para evitar errores al cambiar de escena.
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.OnLivesChanged -= UpdateHearts;
    }

    /// <summary>
    /// Muestra tantos corazones como vidas tenga el jugador.
    /// Ej: 2 vidas -> Vida 0 y Vida 1 visibles, Vida 2 oculto.
    /// </summary>
    private void UpdateHearts(int current, int max)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
                hearts[i].SetActive(i < current);
        }
    }
}

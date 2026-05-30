using UnityEngine;

/// <summary>
/// Va en la puerta del primer escenario.
/// Se suscribe al PuzzleManager y se abre automáticamente
/// cuando se han recogido todos los objetos.
///
/// Soporta 3 formas de "abrir" (elige una en el inspector):
///  - Animator: dispara un trigger de animación.
///  - Mover: desliza la puerta hacia arriba/lado.
///  - Desactivar: simplemente la quita (la más simple para probar).
/// </summary>
public class Door : MonoBehaviour
{
    private enum OpenMode { Animator, Mover, Desactivar }

    [Header("Cómo se abre")]
    [SerializeField] private OpenMode mode = OpenMode.Mover;

    [Header("Modo Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "Open";

    [Header("Modo Mover")]
    [Tooltip("Desplazamiento al abrir, en metros. Ej: (0, 3, 0) sube 3m.")]
    [SerializeField] private Vector3 moveOffset = new Vector3(0f, 3f, 0f);
    [SerializeField] private float moveSpeed = 3f;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool opening = false;

    private void Awake()
    {
        closedPos = transform.position;
        openPos = closedPos + moveOffset;
    }

    private void OnEnable()
    {
        // Suscribirse de forma segura (el manager podría no existir aún en Awake)
        if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.OnAllItemsCollected += OpenDoor;
    }

    private void Start()
    {
        // Doble seguro: si en OnEnable el manager no existía todavía
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.OnAllItemsCollected -= OpenDoor; // evita doble suscripción
            PuzzleManager.Instance.OnAllItemsCollected += OpenDoor;
        }
    }

    private void OnDisable()
    {
        if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.OnAllItemsCollected -= OpenDoor;
    }

    private void OpenDoor()
    {
        switch (mode)
        {
            case OpenMode.Animator:
                if (animator != null) animator.SetTrigger(openTrigger);
                break;

            case OpenMode.Mover:
                opening = true; // el Update se encarga de deslizarla
                break;

            case OpenMode.Desactivar:
                gameObject.SetActive(false);
                break;
        }
    }

    private void Update()
    {
        if (!opening) return;

        transform.position = Vector3.MoveTowards(
            transform.position, openPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, openPos) < 0.01f)
            opening = false; // llegó, dejar de mover
    }
}

using UnityEngine;

/// <summary>
/// Abre UNA puerta por codigo (sin animaciones).
///
/// Ponlo en el objeto que ES la puerta: el que tiene la malla visible Y el
/// Box Collider. Al moverse/girar, el collider se va con el, asi que el
/// jugador puede pasar. El muro y el marco son OTROS objetos y NO se tocan.
///
/// Dos formas de abrir (elige en el inspector):
///  - Deslizar: la puerta se corre en linea recta (sube como reja o se va al lado).
///  - Girar: la puerta gira sobre una bisagra, como una puerta normal.
/// </summary>
public class Door : MonoBehaviour
{
    private enum OpenMode { Deslizar, Girar }

    [Header("Forma de abrir (por código)")]
    [SerializeField] private OpenMode mode = OpenMode.Girar;

    [Header("Deslizar")]
    [Tooltip("Cuánto y hacia dónde se corre, en metros. " +
             "Ej (0,3,0) sube 3m; (3,0,0) se va a la derecha.")]
    [SerializeField] private Vector3 slideOffset = new Vector3(0f, 3f, 0f);
    [Tooltip("Velocidad de deslizamiento en metros/segundo.")]
    [SerializeField] private float slideSpeed = 3f;

    [Header("Girar")]
    [Tooltip("Ángulo final de apertura, en grados. Ej 90.")]
    [SerializeField] private float openAngle = 90f;
    [Tooltip("Velocidad de giro en grados/segundo.")]
    [SerializeField] private float rotateSpeed = 120f;
    [Tooltip("Eje de giro (en mundo). Para una puerta normal: Y = (0,1,0).")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [Tooltip("Dónde está la bisagra respecto al CENTRO de la puerta, en local. " +
             "Si la bisagra está en el borde, pon algo como (-0.5,0,0) y ajusta. " +
             "Déjalo en cero si el pivote ya está en la bisagra.")]
    [SerializeField] private Vector3 hingeLocalOffset = Vector3.zero;

    private bool opening = false;

    // Deslizar
    private Vector3 openPos;

    // Girar
    private float rotatedSoFar = 0f;

    private void Awake()
    {
        openPos = transform.position + slideOffset;
    }

    private void OnEnable()
    {
        if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.OnAllItemsCollected += OpenDoor;
    }

    private void Start()
    {
        // Doble seguro por si el manager no existía aún en OnEnable
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.OnAllItemsCollected -= OpenDoor;
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
        opening = true; // el Update hace el movimiento
    }

    private void Update()
    {
        if (!opening) return;

        if (mode == OpenMode.Deslizar)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, openPos, slideSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, openPos) < 0.01f)
                opening = false; // llegó
        }
        else // Girar
        {
            float step = Mathf.Min(rotateSpeed * Time.deltaTime, openAngle - rotatedSoFar);

            // La bisagra es un punto fijo en el mundo; giramos alrededor de él.
            Vector3 hingeWorld = transform.TransformPoint(hingeLocalOffset);
            transform.RotateAround(hingeWorld, rotationAxis.normalized, step);

            rotatedSoFar += step;
            if (rotatedSoFar >= openAngle - 0.01f)
                opening = false; // terminó de abrir
        }
    }
}

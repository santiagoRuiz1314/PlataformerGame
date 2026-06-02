using UnityEngine;

/// <summary>
/// La FUENTE que se mueve para dejar al descubierto un hueco que tiene debajo.
///
/// Ponlo en el objeto que ES la fuente (el que tiene la malla visible y su
/// Collider). Al deslizarse, el collider se va con ella, así que el jugador
/// puede pasar por el hueco que quede destapado.
///
/// No se mueve sola: alguien tiene que llamar a Activate() (lo hace el jarrón
/// con el script VaseTrigger). Una vez activada, se desliza una sola vez.
/// </summary>
public class MovingFountain : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Cuánto y hacia dónde se corre la fuente, en metros. " +
             "Ej (3,0,0) se va a la derecha; (0,0,3) se va en profundidad; " +
             "(0,3,0) sube. Ajústalo para que destape el hueco de abajo.")]
    [SerializeField] private Vector3 moveOffset = new Vector3(3f, 0f, 0f);

    [Tooltip("Velocidad del deslizamiento en metros/segundo.")]
    [SerializeField] private float moveSpeed = 2f;

    [Tooltip("Segundos de espera antes de empezar a moverse (para dar dramatismo). 0 = al instante.")]
    [SerializeField] private float startDelay = 0f;

    [Header("Sonido (opcional)")]
    [Tooltip("Sonido al activarse el mecanismo (ruido de piedra arrastrándose, etc.). Puede quedar vacío.")]
    [SerializeField] private AudioClip moveSfx;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    private Vector3 targetPos;
    private bool activated = false;   // ya se le dio la orden de moverse
    private bool moving = false;      // se está moviendo ahora mismo
    private float delayTimer = 0f;

    /// <summary>True cuando la fuente ya terminó de moverse (hueco destapado).</summary>
    public bool IsOpen { get; private set; }

    private void Awake()
    {
        targetPos = transform.position + moveOffset;
    }

    /// <summary>
    /// Llama a esto para disparar el mecanismo. Lo usa el jarrón (VaseTrigger).
    /// Si ya fue activada, no hace nada (se mueve una sola vez).
    /// </summary>
    public void Activate()
    {
        if (activated) return;
        activated = true;
        delayTimer = startDelay;

        if (moveSfx != null)
            AudioSource.PlayClipAtPoint(moveSfx, transform.position, sfxVolume);
    }

    private void Update()
    {
        if (!activated || IsOpen) return;

        // Espera del delay antes de arrancar.
        if (!moving)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer > 0f) return;
            moving = true;
        }

        transform.position = Vector3.MoveTowards(
            transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            transform.position = targetPos;
            moving = false;
            IsOpen = true; // listo: el hueco quedó destapado
        }
    }

    // Dibuja en el editor a dónde va a llegar la fuente (línea + esfera).
    private void OnDrawGizmosSelected()
    {
        Vector3 from = Application.isPlaying ? transform.position : transform.position;
        Vector3 to = Application.isPlaying ? targetPos : transform.position + moveOffset;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawWireSphere(to, 0.3f);
    }
}

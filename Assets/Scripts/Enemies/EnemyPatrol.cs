using UnityEngine;

/// <summary>
/// Hace que un enemigo patrulle de ida y vuelta entre dos puntos.
/// Mueve el objeto con Transform directo (sin Rigidbody ni CharacterController).
/// Pensado para Unity 6 (6000.0.x) + URP.
/// </summary>
public class EnemyPatrol : MonoBehaviour
{
    [Header("Puntos de patrulla")]
    [Tooltip("Punto A: extremo izquierdo (o inicial) del recorrido.")]
    [SerializeField] private Transform pointA;

    [Tooltip("Punto B: extremo derecho (o final) del recorrido.")]
    [SerializeField] private Transform pointB;

    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento en unidades por segundo.")]
    [SerializeField] private float speed = 2f;

    [Tooltip("Distancia a la que se considera que ya llegó al punto destino.")]
    [SerializeField] private float arriveThreshold = 0.05f;

    [Tooltip("Segundos que espera quieto al llegar a cada extremo (0 = no espera).")]
    [SerializeField] private float waitTimeAtPoints = 0.5f;

    [Header("Orientación")]
    [Tooltip("Si está activo, el enemigo gira para mirar hacia donde se mueve.")]
    [SerializeField] private bool rotateTowardsMovement = true;

    [Tooltip("Qué tan rápido gira el enemigo al cambiar de dirección.")]
    [SerializeField] private float rotationSpeed = 10f;

    // --- Estado interno ---
    private Transform currentTarget;   // Hacia qué punto se dirige ahora
    private float waitTimer;           // Cronómetro de espera en los extremos

    private void Start()
    {
        // Validación: si faltan puntos, avisamos y desactivamos el script.
        if (pointA == null || pointB == null)
        {
            Debug.LogError($"[EnemyPatrol] Faltan asignar pointA o pointB en '{name}'.", this);
            enabled = false;
            return;
        }

        // Empezamos dirigiéndonos al punto B.
        currentTarget = pointB;
    }

    private void Update()
    {
        // Si está en pausa esperando en un extremo, descontamos el tiempo y salimos.
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        MoveTowardsTarget();
        if (rotateTowardsMovement) RotateTowardsTarget();
    }

    /// <summary>
    /// Desplaza al enemigo hacia el punto destino actual.
    /// Solo se mueve en el plano horizontal (X y Z); la altura Y se mantiene.
    /// </summary>
    private void MoveTowardsTarget()
    {
        // Tomamos la posición del destino pero conservamos nuestra propia Y,
        // para que el enemigo no suba ni baje aunque los puntos estén a otra altura.
        Vector3 targetPos = currentTarget.position;
        targetPos.y = transform.position.y;

        // Movemos a velocidad constante hacia el destino.
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            speed * Time.deltaTime
        );

        // ¿Ya llegamos lo suficientemente cerca?
        float distance = Vector3.Distance(transform.position, targetPos);
        if (distance <= arriveThreshold)
        {
            // Cambiamos de objetivo: si íbamos a B ahora vamos a A, y viceversa.
            currentTarget = (currentTarget == pointA) ? pointB : pointA;

            // Activamos la pausa en el extremo.
            waitTimer = waitTimeAtPoints;
        }
    }

    /// <summary>
    /// Gira suavemente al enemigo para que mire hacia su punto destino.
    /// </summary>
    private void RotateTowardsTarget()
    {
        Vector3 direction = currentTarget.position - transform.position;
        direction.y = 0f; // No queremos que se incline hacia arriba/abajo.

        if (direction.sqrMagnitude < 0.0001f) return; // Evita rotaciones inválidas.

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Detecta el contacto con el jugador.
    /// Requiere que el Collider del enemigo esté marcado como "Is Trigger".
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Aquí decides qué pasa al tocar al jugador.
            // Opción simple: lo mandamos al Respawn de la escena.
            Debug.Log("[EnemyPatrol] El enemigo tocó al jugador.");

            // EJEMPLO (descomenta y adapta al sistema de respawn que uses):
            // GameObject respawn = GameObject.Find("Respawn");
            // if (respawn != null) other.transform.position = respawn.transform.position;
        }
    }

    /// <summary>
    /// Dibuja en el editor el recorrido de patrulla para verlo sin darle Play.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (pointA == null || pointB == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pointA.position, pointB.position);
        Gizmos.DrawWireSphere(pointA.position, 0.2f);
        Gizmos.DrawWireSphere(pointB.position, 0.2f);
    }
}
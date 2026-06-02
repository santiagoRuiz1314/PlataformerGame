using System.Collections;
using UnityEngine;

/// <summary>
/// Plataforma "falsa": parece sólida y se puede pisar, pero al tocarla
/// (tras un pequeño aviso opcional) desaparece y el jugador cae. Puede
/// reaparecer pasado un tiempo o quedarse ida para siempre.
///
/// Por qué hace falta una zona-trigger:
///   El jugador usa CharacterController. Al pisar un Collider SÓLIDO, Unity
///   NO dispara OnTriggerEnter (solo lo bloquea físicamente). Por eso este
///   script crea automáticamente una zona-trigger delgada justo ENCIMA de la
///   superficie para enterarse de cuándo el jugador aterriza. La superficie
///   sigue siendo sólida (se puede caminar) hasta el momento de desaparecer.
///
/// Requisitos en el GameObject de la plataforma:
///   - Un Collider NO-trigger (idealmente BoxCollider) = la superficie sólida.
///   - Un MeshRenderer (o cualquier Renderer) para que se vea.
///   - El jugador debe tener el tag "Player".
///
/// Pensado para Unity 6 (6000.0.x) + URP, igual que el resto del proyecto.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DisappearingPlatform : MonoBehaviour
{
    [Header("Comportamiento")]
    [Tooltip("Segundos que la plataforma aguanta tras ser pisada antes de desaparecer.\n0 = desaparece al instante.")]
    [SerializeField] private float delayBeforeDisappear = 0.4f;

    [Tooltip("ACTIVO: la plataforma vuelve a aparecer tras 'respawnDelay'.\nINACTIVO: desaparece para siempre.")]
    [SerializeField] private bool reappear = true;

    [Tooltip("Segundos que tarda en reaparecer (solo si 'reappear' está activo).")]
    [SerializeField] private float respawnDelay = 2.5f;

    [Header("Aviso visual")]
    [Tooltip("Hace temblar la plataforma durante el 'delayBeforeDisappear' como aviso al jugador.")]
    [SerializeField] private bool shakeWarning = true;

    [Tooltip("Cuánto tiembla (en unidades del mundo).")]
    [SerializeField] private float shakeAmount = 0.06f;

    [Header("Detección")]
    [Tooltip("Tag del jugador.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Alto de la zona de detección que se crea por encima de la superficie.")]
    [SerializeField] private float detectionHeight = 0.5f;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxDisappear;
    [SerializeField] private AudioClip sfxReappear;

    // --- Referencias internas ---
    private Collider solidCollider;       // la superficie sólida (la que ya tenías)
    private BoxCollider detectionTrigger; // zona-trigger creada en runtime
    private Renderer[] renderers;
    private Vector3 startLocalPosition;
    private bool isTriggered = false;

    private void Awake()
    {
        // Captura el collider sólido ANTES de añadir el trigger.
        solidCollider = GetComponent<Collider>();
        solidCollider.isTrigger = false; // la superficie SIEMPRE es sólida

        renderers = GetComponentsInChildren<Renderer>();
        startLocalPosition = transform.localPosition;

        BuildDetectionZone();
    }

    /// <summary>
    /// Crea una caja-trigger fina pegada a la cara superior del collider sólido.
    /// Así solo se activa cuando el jugador aterriza ENCIMA, no al rozar un lado.
    /// </summary>
    private void BuildDetectionZone()
    {
        BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;

        if (solidCollider is BoxCollider box)
        {
            trigger.size = new Vector3(box.size.x * 0.95f, detectionHeight, box.size.z * 0.95f);
            trigger.center = box.center + Vector3.up * (box.size.y * 0.5f + trigger.size.y * 0.5f);
        }
        else
        {
            // Fallback para colliders no-Box: cubre todo el volumen (menos preciso).
            Bounds b = solidCollider.bounds;
            trigger.center = transform.InverseTransformPoint(b.center);
            trigger.size = b.size;
            Debug.LogWarning("[DisappearingPlatform] Usa un BoxCollider para una detección precisa por arriba.", this);
        }

        detectionTrigger = trigger;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;
        if (!other.CompareTag(playerTag)) return;

        isTriggered = true;
        StartCoroutine(DisappearRoutine());
    }

    private IEnumerator DisappearRoutine()
    {
        if (delayBeforeDisappear > 0f)
        {
            if (shakeWarning)
                yield return StartCoroutine(Shake(delayBeforeDisappear));
            else
                yield return new WaitForSeconds(delayBeforeDisappear);
        }

        SetActiveState(false); // se oculta y deja de colisionar → el jugador cae
        if (sfxDisappear && audioSource) audioSource.PlayOneShot(sfxDisappear);

        if (reappear)
        {
            yield return new WaitForSeconds(respawnDelay);
            SetActiveState(true);
            if (sfxReappear && audioSource) audioSource.PlayOneShot(sfxReappear);
            isTriggered = false;
        }
    }

    /// <summary>Temblor de aviso durante 'duration' segundos.</summary>
    private IEnumerator Shake(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            Vector3 offset = new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                0f,
                Random.Range(-shakeAmount, shakeAmount));
            transform.localPosition = startLocalPosition + offset;
            yield return null;
        }
        transform.localPosition = startLocalPosition;
    }

    /// <summary>
    /// Activa/desactiva la plataforma sin desactivar el GameObject (para no
    /// matar las corrutinas): solo apaga renderers y colliders.
    /// </summary>
    private void SetActiveState(bool active)
    {
        foreach (var r in renderers)
            if (r) r.enabled = active;

        if (solidCollider) solidCollider.enabled = active;
        if (detectionTrigger) detectionTrigger.enabled = active;

        transform.localPosition = startLocalPosition; // por si quedó desfasada del shake
    }

    // Dibuja la zona de detección en el editor (al estilo de MovingPlatform).
    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.6f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 size = new Vector3(box.size.x * 0.95f, detectionHeight, box.size.z * 0.95f);
            Vector3 center = box.center + Vector3.up * (box.size.y * 0.5f + size.y * 0.5f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}

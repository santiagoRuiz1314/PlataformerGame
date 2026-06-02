using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Cuadro de diálogo estilo "novela visual" para mostrar las pistas del acertijo.
/// El personaje "aparece" como un retrato en una barra inferior, con su nombre
/// y el texto de la pista (con fade-in y efecto de máquina de escribir).
///
/// DÓNDE VA: en un objeto del Canvas (ej. el propio Canvas o un objeto vacío
/// llamado "DialogueUI"). Luego arrastras las referencias en el Inspector.
///
/// Es compatible con tu CollectibleItem actual: sigue funcionando ShowClue(text).
/// La versión ShowClue(text, nombre, retrato) permite cambiar el personaje.
/// </summary>
public class ClueUI : MonoBehaviour
{
    public static ClueUI Instance { get; private set; }

    [Header("Referencias UI")]
    [Tooltip("El contenedor del cuadro de diálogo (se activa/desactiva).")]
    [SerializeField] private GameObject panel;
    [Tooltip("CanvasGroup en el mismo panel, para el fade in/out.")]
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("Imagen del retrato del personaje (a la izquierda del cuadro).")]
    [SerializeField] private Image portraitImage;
    [Tooltip("Texto con el nombre del personaje.")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [Tooltip("Texto donde se escribe la pista.")]
    [SerializeField] private TextMeshProUGUI clueLabel;

    [Header("Personaje por defecto (la guía)")]
    [SerializeField] private string defaultSpeakerName = "Guía";
    [SerializeField] private Sprite defaultPortrait;

    [Header("Animación")]
    [Tooltip("Duración del fade in/out, en segundos.")]
    [SerializeField] private float fadeTime = 0.25f;
    [Tooltip("Segundos que el cuadro permanece visible tras terminar de escribir.")]
    [SerializeField] private float displayTime = 4f;
    [SerializeField] private bool useTypewriter = true;
    [Tooltip("Velocidad del efecto máquina de escribir (caracteres por segundo).")]
    [SerializeField] private float charsPerSecond = 40f;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Sonido al aparecer el personaje.")]
    [SerializeField] private AudioClip appearSfx;

    private Coroutine routine;

    private void Awake()
    {
        // Singleton seguro (igual que tu PuzzleManager).
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (panel != null) panel.SetActive(false);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Compatibilidad con tu CollectibleItem actual: usa el personaje por defecto.
    /// </summary>
    public void ShowClue(string text)
    {
        ShowClue(text, defaultSpeakerName, defaultPortrait);
    }

    /// <summary>
    /// Versión completa: permite cambiar nombre y retrato por cada objeto.
    /// </summary>
    public void ShowClue(string text, string speakerName, Sprite portrait)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(text, speakerName, portrait));
    }

    private IEnumerator ShowRoutine(string text, string speakerName, Sprite portrait)
    {
        if (panel != null) panel.SetActive(true);

        // Nombre del personaje
        if (nameLabel != null)
            nameLabel.text = string.IsNullOrEmpty(speakerName) ? defaultSpeakerName : speakerName;

        // Retrato (usa el override si lo hay; si no, el por defecto)
        if (portraitImage != null)
        {
            Sprite p = portrait != null ? portrait : defaultPortrait;
            portraitImage.sprite = p;
            portraitImage.enabled = p != null;
        }

        if (clueLabel != null) clueLabel.text = "";

        if (audioSource != null && appearSfx != null)
            audioSource.PlayOneShot(appearSfx);

        // 1) El personaje "aparece" (fade in)
        yield return Fade(0f, 1f, fadeTime);

        // 2) Escribir la pista
        if (useTypewriter && clueLabel != null && charsPerSecond > 0f)
        {
            float delay = 1f / charsPerSecond;
            foreach (char c in text)
            {
                clueLabel.text += c;
                yield return new WaitForSeconds(delay);
            }
        }
        else if (clueLabel != null)
        {
            clueLabel.text = text;
        }

        // 3) Mantener visible
        yield return new WaitForSeconds(displayTime);

        // 4) Desaparecer (fade out)
        yield return Fade(1f, 0f, fadeTime);
        if (panel != null) panel.SetActive(false);
        routine = null;
    }

    private IEnumerator Fade(float from, float to, float time)
    {
        if (canvasGroup == null) yield break;

        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / time);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Pantalla de VICTORIA / fin del juego ("¡Felicidades, lo lograste!").
/// A diferencia de ClueUI, este mensaje NO se oculta solo: se queda en pantalla
/// y (opcional) congela el juego, porque marca el final.
///
/// DÓNDE VA: en un objeto del Canvas (ej. un objeto vacío "WinUI").
/// Luego arrastras las referencias en el Inspector.
///
/// Singleton igual que tu PuzzleManager / ClueUI: solo debe existir uno.
/// </summary>
public class WinUI : MonoBehaviour
{
    public static WinUI Instance { get; private set; }

    [Header("Referencias UI")]
    [Tooltip("El contenedor de la pantalla de victoria (se activa/desactiva).")]
    [SerializeField] private GameObject panel;
    [Tooltip("CanvasGroup en el mismo panel, para el fade in.")]
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("Texto donde se muestra el mensaje de victoria.")]
    [SerializeField] private TextMeshProUGUI messageLabel;

    [Header("Texto por defecto")]
    [SerializeField, TextArea(2, 4)] private string defaultMessage = "¡Felicidades, lo lograste!";

    [Header("Animación")]
    [Tooltip("Duración del fade in, en segundos.")]
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private bool useTypewriter = false;
    [Tooltip("Velocidad del efecto máquina de escribir (caracteres por segundo).")]
    [SerializeField] private float charsPerSecond = 40f;

    [Header("Comportamiento")]
    [Tooltip("Congela el juego (Time.timeScale = 0) al mostrar la victoria.")]
    [SerializeField] private bool freezeGame = true;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Sonido al ganar.")]
    [SerializeField] private AudioClip winSfx;

    private Coroutine routine;

    private void Awake()
    {
        // Singleton seguro (igual que tu PuzzleManager / ClueUI).
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (panel != null) panel.SetActive(false);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    /// <summary>Muestra el mensaje por defecto.</summary>
    public void ShowWin()
    {
        ShowWin(defaultMessage);
    }

    /// <summary>Muestra la pantalla de victoria con un mensaje personalizado.</summary>
    public void ShowWin(string message)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(string.IsNullOrEmpty(message) ? defaultMessage : message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        if (panel != null) panel.SetActive(true);

        // Congela el juego de una (el jugador se detiene al instante).
        // OJO: usamos tiempo "unscaled" abajo para que el fade igual se anime.
        if (freezeGame) Time.timeScale = 0f;

        if (messageLabel != null) messageLabel.text = useTypewriter ? "" : message;

        if (audioSource != null && winSfx != null)
            audioSource.PlayOneShot(winSfx);

        // 1) Aparecer (fade in). Usa unscaledDeltaTime por si el juego está congelado.
        yield return Fade(0f, 1f, fadeTime);

        // 2) Escribir el mensaje (opcional).
        if (useTypewriter && messageLabel != null && charsPerSecond > 0f)
        {
            float delay = 1f / charsPerSecond;
            foreach (char c in message)
            {
                messageLabel.text += c;
                yield return new WaitForSecondsRealtime(delay); // realtime: ignora el timeScale
            }
        }

        routine = null;
    }

    // ---- Para enganchar a botones del Canvas (evento OnClick) ----

    /// <summary>Reinicia la escena actual.</summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Cierra el juego (o detiene el Play Mode en el editor).</summary>
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator Fade(float from, float to, float time)
    {
        if (canvasGroup == null) yield break;

        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime; // unscaled: funciona aunque timeScale = 0
            canvasGroup.alpha = Mathf.Lerp(from, to, t / time);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}

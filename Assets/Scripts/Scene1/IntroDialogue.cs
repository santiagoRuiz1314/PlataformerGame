using System.Collections;
using UnityEngine;

/// <summary>
/// Muestra una pista/contexto inicial al empezar la escena, usando el mismo
/// cuadro de diálogo (ClueUI) que las pistas de los objetos.
///
/// DÓNDE VA: en un GameObject vacío en la escena (ej. "IntroDialogue").
/// Escribes el texto en el Inspector y listo.
/// </summary>
public class IntroDialogue : MonoBehaviour
{
    [Header("Mensaje inicial")]
    [Tooltip("Texto de contexto que aparece al empezar el nivel.")]
    [SerializeField, TextArea(2, 5)]
    private string introText = "Bienvenido... busca los 3 objetos para abrir la puerta.";

    [Header("Quién lo dice (opcional)")]
    [Tooltip("Si lo dejas vacío, usa el personaje por defecto del ClueUI.")]
    [SerializeField] private string speakerName = "";
    [Tooltip("Si lo dejas vacío, usa el retrato por defecto del ClueUI.")]
    [SerializeField] private Sprite portraitOverride;

    [Header("Config")]
    [Tooltip("Segundos de espera antes de mostrar el mensaje (deja ver la escena).")]
    [SerializeField] private float startDelay = 0.5f;
    [Tooltip("Si lo activas, solo se muestra la primera vez que se carga la escena.")]
    [SerializeField] private bool onlyOnce = false;

    private static bool alreadyShown = false;

    private IEnumerator Start()
    {
        if (onlyOnce && alreadyShown) yield break;

        // Esperar a que el ClueUI exista y a que termine el delay.
        yield return new WaitForSeconds(startDelay);

        if (ClueUI.Instance != null)
        {
            ClueUI.Instance.ShowClue(introText, speakerName, portraitOverride);
            alreadyShown = true;
        }
        else
        {
            Debug.LogWarning("[IntroDialogue] No encontré un ClueUI en la escena.", this);
        }
    }
}

using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Muestra el texto de la pista en pantalla por unos segundos.
/// Va en un objeto del Canvas. Necesitas:
///  - Un panel (GameObject) que contenga el texto.
///  - Un componente TextMeshProUGUI para el texto.
///
/// Si no usas TextMeshPro, cambia "TextMeshProUGUI" por
/// "UnityEngine.UI.Text" y el using por "using UnityEngine.UI;".
/// </summary>
public class ClueUI : MonoBehaviour
{
    public static ClueUI Instance { get; private set; }

    [Header("Referencias UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI clueLabel;

    [Header("Config")]
    [Tooltip("Segundos que se muestra la pista antes de ocultarse.")]
    [SerializeField] private float displayTime = 4f;

    private Coroutine hideRoutine;

    private void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    public void ShowClue(string text)
    {
        if (panel != null) panel.SetActive(true);
        if (clueLabel != null) clueLabel.text = text;

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        if (panel != null) panel.SetActive(false);
    }
}

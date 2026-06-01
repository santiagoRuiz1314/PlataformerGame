using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema de vidas del jugador.
/// Lleva la cuenta de vidas, aplica el daño, hace el respawn y avisa
/// (por eventos) al HUD cuando cambian las vidas o cuando hay Game Over.
/// Colócalo en el GameObject del jugador (el que tiene New_Character).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerHealth : MonoBehaviour
{
    // Acceso global rápido para que el enemigo / HUD lo encuentren.
    public static PlayerHealth Instance { get; private set; }

    [Header("Vidas")]
    [Tooltip("Cantidad de vidas con la que empieza el jugador.")]
    [SerializeField] private int maxLives = 3;
    [Tooltip("Tope absoluto de vidas, contando las extra de power-up.")]
    [SerializeField] private int absoluteMaxLives = 5;

    [Header("Respawn")]
    [Tooltip("Punto al que vuelve el jugador al recibir un golpe (si aún le quedan vidas). Arrastra aquí el objeto 'Respawn'.")]
    [SerializeField] private Transform respawnPoint;

    [Tooltip("ACTIVO: al perder una vida, el jugador vuelve al punto de respawn.\nINACTIVO: pierde la vida pero NO se mueve de donde está.")]
    [SerializeField] private bool respawnOnDeath = true;

    [Header("Invulnerabilidad")]
    [Tooltip("Segundos de invulnerabilidad tras un golpe. Evita perder varias vidas de un solo toque seguido.")]
    [SerializeField] private float invulnerabilityTime = 1.5f;

    // --- Estado interno ---
    private int currentLives;
    private float invulnerableTimer;
    private CharacterController characterController;
    private bool isShielded;
    private Coroutine shieldRoutine;
    public bool IsShielded => isShielded;
    public event Action<bool> OnShieldChanged; // opcional, para HUD/VFX

    // --- Propiedades de solo lectura ---
    public int CurrentLives => currentLives;
    public int MaxLives => maxLives;
    public bool IsInvulnerable => invulnerableTimer > 0f;
    public bool IsDead => currentLives <= 0;

    // --- Eventos para el HUD y otros sistemas ---
    public event Action<int, int> OnLivesChanged; // (vidasActuales, vidasMax)
    public event Action OnDamaged;                 // recibió un golpe pero sobrevive
    public event Action OnGameOver;                // se quedó sin vidas

    private void Awake()
    {
        // Singleton simple: solo debe existir uno.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        characterController = GetComponent<CharacterController>();
        currentLives = maxLives;
    }

    private void Start()
    {
        // Avisamos al HUD del estado inicial (3/3, por ejemplo).
        OnLivesChanged?.Invoke(currentLives, maxLives);
    }

    private void Update()
    {
        if (invulnerableTimer > 0f)
            invulnerableTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Aplica un golpe al jugador: le quita UNA vida.
    /// Lo llama el enemigo (o cualquier cosa que deba dañar).
    /// Devuelve true si el golpe contó (no estaba invulnerable ni muerto).
    /// </summary>
    public bool TakeDamage()
    {
        if (IsInvulnerable || IsShielded || IsDead) return false;

        currentLives--;
        OnLivesChanged?.Invoke(currentLives, maxLives);

        Debug.Log($"[PlayerHealth] Golpe recibido. Vidas restantes: {currentLives}/{maxLives}");

        if (currentLives <= 0)
        {
            Die();
        }
        else
        {
            // Aún le quedan vidas: arranca la invulnerabilidad y (opcional) respawnea.
            invulnerableTimer = invulnerabilityTime;
            OnDamaged?.Invoke();

            if (respawnOnDeath)
                MoveToRespawn();
        }

        return true;

    }
    

    /// <summary>
    /// Se queda sin vidas: Game Over.
    /// </summary>
    private void Die()
    {
        Debug.Log("[PlayerHealth] ¡GAME OVER! El jugador se quedó sin vidas.");
        OnGameOver?.Invoke();

        // Aquí decides qué hacer al perder. Ejemplos (descomenta el que quieras):
        // -- Recargar la escena actual:
        // UnityEngine.SceneManagement.SceneManager.LoadScene(
        //     UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        //
        // -- Congelar el juego:
        // Time.timeScale = 0f;
    }

    /// <summary>
    /// Teletransporta al jugador al punto de respawn.
    /// Hay que desactivar el CharacterController para poder mover el transform,
    /// igual que hace tu script Respawn_New del Dead_Zone.
    /// </summary>
    private void MoveToRespawn()
    {
        if (respawnPoint == null)
        {
            Debug.LogWarning("[PlayerHealth] No hay 'respawnPoint' asignado en el Inspector.", this);
            return;
        }

        if (characterController != null) characterController.enabled = false;
        transform.position = respawnPoint.position;
        if (characterController != null) characterController.enabled = true;

        Debug.Log("[PlayerHealth] Jugador devuelto al punto de respawn.");
    }
    /// <summary>Suma vidas (power-up de vida extra). Respeta el tope absoluto.</summary>
    public bool AddLife(int amount = 1)
    {
        if (IsDead) return false;
        if (currentLives >= absoluteMaxLives) return false;

        currentLives = Mathf.Min(currentLives + amount, absoluteMaxLives);
        OnLivesChanged?.Invoke(currentLives, maxLives);
        Debug.Log($"[PlayerHealth] +{amount} vida. Vidas: {currentLives}");
        return true;
    }

    /// <summary>Activa el escudo (invulnerabilidad temporal) por 'duration' segundos.</summary>
    public void ActivateShield(float duration)
    {
        if (shieldRoutine != null) StopCoroutine(shieldRoutine);
        shieldRoutine = StartCoroutine(ShieldRoutine(duration));
    }

    private IEnumerator ShieldRoutine(float duration)
    {
        isShielded = true;
        OnShieldChanged?.Invoke(true);

        yield return new WaitForSeconds(duration);

        isShielded = false;
        OnShieldChanged?.Invoke(false);
        shieldRoutine = null;
    }
}

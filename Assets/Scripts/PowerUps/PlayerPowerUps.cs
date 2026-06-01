using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Punto de entrada único de los power-ups. Va en el GameObject del jugador
/// (el mismo que tiene New_Character y PlayerHealth).
///
/// Los objetos PowerUpPickup le avisan qué activar, igual que los
/// CollectibleItem le avisan al PuzzleManager.
///
/// - Velocidad: multiplica la velocidad durante 'speedDuration' segundos.
/// - Vida extra: suma una vida (dura hasta que el jugador la pierda).
/// - Escudo: vuelve al jugador inmune durante 'shieldDuration' segundos.
/// </summary>
[RequireComponent(typeof(New_Character))]
public class PlayerPowerUps : MonoBehaviour
{
    public static PlayerPowerUps Instance { get; private set; }

    [Header("Velocidad")]
    [Tooltip("Cuánto se multiplica la velocidad. 1.6 = +60%.")]
    [SerializeField] private float speedMultiplier = 1.6f;
    [Tooltip("Duración del power-up de velocidad, en segundos.")]
    [SerializeField] private float speedDuration = 5f;

    [Header("Escudo")]
    [Tooltip("Duración del escudo (invulnerabilidad), en segundos.")]
    [SerializeField] private float shieldDuration = 10f;

    // --- Eventos opcionales para HUD / VFX (un icono, una barra de tiempo, etc.) ---
    public event Action<float> OnSpeedStarted; // pasa la duración
    public event Action OnSpeedEnded;

    private New_Character character;
    private Coroutine speedRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        character = GetComponent<New_Character>();
    }

    /// <summary>
    /// Lo llama PowerUpPickup según el tipo recogido.
    /// </summary>
    public void Activate(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Velocidad: GiveSpeed(); break;
            case PowerUpType.VidaExtra: GiveExtraLife(); break;
            case PowerUpType.Escudo:    GiveShield(); break;
        }
    }

    // ---------- VELOCIDAD (dura speedDuration segundos) ----------
    public void GiveSpeed()
    {
        // Si ya había uno activo, reinicia el cronómetro (no acumula).
        if (speedRoutine != null) StopCoroutine(speedRoutine);
        speedRoutine = StartCoroutine(SpeedRoutine());
    }

    private IEnumerator SpeedRoutine()
    {
        character.SpeedMultiplier = speedMultiplier;
        OnSpeedStarted?.Invoke(speedDuration);
        Debug.Log($"[PowerUp] Velocidad x{speedMultiplier} durante {speedDuration}s.");

        yield return new WaitForSeconds(speedDuration);

        character.SpeedMultiplier = 1f;
        OnSpeedEnded?.Invoke();
        speedRoutine = null;
        Debug.Log("[PowerUp] Velocidad terminada.");
    }

    // ---------- VIDA EXTRA (+1 vida, dura hasta perderla) ----------
    public void GiveExtraLife()
    {
        if (PlayerHealth.Instance == null) return;

        bool added = PlayerHealth.Instance.AddLife();
        Debug.Log(added
            ? "[PowerUp] +1 vida extra."
            : "[PowerUp] Ya estás en el máximo de vidas, no se añadió.");
    }

    // ---------- ESCUDO (invulnerable shieldDuration segundos) ----------
    public void GiveShield()
    {
        if (PlayerHealth.Instance == null) return;

        PlayerHealth.Instance.ActivateShield(shieldDuration);
        Debug.Log($"[PowerUp] Escudo activado por {shieldDuration}s.");
    }
}

using UnityEngine;

public class Respawn_New : MonoBehaviour
{
    [Header("Referencia al punto de reapawn")]
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && respawnPoint != null)
        {
            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller != null)
            {
                controller.enabled = false;
                other.transform.position = respawnPoint.position;
                controller.enabled = true;

                Debug.Log("Jugador ha sido respawneado");
            }
            else
            {
                other.transform.position = respawnPoint.position;
                Debug.Log("Jugador sin characterController fue movido al respawn");
            }
        }
    }
}
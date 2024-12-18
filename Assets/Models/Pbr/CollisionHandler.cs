using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    public Vector3 respawnPosition; // Posición inicial a la que volverá el jugador
    private ButtonHandler buttonHandler; // Referencia al script principal para reiniciar el movimiento

    void Start()
    {
        // Busca el ButtonHandler en la escena
        buttonHandler = FindObjectOfType<ButtonHandler>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        // Detecta si la colisión es con un obstáculo
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Colisión detectada con un obstáculo. Volviendo a la posición inicial.");

            // Reinicia la posición y el estado desde el ButtonHandler
            transform.position = respawnPosition;
            buttonHandler.ResetPlayer();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClickHandler : MonoBehaviour
{
    private Vector3 originalScale;       // Tamaño original del objeto
    private Vector3 targetScale;         // Tamaño máximo permitido
    private bool isMaxSize = false;      // Flag para controlar si está en el tamaño máximo

    [SerializeField]
    private float scaleMultiplier = 2f;  // Factor de escala al crecer
    [SerializeField]
    private float maxScaleFactor = 3f;   // Tamaño máximo relativo al original

    void Update()
    {
        // Verificar clic de ratón
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckRaycast(Mouse.current.position.ReadValue());
        }

        // Verificar toque en pantalla
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            var touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            CheckRaycast(touchPosition);
        }
    }

    void Start()
    {
        // Guardamos el tamaño original y calculamos el tamaño máximo
        originalScale = transform.localScale;
        targetScale = originalScale * maxScaleFactor;
    }

    private void CheckRaycast(Vector2 inputPosition)
    {
        // Convierte la posición de entrada en un rayo desde la cámara
        Ray ray = Camera.main.ScreenPointToRay(inputPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                if (!isMaxSize)
                {
                    GrowObstacle();
                }
                else
                {
                    ResetObstacle();
                }
            }
        }
    }

    private void GrowObstacle()
    {
        // Aumenta el tamaño del obstáculo
        transform.localScale = Vector3.Min(transform.localScale * scaleMultiplier, targetScale);
        Debug.Log($"Obstáculo aumentado. Tamaño actual: {transform.localScale}");

        // Verifica si alcanzó el tamaño máximo
        if (transform.localScale == targetScale)
        {
            isMaxSize = true;
        }
    }

    private void ResetObstacle()
    {
        // Restaura el tamaño original
        transform.localScale = originalScale;
        Debug.Log("Obstáculo restaurado a su tamaño original.");
        isMaxSize = false;
    }
}

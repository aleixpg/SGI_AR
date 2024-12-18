using UnityEngine;

public class ObjectScaler : MonoBehaviour
{
    private float initialRotationY;
    private Vector3 initialScale;

    void Start()
    {
        // Guarda la escala inicial del objeto
        initialScale = transform.localScale;
    }

    public void AdjustScale(float currentRotationY)
    {
        // Calcula la diferencia de rotación en el eje Y
        float rotationDifference = currentRotationY - initialRotationY;

        // Define un factor de escala basado en la rotación
        float scaleFactor = Mathf.Clamp(1 + (rotationDifference / 360f), 0.5f, 2.0f);

        // Ajusta la escala del objeto
        transform.localScale = initialScale * scaleFactor;
    }

    public void UpdateInitialRotation(float rotationY)
    {
        initialRotationY = rotationY;
    }
}

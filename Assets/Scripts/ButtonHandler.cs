using UnityEngine;
using UnityEngine.UI;
using TMPro; // Asegúrate de incluir esta librería

public class ButtonHandler : MonoBehaviour
{
    public Button startButton; // Botón de Start/Restart
    public Button jumpButton; // Botón de Jump
    public Scrollbar speedScrollbar; // Scrollbar para ajustar la velocidad
    public GameObject cubePrefab; // Prefab del cubo que será instanciado

    private ImageTracker imageTracker; // Referencia al ImageTracker
    private GameObject movingCube; // Referencia al cubo que se moverá
    private bool isMoving = false; // Estado para rastrear si el cubo se está moviendo
    private bool isJumping = false; // Estado para rastrear si el cubo está saltando
    private float movementSpeed = 0.2f; // Velocidad inicial del movimiento del cubo
    private float t = 0f; // Variable para interpolar a lo largo de la curva
    private Vector3 startPoint; // Punto de inicio del cubo
    private float jumpHeight = 0.04f; // Altura máxima del salto
    private float jumpDuration = 0.8f; // Duración del salto
    private float jumpTimer = 0f; // Temporizador para el salto
    private Vector3 initialJumpPosition; // Posición inicial del salto
  void Start()
    {
        // Encuentra el ImageTracker en la escena
        imageTracker = FindObjectOfType<ImageTracker>();

        // Inicialmente deshabilita los botones
        startButton.interactable = false;
        jumpButton.interactable = false;

        // Configura el texto inicial del botón
        startButton.GetComponentInChildren<TMP_Text>().text = "Start";

        // Configura el valor inicial del scrollbar
        if (speedScrollbar != null)
        {
            speedScrollbar.value = NormalizeSpeed(movementSpeed); // Normaliza la velocidad para el rango del scrollbar
        }
    }

    void Update()
    {
        // Verifica si el botón Start debe habilitarse
        if (imageTracker != null && !startButton.interactable)
        {
            if (imageTracker.HasStart && imageTracker.HasFinish)
            {
                startButton.interactable = true;

                // Spawnea el cubo en el punto Tracking-Start
                SpawnCubeAtTrackingStart();
            }
        }

        if (speedScrollbar != null)
        {
            movementSpeed = Mathf.Lerp(0.2f, 1f, speedScrollbar.value);
        }

        UpdateOrientation();

        // Mueve el cubo si está en movimiento
        if (isMoving && movingCube != null && !isJumping)
        {
            MoveCubeAlongCurve();
        }

        // Maneja el salto
        if (isJumping && movingCube != null)
        {
            HandleJump();
        }
    }

    public void OnStartButtonPressed()
    {
        string currentText = startButton.GetComponentInChildren<TMP_Text>().text;

        if (currentText == "Start")
        {
            Debug.Log("Start button clicked!");
            jumpButton.interactable = true; // Habilita el botón de salto
            isMoving = true; // Inicia el movimiento
        }
        else if (currentText == "Restart")
        {
            Debug.Log("Restart button clicked!");
            TeleportCubeToStart(); // Teletransporta el cubo al punto de inicio
            startButton.GetComponentInChildren<TMP_Text>().text = "Start"; // Cambia el texto del botón
        }
    }

    public void OnJumpButtonPressed()
    {
        if (!isJumping && movingCube != null)
        {
            Debug.Log("Jump button clicked!");
            isJumping = true;
            jumpTimer = 0f;
            initialJumpPosition = movingCube.transform.position;
        }
    }

    private void SpawnCubeAtTrackingStart()
    {
        if (imageTracker != null && imageTracker.HasStart)
        {
            // Obtén la posición del punto Tracking-Start desde el ImageTracker
            startPoint = imageTracker.GetStartPoint();

            // Calcula la dirección inicial de la curva
            var controlPoints = imageTracker.GetControlPoints();
            float initialDeltaT = 0.01f; // Pequeño incremento para calcular la dirección inicial
            Vector3 nextPosition = PolynomialCurve.CalculatePoint(initialDeltaT, controlPoints);
            Vector3 initialDirection = (nextPosition - startPoint).normalized;

            // Instancia el personaje
            movingCube = Instantiate(cubePrefab, startPoint, Quaternion.LookRotation(initialDirection, Vector3.up));

            // Añadir un BoxCollider si no existe
            if (movingCube.GetComponent<Collider>() == null)
            {
                movingCube.AddComponent<BoxCollider>();
            }

            // Añadir un Rigidbody si no existe
            if (movingCube.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = movingCube.AddComponent<Rigidbody>();
                rb.isKinematic = false; // Asegúrate de que no sea cinemático para detectar colisiones físicas
                rb.useGravity = false;  // Si no quieres gravedad
            }

            // Debug
            Debug.Log("Personaje instanciado con Rigidbody y Collider.");
        }
    }

    public void ResetPlayer()
    {
        if (movingCube != null)
        {
            // Reinicia la posición y el estado
            movingCube.transform.position = startPoint;
            t = 0f; // Reinicia el parámetro t
            isMoving = false;
            jumpButton.interactable = false;
            Debug.Log("Jugador reiniciado tras colisión.");
        }
    }
    

    private void TeleportCubeToStart()
    {
        if (movingCube != null)
        {
            // Teletransporta el cubo al punto de inicio
            movingCube.transform.position = startPoint;

            // Reinicia el parámetro t
            t = 0f;

            Debug.Log("Cubo teletransportado al punto de inicio");
        }
    }

    private void UpdateOrientation()
    {
        if (movingCube != null && imageTracker != null)
        {
            var controlPoints = imageTracker.GetControlPoints();
            Vector3 currentPosition = PolynomialCurve.CalculatePoint(t, controlPoints);

            // Calcula una posición ligeramente adelantada en la curva (usando un delta pequeño en t)
            float deltaT = 0.01f;
            float nextT = Mathf.Clamp(t + deltaT, 0f, 1f);
            Vector3 nextPosition = PolynomialCurve.CalculatePoint(nextT, controlPoints);

            // Calcula la dirección del movimiento
            Vector3 direction = (nextPosition - currentPosition).normalized;

            // Orienta el personaje hacia la dirección del movimiento
            if (movingCube != null) // movingCube es ahora tu personaje
            {
                movingCube.transform.position = currentPosition;
                movingCube.transform.forward = direction; // Asegura que el personaje apunte hacia adelante en la curva
            }
        }
    }
    private void MoveCubeAlongCurve()
    {
        // Obtiene los puntos de control de la curva desde el ImageTracker
        var controlPoints = imageTracker.GetControlPoints();

        // Incrementa el parámetro t basado en la velocidad
        t += movementSpeed * Time.deltaTime / controlPoints.Count;

        // Si se alcanza el final de la curva, detén el movimiento
        if (t >= 1f)
        {
            t = 1f;
            isMoving = false; // Detiene el movimiento
            startButton.GetComponentInChildren<TMP_Text>().text = "Restart"; // Cambia el texto del botón
            Debug.Log("El personaje ha alcanzado el final de la curva");
            return;
        }

        // Calcula la posición actual en la curva
        Vector3 currentPosition = PolynomialCurve.CalculatePoint(t, controlPoints);

        // Calcula una posición ligeramente adelantada en la curva (usando un delta pequeño en t)
        float deltaT = 0.01f;
        float nextT = Mathf.Clamp(t + deltaT, 0f, 1f);
        Vector3 nextPosition = PolynomialCurve.CalculatePoint(nextT, controlPoints);

        // Calcula la dirección del movimiento
        Vector3 direction = (nextPosition - currentPosition).normalized;

        // Orienta el personaje hacia la dirección del movimiento
        if (movingCube != null) // movingCube es ahora tu personaje
        {
            movingCube.transform.position = currentPosition;
            movingCube.transform.forward = direction; // Asegura que el personaje apunte hacia adelante en la curva
        }
    }

    private void HandleJump()
    {
        jumpTimer += Time.deltaTime;
        float normalizedTime = jumpTimer / jumpDuration;

        if (normalizedTime >= 1f)
        {
            // Finaliza el salto y regresa al estado normal
            movingCube.transform.position = new Vector3(movingCube.transform.position.x, initialJumpPosition.y, movingCube.transform.position.z);
            isJumping = false;
            Debug.Log("El cubo ha completado el salto");
        }
        else
        {
            // Calcula una trayectoria parabólica y considera el movimiento horizontal si está en movimiento
            float height = 4 * jumpHeight * normalizedTime * (1 - normalizedTime); // Parábola

            if (isMoving)
            {
                // Mantén el movimiento horizontal durante el salto
                MoveCubeAlongCurve();
            }

            Vector3 jumpPosition = new Vector3(movingCube.transform.position.x, initialJumpPosition.y + height, movingCube.transform.position.z);
            movingCube.transform.position = jumpPosition;
        }
    }

    // Método para ajustar la velocidad desde el Scrollbar
    public void AdjustSpeedFromScrollbar()
    {
        if (speedScrollbar != null)
        {
            // Escala el valor del scrollbar al rango de velocidades (0.2f a 1f)
            movementSpeed = Mathf.Lerp(0.2f, 1f, speedScrollbar.value);
            Debug.Log($"Velocidad ajustada a: {movementSpeed}");
        }
    }

    // Normaliza la velocidad para inicializar el scrollbar (de 0.2f-1f a 0-1)
    private float NormalizeSpeed(float speed)
    {
        return (speed - 0.2f) / (1f - 0.2f);
    }
}

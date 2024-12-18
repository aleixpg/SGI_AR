using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonHandler : MonoBehaviour
{
    public Button startButton;
    public Button jumpButton;
    public GameObject cubePrefab;
    public TextMeshProUGUI scoreText; // Referencia al texto del puntaje

    private ImageTracker imageTracker;
    private GameObject movingCube;
    private bool isMoving = false;
    private bool isJumping = false;
    private float movementSpeed = 0.2f;
    private float t = 0f;
    private Vector3 startPoint;
    private float jumpHeight = 0.04f;
    private float jumpDuration = 0.5f;
    private float jumpTimer = 0f;
    private Vector3 initialJumpPosition;

    private int score = 0; // Puntaje del jugador
    private float speedIncrement = 0.1f; // Incremento de velocidad por punto
    void Start()
    {
        imageTracker = FindObjectOfType<ImageTracker>();
        startButton.interactable = false;
        jumpButton.interactable = false;

        startButton.GetComponentInChildren<TMP_Text>().text = "Start";

        // Inicializa el texto del puntaje
        UpdateScoreText();
    }

    void Update()
    {
        if (imageTracker != null && !startButton.interactable)
        {
            if (imageTracker.HasStart && imageTracker.HasFinish)
            {
                startButton.interactable = true;
                SpawnCubeAtTrackingStart();
            }
        }

        if (isMoving && movingCube != null && !isJumping)
        {
            MoveCubeAlongCurve();
        }

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
            TeleportCubeToStart();
            startButton.GetComponentInChildren<TMP_Text>().text = "Start";
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

            movementSpeed = 0.2f; // Reinicia la velocidad
            score = 0; // Reinicia el puntaje
            UpdateScoreText();

            Debug.Log("Jugador reiniciado tras colisión.");
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
        var controlPoints = imageTracker.GetControlPoints();
        t += movementSpeed * Time.deltaTime / controlPoints.Count;

        if (t >= 1f)
        {
            t = 1f;
            isMoving = false;
            startButton.GetComponentInChildren<TMP_Text>().text = "Restart";
            Debug.Log("El personaje ha alcanzado el final de la curva");

            // Incrementa el puntaje y actualiza el texto
            score++;
            movementSpeed += speedIncrement;
            Debug.Log($"Puntaje: {score}, Velocidad: {movementSpeed}");
            UpdateScoreText();

            return;
        }

        Vector3 currentPosition = PolynomialCurve.CalculatePoint(t, controlPoints);
        float deltaT = 0.01f;
        float nextT = Mathf.Clamp(t + deltaT, 0f, 1f);
        Vector3 nextPosition = PolynomialCurve.CalculatePoint(nextT, controlPoints);
        Vector3 direction = (nextPosition - currentPosition).normalized;

        if (movingCube != null)
        {
            movingCube.transform.position = currentPosition;
            movingCube.transform.forward = direction;
        }
    }

    private void HandleJump()
    {
        jumpTimer += Time.deltaTime;
        float normalizedTime = jumpTimer / jumpDuration;

        if (normalizedTime >= 1f)
        {
            movingCube.transform.position = new Vector3(movingCube.transform.position.x, initialJumpPosition.y, movingCube.transform.position.z);
            isJumping = false;
            Debug.Log("El cubo ha completado el salto");
        }
        else
        {
            float height = 4 * jumpHeight * normalizedTime * (1 - normalizedTime); // Parábola

            if (isMoving)
            {
                MoveCubeAlongCurve();
            }

            Vector3 jumpPosition = new Vector3(movingCube.transform.position.x, initialJumpPosition.y + height, movingCube.transform.position.z);
            movingCube.transform.position = jumpPosition;
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

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Puntaje: {score} \nVelocidad: {NormalizeSpeed(movementSpeed)*20} km/h";
        }
    }

    private float NormalizeSpeed(float speed)
    {
        return (speed - 0.2f) / (1f - 0.2f);
    }
}

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
            isMoving = true;
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

    private void SpawnCubeAtTrackingStart()
    {
        if (imageTracker != null && imageTracker.HasStart)
        {
            startPoint = imageTracker.GetStartPoint();
            var controlPoints = imageTracker.GetControlPoints();
            float initialDeltaT = 0.01f;
            Vector3 nextPosition = PolynomialCurve.CalculatePoint(initialDeltaT, controlPoints);
            Vector3 initialDirection = (nextPosition - startPoint).normalized;

            movingCube = Instantiate(cubePrefab, startPoint, Quaternion.LookRotation(initialDirection, Vector3.up));
            Debug.Log("Personaje spawneado en Tracking-Start y orientado hacia la curva");
            jumpButton.interactable = true;
        }
    }

    private void TeleportCubeToStart()
    {
        if (movingCube != null)
        {
            movingCube.transform.position = startPoint;
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

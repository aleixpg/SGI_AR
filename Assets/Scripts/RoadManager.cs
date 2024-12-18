using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Gestor responsable de la generación y administración de segmentos de carretera.
/// Utiliza puntos de control (inicio, obstáculos y fin) para construir una carretera curva
/// representada por múltiples segmentos conectados.
/// </summary>
/// <remarks>
/// Este gestor calcula posiciones y orientaciones de los segmentos en base a una curva polinómica
/// generada por puntos de control, y actualiza la carretera dinámicamente según las imágenes rastreadas.
/// </remarks>
/// <author>TuNombre</author>
/// <date>FechaActual</date>
public class RoadManager
{
    // Prefab del segmento de carretera
    private GameObject roadPrefab;

    // Lista que almacena los segmentos de carretera instanciados
    private List<GameObject> roadSegments = new List<GameObject>();

    // Longitud de un segmento de carretera
    private float roadSegmentLength;

    // Puntos de control principales: inicio, fin y obstáculos
    private Vector3 start;
    private Vector3 finish;
    private List<Vector3> obstaclePositions = new List<Vector3>();

    /// <summary>
    /// Constructor que inicializa el gestor con el prefab de carretera.
    /// </summary>
    /// <param name="roadPrefab">Prefab que se usará para generar los segmentos de carretera.</param>
    public RoadManager(GameObject roadPrefab)
    {
        this.roadPrefab = roadPrefab;
        Initialize();
    }

    /// <summary>
    /// Inicializa el gestor calculando la longitud del segmento de carretera.
    /// </summary>
    public void Initialize()
    {
        if (roadPrefab != null)
        {
            var renderer = roadPrefab.GetComponent<Renderer>();
            // Calcula la longitud en base al tamaño del prefab; usa un valor por defecto si no tiene Renderer
            roadSegmentLength = renderer != null ? renderer.bounds.size.z : 1.0f;
        }
    }

    /// <summary>
    /// Actualiza los puntos de control (inicio, fin, obstáculos) en base a las imágenes rastreadas.
    /// </summary>
    /// <param name="trackedImage">La imagen rastreada para actualizar los puntos de control.</param>
    public void UpdateTrackingPoints(ARTrackedImage trackedImage)
    {
        // Configura los puntos de inicio y fin según los nombres de las imágenes rastreadas
        if (trackedImage.referenceImage.name == "Tracking-Start")
        {
            start = trackedImage.transform.position;
        }
        else if (trackedImage.referenceImage.name == "Tracking-Finish")
        {
            finish = trackedImage.transform.position;
        }
        // Agrega obstáculos a la lista si no están ya registrados
        else if (trackedImage.referenceImage.name.StartsWith("Tracking-Obstacle"))
        {
            if (!obstaclePositions.Contains(trackedImage.transform.position))
            {
                obstaclePositions.Add(trackedImage.transform.position);
            }
        }
    }

    /// <summary>
    /// Obtiene la lista de puntos de control para la generación de la carretera.
    /// </summary>
    /// <returns>Una lista de puntos de control ordenados.</returns>
    public List<Vector3> GetControlPoints()
    {
        List<Vector3> controlPoints = new List<Vector3> { start };
        controlPoints.AddRange(obstaclePositions); // Añade los obstáculos
        controlPoints.Add(finish); // Añade el punto final
        return controlPoints;
    }

    /// <summary>
    /// Genera o actualiza los segmentos de carretera en base a los puntos de control.
    /// </summary>
    public void UpdateRoadSegments()
    {
        // Verifica que tanto el punto de inicio como el de fin están configurados
        if (start == Vector3.zero || finish == Vector3.zero) return;

        // Limpia los segmentos existentes
        ClearSegments();

        // Obtén los puntos de control para generar la curva
        List<Vector3> controlPoints = GetControlPoints();

        // Calcula el número de segmentos necesarios, con mayor resolución para suavidad
        int totalSegments = Mathf.CeilToInt(Vector3.Distance(start, finish) / roadSegmentLength * 2);

        for (int i = 0; i <= totalSegments; i++)
        {
            // Calcula la posición actual en la curva para este segmento
            float t = (float)i / totalSegments;
            Vector3 roadPosition = PolynomialCurve.CalculatePoint(t, controlPoints);

            // Calcula la posición del siguiente punto en la curva
            Vector3 nextPosition = (i < totalSegments)
                ? PolynomialCurve.CalculatePoint((float)(i + 1) / totalSegments, controlPoints)
                : finish;

            // Calcula la dirección hacia el siguiente segmento
            Vector3 directionToNext = (nextPosition - roadPosition).normalized;
            if (directionToNext == Vector3.zero) directionToNext = Vector3.forward; // Prevención de división por cero

            // Instancia un nuevo segmento de carretera
            GameObject segment = Object.Instantiate(roadPrefab, roadPosition, Quaternion.LookRotation(directionToNext));

            // Ajusta la escala de los segmentos para evitar espacios vacíos
            if (i == totalSegments)
            {
                // Para el último segmento, ajusta la longitud para conectar exactamente con el punto final
                float lastSegmentLength = Vector3.Distance(roadPosition, finish);
                Vector3 localScale = segment.transform.localScale;
                segment.transform.localScale = new Vector3(localScale.x, localScale.y, lastSegmentLength);
            }
            else
            {
                // Para otros segmentos, añade un poco de solapamiento para asegurar continuidad
                segment.transform.localScale = new Vector3(
                    segment.transform.localScale.x,
                    segment.transform.localScale.y,
                    segment.transform.localScale.z * 2.1f // Factor de solapamiento
                );
            }

            // Agrega el segmento a la lista de segmentos
            roadSegments.Add(segment);
        }
    }

    /// <summary>
    /// Limpia todos los segmentos de carretera generados previamente.
    /// </summary>
    private void ClearSegments()
    {
        foreach (var segment in roadSegments)
        {
            Object.Destroy(segment); // Destruye cada segmento
        }
        roadSegments.Clear(); // Vacía la lista de segmentos
    }
}

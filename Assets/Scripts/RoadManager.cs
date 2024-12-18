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

    //Pool para optimizar la creación de objetos
    private ObjectPool roadSegmentPool;

    /// <summary>
    /// Constructor que inicializa el gestor con el prefab de carretera.
    /// </summary>
    /// <param name="roadPrefab">Prefab que se usará para generar los segmentos de carretera.</param>
    public RoadManager(GameObject roadPrefab, int poolSize = 10)
    {
        this.roadPrefab = roadPrefab;

        // Crear el pool de segmentos
        var poolObject = new GameObject("RoadSegmentPool");
        roadSegmentPool = poolObject.AddComponent<ObjectPool>();

        // Inicializa el pool con el prefab y el tamaño inicial
        roadSegmentPool.Initialize(roadPrefab, poolSize);
    }

    /// <summary>
    /// Inicializa el gestor calculando la longitud del segmento de carretera.
    /// </summary>
    public void Initialize()
    {
        if (roadPrefab == null)
        {
            Debug.LogError("Road prefab is not assigned in RoadManager. Please assign a valid prefab.");
            roadSegmentLength = 1.0f; // Valor por defecto para evitar errores
            return;
        }

        var renderer = roadPrefab.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning("Road prefab is missing a Renderer component. Using default segment length.");
            roadSegmentLength = 1.0f; // Valor por defecto
        }
        else
        {
            roadSegmentLength = renderer.bounds.size.z;
            Debug.Log($"Road Segment Length: {roadSegmentLength}");
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
            Debug.Log($"Start Point Set: {start}");
        }
        else if (trackedImage.referenceImage.name == "Tracking-Finish")
        {
            finish = trackedImage.transform.position;
            Debug.Log($"Finish Point Set: {finish}");
        }
        // Agrega obstáculos a la lista si no están ya registrados
        else if (trackedImage.referenceImage.name.StartsWith("Tracking-Obstacle"))
        {
            if (!obstaclePositions.Contains(trackedImage.transform.position))
            {
                obstaclePositions.Add(trackedImage.transform.position);
                Debug.Log($"Obstacle Added: {trackedImage.transform.position}");
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
    if (start == Vector3.zero || finish == Vector3.zero)
    {
        Debug.LogWarning("Start or Finish points are not set. Cannot generate road segments.");
        return;
    }

    if (roadSegmentLength <= 0)
    {
        Debug.LogError("Road Segment Length is invalid. Cannot generate road segments.");
        return;
    }

    // Calcula la distancia total y divide la carretera en segmentos
    float distance = Vector3.Distance(start, finish);
    int totalSegments = Mathf.CeilToInt(distance / roadSegmentLength);

    Debug.Log($"Generating {totalSegments} road segments...");

    // Limpia los segmentos existentes
    ClearSegments();

    // Obtén los puntos de control para generar la curva
    List<Vector3> controlPoints = GetControlPoints();

    Vector3 previousDirection = Vector3.zero; // Dirección del segmento previo

    for (int i = 0; i < totalSegments; i++)
    {
        // Calcula los puntos actuales y siguientes en la curva
        float t = (float)i / totalSegments;
        float nextT = (float)(i + 1) / totalSegments;

        Vector3 roadPosition = PolynomialCurve.CalculatePoint(t, controlPoints);
        Vector3 nextPosition = PolynomialCurve.CalculatePoint(nextT, controlPoints);

        // Calcula la dirección hacia el siguiente punto
        Vector3 directionToNext = (nextPosition - roadPosition).normalized;
        if (directionToNext == Vector3.zero)
        {
            directionToNext = Vector3.forward; // Prevención de errores
        }

        // Verifica si es el primer segmento
        bool isFirstSegment = i == 0;

        // Calcula el ángulo con el segmento previo para determinar la curvatura
        float angle = isFirstSegment ? 0 : Vector3.Angle(previousDirection, directionToNext);

        // Calcula la longitud de Z basada en el ángulo (0.001 en curvas pronunciadas a partir de 2.5 grados)
        float adjustedLength;
        if (angle > 2.5f)
        {
            adjustedLength = Mathf.Lerp(0.0016f, 0.005f, Mathf.Clamp01((5f - angle) / 2.5f)); // Progresivo entre 0.001 y 0.005
        }
        else
        {
            adjustedLength = Mathf.Lerp(0.005f, 0.01f, Mathf.Clamp01(1 - (angle / 2.5f))); // Progresivo entre 0.005 y 0.01
        }

        // Obtén un segmento del pool
        GameObject segment = roadSegmentPool.GetObject();
        segment.transform.position = roadPosition;
        segment.transform.rotation = Quaternion.LookRotation(directionToNext);

        // Ajusta la escala para el segmento
        Vector3 localScale = segment.transform.localScale;
        segment.transform.localScale = new Vector3(localScale.x, localScale.y, adjustedLength);

        // Desplaza ligeramente hacia el exterior en curvas pronunciadas
        if (angle > 2.5f) // Curvas más pronunciadas
        {
            Vector3 outwardDirection = Vector3.Cross(directionToNext, Vector3.up).normalized;
            segment.transform.position += outwardDirection * (roadSegmentLength * 0.1f);
        }

        // Agrega el segmento a la lista
        roadSegments.Add(segment);

        Debug.Log($"Segment {i} spawned at {roadPosition} with length {adjustedLength} and angle {angle}");

        // Actualiza la dirección previa
        previousDirection = directionToNext;
    }
}

    /// <summary>
    /// Limpia todos los segmentos de carretera generados previamente.
    /// </summary>
    private void ClearSegments()
    {
        foreach (var segment in roadSegments)
        {
            roadSegmentPool.ReturnObject(segment); // Devuelve el segmento al pool
        }
        roadSegments.Clear(); // Vacía la lista de segmentos
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Gestor para los objetos instanciados en la escena.
/// Controla la orientación de los objetos en base a las posiciones de las imágenes rastreadas y su alineación con una curva generada.
/// </summary>
/// <remarks>
/// Esta clase utiliza un diccionario para mapear objetos instanciados a las imágenes rastreadas.
/// Es compatible con curvas polinómicas definidas por puntos de control.
/// </remarks>
/// <author>TuNombre</author>
/// <date>FechaActual</date>
public class TrackedObjectManager
{
    // Diccionario que almacena los objetos instanciados asociados a las imágenes rastreadas
    private Dictionary<string, GameObject> instantiatedObjects;

    /// <summary>
    /// Constructor que inicializa el diccionario de objetos instanciados.
    /// </summary>
    /// <param name="objects">Diccionario de objetos instanciados, mapeados por el nombre de las imágenes rastreadas.</param>
    public TrackedObjectManager(Dictionary<string, GameObject> objects)
    {
        instantiatedObjects = objects;
    }

    /// <summary>
    /// Actualiza la orientación de los objetos instanciados en función de una curva.
    /// </summary>
    /// <param name="trackedImages">Las imágenes rastreadas por ARFoundation.</param>
    /// <param name="controlPoints">Los puntos de control que definen la curva.</param>
    public void UpdateTrackedObjectsOrientation(IEnumerable<ARTrackedImage> trackedImages, List<Vector3> controlPoints)
    {
        // Itera sobre las imágenes rastreadas
        foreach (var trackedImage in trackedImages)
        {
            // Verifica si existe un objeto instanciado para la imagen rastreada
            if (!instantiatedObjects.TryGetValue(trackedImage.referenceImage.name, out var instantiatedObject)) 
                continue;

            // Encuentra el punto más cercano en la curva al objeto rastreado
            float t = GetClosestTOnCurve(trackedImage.transform.position, controlPoints);

            // Calcula la tangente de la curva en el punto más cercano
            Vector3 tangent = PolynomialCurve.CalculateTangent(t, controlPoints);

            // Si la tangente no es un vector cero, ajusta la orientación del objeto
            if (!tangent.IsZero())
            {
                instantiatedObject.transform.rotation = Quaternion.LookRotation(tangent);
            }
        }
    }

    /// <summary>
    /// Encuentra el valor 't' en la curva más cercano a una posición dada.
    /// </summary>
    /// <param name="position">La posición objetivo en el espacio.</param>
    /// <param name="controlPoints">Lista de puntos de control de la curva.</param>
    /// <returns>El valor 't' más cercano en el rango [0, 1].</returns>
    private float GetClosestTOnCurve(Vector3 position, List<Vector3> controlPoints)
    {
        float closestT = 0f; // Almacena el valor t más cercano
        float minDistance = float.MaxValue; // Almacena la distancia mínima encontrada
        const int totalSegments = 100; // Número de segmentos en los que se divide la curva

        // Itera sobre los segmentos de la curva
        for (int i = 0; i <= totalSegments; i++)
        {
            // Calcula el valor 't' para el segmento actual
            float t = (float)i / totalSegments;

            // Calcula el punto correspondiente en la curva
            Vector3 curvePoint = PolynomialCurve.CalculatePoint(t, controlPoints);

            // Calcula la distancia entre la posición dada y el punto en la curva
            float distance = Vector3.Distance(position, curvePoint);

            // Si la distancia es menor que la distancia mínima encontrada, actualiza el valor más cercano
            if (distance < minDistance)
            {
                minDistance = distance;
                closestT = t;
            }
        }

        return closestT; // Devuelve el valor 't' más cercano
    }
}

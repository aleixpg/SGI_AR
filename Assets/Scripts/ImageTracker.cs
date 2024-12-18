using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ImageTracker : MonoBehaviour
{
    private ARTrackedImageManager trackedImages;
    public GameObject[] ArPrefabs;

    public GameObject roadPrefab;

    private RoadManager roadManager;
    private TrackedObjectManager trackedObjectManager;

    private Dictionary<string, GameObject> instantiatedObjects = new Dictionary<string, GameObject>();


    public bool HasStart { get; private set; } = false;
    public bool HasFinish { get; private set; } = false;
    private Vector3 start; // Almacena la posición del Tracking-Start

    public Vector3 GetStartPoint()
    {
        return start;
    }


    public List<Vector3> GetControlPoints()
    {
        return roadManager.GetControlPoints();
    }

    void Awake()
    {
        trackedImages = GetComponent<ARTrackedImageManager>();

        roadManager = new RoadManager(roadPrefab);
        trackedObjectManager = new TrackedObjectManager(instantiatedObjects);
    }

    void OnEnable()
    {
        trackedImages.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImages.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Procesa las imágenes recién añadidas
        foreach (var trackedImage in eventArgs.added)
        {
            HandleTrackedImage(trackedImage);
        }

        // Procesa las imágenes actualizadas
        foreach (var trackedImage in eventArgs.updated)
        {
            HandleTrackedImage(trackedImage);
        }

        // Actualiza los segmentos de carretera
        roadManager.UpdateRoadSegments();

        // Obtiene los puntos de control para la carretera
        List<Vector3> controlPoints = roadManager.GetControlPoints();

        // Convierte el TrackableCollection en una lista compatible con IEnumerable
        var trackedImageList = new List<ARTrackedImage>();
        foreach (var trackedImage in trackedImages.trackables)
        {
            trackedImageList.Add(trackedImage);
        }

        // Actualiza la orientación de los objetos instanciados basándose en la curva
        trackedObjectManager.UpdateTrackedObjectsOrientation(trackedImageList, controlPoints);
    }

    private void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        // Actualiza los puntos de control de la carretera según la imagen rastreada
        roadManager.UpdateTrackingPoints(trackedImage);

        // Verifica si las imágenes requeridas están presentes
        if (trackedImage.referenceImage.name == "Tracking-Start")
        {
            start = trackedImage.transform.position;
            HasStart = true;
        }
        else if (trackedImage.referenceImage.name == "Tracking-Finish")
        {
            HasFinish = true;
        }

        // Si no existe un objeto instanciado asociado a esta imagen, lo crea
        if (!instantiatedObjects.ContainsKey(trackedImage.referenceImage.name))
        {
            foreach (var prefab in ArPrefabs)
            {
                if (prefab.name == trackedImage.referenceImage.name)
                {
                    var instantiatedObject = Instantiate(prefab, trackedImage.transform);
                    instantiatedObjects[trackedImage.referenceImage.name] = instantiatedObject;
                }
            }
        }
    }
}

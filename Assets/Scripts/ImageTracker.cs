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

        if (roadPrefab != null)
        {
            roadManager = new RoadManager(roadPrefab, 40);
            roadManager.Initialize(); // Llama al método Initialize
        }
        else
        {
            Debug.LogError("Road prefab is not assigned in ImageTracker.");
        }

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

    // Verifica si tanto el inicio como el fin están configurados
    if (HasStart && HasFinish)
    {
        Debug.Log("Start and Finish points are set. Generating road segments...");

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
    else
    {
        Debug.LogWarning("Start or Finish points are missing. Skipping road generation.");
    }
}


    private void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        roadManager.UpdateTrackingPoints(trackedImage);

        Debug.Log($"Processing tracked image: {trackedImage.referenceImage.name}");

        if (trackedImage.referenceImage.name == "Tracking-Start")
        {
            start = trackedImage.transform.position;
            HasStart = true;
            Debug.Log($"Start detected at: {start}");
        }
        else if (trackedImage.referenceImage.name == "Tracking-Finish")
        {
            HasFinish = true;
            Debug.Log($"Finish detected.");
        }

        if (!instantiatedObjects.ContainsKey(trackedImage.referenceImage.name))
        {
            foreach (var prefab in ArPrefabs)
            {
                if (prefab.name == trackedImage.referenceImage.name)
                {
                    var instantiatedObject = Instantiate(prefab, trackedImage.transform);
                    instantiatedObjects[trackedImage.referenceImage.name] = instantiatedObject;
                    Debug.Log($"Instantiated object for: {trackedImage.referenceImage.name}");
                }
            }
        }
    }
}

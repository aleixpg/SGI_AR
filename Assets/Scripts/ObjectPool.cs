using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab; // Prefab que será instanciado

    private Queue<GameObject> pool = new Queue<GameObject>();

    /// <summary>
    /// Configura el prefab que será utilizado en el pool.
    /// </summary>
    public void Initialize(GameObject prefab, int initialCount)
    {
        this.prefab = prefab; // Asigna el prefab
        for (int i = 0; i < initialCount; i++)
        {
            var obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            var obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            var obj = Instantiate(prefab);
            obj.SetActive(true);
            return obj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
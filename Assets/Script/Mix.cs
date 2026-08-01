using UnityEngine;
using System.Collections.Generic;

public class Mix : MonoBehaviour
{
    public GameObject prefab;
    public Transform spawnPoint;

    public string[] requiredTags;

    public Vector3 spawnScale = new Vector3(5f, 5f, 5f);


    private void Update()
    {

        Collider[] hits = Physics.OverlapBox(
            transform.position,
            transform.localScale / 2,
            transform.rotation
        );

        Dictionary<string, GameObject> foundObjects =
            new Dictionary<string, GameObject>();

        foreach (Collider hit in hits)
        {
            foreach (string tag in requiredTags)
            {
                if (hit.CompareTag(tag) && !foundObjects.ContainsKey(tag))
                {
                    foundObjects.Add(tag, hit.gameObject);
                }
            }
        }

        if (foundObjects.Count == requiredTags.Length)
        {
            foreach (GameObject obj in foundObjects.Values)
            {
                Destroy(obj);
            }

            GameObject result = Instantiate(
                prefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            result.transform.localScale = spawnScale;

        }
    }
}
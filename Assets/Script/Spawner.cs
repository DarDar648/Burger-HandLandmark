using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    public Transform spawnPoint;
    public string objectTag = "Pickup";

    public Vector3 spawnScale = new Vector3(5f, 5f, 5f);

    private void Update()
    {
        bool found = false;

        Collider[] hits = Physics.OverlapBox(
            transform.position,
            transform.localScale / 2,
            transform.rotation
        );

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(objectTag))
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            GameObject obj = Instantiate(
                prefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            obj.transform.localScale = spawnScale;
        }
    }
}
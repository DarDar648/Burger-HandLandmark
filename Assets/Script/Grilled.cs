using UnityEngine;

public class Grilled : MonoBehaviour
{
    public GameObject prefab;
    public Transform spawnPoint;
    public string objectTag = "Pickup";

    public Vector3 spawnScale = new Vector3(5f, 5f, 5f);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(objectTag))
        {
            Destroy(other.gameObject);

            GameObject obj = Instantiate(
                prefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            obj.transform.localScale = spawnScale;
        }
    }
}
using UnityEngine;

public class MiddlePoint : MonoBehaviour
{
    public Transform cubeA;
    public Transform cubeB;
    public Transform Lock;
    public Transform Objek;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        Vector3 mid = Vector3.Lerp(cubeA.position, cubeB.position, 0.5f);
        mid.z = 70f;

        transform.position = mid;

        if (Picking.Status.Pick)
        {
            rend.material.color = Color.red;
            if (Objek == null)
            {
                Objek = Lock;
            }
        }
        else
        {
            rend.material.color = Color.blue;
            Objek = null;
        }
        if (Objek != null)
{
    Objek.position = new Vector3(
        transform.position.x,
        transform.position.y,
        50f
    );
}
    }

    void OnTriggerStay(Collider other)
    {
        if (Objek == null)
        {
            if (!other.CompareTag("Respawn"))
            {
                Objek = other.transform;
            }
        }
    }
}
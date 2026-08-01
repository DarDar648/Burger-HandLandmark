using UnityEngine;
using System.Collections;

public class Picking : MonoBehaviour
{
    public GameObject targetCube;

    private Renderer rend;
    private Coroutine resetCoroutine;

    public static class Status
    {
        public static bool Pick = false;
    }

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == targetCube)
        {
            if (resetCoroutine != null)
                StopCoroutine(resetCoroutine);

            rend.material.color = Color.red;
            Status.Pick = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == targetCube)
        {
            resetCoroutine = StartCoroutine(ReturnToNormal());
        }
    }

    IEnumerator ReturnToNormal()
    {
        yield return new WaitForSeconds(0.2f);
        rend.material.color = Color.white;
        Status.Pick = false;
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class projectileScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private playerMovement playerMovementScript;
    void Start()
    {
        playerMovementScript = GameObject.FindWithTag("Player").GetComponent<playerMovement>();
        Destroy(gameObject, 5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 hitPoint = contact.point;
        Vector3 hitNormal = contact.normal;

        playerMovementScript.placePortal(hitPoint, hitNormal);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject destroy_effect;

    private void OnCollisionEnter(Collision collision)
    {
        // if collision tag player, ignore
        if (collision != null && collision.gameObject != null && ( collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("bullet")))
        {
            return;
        }
        // log which collider tag we hit
        Debug.Log("Bomb hit object with tag: " + collision.gameObject.tag);
        Detonate();
    }

    private void Detonate()
    {
        if (destroy_effect != null)
        {
            Instantiate(destroy_effect, transform.position, transform.rotation);
        }
        // Stub: for now, just destroy
        Destroy(gameObject);
    }
}

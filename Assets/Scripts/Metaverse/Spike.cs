using UnityEngine;

public class Spike : MonoBehaviour
{
    public Transform respawnPoint;


    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            HealthSystem h = col.gameObject.GetComponent<HealthSystem>();
            h.respawnPoint = respawnPoint;
            h.TakeDamage();
        }
    }
}

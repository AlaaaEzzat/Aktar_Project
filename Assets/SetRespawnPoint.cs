using UnityEngine;

public class SetRespawnPoint : MonoBehaviour
{
    public Spike TargetTransformReplace;
    public Transform newRespawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(TargetTransformReplace != null)
            {
                TargetTransformReplace.respawnPoint = newRespawnPoint;
            }
            collision.GetComponent<HealthSystem>().respawnPoint = newRespawnPoint;
        }
    }

}

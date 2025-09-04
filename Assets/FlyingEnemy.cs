using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [SerializeField] private Transform SpawanPoint;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private Vector3 moveDirection;
    [SerializeField] private float moveSpeed;

    void Start()
    {
        transform.position = startPoint.position;
    }

    void Update()
    {
        if(Vector3.Distance(transform.position , endPoint.position) > 1) 
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
        else
        {
            transform.position = startPoint.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GetComponent<SpriteRenderer>().enabled = false;
            HealthSystem h  = collision.GetComponent<HealthSystem>();
            h.TakeDamage();
            Invoke("ResetEnemy", 2);
        }
    }

    private void ResetEnemy()
    {
        transform.position = startPoint.position;
        GetComponent<SpriteRenderer>().enabled = true;


    }
}

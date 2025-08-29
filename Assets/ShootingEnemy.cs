using UnityEngine;
using System.Collections.Generic;

public class ShootingEnemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private float health = 3f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform instantiatePosition;
    [SerializeField] private float coolDownTime = 1f;
    [SerializeField] private int maxActiveProjectiles = 3;

    private List<GameObject> projectiles = new List<GameObject>();
    private float shootTimer;

    private void Update()
    {
        HandleShooting();
        CleanupProjectiles();
    }

    private void HandleShooting()
    {
        shootTimer -= Time.deltaTime;

        if (shootTimer > 0f || projectiles.Count >= maxActiveProjectiles)
            return;

        ShootProjectile();
        shootTimer = coolDownTime;
    }

    private void ShootProjectile()
    {
        GameObject obj = Instantiate(projectilePrefab, instantiatePosition.position, Quaternion.identity , instantiatePosition);
        projectiles.Add(obj);
    }

    private void CleanupProjectiles()
    {
        projectiles.RemoveAll(p => p == null);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<PlayerController>(out var player))
            {
                if (player.IsAttacking)
                {
                    TakeDamage();
                }
                else if (collision.TryGetComponent<HealthSystem>(out var healthSystem))
                {
                    healthSystem.TakeDamage();
                }
            }
        }
    }

    public void TakeDamage()
    {
        health--;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}

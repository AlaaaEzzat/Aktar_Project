using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxLives = 3;
    public int currentLives;

    [Header("UI References")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;
    public GameObject losePanal;
    public ParticleSystem getHitEffect;

    [Header("Respawn Settings")]
    public Transform respawnPoint;

    private bool isDead = false;
    private Animator animator;
    private bool canTakeDmg = true;

    void Start()
    {
        currentLives = maxLives;
        animator = GetComponent<Animator>();
        UpdateHeartsUI();
    }

    public void TakeDamage()
    {
        if (isDead || canTakeDmg == false) return;
        canTakeDmg = false;

        currentLives--;
        getHitEffect.Play();

        UpdateHeartsUI();
        animator.SetTrigger("GetHit");

        if (currentLives > 0)
        {
            Invoke("Respawn", 1);
        }
        else
        {
            GameOver();
        }
    }

    private void UpdateHeartsUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < currentLives;
        }
    }

    private void Respawn()
    {
        transform.position = respawnPoint.position;
        canTakeDmg = true;
    }

    private void GameOver()
    {
        isDead = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //losePanal.SetActive(true);
    }

}

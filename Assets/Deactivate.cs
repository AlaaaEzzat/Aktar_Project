using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Deactivate : MonoBehaviour
{
    [SerializeField] private float TimeBeforeDeactivate = 2f;
    [SerializeField] private GameObject iconIndicator;
    [SerializeField] private Image TimeLeftImage;
    public UnityEvent OnDeactivate;
    void Start()
    {
        iconIndicator.SetActive(true);
    }

    void Update()
    {
        if(TimeLeftImage !=  null && TimeLeftImage.fillAmount > 0)
        {
            TimeLeftImage.fillAmount -= 1 / TimeBeforeDeactivate * Time.deltaTime;
        }
        else
        {
            OnDeactivate?.Invoke();
            iconIndicator.SetActive(false);
            this.gameObject.SetActive(false);
        }
    }
}

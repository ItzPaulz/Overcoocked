using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(AudioSource))]
public class JuicyObject : MonoBehaviour
{
    [Header("Color Flash")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;

    [Header("Scale Pop")]
    public float popScaleFactor = 1.2f;
    public float popDuration = 0.1f;

    [Header("Highlight")]
    public GameObject highlightEffect; // activa/desactiva este objeto visual

    private Renderer rend;
    private AudioSource audioSource;
    private Color originalColor;
    private Vector3 originalScale;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        originalColor = rend.material.color;
        originalScale = transform.localScale;

        if (highlightEffect != null)
            highlightEffect.SetActive(false);
    }

    public void PlayPickupFeedback(AudioClip pickupSound)
    {
        StartCoroutine(FlashColor());
        StartCoroutine(ScalePop());

        if (pickupSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(pickupSound);
            audioSource.pitch = 1f;
        }
    }

    private IEnumerator FlashColor()
    {
        rend.material.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        rend.material.color = originalColor;
    }

    private IEnumerator ScalePop()
    {
        transform.localScale = originalScale * popScaleFactor;
        yield return new WaitForSeconds(popDuration);
        transform.localScale = originalScale;
    }

    public void ShowHighlight()
    {
        if (highlightEffect != null)
            highlightEffect.SetActive(true);
    }

    public void HideHighlight()
    {
        if (highlightEffect != null)
            highlightEffect.SetActive(false);
    }
}

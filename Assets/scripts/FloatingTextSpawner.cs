using UnityEngine;
using TMPro;

public class FloatingTextSpawner : MonoBehaviour
{
    [Header("References")]
    public RectTransform targetUIElement;     // The main score window (RectTransform)
    public GameObject floatingTextPrefab;     // Prefab with TMP + CanvasGroup

    [Header("Settings")]
    public float spawnRadius = 80f;           // How far around the score window the popups appear
    public float moveUpDistance = 40f;
    public float fadeDuration = 1f;

    public void SpawnFloatingText(string text)
    {
        // Instantiate
        GameObject obj = Instantiate(floatingTextPrefab, transform);
        RectTransform rect = obj.GetComponent<RectTransform>();
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        TMP_Text tmp = obj.GetComponent<TMP_Text>();

        tmp.text = text;

        // Random offset around the score window
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 startPosition = targetUIElement.anchoredPosition + randomOffset;
        rect.anchoredPosition = startPosition;

        // Start effect
        StartCoroutine(FloatingEffect(rect, cg));
    }

    private System.Collections.IEnumerator FloatingEffect(RectTransform rect, CanvasGroup cg)
    {
        Vector2 initialPos = rect.anchoredPosition;
        Vector2 targetPos = initialPos + new Vector2(0, moveUpDistance);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            // Move upward
            rect.anchoredPosition = Vector2.Lerp(initialPos, targetPos, t);

            // Fade out
            cg.alpha = 1f - t;

            yield return null;
        }

        Destroy(rect.gameObject);
    }
}

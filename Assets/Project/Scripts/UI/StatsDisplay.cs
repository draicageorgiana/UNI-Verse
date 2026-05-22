using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatsDisplay : MonoBehaviour
{
    [SerializeField] private Text socialStatText;
    [SerializeField] private Image socialStatBar;
    [SerializeField] private Text academicStatText;
    [SerializeField] private Image academicStatBar;
    [SerializeField] private Text healthStatText;
    [SerializeField] private Image healthStatBar;
    
    [SerializeField] private float statChangeDisplayDuration = 3f;
    [SerializeField] private CanvasGroup canvasGroup;
    
    private Coroutine fadeOutCoroutine;
    
    void Start()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        
        // Subscribe to stat changes
        StatSystem.OnStatChanged += OnStatChanged;
        
        // Initial display
        UpdateStatDisplay();
        
        // Hide initially if we want fade-in on stat change
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
    
    void OnDestroy()
    {
        StatSystem.OnStatChanged -= OnStatChanged;
    }
    
    private void OnStatChanged(string statName, int newValue)
    {
        UpdateStatDisplay();
        ShowStatsTemporarily();
    }
    
    private void UpdateStatDisplay()
    {
        if (StatSystem.instance == null)
        {
            Debug.LogWarning("StatsDisplay: StatSystem instance not found");
            return;
        }
        
        // Update Social Stat
        int socialStat = StatSystem.instance.GetStat("Social");
        if (socialStatText != null)
        {
            socialStatText.text = $"Social: {socialStat}/100";
        }
        if (socialStatBar != null)
        {
            socialStatBar.fillAmount = socialStat / 100f;
        }
        
        // Update Academic Stat
        int academicStat = StatSystem.instance.GetStat("Academic");
        if (academicStatText != null)
        {
            academicStatText.text = $"Academic: {academicStat}/100";
        }
        if (academicStatBar != null)
        {
            academicStatBar.fillAmount = academicStat / 100f;
        }
        
        // Update Health Stat
        int healthStat = StatSystem.instance.GetStat("Health");
        if (healthStatText != null)
        {
            healthStatText.text = $"Health: {healthStat}/100";
        }
        if (healthStatBar != null)
        {
            healthStatBar.fillAmount = healthStat / 100f;
        }
    }
    
    private void ShowStatsTemporarily()
    {
        if (canvasGroup == null)
            return;
        
        // Stop previous fade out
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }
        
        // Show stats
        canvasGroup.alpha = 1f;
        
        // Start fade out
        fadeOutCoroutine = StartCoroutine(FadeOutStats());
    }
    
    private IEnumerator FadeOutStats()
    {
        yield return new WaitForSeconds(statChangeDisplayDuration);
        
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }
    
    public void ShowStatsPanel()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        UpdateStatDisplay();
    }
    
    public void HideStatsPanel()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
}

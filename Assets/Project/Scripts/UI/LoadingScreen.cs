using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Image loadingFill;
    [SerializeField] private Text loadingText;
    [SerializeField] private Transform booksContainer;
    [SerializeField] private Image bookPrefab;
    [SerializeField] private Sprite[] bookSprites;
    
    [SerializeField] private int numberOfBooks = 5;
    [SerializeField] private float loadingDuration = 3f;
    [SerializeField] private float animationSpeed = 2f;
    
    private Image[] floatingBooks;
    
    void Start()
    {
        InitializeLoadingScreen();
        StartCoroutine(AnimateLoading());
    }
    
    private void InitializeLoadingScreen()
    {
        if (loadingFill != null)
        {
            loadingFill.fillAmount = 0f;
        }
        
        if (loadingText != null)
        {
            loadingText.text = "Loading...";
        }
        
        CreateFloatingBooks();
    }
    
    private void CreateFloatingBooks()
    {
        if (booksContainer == null || bookPrefab == null || bookSprites == null || bookSprites.Length == 0)
        {
            Debug.LogWarning("LoadingScreen: Missing book container, prefab, or sprites");
            return;
        }
        
        floatingBooks = new Image[numberOfBooks];
        
        for (int i = 0; i < numberOfBooks; i++)
        {
            Image bookImage = Instantiate(bookPrefab, booksContainer);
            bookImage.sprite = bookSprites[Random.Range(0, bookSprites.Length)];
            
            // Random starting position
            RectTransform rectTransform = bookImage.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(
                Random.Range(-400f, 400f),
                Random.Range(-200f, 200f)
            );
            
            floatingBooks[i] = bookImage;
            
            // Start floating animation for each book
            StartCoroutine(FloatBook(bookImage, i));
        }
    }
    
    private IEnumerator FloatBook(Image book, int index)
    {
        RectTransform rectTransform = book.GetComponent<RectTransform>();
        Vector2 startPos = rectTransform.anchoredPosition;
        float offset = (float)index / numberOfBooks * 360f;
        
        while (gameObject.activeInHierarchy)
        {
            float time = Time.time * animationSpeed + offset;
            
            // Create a floating sine wave motion
            float x = startPos.x + Mathf.Sin(time * 0.5f) * 50f;
            float y = startPos.y + Mathf.Cos(time * 0.3f) * 30f;
            
            rectTransform.anchoredPosition = new Vector2(x, y);
            
            // Slight rotation
            rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(time) * 5f);
            
            yield return null;
        }
    }
    
    private IEnumerator AnimateLoading()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < loadingDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / loadingDuration;
            
            if (loadingFill != null)
            {
                loadingFill.fillAmount = progress;
            }
            
            if (loadingText != null)
            {
                int dots = (int)(progress * 4) % 4;
                loadingText.text = "Loading" + new string('.', dots);
            }
            
            yield return null;
        }
        
        // Ensure fill is at 100%
        if (loadingFill != null)
        {
            loadingFill.fillAmount = 1f;
        }
        
        if (loadingText != null)
        {
            loadingText.text = "Ready!";
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Get the next scene to load from the scene manager
        // This will be called after the actual scene is loaded in StartPageUI
    }
    
    public void SetLoadingText(string text)
    {
        if (loadingText != null)
        {
            loadingText.text = text;
        }
    }
}

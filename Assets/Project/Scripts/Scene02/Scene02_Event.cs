using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Scene02_Event : MonoBehaviour
{
    [SerializeField] GameObject fadeScreenIn;
    [SerializeField] GameObject characterOne;
    [SerializeField] GameObject characterTwo;
    public GameObject textBox;
    [SerializeField] GameObject mainTextObject;
    [SerializeField] GameObject nextButton;
    [SerializeField] string textToSpeak;
    [SerializeField] int currentTextLenght;
    [SerializeField] int textLenght;
    [SerializeField] int eventPos = 0;
    [SerializeField] GameObject charName;
    [SerializeField] GameObject treeInteract;
    [SerializeField] GameObject houseInteract; 
    [SerializeField] GameObject fadeOut;

    //sounds for depending of day/night
    [SerializeField] GameObject daySound;
    [SerializeField] GameObject nightSound;
    [SerializeField] GameObject dayBGM;
    [SerializeField] GameObject nightBGM;
    
    // Configurable timing values
    [SerializeField] private float initialWaitTime = 2f;
    [SerializeField] private float postCharacterWaitTime = 2f;
    [SerializeField] private float textPrintDelay = 0.05f;
    [SerializeField] private float postTextWaitTime = 0.5f;
    [SerializeField] private float interactionWaitTime = 2f;
    
    // Scene transition
    [SerializeField] private string nextSceneName = "Scene03";
    
    // State tracking
    private bool hasTreeInteracted = false;
    private bool hasHouseInteracted = false;
    private bool eventInProgress = false;
    
    void Start()
    {
        // Null safety checks
        if (daySound == null || nightSound == null || dayBGM == null || nightBGM == null)
        {
            Debug.LogError("Scene02_Event: Missing audio GameObjects!");
            return;
        }
        
        if (TimeManager.IsDay())
        {
            daySound.SetActive(true);
            dayBGM.SetActive(true);
            nightSound.SetActive(false);
            nightBGM.SetActive(false);
        }
        else
        {
            nightSound.SetActive(true);
            nightBGM.SetActive(true);
            daySound.SetActive(false);
            dayBGM.SetActive(false);
        }
        
        StartCoroutine(EventStarter());
    }

    IEnumerator EventStarter()
    {
        eventInProgress = true;
        
        // event = 0 
        // wait before the first character appears
        yield return new WaitForSeconds(initialWaitTime);
        fadeScreenIn.SetActive(true);
        fadeScreenIn.SetActive(false);
        characterOne.SetActive(true);

        yield return new WaitForSeconds(postCharacterWaitTime);

        // Display first dialogue with player name substitution
        mainTextObject.SetActive(true);
        string playerName = PlayerData.GetPlayerName() ?? "Player";
        textToSpeak = $"I wonder where {playerName} is? She said she's going to meet me here but I don't see her anywhere...";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(textPrintDelay);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(postTextWaitTime);
        
        // auto start looking for character two after the first text is done
        yield return new WaitForSeconds(interactionWaitTime);
        characterOne.SetActive(false);
        mainTextObject.SetActive(false);

        treeInteract.SetActive(true);
        houseInteract.SetActive(true);
        eventInProgress = false;
    }

    public void TreeInteract()
    {
        if (hasTreeInteracted || eventInProgress) return;
        StartCoroutine(TreeInteractSeq());   
    }

    IEnumerator TreeInteractSeq()
    {
        hasTreeInteracted = true;
        eventInProgress = true;
        treeInteract.SetActive(false);
        houseInteract.SetActive(false);
        characterOne.SetActive(true);
        mainTextObject.SetActive(true);
        charName.GetComponent<TMPro.TMP_Text>().text = "Character One (Amaara)";
        textToSpeak = "Hmm... I don't think she's hiding behind that tree for sure... Maybe somewhere else?";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(textPrintDelay);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(postTextWaitTime);

        yield return new WaitForSeconds(interactionWaitTime);
        characterOne.SetActive(false);
        mainTextObject.SetActive(false);
        houseInteract.SetActive(true);
        treeInteract.SetActive(true);
        eventInProgress = false;
    }

    public void HouseInteract()
    {
        if (hasHouseInteracted || eventInProgress) return;
        StartCoroutine(HouseInteractSeq());
    }

    IEnumerator HouseInteractSeq()
    {
        hasHouseInteracted = true;
        eventInProgress = true;
        treeInteract.SetActive(false);
        houseInteract.SetActive(false);
        characterOne.SetActive(true);
        mainTextObject.SetActive(true);
        charName.GetComponent<TMPro.TMP_Text>().text = "Character One (Amaara)";
        textToSpeak = "Oh! I see her! She's next to that house! I should go check on her!";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(textPrintDelay);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(postTextWaitTime);
        characterTwo.SetActive(true);
        yield return new WaitForSeconds(postCharacterWaitTime);
        charName.GetComponent<TMPro.TMP_Text>().text = "Character Two";
        // TODO: Add choice system here - whether character one should go to character two or not
        textToSpeak = "Hey! I'm over here! Sorry for the delay, I couldn't find the park, haha! I saw a really nice coffee shop down the road. Do you want to check it out?";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(textPrintDelay);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(postTextWaitTime);

        yield return new WaitForSeconds(postCharacterWaitTime);
        charName.GetComponent<TMPro.TMP_Text>().text = "Character One (Amaara)";
        textToSpeak = "Hey! No worries, I was just looking around for you! I would love to check out the coffee shop with you! I could use a nice cup of coffee right now!";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(textPrintDelay);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(postTextWaitTime);

        characterTwo.SetActive(false);
        mainTextObject.SetActive(false);
        fadeOut.SetActive(true);
        
        // Scene transition with fade out timing
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(nextSceneName);
    }

    void Update()
    {
        textLenght = TextCreator.charCount;
    }
    
    void OnDestroy()
    {
        // Clean up coroutines if scene unloads mid-event
        StopAllCoroutines();
    }

}

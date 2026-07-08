using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class Scene01_Event : MonoBehaviour
{
    // Variables
    public GameObject fadeScreenIn;
    public GameObject characterOne;
    public GameObject characterTwo;

    public GameObject textBox;

    [SerializeField] AudioSource girlSigh;
    [SerializeField] AudioSource girlGasp;

    [SerializeField] string textToSpeak;
    [SerializeField] int currentTextLenght;
    [SerializeField] int textLenght;
    [SerializeField] GameObject mainTextObject;
    [SerializeField] GameObject nextButton;
    [SerializeField] int eventPos = 0;
    [SerializeField] GameObject charName;
    [SerializeField] GameObject fadeOut;
    
    // Name input UI
    [SerializeField] GameObject nameInputPanel;
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] GameObject nameInputButton;
    
    private string playerCharacterName = "";
    private bool nameConfirmed = false;

    void Update()
    {
        textLenght = TextCreator.charCount;
    }

    // Start method is called before the first frame update
    void Start()
    {
        // §3.5 Character Customization: keep the input panel hidden until the
        // story asks for a name, and wire the confirm button in code so no
        // extra Inspector hookup is required.
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(false);
        }
        if (nameInputButton != null)
        {
            UnityEngine.UI.Button confirmButton = nameInputButton.GetComponent<UnityEngine.UI.Button>();
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(ConfirmNameInput);
            }
        }

        StartCoroutine(EventStarter());
    }

    /// <summary>Confirm handler for the character customization panel (§3.5).</summary>
    public void ConfirmNameInput()
    {
        string typedName = nameInputField != null ? nameInputField.text : "";
        playerCharacterName = string.IsNullOrWhiteSpace(typedName) ? "Player" : typedName.Trim();
        nameConfirmed = true;
    }

    IEnumerator EventStarter()
    {
        // event = 0 
        // wait 2 sec after the fade screen disappears before the first character appears
        yield return new WaitForSeconds(2);
        fadeScreenIn.SetActive(false);
        characterOne.SetActive(true);

        yield return new WaitForSeconds(2);

        // This is where text function goes 
        mainTextObject.SetActive(true);
        textToSpeak = "Cannot believe I am finally in UNI-Versity! I am so frightened, yet so excited...";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(0.5f);
        nextButton.SetActive(true);
        eventPos =1;

       
    }

    IEnumerator EventOne()
    {
        //make sure fade in is disabled and character one is active before the event starts
        fadeOut.SetActive(false);
        //event 1
        nextButton.SetActive(false);
        textBox.SetActive(true);
        girlSigh.Play();

        yield return new WaitForSeconds(2);
        characterTwo.SetActive(true);
        yield return new WaitForSeconds(2);
        girlGasp.Play(); 
        yield return new WaitForSeconds(2);
        characterOne.SetActive(false);  

        // Conversation text
        textToSpeak = "Oh, my God! You've startled me! Are you a student here too?";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(1);
        charName.GetComponent<TMPro.TMP_Text>().text = "Character Two";
        textToSpeak = "Excuse me, that wasn't my intention. Yes, it's my first day. I presume yours is too?";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(0.5f);
        nextButton.SetActive(true);
        eventPos = 2;
        
    }

        IEnumerator EventTwo()
    {
        //event 2
        nextButton.SetActive(false);
        textBox.SetActive(true);
        characterOne.SetActive(false);  

        // Conversation text
        charName.GetComponent<TMPro.TMP_Text>().text = "Character One (Amaara)";
        textToSpeak = "Precisely! I am Amaara, by the way. It's nice to meet you.";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(0.5f);
        nextButton.SetActive(true);
        eventPos++;
        
    }

            IEnumerator EventThree()
    {
        //event 2
        nextButton.SetActive(false);
        textBox.SetActive(true);
        characterOne.SetActive(false);  

        // Conversation text
        // §3.5 Character Customization use case: invoked at initial system
        // boot of the journey — the player defines their identity, which is
        // converted into an immutable string field inside the data layer
        // (SaveData → gamesave.json) through PlayerData.SetPlayerName.
        if (nameInputPanel != null && nameInputField != null)
        {
            nameConfirmed = false;
            nameInputPanel.SetActive(true);
            yield return new WaitUntil(() => nameConfirmed);
            nameInputPanel.SetActive(false);
        }
        if (string.IsNullOrWhiteSpace(playerCharacterName))
        {
            playerCharacterName = PlayerData.GetPlayerName() ?? "Player";
        }
        PlayerData.SetPlayerName(playerCharacterName);

        charName.GetComponent<TMPro.TMP_Text>().text = playerCharacterName;
        textToSpeak = $"Nice to meet you too! I am {playerCharacterName}! What are you studying?";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(0.5f);
        nextButton.SetActive(true);
        eventPos++;
        
    }

     IEnumerator EventFour()
    {
        //event 4 Fade out at the end of scene 1
        nextButton.SetActive(false);
        textBox.SetActive(true);
        characterOne.SetActive(false);
        charName.GetComponent<TMPro.TMP_Text>().text = "Character One (Amaara)";
        textToSpeak = "I am studying Computer Science! What about you?";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(0.5f);
        charName.GetComponent<TMPro.TMP_Text>().text = string.IsNullOrEmpty(playerCharacterName) ? "Character Two" : playerCharacterName;
        textToSpeak = "Me too! Do you want to meet in the park to hang out? I don't know anyone here yet and it would be nice to have a friend to explore the university with.";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(2);
        charName.GetComponent<TMPro.TMP_Text>().text = "Character One (Amaara)";
        textToSpeak = "Count me in! I will meet you there in just a bit! ";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(2);
        fadeOut.SetActive(true);
        nextButton.SetActive(true);
        yield return new WaitForSeconds(2);
        eventPos++;
        SceneManager.LoadScene(3);
        
    }

    public void NextButton()
    {
        switch(eventPos)
        {
            case 1:
                StartCoroutine(EventOne());
                break;
            case 2:
                StartCoroutine(EventTwo());
                break;
            case 3:
                StartCoroutine(EventThree());
                break;
            case 4:
                StartCoroutine(EventFour());
                break;
    }
}


    
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
    void Update()
    {
        textLenght = TextCreator.charCount;
    }

    // Start method is called before the first frame update
    void Start()
    {
        StartCoroutine(EventStarter());
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
        textToSpeak = "Precisely! I am <Insert Name>, by the way. It's nice to meet you.";
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
        charName.GetComponent<TMPro.TMP_Text>().text = "Character Two";
        textToSpeak = "Nice to meet you too! I am <Insert Name>! What are you studying?";
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
        charName.GetComponent<TMPro.TMP_Text>().text = "Character Two";
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
        SceneManager.LoadScene(1);
        
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

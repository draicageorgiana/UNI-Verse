using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    void Start()
    {
        if (TimeManager.IsDay())
        {
            daySound.SetActive(true);
            dayBGM.SetActive(true);
        }
        else
        {
            nightSound.SetActive(true);
            nightBGM.SetActive(true);
        }
        
        StartCoroutine(EventStarter());
    }

    IEnumerator EventStarter()
    {
        // event = 0 
        // wait 2 sec after the fade screen disappears before the first character appears
        yield return new WaitForSeconds(2);
        fadeScreenIn.SetActive(true);
        fadeScreenIn.SetActive(false);
        characterOne.SetActive(true);

        yield return new WaitForSeconds(2);

        // This is where text function goes 
        mainTextObject.SetActive(true);
        textToSpeak = "I wonder where <Insert name> is? She said she's going to meet me here but I don't see her anywhere...";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(0.5f);
        //nextButton.SetActive(true);
        //eventPos = 1;
        // auto start looking for character two after the first text is done
        yield return new WaitForSeconds(2);
        characterOne.SetActive(false);
        mainTextObject.SetActive(false);

        treeInteract.SetActive(true);
        houseInteract.SetActive(true);

        
    }

    public void TreeInteract()
    {
          StartCoroutine(TreeInteractSeq());   
    }

    IEnumerator TreeInteractSeq()
    {
        treeInteract.SetActive(false);
        houseInteract.SetActive(false);
        characterOne.SetActive(true);
        mainTextObject.SetActive(true);
        charName.GetComponent<TMPro.TMP_Text>().text = "Character One (Amaara)";
        textToSpeak = "Hmm... I don't think she's hiding behind that tree for sure... Maybe somewhere else?";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(0.5f);

        yield return new WaitForSeconds(2);
        characterOne.SetActive(false);
        mainTextObject.SetActive(false);
        houseInteract.SetActive(true);
    }

    public void HouseInteract()
    {
        StartCoroutine(HouseInteractSeq());
    }

    IEnumerator HouseInteractSeq()
    {
        treeInteract.SetActive(false);
        houseInteract.SetActive(false);
        characterOne.SetActive(true);
        mainTextObject.SetActive(true);
        charName.GetComponent<TMPro.TMP_Text>().text = "Character One (Amaara)";
        textToSpeak = "Oh! I see her! She's next to that house! I should go check on her!";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(0.5f);
        characterTwo.SetActive(true);
        yield return new WaitForSeconds(2);
        charName.GetComponent<TMPro.TMP_Text>().text = "Character Two";
        // soon here I will add a choice whether character one should go to character two or not, but for now it will just automatically start the conversation
        textToSpeak = "Hey! I'm over here! Sorry for the delay, I couldn't find the park, haha! I saw a really nice coffee shop down the road. Do you want to check it out?";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(0.5f);

        yield return new WaitForSeconds(2);
        charName.GetComponent<TMPro.TMP_Text>().text = "Character One (Amaara)";
        textToSpeak = "Hey! No worries, I was just looking around for you! I would love to check out the coffee shop with you! I could use a nice cup of coffee right now!";    
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;  
        currentTextLenght = textToSpeak.Length;
        TextCreator.runTextPrint = true;  
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLenght == currentTextLenght);
        yield return new WaitForSeconds(0.5f);

        characterTwo.SetActive(false);
        mainTextObject.SetActive(false);
        fadeOut.SetActive(true);

    }


    void Update()
    {
        textLenght = TextCreator.charCount;

    }
    
}

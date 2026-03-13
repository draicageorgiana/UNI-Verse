using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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


        textBox.SetActive(true);
        girlSigh.Play();

        yield return new WaitForSeconds(2);
        characterTwo.SetActive(true);
        yield return new WaitForSeconds(2);
        girlGasp.Play();
    }


    
}

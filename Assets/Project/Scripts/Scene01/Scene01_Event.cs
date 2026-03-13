using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scene01_Event : MonoBehaviour
{
    // Variables
    public GameObject fadeScreenIn;
    public GameObject characterOne;
    public GameObject characterTwo;

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
        yield return new WaitForSeconds(2);
        characterTwo.SetActive(true);
    }


    
}

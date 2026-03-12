using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Ink.Runtime; // This allows the script to talk to Inkle
using TMPro;       // This allows the script to talk to TextMeshPro

public class StoryManager : MonoBehaviour {
    [Header("Ink Configuration")]
    [SerializeField] private TextAsset inkJSONAsset;
    private Story story;

    [Header("UI Layout")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Button choiceButtonPrefab;

    void Start() {
        if (inkJSONAsset != null) {
            story = new Story(inkJSONAsset.text);
            RefreshView();
        } else {
            Debug.LogError("Hey! You forgot to drag your JSON file into the Inspector!");
        }
    }

    // This function clears the old text and choices, then shows the new ones
    void RefreshView() {
        // 1. Clear existing buttons
        foreach (Transform child in choiceContainer) {
            Destroy(child.gameObject);
        }

        // 2. Get the next line of text
        if (story.canContinue) {
            string text = story.Continue();
            dialogueText.text = text.Trim();
        }

        // 3. Display choices (if there are any)
        foreach (Choice choice in story.currentChoices) {
            Button button = Instantiate(choiceButtonPrefab, choiceContainer);
            
            // Set the button text
            TextMeshProUGUI btnText = button.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = choice.text;

            // When clicked, tell Ink which choice was made
            button.onClick.AddListener(() => OnChoiceSelected(choice));
        }
    }

    void OnChoiceSelected(Choice choice) {
        story.ChooseChoiceIndex(choice.index);
        RefreshView();
    }
}
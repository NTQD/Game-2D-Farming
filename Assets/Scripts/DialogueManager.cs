using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueBox;
    public bool dialogueActive;
    GameObject toolbar;
    GameObject inventory;
    GameObject shop;
    GameObject chest;
    public string[] sentences;
    private int index;
    public Text textDisplay;
    public float typingSpeed = 0.02f; // faster default typing speed
    public GameObject pressToContinue;

    private void Awake()
    {
        toolbar = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.CompareTag("toolbar"));
        shop = Resources.FindObjectsOfTypeAll<GameObject>().LastOrDefault(g => g.CompareTag("shop"));
        chest = Resources.FindObjectsOfTypeAll<GameObject>().LastOrDefault(g => g.CompareTag("chest"));
        inventory = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.CompareTag("inventory"));
    }

    // Start is called before the first frame update
    void Start()
    {
        toolbar.SetActive(false);
        shop.SetActive(false);
        chest.SetActive(false);
        inventory.SetActive(false);

        StartCoroutine(Type());
    }


    // Update is called once per frame
    void Update()
    {
        // Skip entire tutorial with Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            EndDialogue();
            return;
        }

        // Stopping random letters from appearing
        if (textDisplay.text == sentences[index] && dialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            GoToNextSentence();
            pressToContinue.SetActive(true);

        }
        
    }

    IEnumerator Type()
    {
        // Writing sentences in a certain speed
        foreach(char letter in sentences[index].ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    // Moving through sentences
    public void GoToNextSentence()
    {
        pressToContinue.SetActive(false);

        if (index < sentences.Length - 1)
        {
            textDisplay.text = "";
            index++;
            StartCoroutine(Type());
            
        }
        else
        {
            EndDialogue();
            
        }
    }

    // Ends the dialogue/tutorial and restores gameplay UI
    private void EndDialogue()
    {
        dialogueActive = false;
        StopAllCoroutines();
        textDisplay.text = "";
        dialogueBox.SetActive(false);
        if (toolbar != null) toolbar.SetActive(true);
        if (chest != null) chest.SetActive(true);
        if (shop != null) shop.SetActive(true);
        if (inventory != null) inventory.SetActive(false);
    }
}

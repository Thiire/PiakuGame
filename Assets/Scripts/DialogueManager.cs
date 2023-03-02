using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager SharedInstance;

    public Text dialogueText;
    public Animator dialogueAnimator;
    public bool isDialogue = false;
    [Range(0.02f, 0.2f)] public float delayBetweenWords = 0.02f;
    private Queue<string> sentences;

    void Start()
    {
        sentences = new Queue<string>();
        if (!isDialogue)
            Cursor.lockState = CursorLockMode.Locked;
        SharedInstance = this;
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue.sentences.Length == 0)
            return;

        dialogueAnimator.SetBool("IsOpen", true);
        isDialogue = true;
        Cursor.lockState = CursorLockMode.None;
        sentences.Clear();
        dialogueText.text = "";

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        Invoke(nameof(DisplayNextSentence), 0.25f);
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    void Update()
    {
        if (isDialogue && Input.GetButtonDown("Jump"))
        {
            DisplayNextSentence();
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        int numberOfOpenBracket = 0;
        int numberOfSlashBracket = 0;
        int numberOfCloseBracket = 0;

        string tmpBracketString = "";
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            if (letter == '<')
            {
                numberOfOpenBracket++;
            }
            else if (letter == '/')
            {
                numberOfSlashBracket++;
                numberOfOpenBracket--;
            }
            else if (letter == '>' && numberOfSlashBracket > 0)
            {
                numberOfSlashBracket--;
                numberOfCloseBracket++;
            }

            if (numberOfOpenBracket != numberOfCloseBracket)
            {
                tmpBracketString += letter;
            }
            else if (tmpBracketString.Length != 0)
            {
                dialogueText.text += tmpBracketString + '>';
                tmpBracketString = "";
                yield return new WaitForSeconds(delayBetweenWords);
            }
            else
            {
                dialogueText.text += letter;
                if (letter == ' ')
                {
                    yield return new WaitForSeconds(delayBetweenWords);
                }
            }
        }

        if (tmpBracketString.Length != 0)
        {
            dialogueText.text += tmpBracketString + '>';
            tmpBracketString = "";
        }
    }

    void EndDialogue()
    {
        dialogueAnimator.SetBool("IsOpen", false);
        Cursor.lockState = CursorLockMode.Locked;
        isDialogue = false;
    }
}

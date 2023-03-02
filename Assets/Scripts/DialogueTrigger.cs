using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("General")]
    public bool loadOnStart = false;
    [Header("Dialogue")]
    public Dialogue dialogue;
    private bool isShown = false;
    void Start()
    {
        if (loadOnStart)
        {
            Invoke(nameof(TriggerDialogue), 0.5f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !isShown)
        {
            isShown = true;
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        DialogueManager.SharedInstance.StartDialogue(dialogue);
    }

}

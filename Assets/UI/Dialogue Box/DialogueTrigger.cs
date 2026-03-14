using System.Collections.Generic;
using UnityEngine;
 
[System.Serializable]
public class DialogueCharacter
{
    public string name;
    public Sprite icon;
}
 
[System.Serializable]
public class DialogueLine
{
    public DialogueCharacter character;
    [TextArea(3, 10)]
    public string line;
}
 
[System.Serializable]
public class Dialogue
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}
 
public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
 
    public void TriggerDialogue()
    {
        Debug.Log("TriggerDialogue called"); // Is this firing?
        Debug.Log("Lines count: " + dialogue.dialogueLines.Count); // Is data populated?

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager Instance is NULL! Is it in the scene?");
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogue);
    }
 
    private void OnTriggerEnter(Collider collision)
    {
        if(collision.CompareTag("Player"))
        {
            TriggerDialogue();
        }
    }
}
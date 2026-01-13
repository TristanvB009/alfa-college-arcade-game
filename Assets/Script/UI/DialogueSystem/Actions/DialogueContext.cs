using UnityEngine;

public class DialogueContext
{
    public DialogueManager dialogueManager;
    public DialogueStarter dialogueStarter;
    public DialogueData dialogueData;

    public DialogueNode currentNode;
    public DialogueChoice currentChoice;

    public GameObject player;
}

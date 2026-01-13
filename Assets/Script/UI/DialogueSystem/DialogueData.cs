using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Data", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Information")]
    public string dialogueName = "New Dialogue";
    
    [TextArea(2, 4)]
    public string description = "Description of this dialogue";
    
    [Header("Dialogue Nodes")]
    public List<DialogueNode> dialogueNodes = new List<DialogueNode>();

    [Header("Dialogue Actions")]
    [Tooltip("Actions that run when this DialogueData starts")]
    public List<DialogueAction> onDialogueStartActions = new List<DialogueAction>();

    [Tooltip("Actions that run when this DialogueData ends")]
    public List<DialogueAction> onDialogueEndActions = new List<DialogueAction>();
}
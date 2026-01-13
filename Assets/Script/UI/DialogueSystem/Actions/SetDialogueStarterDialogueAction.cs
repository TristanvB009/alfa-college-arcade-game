using UnityEngine;

[CreateAssetMenu(fileName = "SetDialogueStarterDialogueAction", menuName = "Dialogue System/Actions/Set Starter Dialogue")]
public class SetDialogueStarterDialogueAction : DialogueAction
{
    [Header("Set By Asset (preferred)")]
    public DialogueData dialogueToSelect;

    [Header("Set By Index (fallback)")]
    [Tooltip("Used only when Dialogue To Select is not set. -1 = ignore.")]
    public int dialogueIndex = -1;

    public override void Execute(DialogueContext context)
    {
        if (context == null || context.dialogueStarter == null)
        {
            return;
        }

        DialogueStarter starter = context.dialogueStarter;

        if (dialogueToSelect != null)
        {
            int idx = starter.availableDialogues != null ? starter.availableDialogues.IndexOf(dialogueToSelect) : -1;
            if (idx >= 0)
            {
                starter.SwitchDialogue(idx);
            }
            else
            {
                Debug.LogWarning($"[Dialogue] Dialogue '{dialogueToSelect.name}' not found in starter.availableDialogues on '{starter.name}'.");
            }

            return;
        }

        if (dialogueIndex >= 0)
        {
            starter.SwitchDialogue(dialogueIndex);
        }
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "ChangeSpecificTilesDialogueAction", menuName = "Dialogue System/Actions/Change Tiles To Secondary")]
public class ChangeSpecificTilesDialogueAction : DialogueAction
{
    public enum TargetResolver
    {
        UseAssignedReference,
        FindFirstInScene,
        FindByName,
        FindByTag
    }

    [Header("TilemapChanger Target")]
    [Tooltip("NOTE: Dialogue actions are ScriptableObject assets. Unity generally cannot serialize references to scene objects inside assets.\n" +
             "If you see 'type mismatch' when trying to drag a scene GameObject here, prefer FindFirstInScene / FindByName / FindByTag.")]
    public TargetResolver resolver = TargetResolver.FindFirstInScene;

    [Tooltip("Used when Resolver = UseAssignedReference.\n" +
             "Tip: this field expects a TilemapChanger component, not a GameObject.\n" +
             "Also: assigning scene objects to ScriptableObject assets usually won't persist; use a Find* resolver instead.")]
    public TilemapChanger tilemapChanger;

    [Tooltip("Used when Resolver = FindByName")]
    public string targetGameObjectName;

    [Tooltip("Used when Resolver = FindByTag")]
    public string targetTag;

    [Header("Secondary Mapping Selection")]
    [Tooltip("If true, calls ChangeTilesToSecondary() using all secondary mappings. If false, uses Secondary Mapping Index.")]
    public bool useAllSecondaryMappings = false;

    [Tooltip("Which entry in TilemapChanger.secondaryMappings to apply. Only used when Use All Secondary Mappings is false.")]
    public int secondaryMappingIndex = 0;

    public override void Execute(DialogueContext context)
    {
        TilemapChanger changer = ResolveChanger(context);
        if (changer == null)
        {
            Debug.LogWarning("[Dialogue] ChangeSpecificTilesDialogueAction: TilemapChanger not found.");
            return;
        }

        if (useAllSecondaryMappings)
        {
            changer.ChangeTilesToSecondary();
        }
        else
        {
            changer.ChangeTilesToSecondaryMapping(secondaryMappingIndex);
        }
    }

    private TilemapChanger ResolveChanger(DialogueContext context)
    {
        if (resolver == TargetResolver.UseAssignedReference)
        {
            // If this action is an asset, the assigned reference may not be possible to persist.
            // Fall back to finding the first TilemapChanger in the scene.
            if (tilemapChanger != null) return tilemapChanger;
            return Object.FindFirstObjectByType<TilemapChanger>();
        }

        if (resolver == TargetResolver.FindFirstInScene)
        {
            return Object.FindFirstObjectByType<TilemapChanger>();
        }

        if (resolver == TargetResolver.FindByName)
        {
            if (string.IsNullOrWhiteSpace(targetGameObjectName)) return null;
            GameObject go = GameObject.Find(targetGameObjectName);
            return go != null ? go.GetComponent<TilemapChanger>() : null;
        }

        if (resolver == TargetResolver.FindByTag)
        {
            if (string.IsNullOrWhiteSpace(targetTag)) return null;
            GameObject go = GameObject.FindGameObjectWithTag(targetTag);
            return go != null ? go.GetComponent<TilemapChanger>() : null;
        }

        return null;
    }
}

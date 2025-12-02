using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mapping between default and colored sprites for GameObjects
/// Similar to TileMapping but for sprite changes on GameObjects
/// </summary>
[System.Serializable]
public class SpriteMapping
{
    [Header("Default Sprite")]
    [Tooltip("The default sprite to look for")]
    public Sprite defaultSprite;
    
    [Tooltip("Alternative: prefab containing default sprite (SpriteRenderer will be extracted)")]
    public GameObject defaultPrefab;
    
    [Header("Colored Sprite")]
    [Tooltip("The colored sprite to replace with")]
    public Sprite coloredSprite;
    
    [Tooltip("Alternative: prefab containing colored sprite (SpriteRenderer will be extracted)")]
    public GameObject coloredPrefab;
    
    [Header("Options")]
    [Tooltip("Tag to filter which GameObjects this mapping applies to (leave empty for all)")]
    public string targetTag = "";
    
    /// <summary>
    /// Get the default sprite from either direct sprite or prefab
    /// </summary>
    public Sprite GetDefaultSprite()
    {
        if (defaultSprite != null)
        {
            return defaultSprite;
        }
        
        if (defaultPrefab != null)
        {
            SpriteRenderer spriteRenderer = defaultPrefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                return spriteRenderer.sprite;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Get the colored sprite from either direct sprite or prefab
    /// </summary>
    public Sprite GetColoredSprite()
    {
        if (coloredSprite != null)
        {
            return coloredSprite;
        }
        
        if (coloredPrefab != null)
        {
            SpriteRenderer spriteRenderer = coloredPrefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                return spriteRenderer.sprite;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Validate this sprite mapping
    /// </summary>
    public bool IsValid()
    {
        // At least one default source (Sprite or prefab)
        bool hasDefault = defaultSprite != null || defaultPrefab != null;
        
        // At least one colored source (Sprite or prefab)  
        bool hasColored = coloredSprite != null || coloredPrefab != null;
        
        return hasDefault && hasColored;
    }
    
    /// <summary>
    /// Get description of this mapping for debugging
    /// </summary>
    public string GetDescription()
    {
        string defaultSource = defaultSprite != null ? $"Sprite({defaultSprite.name})" : 
                              defaultPrefab != null ? $"Prefab({defaultPrefab.name})" : "None";
        
        string coloredSource = coloredSprite != null ? $"Sprite({coloredSprite.name})" : 
                              coloredPrefab != null ? $"Prefab({coloredPrefab.name})" : "None";
        
        string tagFilter = !string.IsNullOrEmpty(targetTag) ? $" [Tag: {targetTag}]" : " [All Objects]";
        
        return $"{defaultSource} -> {coloredSource}{tagFilter}";
    }
}

public class SpriteChanger : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Search for SpriteRenderers in children of this transform (leave empty to search entire scene)")]
    [SerializeField] private Transform targetParent;
    
    [Header("Sprite Mappings")]
    [Tooltip("List of sprite pairs - default sprites will be replaced with colored sprites")]
    [SerializeField] private List<SpriteMapping> spriteMappings = new List<SpriteMapping>();
    
    [Header("Terminal Integration")]
    [Tooltip("Check for terminal completion automatically")]
    [SerializeField] private bool autoCheckTerminals = true;
    
    [Tooltip("How often to check terminal status (in seconds)")]
    [SerializeField] private float checkInterval = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Performance")]
    [Tooltip("Find and cache all sprite renderers at start to avoid lag during gameplay")]
    [SerializeField] private bool preloadSpriteRenderers = true;
    
    [Header("Spotlight Effects")]
    [Tooltip("Spotlight objects to activate when sprites change to colored versions")]
    [SerializeField] private GameObject[] spotlightObjects = new GameObject[0];
    
    private bool hasChangedSprites = false;
    
    // Cache of all sprite renderers that could be changed
    private List<SpriteRenderer> cachedSpriteRenderers = new List<SpriteRenderer>();
    
    // Precomputed sprite change data for instant execution
    private List<SpriteRenderer> renderersToChange = new List<SpriteRenderer>();
    private List<Sprite> newSpritesForRenderers = new List<Sprite>();
    private List<Sprite> oldSpritesForRenderers = new List<Sprite>();
    
    void Start()
    {
        // Use this transform as target parent if not assigned
        if (targetParent == null)
        {
            targetParent = transform;
        }
        
        // Start checking terminals if auto-check is enabled
        if (autoCheckTerminals)
        {
            InvokeRepeating(nameof(CheckTerminalStatus), 1f, checkInterval);
        }
        
        // Preload sprite renderers to avoid lag spikes during gameplay
        if (preloadSpriteRenderers)
        {
            PreloadSpriteRenderers();
        }

        if (showDebugLogs)
        {
            Debug.Log($"SpriteChanger initialized with {spriteMappings.Count} sprite mappings targeting: {(targetParent == transform ? "children" : targetParent.name)}");
        }
    }
    
    /// <summary>
    /// Find and cache all sprite renderers that could be changed
    /// Also precompute sprite change data for instant execution
    /// </summary>
    private void PreloadSpriteRenderers()
    {
        if (spriteMappings == null || spriteMappings.Count == 0)
        {
            return;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"=== PRELOADING SPRITE RENDERERS AND PRECOMPUTING SPRITE CHANGES ===");
        }
        
        // Clear previous cached data
        cachedSpriteRenderers.Clear();
        renderersToChange.Clear();
        newSpritesForRenderers.Clear();
        oldSpritesForRenderers.Clear();
        
        // Find all sprite renderers in target area
        SpriteRenderer[] foundRenderers;
        if (targetParent == transform)
        {
            // Search in children only
            foundRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
        else
        {
            // Search in specified parent's children
            foundRenderers = targetParent.GetComponentsInChildren<SpriteRenderer>();
        }
        
        int cachedCount = 0;
        int precomputedCount = 0;
        
        // Process each found sprite renderer
        foreach (SpriteRenderer renderer in foundRenderers)
        {
            if (renderer.sprite != null)
            {
                cachedSpriteRenderers.Add(renderer);
                cachedCount++;
                
                // Check if this sprite renderer will need changes
                foreach (SpriteMapping mapping in spriteMappings)
                {
                    if (mapping.IsValid())
                    {
                        // Check tag filter if specified
                        if (!string.IsNullOrEmpty(mapping.targetTag) && !renderer.gameObject.CompareTag(mapping.targetTag))
                        {
                            continue;
                        }
                        
                        Sprite defaultSprite = mapping.GetDefaultSprite();
                        
                        // If this sprite matches a mapping
                        if (defaultSprite != null && renderer.sprite.name == defaultSprite.name)
                        {
                            Sprite coloredSprite = mapping.GetColoredSprite();
                            if (coloredSprite != null)
                            {
                                // Precompute this change
                                renderersToChange.Add(renderer);
                                newSpritesForRenderers.Add(coloredSprite);
                                oldSpritesForRenderers.Add(renderer.sprite);
                                precomputedCount++;
                                
                                if (showDebugLogs)
                                {
                                    Debug.Log($"✓ Precomputed: {renderer.gameObject.name} - {renderer.sprite.name} -> {coloredSprite.name}");
                                }
                                break; // Found a match, no need to check other mappings for this renderer
                            }
                        }
                    }
                }
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"✓ PRELOADING COMPLETE: {cachedCount} sprite renderers cached, {precomputedCount} sprite changes precomputed");
        }
    }
    
    /// <summary>
    /// Check if all terminals are completed and change sprites if so
    /// </summary>
    private void CheckTerminalStatus()
    {
        if (hasChangedSprites)
        {
            return; // Already changed, no need to keep checking
        }
        
        // Check if all terminals are completed using the static method from PowerTerminalMinigame
        if (PowerTerminalMinigame.AreAllTerminalsCompleted())
        {
            if (showDebugLogs)
            {
                Debug.Log("All terminals completed! Changing sprites to colored versions.");
            }
            
            ChangeSpritesToColored();
            
            // Stop checking since we've completed the change
            CancelInvoke(nameof(CheckTerminalStatus));
        }
    }
    
    /// <summary>
    /// Instantly trigger the sprite change using precomputed data (no lag!)
    /// </summary>
    [ContextMenu("Change Sprites to Colored")]
    public void ChangeSpritesToColored()
    {
        if (showDebugLogs)
        {
            Debug.Log($"=== INSTANT SPRITE CHANGE PROCESS ===");
        }
        
        int changedSpriteCount = 0;
        
        // Apply precomputed sprite changes
        for (int i = 0; i < renderersToChange.Count; i++)
        {
            SpriteRenderer renderer = renderersToChange[i];
            Sprite newSprite = newSpritesForRenderers[i];
            
            if (renderer != null && newSprite != null)
            {
                string oldSpriteName = renderer.sprite != null ? renderer.sprite.name : "null";
                renderer.sprite = newSprite;
                changedSpriteCount++;
                
                if (showDebugLogs)
                {
                    Debug.Log($"✓ Changed: {renderer.gameObject.name} - {oldSpriteName} -> {newSprite.name}");
                }
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"=== INSTANT SPRITE CHANGE COMPLETE: {changedSpriteCount} sprites changed ===");
        }
        
        // Activate spotlight objects
        ActivateSpotlights();
        
        hasChangedSprites = true;
    }
    
    /// <summary>
    /// Activate all spotlight objects when sprites change
    /// </summary>
    private void ActivateSpotlights()
    {
        if (spotlightObjects != null && spotlightObjects.Length > 0)
        {
            int activatedCount = 0;
            
            foreach (GameObject spotlight in spotlightObjects)
            {
                if (spotlight != null)
                {
                    spotlight.SetActive(true);
                    activatedCount++;
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"✓ Activated spotlight: {spotlight.name}");
                    }
                }
            }
            
            if (showDebugLogs)
            {
                Debug.Log($"=== SPOTLIGHTS ACTIVATED: {activatedCount}/{spotlightObjects.Length} spotlights activated ===");
            }
        }
    }
    
    /// <summary>
    /// Deactivate all spotlight objects when sprites are reset
    /// </summary>
    private void DeactivateSpotlights()
    {
        if (spotlightObjects != null && spotlightObjects.Length > 0)
        {
            int deactivatedCount = 0;
            
            foreach (GameObject spotlight in spotlightObjects)
            {
                if (spotlight != null)
                {
                    spotlight.SetActive(false);
                    deactivatedCount++;
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"✓ Deactivated spotlight: {spotlight.name}");
                    }
                }
            }
            
            if (showDebugLogs)
            {
                Debug.Log($"=== SPOTLIGHTS DEACTIVATED: {deactivatedCount}/{spotlightObjects.Length} spotlights deactivated ===");
            }
        }
    }

    /// <summary>
    /// Debug method to examine current sprites in the target area
    /// </summary>
    [ContextMenu("Debug Current Sprites")]
    public void DebugCurrentSprites()
    {
        if (showDebugLogs)
        {
            Debug.Log($"=== DEBUGGING SPRITES IN: {(targetParent == transform ? "children" : targetParent.name)} ===");
        }
        
        Dictionary<string, int> spriteCount = new Dictionary<string, int>();
        int totalRenderers = 0;
        
        foreach (SpriteRenderer renderer in cachedSpriteRenderers)
        {
            if (renderer != null && renderer.sprite != null)
            {
                totalRenderers++;
                string spriteName = renderer.sprite.name;
                
                if (spriteCount.ContainsKey(spriteName))
                {
                    spriteCount[spriteName]++;
                }
                else
                {
                    spriteCount[spriteName] = 1;
                    if (showDebugLogs)
                    {
                        Debug.Log($"Found sprite: {spriteName} on {renderer.gameObject.name} (Tag: {renderer.gameObject.tag})");
                    }
                }
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"Total sprite renderers found: {totalRenderers}");
            Debug.Log("Sprite distribution:");
            foreach (var kvp in spriteCount)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value} instances");
            }
            
            Debug.Log($"Available mappings: {spriteMappings?.Count ?? 0}");
            if (spriteMappings != null)
            {
                foreach (var mapping in spriteMappings)
                {
                    Debug.Log($"  Mapping: {mapping.GetDescription()}");
                }
            }
        }
    }
    
    /// <summary>
    /// Reset sprites back to default state (useful for testing)
    /// </summary>
    [ContextMenu("Reset Sprites to Default")]
    public void ResetSpritesToDefault()
    {
        if (showDebugLogs)
        {
            Debug.Log($"=== RESETTING SPRITES TO DEFAULT ===");
        }
        
        int changedSpriteCount = 0;
        
        if (spriteMappings != null && spriteMappings.Count > 0)
        {
            // Iterate through cached sprite renderers to reset colored sprites back to default
            foreach (SpriteRenderer renderer in cachedSpriteRenderers)
            {
                if (renderer != null && renderer.sprite != null)
                {
                    // Check each mapping to see if this colored sprite should be reset to default
                    foreach (SpriteMapping mapping in spriteMappings)
                    {
                        if (mapping.IsValid())
                        {
                            // Check tag filter if specified
                            if (!string.IsNullOrEmpty(mapping.targetTag) && !renderer.gameObject.CompareTag(mapping.targetTag))
                            {
                                continue;
                            }
                            
                            // Check if current sprite matches the colored sprite in this mapping
                            Sprite coloredSprite = mapping.GetColoredSprite();
                            if (coloredSprite != null && renderer.sprite.name == coloredSprite.name)
                            {
                                Sprite defaultSprite = mapping.GetDefaultSprite();
                                if (defaultSprite != null)
                                {
                                    string oldSpriteName = renderer.sprite.name;
                                    renderer.sprite = defaultSprite;
                                    changedSpriteCount++;
                                    
                                    if (showDebugLogs)
                                    {
                                        Debug.Log($"✓ Reset: {renderer.gameObject.name} - {oldSpriteName} -> {defaultSprite.name}");
                                    }
                                    break; // Found a match, no need to check other mappings
                                }
                            }
                        }
                    }
                }
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"=== RESET COMPLETE: {changedSpriteCount} sprites reset to default ===");
        }
        
        // Deactivate spotlight objects
        DeactivateSpotlights();
        
        hasChangedSprites = false;
        
        // Restart terminal checking if auto-check is enabled
        if (autoCheckTerminals)
        {
            CancelInvoke(nameof(CheckTerminalStatus));
            InvokeRepeating(nameof(CheckTerminalStatus), 1f, checkInterval);
        }
    }
    
    /// <summary>
    /// Refresh the cached sprite renderers and precomputed data
    /// Call this if GameObjects with sprite renderers are added/removed at runtime
    /// </summary>
    [ContextMenu("Refresh Sprite Cache")]
    public void RefreshSpriteCache()
    {
        if (showDebugLogs)
        {
            Debug.Log("Refreshing sprite renderer cache...");
        }
        
        PreloadSpriteRenderers();
    }
    
    /// <summary>
    /// Force change sprites regardless of terminal status (useful for testing)
    /// </summary>
    [ContextMenu("Force Change Sprites")]
    public void ForceChangeSprites()
    {
        ChangeSpritesToColored();
    }
    
    /// <summary>
    /// Manual method to check terminal status once
    /// </summary>
    [ContextMenu("Check Terminals Now")]
    public void CheckTerminalsManually()
    {
        CheckTerminalStatus();
    }
    
    /// <summary>
    /// Test method to toggle spotlights manually
    /// </summary>
    [ContextMenu("Toggle Spotlights")]
    public void ToggleSpotlights()
    {
        if (hasChangedSprites)
        {
            DeactivateSpotlights();
        }
        else
        {
            ActivateSpotlights();
        }
    }
    
    /// <summary>
    /// Get info about current state
    /// </summary>
    public bool HasChangedSprites()
    {
        return hasChangedSprites;
    }
    
    public int GetCachedRendererCount()
    {
        return cachedSpriteRenderers.Count;
    }
    
    public int GetPrecomputedChangeCount()
    {
        return renderersToChange.Count;
    }
}
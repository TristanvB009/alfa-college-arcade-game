using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[System.Serializable]
public class TileMapping
{
    [Header("Default Tile (What to Replace)")]
    [Tooltip("The default/turned off tile (can be TileBase or prefab)")]
    public TileBase defaultTile;
    
    [Tooltip("The default/turned off prefab (alternative to TileBase above)")]
    public GameObject defaultPrefab;
    
    [Header("Colored Tile (What to Replace With)")]
    [Tooltip("The colored/turned on tile (can be TileBase or prefab)")]
    public TileBase coloredTile;
    
    [Tooltip("The colored/turned on prefab (alternative to TileBase above)")]
    public GameObject coloredPrefab;
    
    /// <summary>
    /// Get the effective default tile, prioritizing TileBase over prefab
    /// </summary>
    public TileBase GetDefaultTile()
    {
        if (defaultTile != null)
        {
            return defaultTile;
        }
        
        if (defaultPrefab != null)
        {
            return CreateTileFromPrefab(defaultPrefab);
        }
        
        return null;
    }
    
    /// <summary>
    /// Get the effective colored tile, prioritizing TileBase over prefab
    /// </summary>
    public TileBase GetColoredTile()
    {
        if (coloredTile != null)
        {
            return coloredTile;
        }
        
        if (coloredPrefab != null)
        {
            return CreateTileFromPrefab(coloredPrefab);
        }
        
        return null;
    }
    
    /// <summary>
    /// Creates a tile from a prefab GameObject
    /// </summary>
    private TileBase CreateTileFromPrefab(GameObject prefab)
    {
        if (prefab == null) return null;
        
        // First, check if the prefab itself is a TileBase (for Tile assets made into prefabs)
        TileBase tileComponent = prefab.GetComponent<TileBase>();
        if (tileComponent != null)
        {
            return tileComponent;
        }
        
        // Check if any child has a TileBase component
        tileComponent = prefab.GetComponentInChildren<TileBase>();
        if (tileComponent != null)
        {
            return tileComponent;
        }
        
        // For GameObjects that aren't TileBase components, we need to create a Sprite tile
        // and use the prefab's sprite
        SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = prefab.GetComponentInChildren<SpriteRenderer>();
        }
        
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            // Create a new Sprite tile using the prefab's sprite
            UnityEngine.Tilemaps.Tile newTile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
            newTile.sprite = spriteRenderer.sprite;
            
            Debug.Log($"Created Sprite tile from prefab {prefab.name} using sprite {spriteRenderer.sprite.name}");
            
            return newTile;
        }
        
        Debug.LogWarning($"Prefab {prefab.name} doesn't have a TileBase component or SpriteRenderer with sprite. Cannot create tile from prefab.");
        return null;
    }
    
    /// <summary>
    /// Validate this tile mapping
    /// </summary>
    public bool IsValid()
    {
        // At least one default source (TileBase or prefab)
        bool hasDefault = defaultTile != null || defaultPrefab != null;
        
        // At least one colored source (TileBase or prefab)  
        bool hasColored = coloredTile != null || coloredPrefab != null;
        
        return hasDefault && hasColored;
    }
    
    /// <summary>
    /// Get description of this mapping for debugging
    /// </summary>
    public string GetDescription()
    {
        string defaultSource = defaultTile != null ? $"TileBase({defaultTile.name})" : 
                              defaultPrefab != null ? $"Prefab({defaultPrefab.name})" : "None";
        
        string coloredSource = coloredTile != null ? $"TileBase({coloredTile.name})" : 
                              coloredPrefab != null ? $"Prefab({coloredPrefab.name})" : "None";
        
        return $"{defaultSource} -> {coloredSource}";
    }
}

public class TilemapChanger : MonoBehaviour
{
    [Header("Tilemap Reference")]
    [SerializeField] private Tilemap targetTilemap;
    
    [Header("Tile Mappings")]
    [Tooltip("List of tile pairs - default tiles will be replaced with colored tiles")]
    [SerializeField] private List<TileMapping> tileMappings = new List<TileMapping>();
    
    [Header("Terminal Integration")]
    [Tooltip("Check for terminal completion automatically")]
    [SerializeField] private bool autoCheckTerminals = true;
    
    [Tooltip("How often to check terminal status (in seconds)")]
    [SerializeField] private float checkInterval = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool hasChangedTiles = false;
    
    void Start()
    {
        // Get tilemap component if not assigned
        if (targetTilemap == null)
        {
            targetTilemap = GetComponent<Tilemap>();
        }
        
        // Validate setup
        if (targetTilemap == null)
        {
            Debug.LogError($"TilemapChanger on {gameObject.name}: No Tilemap found! Please assign a Tilemap or attach this script to a GameObject with a Tilemap component.");
            enabled = false;
            return;
        }
        
        // Start checking terminals if auto-check is enabled
        if (autoCheckTerminals)
        {
            InvokeRepeating(nameof(CheckTerminalStatus), 1f, checkInterval);
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"TilemapChanger initialized with {tileMappings.Count} tile mappings on tilemap: {targetTilemap.name}");
        }
    }
    

    private void CheckTerminalStatus()
    {
        // Only check if we haven't already changed the tiles
        if (hasChangedTiles) return;
        
        // Check if all terminals are completed using the static method from PowerTerminalMinigame
        if (PowerTerminalMinigame.AreAllTerminalsCompleted())
        {
            if (showDebugLogs)
            {
                Debug.Log("All terminals completed! Changing tilemap to colored tiles...");
            }
            
            ChangeTilesToColored();
            
            // Stop checking since we've completed the change
            CancelInvoke(nameof(CheckTerminalStatus));
            hasChangedTiles = true;
        }
    }
    
    /// <summary>
    /// Manually trigger the tile change (useful for testing or other triggers)
    /// </summary>
    [ContextMenu("Change Tiles to Colored")]
    public void ChangeTilesToColored()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("TilemapChanger: No tilemap assigned!");
            return;
        }
        
        if (tileMappings == null || tileMappings.Count == 0)
        {
            Debug.LogWarning("TilemapChanger: No tile mappings configured!");
            return;
        }
        
        Debug.Log($"=== STARTING TILE CHANGE PROCESS ===");
        Debug.Log($"Tilemap: {targetTilemap.name}");
        Debug.Log($"Available mappings: {tileMappings.Count}");
        
        // Log all available mappings
        foreach (var mapping in tileMappings)
        {
            Debug.Log($"Mapping: {mapping.GetDescription()}");
        }
        
        int changedTileCount = 0;
        int totalTilesChecked = 0;
        
        // Get the bounds of the tilemap
        BoundsInt bounds = targetTilemap.cellBounds;
        Debug.Log($"Tilemap bounds: {bounds}");
        
        // Create arrays to store positions and new tiles
        List<Vector3Int> positionsToChange = new List<Vector3Int>();
        List<TileBase> newTiles = new List<TileBase>();
        List<TileBase> oldTiles = new List<TileBase>();
        
        // Iterate through all tiles in the tilemap
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            TileBase currentTile = targetTilemap.GetTile(position);
            totalTilesChecked++;
            
            if (currentTile != null)
            {
                // Debug every tile found
                if (showDebugLogs && totalTilesChecked <= 10) // Limit initial debug spam
                {
                    Debug.Log($"Found tile at {position}: {currentTile.name} (Type: {currentTile.GetType().Name})");
                }
                
                // Check each mapping to see if this tile should be replaced
                foreach (TileMapping mapping in tileMappings)
                {
                    if (mapping.IsValid())
                    {
                        // Check if current tile matches the default tile in this mapping
                        TileBase defaultTile = mapping.GetDefaultTile();
                        
                        if (defaultTile != null && currentTile.name == defaultTile.name)
                        {
                            // Determine if we're going to a tile or a prefab
                            if (mapping.coloredTile != null)
                            {
                                // TileBase to TileBase conversion
                                positionsToChange.Add(position);
                                newTiles.Add(mapping.coloredTile);
                                oldTiles.Add(currentTile);
                                changedTileCount++;
                                
                                Debug.Log($"✓ MATCH FOUND at {position}: {currentTile.name} -> {mapping.coloredTile.name}");
                            }
                            else if (mapping.coloredPrefab != null)
                            {
                                // TileBase to Prefab conversion - remove tile and instantiate prefab
                                try
                                {
                                    // Remove the tile first
                                    targetTilemap.SetTile(position, null);
                                    
                                    // Convert tile position to world position
                                    Vector3 worldPosition = targetTilemap.CellToWorld(position);
                                    
                                    // Adjust position to center of tile
                                    worldPosition.x += targetTilemap.cellSize.x * 0.5f;
                                    worldPosition.y += targetTilemap.cellSize.y * 0.5f;
                                    
                                    // Instantiate the prefab at the world position
                                    GameObject instantiatedPrefab = Instantiate(mapping.coloredPrefab, worldPosition, Quaternion.identity);
                                    
                                    // Parent it to this transform for organization
                                    instantiatedPrefab.transform.SetParent(transform);
                                    
                                    changedTileCount++;
                                    
                                    Debug.Log($"✓ PREFAB INSTANTIATED at {position}: Removed {currentTile.name}, instantiated {mapping.coloredPrefab.name}");
                                }
                                catch (System.Exception e)
                                {
                                    Debug.LogError($"✗ EXCEPTION instantiating prefab at {position}: {e.Message}");
                                }
                            }
                            
                            break; // Found a match, no need to check other mappings
                        }
                    }
                }
            }
        }
        
        Debug.Log($"=== SCAN COMPLETE ===");
        Debug.Log($"Total tiles checked: {totalTilesChecked}");
        Debug.Log($"Tiles to change: {changedTileCount}");
        
        // Apply all tile changes
        if (positionsToChange.Count > 0)
        {
            Debug.Log($"=== APPLYING {positionsToChange.Count} TILE CHANGES ===");
            
            for (int i = 0; i < positionsToChange.Count; i++)
            {
                Vector3Int pos = positionsToChange[i];
                TileBase oldTile = oldTiles[i];
                TileBase newTile = newTiles[i];
                
                Debug.Log($"Changing tile {i+1}/{positionsToChange.Count} at {pos}: {oldTile.name} -> {newTile.name}");
                
                try
                {
                    // Store what was there before
                    TileBase beforeChange = targetTilemap.GetTile(pos);
                    
                    // Apply the change
                    targetTilemap.SetTile(pos, newTile);
                    
                    // Verify the change worked
                    TileBase afterChange = targetTilemap.GetTile(pos);
                    
                    if (afterChange == newTile)
                    {
                        Debug.Log($"✓ SUCCESS: Tile at {pos} changed to {afterChange?.name ?? "null"}");
                    }
                    else if (afterChange == beforeChange)
                    {
                        Debug.LogError($"✗ FAILED: Tile at {pos} remained {afterChange?.name ?? "null"} (SetTile had no effect)");
                    }
                    else
                    {
                        Debug.LogWarning($"? UNEXPECTED: Tile at {pos} became {afterChange?.name ?? "null"} instead of {newTile?.name ?? "null"}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"✗ EXCEPTION setting tile at {pos}: {e.Message}\nStack: {e.StackTrace}");
                }
            }
        }
        else
        {
            Debug.LogWarning("No tiles were scheduled for changes. Check your tile mappings.");
        }
        
        Debug.Log($"=== TILE CHANGE PROCESS COMPLETE ===");
        Debug.Log($"Changed {changedTileCount} tiles to their colored versions");
        
        hasChangedTiles = true;
    }

    /// <summary>
    /// Debug method to examine what's currently in the tilemap
    /// </summary>
    [ContextMenu("Debug Current Tilemap")]
    public void DebugCurrentTilemap()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("No tilemap assigned!");
            return;
        }
        
        Debug.Log($"=== DEBUGGING TILEMAP: {targetTilemap.name} ===");
        
        BoundsInt bounds = targetTilemap.cellBounds;
        Debug.Log($"Tilemap bounds: {bounds}");
        
        Dictionary<string, int> tileCount = new Dictionary<string, int>();
        int totalTiles = 0;
        
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            TileBase tile = targetTilemap.GetTile(position);
            if (tile != null)
            {
                totalTiles++;
                string tileName = tile.name;
                
                if (tileCount.ContainsKey(tileName))
                {
                    tileCount[tileName]++;
                }
                else
                {
                    tileCount[tileName] = 1;
                    Debug.Log($"Found tile type: {tileName} (Type: {tile.GetType().Name}, Instance: {tile.GetInstanceID()})");
                }
            }
        }
        
        Debug.Log($"Total tiles found: {totalTiles}");
        Debug.Log("Tile distribution:");
        foreach (var kvp in tileCount)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value} instances");
        }
        
        Debug.Log($"Available mappings: {tileMappings?.Count ?? 0}");
        if (tileMappings != null)
        {
            foreach (var mapping in tileMappings)
            {
                Debug.Log($"  Mapping: {mapping.GetDescription()}");
            }
        }
    }
    
    /// <summary>
    /// Reset tiles back to default state (useful for testing)
    /// </summary>
    [ContextMenu("Reset Tiles to Default")]
    public void ResetTilesToDefault()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("TilemapChanger: No tilemap assigned!");
            return;
        }
        
        if (tileMappings == null || tileMappings.Count == 0)
        {
            Debug.LogWarning("TilemapChanger: No tile mappings configured!");
            return;
        }
        
        int changedTileCount = 0;
        
        // Get the bounds of the tilemap
        BoundsInt bounds = targetTilemap.cellBounds;
        
        // Create arrays to store positions and new tiles
        List<Vector3Int> positionsToChange = new List<Vector3Int>();
        List<TileBase> newTiles = new List<TileBase>();
        
        // Also destroy any instantiated prefabs (children of this transform)
        int destroyedPrefabs = 0;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
            destroyedPrefabs++;
        }
        
        // Iterate through all tiles in the tilemap
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            TileBase currentTile = targetTilemap.GetTile(position);
            
            if (currentTile != null)
            {
                // Check each mapping to see if this colored tile should be reset to default
                foreach (TileMapping mapping in tileMappings)
                {
                    if (mapping.IsValid())
                    {
                        // Check if current tile matches the colored tile in this mapping
                        if (mapping.coloredTile != null && currentTile.name == mapping.coloredTile.name)
                        {
                            TileBase defaultTile = mapping.GetDefaultTile();
                            if (defaultTile != null)
                            {
                                positionsToChange.Add(position);
                                newTiles.Add(defaultTile);
                                changedTileCount++;
                            }
                            break; // Found a match, no need to check other mappings
                        }
                    }
                }
            }
        }
        
        // Apply all tile changes at once
        for (int i = 0; i < positionsToChange.Count; i++)
        {
            targetTilemap.SetTile(positionsToChange[i], newTiles[i]);
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"TilemapChanger: Reset {changedTileCount} tiles to their default versions and destroyed {destroyedPrefabs} instantiated prefabs");
        }
        
        hasChangedTiles = false;
        
        // Restart terminal checking if auto-check is enabled
        if (autoCheckTerminals && !IsInvoking(nameof(CheckTerminalStatus)))
        {
            InvokeRepeating(nameof(CheckTerminalStatus), 1f, checkInterval);
        }
    }
    
    /// <summary>
    /// Add a regular tile mapping programmatically
    /// </summary>
    /// <param name="defaultTile">The tile to replace</param>
    /// <param name="coloredTile">The tile to replace it with</param>
    public void AddTileMapping(TileBase defaultTile, TileBase coloredTile)
    {
        if (defaultTile == null || coloredTile == null)
        {
            Debug.LogWarning("TilemapChanger: Cannot add tile mapping with null tiles");
            return;
        }
        
        // Add to the serialized list
        tileMappings.Add(new TileMapping 
        { 
            defaultTile = defaultTile, 
            coloredTile = coloredTile 
        });
    }
    
    /// <summary>
    /// Add a mixed tile mapping programmatically (TileBase to prefab or vice versa)
    /// </summary>
    /// <param name="defaultTile">The default tile (can be null if using defaultPrefab)</param>
    /// <param name="defaultPrefab">The default prefab (can be null if using defaultTile)</param>
    /// <param name="coloredTile">The colored tile (can be null if using coloredPrefab)</param>
    /// <param name="coloredPrefab">The colored prefab (can be null if using coloredTile)</param>
    public void AddMixedMapping(TileBase defaultTile, GameObject defaultPrefab, TileBase coloredTile, GameObject coloredPrefab)
    {
        TileMapping newMapping = new TileMapping
        {
            defaultTile = defaultTile,
            defaultPrefab = defaultPrefab,
            coloredTile = coloredTile,
            coloredPrefab = coloredPrefab
        };
        
        if (!newMapping.IsValid())
        {
            Debug.LogWarning("TilemapChanger: Cannot add invalid mapping. Must have at least one default and one colored tile/prefab.");
            return;
        }
        
        tileMappings.Add(newMapping);
    }
    
    /// <summary>
    /// Add a prefab tile mapping programmatically
    /// </summary>
    /// <param name="defaultPrefab">The prefab to replace</param>
    /// <param name="coloredPrefab">The prefab to replace it with</param>
    public void AddPrefabMapping(GameObject defaultPrefab, GameObject coloredPrefab)
    {
        AddMixedMapping(null, defaultPrefab, null, coloredPrefab);
    }
    
    /// <summary>
    /// Get the current tile change status
    /// </summary>
    /// <returns>True if tiles have been changed to colored versions</returns>
    public bool HasChangedToColored()
    {
        return hasChangedTiles;
    }
    
    /// <summary>
    /// Manually set the terminal checking state
    /// </summary>
    /// <param name="enabled">Whether to automatically check terminals</param>
    public void SetAutoCheckTerminals(bool enabled)
    {
        autoCheckTerminals = enabled;
        
        if (enabled && !hasChangedTiles && !IsInvoking(nameof(CheckTerminalStatus)))
        {
            InvokeRepeating(nameof(CheckTerminalStatus), 1f, checkInterval);
        }
        else if (!enabled && IsInvoking(nameof(CheckTerminalStatus)))
        {
            CancelInvoke(nameof(CheckTerminalStatus));
        }
    }
    
    void OnValidate()
    {
        // Validate tile mappings in the inspector
        for (int i = tileMappings.Count - 1; i >= 0; i--)
        {
            TileMapping mapping = tileMappings[i];
            
            if (!mapping.IsValid())
            {
                Debug.LogWarning($"TilemapChanger: Tile mapping {i} is invalid. {mapping.GetDescription()}. Please assign at least one default and one colored tile/prefab.");
            }
            
            // Additional validation for prefab components
            if (mapping.defaultPrefab != null && mapping.defaultPrefab.GetComponent<TileBase>() == null && mapping.defaultPrefab.GetComponentInChildren<TileBase>() == null)
            {
                Debug.LogWarning($"TilemapChanger: Default prefab '{mapping.defaultPrefab.name}' in mapping {i} doesn't have a TileBase component. This may cause issues.");
            }
            
            if (mapping.coloredPrefab != null && mapping.coloredPrefab.GetComponent<TileBase>() == null && mapping.coloredPrefab.GetComponentInChildren<TileBase>() == null)
            {
                Debug.LogWarning($"TilemapChanger: Colored prefab '{mapping.coloredPrefab.name}' in mapping {i} doesn't have a TileBase component. This may cause issues.");
            }
        }
    }
}
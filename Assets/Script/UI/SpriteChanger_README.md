# SpriteChanger Documentation

## Overview
SpriteChanger is a Unity component that automatically changes sprites on GameObjects when all PowerTerminal puzzles are completed. It's based on the TilemapChanger but works with individual GameObjects and their SpriteRenderer components instead of tilemaps.

## How to Use

### 1. Setup the Component
1. Add the SpriteChanger component to any GameObject in your scene
2. Configure the settings in the inspector

### 2. Configure Target Settings
- **Target Parent**: Optional transform to search for sprite renderers within. Leave empty to search entire scene, or assign a parent object to limit the search to its children.

### 3. Create Sprite Mappings
Each SpriteMapping defines a pair of sprites to swap:

#### Default Sprite (what to look for):
- **Default Sprite**: Direct sprite reference to find
- **Default Prefab**: Alternative - prefab containing the sprite (SpriteRenderer component will be extracted)

#### Colored Sprite (what to replace with):
- **Colored Sprite**: Direct sprite reference to replace with
- **Colored Prefab**: Alternative - prefab containing the replacement sprite

#### Options:
- **Target Tag**: Optional tag filter. Only GameObjects with this tag will be affected by this mapping. Leave empty to affect all objects.

### 4. Configure Behavior
- **Auto Check Terminals**: Automatically monitor terminal completion (recommended: true)
- **Check Interval**: How often to check terminal status in seconds (recommended: 1.0)
- **Preload Sprite Renderers**: Cache sprite renderers at start for better performance (recommended: true)
- **Show Debug Logs**: Enable detailed logging for debugging

### 5. Example Mapping Setup
```
Mapping 1:
- Default Sprite: "wall_normal"
- Colored Sprite: "wall_powered" 
- Target Tag: "Wall" (only affects GameObjects tagged "Wall")

Mapping 2:
- Default Prefab: NormalLightPrefab (has SpriteRenderer with "light_off" sprite)
- Colored Prefab: PoweredLightPrefab (has SpriteRenderer with "light_on" sprite)  
- Target Tag: "" (affects all objects)
```

## Key Features

### Performance Optimized
- **Preloading**: Finds all relevant sprite renderers at start
- **Precomputed Changes**: Calculates all sprite changes in advance
- **Instant Execution**: No lag when terminals complete - all changes applied immediately
- **Batch Operations**: Efficient sprite swapping

### Smart Target Filtering
- **Tag Support**: Use tags to control which objects each mapping affects
- **Parent Limiting**: Search only within specific transform hierarchy
- **Automatic Discovery**: Finds all SpriteRenderer components automatically

### Debug Features
- **Context Menu Actions**: Right-click the component for testing options:
  - "Change Sprites to Colored" - Force sprite change
  - "Reset Sprites to Default" - Restore original sprites  
  - "Debug Current Sprites" - Log all found sprites
  - "Refresh Sprite Cache" - Rebuild sprite renderer cache
  - "Check Terminals Now" - Manual terminal status check

### Integration with PowerTerminal System
- Uses `PowerTerminalMinigame.AreAllTerminalsCompleted()` for terminal status
- Automatically starts/stops monitoring when appropriate
- Works seamlessly with existing terminal completion system

## Comparison with TilemapChanger

| Feature | TilemapChanger | SpriteChanger |
|---------|----------------|---------------|
| Target | Tilemap tiles | GameObject sprites |
| Method | Tile replacement | SpriteRenderer.sprite assignment |
| Filtering | Tile type matching | Tag-based filtering |
| Performance | Batch tile operations | Precomputed sprite references |
| Scope | Single tilemap | Multiple GameObjects |

## Best Practices

1. **Use Tag Filtering**: Assign meaningful tags to control which objects are affected
2. **Limit Search Scope**: Set Target Parent to avoid unnecessary object searches
3. **Test with Context Menu**: Use the debug methods to verify mappings work correctly
4. **Organize Mappings**: Group related sprite changes together logically
5. **Enable Debug Logs**: Use during development to troubleshoot issues

## Common Use Cases

- **Environmental Changes**: Lights turning on, machinery activating
- **Visual Feedback**: Status indicators, progress displays  
- **Atmospheric Effects**: Area lighting, powered vs unpowered states
- **UI Updates**: Button states, completion markers
- **Decoration Changes**: Banners, signs, decorative elements

## Troubleshooting

- **No sprites changing**: Check that sprite names match exactly between default and found sprites
- **Wrong objects affected**: Verify tag filters are set correctly  
- **Performance issues**: Ensure preloading is enabled and target parent is set appropriately
- **Changes not triggering**: Verify terminal system is working with "Check Terminals Now"
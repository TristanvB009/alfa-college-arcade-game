using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Parts")]
    [Tooltip("Assign your 8 health bar parts in order from 1 to 8 (1 = lowest health part, 8 = highest health part)")]
    public GameObject[] healthBarParts = new GameObject[8];
    
    [Header("Health Reference")]
    [Tooltip("Reference to the player's Health component")]
    public Health playerHealth;
    
    private int lastKnownHealth = -1; // Track the last known health to avoid unnecessary updates
    
    private void Start()
    {
        // Find the player's Health component if not assigned
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<Health>();
            if (playerHealth == null)
            {
                Debug.LogError("HealthBar: No Health component found! Please assign the player's Health component.");
                return;
            }
        }
        
        // Validate health bar parts array
        ValidateHealthBarParts();
        
        // Initial health bar update
        UpdateHealthBar();
    }
    
    private void Update()
    {
        // Only update if health has changed
        if (playerHealth != null && playerHealth.currentHealth != lastKnownHealth)
        {
            UpdateHealthBar();
        }
    }
    
    /// <summary>
    /// Updates the health bar visibility based on current health
    /// </summary>
    public void UpdateHealthBar()
    {
        if (playerHealth == null) return;
        
        int currentHealth = playerHealth.currentHealth;
        lastKnownHealth = currentHealth;
        
        // Update each health bar part
        for (int i = 0; i < healthBarParts.Length; i++)
        {
            if (healthBarParts[i] != null)
            {
                // Part index i corresponds to health value (i + 1)
                // Show the part if current health is greater than the part's health value
                bool shouldBeVisible = currentHealth > i;
                healthBarParts[i].SetActive(shouldBeVisible);
            }
        }
        
        // Debug log for testing
        Debug.Log($"HealthBar: Updated for {currentHealth}/{playerHealth.maxHealth} health");
    }
    
    /// <summary>
    /// Manually update the health bar (useful for immediate updates after damage/healing)
    /// </summary>
    public void ForceUpdate()
    {
        lastKnownHealth = -1; // Force update on next frame
        UpdateHealthBar();
    }
    
    /// <summary>
    /// Validates that all health bar parts are assigned
    /// </summary>
    private void ValidateHealthBarParts()
    {
        bool allPartsAssigned = true;
        
        for (int i = 0; i < healthBarParts.Length; i++)
        {
            if (healthBarParts[i] == null)
            {
                Debug.LogWarning($"HealthBar: Health bar part {i + 1} is not assigned!");
                allPartsAssigned = false;
            }
        }
        
        if (allPartsAssigned)
        {
            Debug.Log("HealthBar: All 8 health bar parts are properly assigned.");
        }
    }
    
    /// <summary>
    /// Test function to simulate health changes (for testing purposes)
    /// </summary>
    [ContextMenu("Test - Simulate Damage")]
    public void TestDamage()
    {
        if (playerHealth != null && playerHealth.currentHealth > 0)
        {
            playerHealth.currentHealth--;
            UpdateHealthBar();
        }
    }
    
    /// <summary>
    /// Test function to simulate healing (for testing purposes)
    /// </summary>
    [ContextMenu("Test - Simulate Heal")]
    public void TestHeal()
    {
        if (playerHealth != null && playerHealth.currentHealth < playerHealth.maxHealth)
        {
            playerHealth.currentHealth++;
            UpdateHealthBar();
        }
    }
}
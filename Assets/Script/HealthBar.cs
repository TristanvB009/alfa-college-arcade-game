using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Parts")]
    [Tooltip("Assign your 8 health bar parts in order from 1 to 8 (1 = lowest health part, 8 = highest health part)")]
    public GameObject[] healthBarParts = new GameObject[8];
    
    [Header("Health Reference")]
    [Tooltip("Reference to the player's Health component")]
    public Health playerHealth;
    
    private int lastKnownHealth = -1; // Track the last known health
    
    private void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<Health>();
            if (playerHealth == null)
            {
                Debug.LogError("HealthBar: No Health component found! Please assign the player's Health component.");
                return;
            }
        }
        
        ValidateHealthBarParts();
        
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
    
    /// Updates the health bar visibility based on current health
    public void UpdateHealthBar()
    {
        if (playerHealth == null) return;
        
        int currentHealth = Mathf.Clamp(playerHealth.currentHealth, 0, playerHealth.maxHealth);
        lastKnownHealth = playerHealth.currentHealth; // Store the actual value for comparison
        
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
    
    /// Manually update the health bar
    public void ForceUpdate()
    {
        lastKnownHealth = -1; // Force update on next frame
        UpdateHealthBar();
    }
    
    /// Validates that all health bar parts are assigned
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
}
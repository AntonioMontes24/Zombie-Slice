using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;

    private void Update()
    {

        UpdateStaminaUI();

    }

    void UpdateStaminaUI()
    {
        float staminaPercent = playerMovement.currentStamina / playerMovement.maxStamina;

        // Update fill amount
        GameManager.instance.playerStaminaBar.fillAmount = staminaPercent;
        GameManager.instance.playerStaminaBar.color = Color.lightBlue;
        GameManager.instance.playerStaminaText.text = Mathf.CeilToInt(playerMovement.currentStamina).ToString();

    }

   }

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image illustrationImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject attackBadge;
    [SerializeField] private GameObject healthBadge;
    [SerializeField] private GameObject spellBadge; 


    [Header("Display Data")]
    private CardData _data;
    public CardData Data => _data;
    
    public void Bind(CardData data)
    {
        _data = data;
        illustrationImage.sprite = data.Illustration; 
        nameText.text = data.CardName;
        descriptionText.text = data.Description; 
        costText.text = data.Cost.ToString();

        //show stat cards based on card type
        bool showStats = data.Type == CardType.Character; 
        attackBadge.SetActive(showStats);
        healthBadge.SetActive(showStats);
        spellBadge.SetActive(!showStats);
        
        attackText.text = data.Attack.ToString();
        healthText.text = data.Health.ToString();
    }
}

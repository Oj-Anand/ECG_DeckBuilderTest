using UnityEngine;

[CreateAssetMenu(fileName = "CardData_", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    
    [Header("Identity")]
    [SerializeField] private string cardId;
    [SerializeField] private string cardName;
    [SerializeField] private CardType cardType; 

    [Header("Stats")]
    [SerializeField] private int cost;
    [SerializeField] private int attack;
    [SerializeField] private int health;

    [Header("Presentatrion")]
    [SerializeField] private Sprite illustration;
    [TextArea(2, 4)]
    [SerializeField] private string description; 

    public string CardId => cardId;
    public string CardName => cardName;
    public int Cost => cost;
    public int Attack => attack;
    public int Health => health;
    public Sprite Illustration => illustration;
    public string Description => description;
    public CardType Type => cardType;


}
public enum CardType { Character, Spell }


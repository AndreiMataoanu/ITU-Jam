using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BlackjackGame : MonoBehaviour
{
    public struct Card
    {
        public enum Rank { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }
        public enum Suit { Clubs, Diamonds, Hearts, Spades }

        public Rank rank;
        public Suit suit;

        //Calculates the numerical value of the card (Ace = 11, J/Q/K Faces = 10)
        public int GetValue()
        {
            if(rank >= Rank.Ten && rank <= Rank.King) return 10;
            if(rank == Rank.Ace) return 11;

            return (int) rank;
        }

        public override string ToString()
        {
            return $"{rank} of {suit}";
        }
    }

    //Manages the deck, including initialization, shuffling, and dealing
    public class Deck
    {
        private List<Card> cards = new List<Card>();

        public Deck()
        {
            InitializeDeck();
        }

        private void InitializeDeck()
        {
            cards.Clear();

            foreach(Card.Suit s in System.Enum.GetValues(typeof(Card.Suit)))
            {
                foreach(Card.Rank r in System.Enum.GetValues(typeof(Card.Rank)))
                {
                    cards.Add(new Card { rank = r, suit = s });
                }
            }
        }

        public void Shuffle()
        {
            //Shuffle implementation
            int n = cards.Count;

            while(n > 1)
            {
                n--;

                int k = Random.Range(0, n + 1);

                Card value = cards[k];

                cards[k] = cards[n];
                cards[n] = value;
            }
        }

        public Card DealCard()
        {
            if(cards.Count == 0)
            {
                InitializeDeck();

                Shuffle();

                Debug.Log("Deck was empty, re-shuffling new deck.");
            }

            Card dealtCard = cards[0];

            cards.RemoveAt(0);

            return dealtCard;
        }
    }

    public class CardInstance
    {
        public Card cardData;

        public CardDisplay displayComponent;

        public bool isHidden;

        public CardInstance(Card card, CardDisplay display, bool hidden = false)
        {
            cardData = card;
            displayComponent = display;
            isHidden = hidden;
        }
    }

    private Deck gameDeck;

    private List<CardInstance> playerHand = new List<CardInstance>();
    private List<CardInstance> dealerHand = new List<CardInstance>();
    private List<GameObject> activeCardObjects = new List<GameObject>();

    [Header("UI")]
    [SerializeField] private TMPro.TextMeshProUGUI playerScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI dealerScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI statusText;

    [SerializeField] private GameObject hitButton;
    [SerializeField] private GameObject standButton;

    [Header("Card Prefab")]
    [SerializeField] private GameObject cardPrefab;

    [SerializeField] private Transform playerCardPosition;
    [SerializeField] private Transform dealerCardPosition;

    [SerializeField] private float cardSpacing = 30.0f;

    private void Start()
    {
        gameDeck = new Deck();

        StartGame();
    }

    //Calculates the total value of a hand. Aces are 1 or 11.
    private int CalculateHandValue(List<CardInstance> hand)
    {
        //convert CardInstance list to Card list for easier processing.
        List<Card> cards = hand.Select(x => x.cardData).ToList();
        
        int value = 0;
        int aceCount = 0;

        foreach(Card card in cards)
        {
            int cardValue = card.GetValue();

            if(card.rank == Card.Rank.Ace)
            {
                aceCount++;
            }

            value += cardValue;
        }

        //adjust aces
        while(value > 21 && aceCount > 0)
        {
            value -= 10;
            aceCount--;
        }

        return value;
    }

    //Resets the game and deals cards
    public void StartGame()
    {
        foreach(GameObject cardObject in activeCardObjects) Destroy(cardObject);

        activeCardObjects.Clear();
        playerHand.Clear();
        dealerHand.Clear();
        gameDeck.Shuffle();

        DealCardToPlayer();
        DealCardToDealer(false); //Dealers first card is visible
        DealCardToPlayer();
        DealCardToDealer(true); //Dealers second card is hidden
        UpdateUI();

        statusText.text = "Game started! Your turn.";

        hitButton.SetActive(true);
        standButton.SetActive(true);

        CheckBlackjack();
    }

    private void CheckBlackjack()
    {
        if(CalculateHandValue(playerHand) == 21)
        {
            StartCoroutine(DealerTurnCoroutine(true));
        }
    }

    //Instantiates a card, sets its data, and adds it to the specified hand.
    private CardInstance DealCardInstance(List<CardInstance> hand, Transform parentTransform, bool isHidden)
    {
        Card newCardData = gameDeck.DealCard();

        Vector3 positionOffset = new Vector3(hand.Count * cardSpacing, 0, 0);

        GameObject cardObject = Instantiate(cardPrefab, parentTransform);

        cardObject.transform.localPosition = positionOffset;

        activeCardObjects.Add(cardObject);

        CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();

        cardDisplay.SetCard(newCardData, isHidden);

        CardInstance newCardInstance = new CardInstance(newCardData, cardDisplay, isHidden);

        hand.Add(newCardInstance);

        return newCardInstance;
    }

    private void DealCardToPlayer()
    {
        DealCardInstance(playerHand, playerCardPosition, false);
    }

    private void DealCardToDealer(bool isHidden)
    {
        DealCardInstance(dealerHand, dealerCardPosition, isHidden);
    }

    //Updates the score and checks for busts.
    private void UpdateUI(bool dealerHidden = true)
    {
        int playerValue = CalculateHandValue(playerHand);

        playerScoreText.text = $"Player Score: {playerValue}";

        //Show only the value of the dealer's visible cards
        int dealerVisibleValue = dealerHidden && dealerHand.Count > 1
            ? CalculateHandValue(dealerHand.Where(x => !x.isHidden).ToList()) //Calculate only visible cards
            : CalculateHandValue(dealerHand); //Calculate all cards

        dealerScoreText.text = $"Dealer Score: {(dealerHidden && dealerHand.Count > 1 ? $"{dealerVisibleValue} + ?" : dealerVisibleValue.ToString())}";

        if(playerValue > 21)
        {
            EndGame("Bust! You lose.");
        }
    }

    public void Hit()
    {
        DealCardToPlayer();
        UpdateUI();
    }

    public void Stand()
    {
        statusText.text = "Player stands. Dealer's turn...";

        hitButton.SetActive(false);
        standButton.SetActive(false);

        StartCoroutine(DealerTurnCoroutine());
    }

    private IEnumerator DealerTurnCoroutine(bool playerHasBlackjack = false)
    {
        //Reveals the dealers hidden card.
        CardInstance hiddenCard = dealerHand.FirstOrDefault(x => x.isHidden);

        if(hiddenCard != null)
        {
            hiddenCard.isHidden = false;
            hiddenCard.displayComponent.SetHidden(false);

            UpdateUI(false);

            yield return new WaitForSeconds(1.0f);
        }

        int dealerValue = CalculateHandValue(dealerHand);
        int playerValue = CalculateHandValue(playerHand);

        if(playerHasBlackjack && dealerValue != 21)
        {
            EndGame("Blackjack! You win!");

            yield break;
        }

        while(dealerValue < 17)
        {
            statusText.text = "Dealer hits...";

            DealCardToDealer(false);
            UpdateUI(false);

            dealerValue = CalculateHandValue(dealerHand);

            yield return new WaitForSeconds(1.5f);
        }

        //Show winner
        string resultMessage = DetermineWinner(playerValue, dealerValue);

        EndGame(resultMessage);
    }

    private string DetermineWinner(int playerValue, int dealerValue)
    {
        if(playerValue > 21)
        {
            return "Bust! You lose.";
        }
        else if(dealerValue > 21)
        {
            return "Dealer busts! You win!";
        }
        else if(playerValue > dealerValue)
        {
            return "You win!";
        }
        else if(dealerValue > playerValue)
        {
            return "Dealer wins.";
        }
        else
        {
            return "It's a tie.";
        }
    }

    private void EndGame(string message)
    {
        statusText.text = $"{message} Press Start Game to play again.";

        hitButton.SetActive(false);
        standButton.SetActive(false);
    }
}

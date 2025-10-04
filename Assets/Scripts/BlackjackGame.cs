using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BlackjackGame : MonoBehaviour
{
    [System.Serializable]
    public class CardVisuals
    {
        public Card.Rank rank;
        public Card.Suit suit;
        public GameObject cardPrefab;
    }

    public struct Card
    {
        public enum Rank { None = 0, Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }
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
                for(int r = (int)Card.Rank.Ace; r <= (int)Card.Rank.King; r++)
                {
                    Card.Rank rank = (Card.Rank)r;

                    cards.Add(new Card { rank = rank, suit = s });
                }
            }
        }

        public void Shuffle()
        {
            //Shuffle
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

    [Header("Betting UI")]
    [SerializeField] private TMPro.TextMeshProUGUI moneyText;
    [SerializeField] private TMPro.TextMeshProUGUI betText;

    [SerializeField] private GameObject betUpButton;
    [SerializeField] private GameObject betDownButton;
    [SerializeField] private GameObject dealButton;

    //Betting Variables
    private int playerMoney = 500;
    private int currentBet = 100;
    private const int betStep = 100;
    private const int minBet = 100;

    private bool isRoundActive = false;

    public int PlayerMoney
    {
        get { return playerMoney; }
        private set { playerMoney = value; }
    }

    [Header("Visual Setup")]
    [SerializeField] private List<CardVisuals> cardPrefabs = new List<CardVisuals>();

    private Dictionary<(Card.Rank, Card.Suit), GameObject> cardPrefabLookup;

    [SerializeField] private Transform playerCardPosition;
    [SerializeField] private Transform dealerCardPosition;

    [SerializeField] private float cardSpacing = 30.0f;
    private const float zOverlap = 0.05f;

    private void Start()
    {
        gameDeck = new Deck();

        InitializeCardLookup();
        StartGame();
    }

    private void UpdateBettingUI()
    {
        moneyText.text = $"Money: ${PlayerMoney}";
        betText.text = $"Current Bet: ${currentBet}";

        bool canBetUp = !isRoundActive && currentBet < PlayerMoney;
        betUpButton.SetActive(canBetUp);

        bool canBetDown = !isRoundActive && currentBet > minBet;
        betDownButton.SetActive(canBetDown);

        dealButton.SetActive(!isRoundActive && currentBet >= minBet && PlayerMoney >= currentBet);
    }

    public void IncreaseBet()
    {
        if(isRoundActive) return;

        int nextBet = currentBet + betStep;

        if(nextBet > PlayerMoney)
        {
            currentBet = PlayerMoney;
        }
        else
        {
            currentBet = nextBet;
        }

        UpdateBettingUI();
    }

    public void DecreaseBet()
    {
        if(isRoundActive) return;

        if(currentBet > minBet)
        {
            currentBet -= betStep;
        }

        if(currentBet < minBet)
        {
            currentBet = minBet;
        }

        UpdateBettingUI();
    }

    //Initializes the card prefab lookup dictionary for quick access.
    private void InitializeCardLookup()
    {
        cardPrefabLookup = new Dictionary<(Card.Rank, Card.Suit), GameObject>();

        foreach(var cardVisual in cardPrefabs)
        {
            if(cardVisual.rank != Card.Rank.None)
            {
                cardPrefabLookup.Add((cardVisual.rank, cardVisual.suit), cardVisual.cardPrefab);
            }
        }

        if(cardPrefabLookup.Count != 52)
        {
            Debug.LogWarning($"Card lookup only contains {cardPrefabLookup.Count} entries. Ensure all 52 cards are assigned in the Inspector!");
        }
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

    private void ClearTable()
    {
        foreach(GameObject cardObject in activeCardObjects)
        {
            Destroy(cardObject);
        }

        activeCardObjects.Clear();
        playerHand.Clear();
        dealerHand.Clear();
    }

    //Resets the game and deals cards
    public void StartGame()
    {
        ClearTable();

        gameDeck.Shuffle();

        isRoundActive = false;

        if(currentBet > PlayerMoney)
        {
            currentBet = PlayerMoney;
        }
        if(currentBet < minBet)
        {
            currentBet = minBet;
        }

        playerScoreText.text = "Player Score: 0";
        dealerScoreText.text = "Dealer Score: 0";

        UpdateBettingUI();

        statusText.text = PlayerMoney > 0
            ? $"Place your bet (Minimum ${minBet}). You have ${PlayerMoney}."
            : "GAME OVER. You ran out of money.";

        hitButton.SetActive(false);
        standButton.SetActive(false);

        if(PlayerMoney < minBet)
        {
            betUpButton.SetActive(false);
            betDownButton.SetActive(false);
            dealButton.SetActive(false);
        }
    }

    public void Deal()
    {
        if(isRoundActive || currentBet < minBet || PlayerMoney < currentBet) return;

        isRoundActive = true;

        betUpButton.SetActive(false);
        betDownButton.SetActive(false);
        dealButton.SetActive(false);
        gameDeck.Shuffle();

        DealCardToPlayer();
        DealCardToDealer(false); //Dealers first card is visible
        DealCardToPlayer();
        DealCardToDealer(true); //Dealers second card is hidden
        UpdateUI();

        statusText.text = $"Round started! Bet: ${currentBet}. Your turn.";

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

    private void UpdateHandVisuals(List<CardInstance> hand)
    {
        int count = hand.Count;

        if(count < 2)
        {
            if(count == 1)
            {
                hand[0].displayComponent.transform.localPosition = new Vector3(0f, 0, 0f);
            }

            return;
        }

        float anchorPos1X = 0f;
        float anchorPos2X = cardSpacing;
        float zPosCard2 = 2 * zOverlap;
        float zPosCard1 = 1 * zOverlap;

        hand[count - 1].displayComponent.transform.localPosition = new Vector3(anchorPos2X, 0, zPosCard2);
        hand[count - 2].displayComponent.transform.localPosition = new Vector3(anchorPos1X, 0, zPosCard1);

        for(int i = 0; i < count - 2; i++)
        {
            CardInstance card = hand[i];

            float xPos = (i + 1) * -cardSpacing;
            float zPos = i * -zOverlap;

            card.displayComponent.transform.localPosition = new Vector3(xPos, 0, zPos);
        }
    }

    //Instantiates a card, sets its data, and adds it to the specified hand.
    private CardInstance DealCardInstance(List<CardInstance> hand, Transform parentTransform, bool isHidden)
    {
        Card newCardData = gameDeck.DealCard();

        if(!cardPrefabLookup.TryGetValue((newCardData.rank, newCardData.suit), out GameObject cardPrefabToUse))
        {
            Debug.LogError($"Prefab not found for {newCardData.rank} of {newCardData.suit}! Dealing failed.");

            return null;
        }

        GameObject cardObject = Instantiate(cardPrefabToUse, parentTransform);

        activeCardObjects.Add(cardObject);

        CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();

        if(cardDisplay == null)
        {
            Debug.LogError($"CardDisplay component missing on prefab: {cardPrefabToUse.name}!");

            return null;
        }

        cardDisplay.SetHidden(isHidden);

        CardInstance newCardInstance = new CardInstance(newCardData, cardDisplay, isHidden);

        hand.Insert(0, newCardInstance);

        UpdateHandVisuals(hand);

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

        UpdateBettingUI();

        if(playerValue > 21)
        {
            EndGame("Bust! You lose.");
        }
    }

    public void Hit()
    {
        if(!isRoundActive) return;

        DealCardToPlayer();
        UpdateUI();
    }

    public void Stand()
    {
        if(!isRoundActive) return;

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
            UpdateHandVisuals(dealerHand);

            yield return new WaitForSeconds(1.0f);
        }

        int dealerValue = CalculateHandValue(dealerHand);
        int playerValue = CalculateHandValue(playerHand);

        if(playerHasBlackjack && dealerValue != 21)
        {
            EndGame("Blackjack! You win!");

            yield break;
        }
        else if(playerHasBlackjack && dealerValue == 21)
        {
            EndGame("Both have Blackjack! It's a tie.");

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
        isRoundActive = false;

        if(message.Contains("You win") || message.Contains("Blackjack! You win"))
        {
            PlayerMoney += currentBet;

            statusText.text = $"WIN! {message} You won ${currentBet}. Press Start Game to play again.";
        }
        else if(message.Contains("It's a tie"))
        {
            statusText.text = $"PUSH! {message} Your bet (${currentBet}) is returned. Press Start Game to play again.";
        }
        else
        {
            PlayerMoney -= currentBet;

            statusText.text = $"LOSS! {message} You lost ${currentBet}. Press Start Game to play again.";
        }

        UpdateUI(false);

        if(PlayerMoney < minBet)
        {
            statusText.text = $"GAME OVER! You ran out of money. Final total: ${PlayerMoney}";

            hitButton.SetActive(false);
            standButton.SetActive(false);
            betUpButton.SetActive(false);
            betDownButton.SetActive(false);
            dealButton.SetActive(false);

            return;
        }

        currentBet = currentBet > PlayerMoney ? PlayerMoney : currentBet;
        currentBet = currentBet < minBet ? minBet : currentBet;

        UpdateBettingUI();
    }
}

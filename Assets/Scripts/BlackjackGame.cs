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

        //Calculates the numerical value of the card (Ace is 11 by default, faces are 10)
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

    private Deck gameDeck;

    private List<Card> playerHand = new List<Card>();
    private List<Card> dealerHand = new List<Card>();

    [Header("UI")]
    public TMPro.TextMeshProUGUI playerScoreText;
    public TMPro.TextMeshProUGUI dealerScoreText;
    public TMPro.TextMeshProUGUI statusText;
    public GameObject hitButton;
    public GameObject standButton;

    private void Start()
    {
        gameDeck = new Deck();

        StartGame();
    }

    //Calculates the total value of a hand. Aces are 1 or 11.
    private int CalculateHandValue(List<Card> hand)
    {
        int value = 0;
        int aceCount = 0;

        foreach(Card card in hand)
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
            DealerTurn(true);
        }
    }

    private void DealCardToPlayer()
    {
        Card newCard = gameDeck.DealCard();

        playerHand.Add(newCard);

        Debug.Log($"Player received: {newCard}");
    }

    private void DealCardToDealer(bool isHidden)
    {
        Card newCard = gameDeck.DealCard();

        dealerHand.Add(newCard);

        Debug.Log($"Dealer received: {(isHidden ? "Hidden Card" : newCard.ToString())}");
    }

    //Updates the score and checks for busts.
    private void UpdateUI(bool dealerHidden = true)
    {
        int playerValue = CalculateHandValue(playerHand);

        playerScoreText.text = $"Player Score: {playerValue}";

        //Show only the value of the dealer's visible cards
        int dealerVisibleValue = dealerHidden && dealerHand.Count > 1
            ? CalculateHandValue(dealerHand.Take(1).ToList())
            : CalculateHandValue(dealerHand);

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

        DealerTurn();
    }

    private void DealerTurn(bool playerHasBlackjack = false)
    {
        //Reveal the hidden card
        UpdateUI(false);

        int dealerValue = CalculateHandValue(dealerHand);
        int playerValue = CalculateHandValue(playerHand);

        if(playerHasBlackjack && dealerValue != 21)
        {
            EndGame("Blackjack! You win!");

            return;
        }

        //Dealer hits until their score is 17 or greater
        StartCoroutine(DealerPlayCoroutine());
    }

    private IEnumerator DealerPlayCoroutine()
    {
        int dealerValue = CalculateHandValue(dealerHand);
        int playerValue = CalculateHandValue(playerHand);

        yield return new WaitForSeconds(1.0f);

        while(dealerValue < 17)
        {
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

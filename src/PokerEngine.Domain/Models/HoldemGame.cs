namespace PokerEngine.Domain.Models
{
    public abstract class HoldemGame<TPlayerCards> : PokerGame where TPlayerCards : HoldemPlayerCards
    {
        protected int MaxPlayersPerDeck => (52 - 9) / CardsPerPlayer;

        protected abstract int CardsPerPlayer { get; }


        protected readonly CardDeck _deck;
        protected readonly List<Card> _communityCards;
        protected readonly Dictionary<ushort, TPlayerCards> _playersCards;

        protected HoldemGame(ushort players, Func<IReadOnlyList<Card>, TPlayerCards> playerCardsFactory, CardDeck? deck = null)
        {
            if (players == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            Players = players;
            _deck = deck ?? new CardDeck();
            _communityCards = new List<Card>();

            if (players > MaxPlayersPerDeck)
            {
                throw new InvalidOperationException(
                    $"A standard deck cannot support {players} players. The maximum supported is {MaxPlayersPerDeck}.");
            }

            Stage = HoldemStage.PreFlop;

            Dictionary<ushort, List<Card>> playerCards = new Dictionary<ushort, List<Card>>();
            _playersCards = new Dictionary<ushort, TPlayerCards>();

            for (ushort playerIndex = 1; playerIndex <= players; playerIndex++)
            {
                playerCards[playerIndex] = new List<Card>();
            }

            for (ushort card = 0; card < CardsPerPlayer; card++)
            {
                for (ushort playerIndex = 1; playerIndex <= players; playerIndex++)
                {
                    playerCards[playerIndex].Add(_deck.Pick());
                }
            }

            foreach (KeyValuePair<ushort, List<Card>> player in playerCards)
            {
                _playersCards[player.Key] = playerCardsFactory(player.Value);
            }
        }

        public override ushort Players { get; }

        public HoldemStage Stage { get; protected set; }

        public IReadOnlyList<Card> CommunityCards => _communityCards.AsReadOnly();

        public IReadOnlyDictionary<ushort, TPlayerCards> PlayersCards => _playersCards;

        public IReadOnlyList<Card> Continue()
        {
            if (EqualityComparer<HoldemStage>.Default.Equals(Stage, HoldemStage.PreFlop))
            {
                BurnTwoCards();
                DealCommunityCards(3);
                Stage = HoldemStage.Flop;
            }
            else if (EqualityComparer<HoldemStage>.Default.Equals(Stage, HoldemStage.Flop))
            {
                BurnOneCard();
                DealCommunityCards(1);
                Stage = HoldemStage.Turn;
            }
            else if (EqualityComparer<HoldemStage>.Default.Equals(Stage, HoldemStage.Turn))
            {
                BurnOneCard();
                DealCommunityCards(1);
                Stage = HoldemStage.River;
            }
            else if (EqualityComparer<HoldemStage>.Default.Equals(Stage, HoldemStage.River))
            {
                Stage = HoldemStage.Complete;
            }

            return CommunityCards;
        }

        public IReadOnlyList<KeyValuePair<ushort, PokerHand>> GetBestHands()
        {
            return GetRankedHands();
        }

        protected void BurnTwoCards()
        {
            EnsureDeckHasCards(2);
            _deck.Pick();
            _deck.Pick();
        }

        protected void BurnOneCard()
        {
            EnsureDeckHasCards(1);
            _deck.Pick();
        }

        protected void DealCommunityCards(int quantity)
        {
            EnsureDeckHasCards(quantity);
            for (int i = 0; i < quantity; i++)
            {
                _communityCards.Add(_deck.Pick());
            }
        }

        protected void EnsureDeckHasCards(int requiredCards)
        {
            if (_deck.Count < requiredCards)
            {
                throw new InvalidOperationException(
                    $"The deck does not have enough cards to continue the hand. Required: {requiredCards}, available: {_deck.Count}.");
            }
        }

        protected override IDictionary<ushort, PokerHand> EvaluateBestHands()
        {
            if (_communityCards.Count < 3)
            {
                throw new InvalidOperationException("The community cards must be flop enough before evaluating best hands.");
            }

            Dictionary<ushort, PokerHand> hands = new();

            for (ushort i = 1; i <= Players; i++)
            {
                PokerHand bestHand = GetBestHandForPlayer(i);
                hands.Add(i, bestHand);
            }

            AddExtraHands(hands);

            return hands;
        }

        protected virtual void AddExtraHands(Dictionary<ushort, PokerHand> hands)
        {
        }

        protected abstract PokerHand GetBestHandForPlayer(ushort player);

        protected static List<Card[]> GetCombinations(IReadOnlyList<Card> cards, int k)
        {
            List<Card[]> result = new();
            GetCombinationsHelper(cards, k, 0, new Card[k], 0, result);
            return result;
        }

        private static void GetCombinationsHelper(IReadOnlyList<Card> cards, int k, int start, Card[] current, int index, List<Card[]> result)
        {
            if (index == k)
            {
                Card[] combination = new Card[k];
                Array.Copy(current, combination, k);
                result.Add(combination);
                return;
            }

            for (int i = start; i <= cards.Count - k + index; i++)
            {
                current[index] = cards[i];
                GetCombinationsHelper(cards, k, i + 1, current, index + 1, result);
            }
        }
    }
}

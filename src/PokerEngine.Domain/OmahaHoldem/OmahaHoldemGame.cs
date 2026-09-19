using PokerEngine.Domain.Models;

namespace PokerEngine.Domain.OmahaHoldem
{
    public sealed class OmahaHoldemGame : HoldemGame
    {

        override protected int CardsPerPlayer => 4;

        private readonly Dictionary<ushort, OmahaHoldemPlayerCards> _playersCards;

        public OmahaHoldemGame(ushort players, CardDeck? deck = null)
            : base(players, deck)
        {
            Dictionary<ushort, List<Card>> playerCards = new Dictionary<ushort, List<Card>>();
            _playersCards = new Dictionary<ushort, OmahaHoldemPlayerCards>();

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
                _playersCards[player.Key] = new OmahaHoldemPlayerCards(player.Value[0], player.Value[1], player.Value[2], player.Value[3]);
            }
        }

        public IReadOnlyDictionary<ushort, OmahaHoldemPlayerCards> PlayersCards => _playersCards;

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

            return hands;
        }

        private List<PokerHand> AddCardsToPossibleHands(OmahaHoldemPlayerCards playerCards, Card card1, Card card2, Card card3)
        {
            List<PokerHand> possibleHands =
            [
                new (card1, card2, card3, playerCards[0], playerCards[1]),
                new (card1, card2, card3, playerCards[0], playerCards[2]),
                new (card1, card2, card3, playerCards[0], playerCards[3]),
                new (card1, card2, card3, playerCards[1], playerCards[2]),
                new (card1, card2, card3, playerCards[1], playerCards[3]),
                new (card1, card2, card3, playerCards[2], playerCards[3]),
            ];

            return possibleHands;
        }

        private PokerHand GetBestHandForPlayer(ushort player)
        {
            OmahaHoldemPlayerCards playerCards = _playersCards[player];
            if (_communityCards.Count < 3)
            {
                throw new InvalidOperationException("The community cards must be flop enough before evaluating best hands.");
            }
            List<PokerHand> possibleHands = AddCardsToPossibleHands(playerCards, _communityCards[0], _communityCards[1], _communityCards[2]);
            if (_communityCards.Count > 3)
            {
                possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[0], _communityCards[1], _communityCards[3]));
                possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[0], _communityCards[2], _communityCards[3]));
                possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[1], _communityCards[2], _communityCards[3]));
                if (_communityCards.Count > 4)
                {
                    possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[0], _communityCards[1], _communityCards[4]));
                    possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[0], _communityCards[2], _communityCards[4]));
                    possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[1], _communityCards[2], _communityCards[4]));
                    possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[0], _communityCards[3], _communityCards[4]));
                    possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[1], _communityCards[3], _communityCards[4]));
                    possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[2], _communityCards[3], _communityCards[4]));
                }
            }
            
            return possibleHands.OrderBy(h => h).First();
        }
    }
}
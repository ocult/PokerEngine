using PokerEngine.Domain.Models;

namespace PokerEngine.Domain.SimpleGame
{
    public sealed class SimpleCardGame : PokerGame
    {
        private const int CardsPerPlayer = 5;
        private readonly CardDeck _deck;
        private readonly Dictionary<ushort, List<Card>> _playersCards;

        public SimpleCardGame(ushort players, CardDeck? deck = null)
        {
            if (players == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            if (players > 10)
            {
                throw new InvalidOperationException(
                    $"A standard deck cannot deal five cards to {players} players. The maximum supported is 10.");
            }

            Players = players;
            _deck = deck ?? new CardDeck();
            _playersCards = new Dictionary<ushort, List<Card>>();

            for (ushort player = 1; player <= players; player++)
            {
                _playersCards[player] = new List<Card>(CardsPerPlayer);
            }

            Stage = SimpleGameStage.NotStarted;
        }

        public override ushort Players { get; }

        public SimpleGameStage Stage { get; private set; }

        public IReadOnlyDictionary<ushort, IReadOnlyList<Card>> PlayersCards =>
            _playersCards.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyList<Card>)pair.Value.AsReadOnly());

        public IReadOnlyList<KeyValuePair<ushort, Card>> DealCards()
        {
            if (Stage == SimpleGameStage.Complete)
            {
                throw new InvalidOperationException("The game has already dealt five cards.");
            }

            if (_deck.Count < Players * 5)
            {
                throw new InvalidOperationException(
                    $"The deck does not have enough cards to deal to all players. Required: {Players}, available: {_deck.Count}.");
            }

            Stage = SimpleGameStage.Dealing;
            
            var dealtCards = new List<KeyValuePair<ushort, Card>>(Players);
            for (ushort c = 0; c < CardsPerPlayer; c++)
            {
                foreach (ushort player in _playersCards.Keys)
                {
                    Card card = _deck.Pick();
                    _playersCards[player].Add(card);
                    dealtCards.Add(new KeyValuePair<ushort, Card>(player, card));
                }
            }

            Stage = SimpleGameStage.Complete;

            return dealtCards.AsReadOnly();
        }

        protected override IDictionary<ushort, PokerHand> EvaluateBestHands()
        {
            if (Stage != SimpleGameStage.Complete)
            {
                throw new InvalidOperationException("All players must receive five cards before evaluating hands.");
            }

            return _playersCards.ToDictionary(
                pair => pair.Key,
                pair => new PokerHand(pair.Value.ToArray()));
        }
    }
}

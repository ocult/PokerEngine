using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerEngine.Domain.Models
{
    public enum TexasHoldemStage
    {
        PreFlop,
        Flop,
        Turn,
        River,
        Complete
    }

    public sealed class TexasHoldemGame
    {
        private readonly CardDeck _deck;
        private readonly Dictionary<ushort, List<Card>> _playersCards;
        private readonly List<Card> _communityCards;

        public TexasHoldemGame(ushort players, CardDeck? deck = null)
        {
            if (players == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            Players = players;
            _deck = deck ?? new CardDeck();
            _playersCards = new Dictionary<ushort, List<Card>>();
            _communityCards = new List<Card>();

            for (ushort c = 0; c < 2; c++)
            {
                for (ushort i = 1; i <= players; i++)
                {
                    if (c == 0) _playersCards[i] = new List<Card>();
                    _playersCards[i].Add(_deck.Pick());
                }
            }

            Stage = TexasHoldemStage.PreFlop;
        }

        public ushort Players { get; }

        public TexasHoldemStage Stage { get; private set; }

        public IReadOnlyDictionary<ushort, IReadOnlyList<Card>> PlayersCards =>
            _playersCards.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<Card>)pair.Value.AsReadOnly());

        public IReadOnlyList<Card> CommunityCards => _communityCards.AsReadOnly();

        public IReadOnlyList<Card> Continue()
        {
            switch (Stage)
            {
                case TexasHoldemStage.PreFlop:
                    BurnTwoCards();
                    DealCommunityCards(3);
                    Stage = TexasHoldemStage.Flop;
                    return CommunityCards;
                case TexasHoldemStage.Flop:
                    BurnOneCard();
                    DealCommunityCards(1);
                    Stage = TexasHoldemStage.Turn;
                    return CommunityCards;
                case TexasHoldemStage.Turn:
                    BurnOneCard();
                    DealCommunityCards(1);
                    Stage = TexasHoldemStage.River;
                    return CommunityCards;
                case TexasHoldemStage.River:
                    Stage = TexasHoldemStage.Complete;
                    return CommunityCards;
                default:
                    return CommunityCards;
            }
        }

        public IReadOnlyList<KeyValuePair<ushort, PokerHand>> GetBestHands()
        {
            if (_communityCards.Count != 5)
            {
                throw new InvalidOperationException("The community cards must be complete before evaluating best hands.");
            }

            var hands = new Dictionary<ushort, PokerHand>();

            for (ushort i = 1; i <= Players; i++)
            {
                var bestHand = GetBestHandForPlayer(i);
                hands.Add(i, bestHand);
            }

            var tableHand = new PokerHand(_communityCards.ToArray());
            hands.Add(0, tableHand);

            return hands.OrderBy(pair => pair.Value).ToList().AsReadOnly();
        }

        private PokerHand GetBestHandForPlayer(ushort player)
        {
            var playerCards = _playersCards[player];
            var tableCards = _communityCards;
            var possibleHands = new List<PokerHand>();

            for (ushort c = 0; c < 5; c++)
            {
                var card1 = c != 0 ? tableCards[0] : playerCards[0];
                var card2 = c != 1 ? tableCards[1] : playerCards[0];
                var card3 = c != 2 ? tableCards[2] : playerCards[0];
                var card4 = c != 3 ? tableCards[3] : playerCards[0];
                var card5 = c != 4 ? tableCards[4] : playerCards[0];
                possibleHands.Add(new PokerHand(card1, card2, card3, card4, card5));

                card1 = c != 0 ? tableCards[0] : playerCards[1];
                card2 = c != 1 ? tableCards[1] : playerCards[1];
                card3 = c != 2 ? tableCards[2] : playerCards[1];
                card4 = c != 3 ? tableCards[3] : playerCards[1];
                card5 = c != 4 ? tableCards[4] : playerCards[1];
                possibleHands.Add(new PokerHand(card1, card2, card3, card4, card5));
            }

            for (ushort c = 0; c < 3; c++)
            {
                var card1 = playerCards[0];
                var card2 = playerCards[1];
                var card3 = tableCards[c];
                var card4 = tableCards[c + 1];
                var card5 = tableCards[c + 2];
                possibleHands.Add(new PokerHand(card1, card2, card3, card4, card5));
            }

            return possibleHands.OrderBy(h => h).First();
        }

        private void BurnTwoCards()
        {
            _deck.Pick();
            _deck.Pick();
        }

        private void BurnOneCard()
        {
            _deck.Pick();
        }

        private void DealCommunityCards(int quantity)
        {
            for (var i = 0; i < quantity; i++)
            {
                _communityCards.Add(_deck.Pick());
            }
        }
    }
}

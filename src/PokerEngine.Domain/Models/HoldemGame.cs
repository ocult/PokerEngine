using System;
using System.Collections.Generic;

namespace PokerEngine.Domain.Models
{
    public abstract class HoldemGame : PokerGame
    {
        protected int MaxPlayersPerDeck => (52 - 9) / CardsPerPlayer;

        protected abstract int CardsPerPlayer { get; }


        protected readonly CardDeck _deck;
        protected readonly List<Card> _communityCards;

        protected HoldemGame(ushort players, CardDeck? deck = null)
        {
            if (players == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            if (players > MaxPlayersPerDeck)
            {
                throw new InvalidOperationException(
                    $"A standard deck cannot support {players} players. The maximum supported is {MaxPlayersPerDeck}.");
            }

            Players = players;
            _deck = deck ?? new CardDeck();
            _communityCards = new List<Card>();

            Stage = HoldemStage.PreFlop;
        }

        public override ushort Players { get; }

        public HoldemStage Stage { get; protected set; }

        public IReadOnlyList<Card> CommunityCards => _communityCards.AsReadOnly();

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
    }
}

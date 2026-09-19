using PokerEngine.Domain.Models;

namespace PokerEngine.Domain.TexasHoldem
{
    public sealed class TexasHoldemGame : HoldemGame<TexasHoldemPlayerCards>
    {
        
        override protected int CardsPerPlayer => 2;

        public TexasHoldemGame(ushort players, CardDeck? deck = null)
            : base(players, cards => new TexasHoldemPlayerCards(cards), deck)
        {
        }

        protected override void AddExtraHands(Dictionary<ushort, PokerHand> hands)
        {
            if (_communityCards.Count == 5)
            {                
                PokerHand tableHand = new(_communityCards.ToArray());
                hands.Add(0, tableHand);
            }
        }

        protected override PokerHand GetBestHandForPlayer(ushort player)
        {
            TexasHoldemPlayerCards playerCards = _playersCards[player];

            List<Card> allCards = new(_communityCards);
            allCards.AddRange(playerCards.Cards);

            List<Card[]> combinations = GetCombinations(allCards, 5);
            List<PokerHand> possibleHands = combinations
                .Where(c => !c.All(card => _communityCards.Contains(card)))
                .Select(c => new PokerHand(c))
                .ToList();

            return possibleHands.OrderBy(h => h).First();
        }
    }
}

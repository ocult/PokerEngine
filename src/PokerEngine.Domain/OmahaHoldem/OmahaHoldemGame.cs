using PokerEngine.Domain.Models;

namespace PokerEngine.Domain.OmahaHoldem
{
    public sealed class OmahaHoldemGame : HoldemGame<OmahaHoldemPlayerCards>
    {

        override protected int CardsPerPlayer => 4;

        public OmahaHoldemGame(ushort players, CardDeck? deck = null)
            : base(players, cards => new OmahaHoldemPlayerCards(cards), deck)
        {
        }

        protected override PokerHand GetBestHandForPlayer(ushort player)
        {
            OmahaHoldemPlayerCards playerCards = _playersCards[player];

            List<Card[]> playerCombos = GetCombinations(playerCards.Cards, 2);
            List<Card[]> communityCombos = GetCombinations(_communityCards, 3);

            List<PokerHand> possibleHands = new();
            foreach (Card[] pCombo in playerCombos)
            {
                foreach (Card[] cCombo in communityCombos)
                {
                    possibleHands.Add(new PokerHand(pCombo[0], pCombo[1], cCombo[0], cCombo[1], cCombo[2]));
                }
            }

            return possibleHands.OrderBy(h => h).First();
        }
    }
}
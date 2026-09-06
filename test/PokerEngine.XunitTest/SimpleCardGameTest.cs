using PokerEngine.Domain.Models;
using PokerEngine.Domain.SimpleGame;
using Xunit;

namespace PokerEngine.XunitTest
{
    public class SimpleCardGameTest
    {
        [Fact]
        public void SimpleCardGame_DealsOneCardToEachPlayer()
        {
            var deck = new CardDeck(new[]
            {
                new Card("2C"), new Card("2H"), new Card("2S"),
                new Card("3C"), new Card("3H"), new Card("3S"),
                new Card("4C"), new Card("4H"), new Card("4S"),
                new Card("5C"), new Card("5H"), new Card("5S"),
                new Card("6C"), new Card("6H"), new Card("6S")
            });
            var game = new SimpleCardGame(3, deck);

            game.DealCards();

            Assert.Equal(SimpleGameStage.Complete, game.Stage);
            Assert.Equal(new[] { new Card("2C"), new Card("3C"), new Card("4C"), new Card("5C"), new Card("6C")}, game.PlayersCards[1]);
            Assert.Equal(new[] { new Card("2H"), new Card("3H"), new Card("4H"), new Card("5H"), new Card("6H")}, game.PlayersCards[2]);
            Assert.Equal(new[] { new Card("2S"), new Card("3S"), new Card("4S"), new Card("5S"), new Card("6S")}, game.PlayersCards[3]);

            var hands = game.GetRankedHands();

            Assert.Equal(3, hands.Count);
            Assert.All(hands, hand => Assert.Equal(5, hand.Value.Cards.Length));
            Assert.Throws<InvalidOperationException>(() => game.DealCards());
        }

        [Fact]
        public void SimpleCardGame_RejectsHandEvaluationBeforeDealing()
        {
            var game = new SimpleCardGame(2);

            Assert.Throws<InvalidOperationException>(() => game.GetRankedHands());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(11)]
        public void SimpleCardGame_RejectsInvalidPlayerCounts(ushort players)
        {
            if (players == 0)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new SimpleCardGame(players));
                return;
            }

            Assert.Throws<InvalidOperationException>(() => new SimpleCardGame(players));
        }

        [Fact]
        public void SimpleCardGame_RejectsDeckWithoutEnoughCards()
        {            
            var game = new SimpleCardGame(3, new CardDeck(new[]
            {
                new Card("2C"), new Card("2H"), new Card("2S")
            }));

            Assert.Throws<InvalidOperationException>(() => game.DealCards());
            Assert.All(game.PlayersCards.Values, cards => Assert.Empty(cards));
        }
    }
}

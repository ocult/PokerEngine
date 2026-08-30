using PokerEngine.Domain.Models;
using Xunit;

namespace PokerEngine.XunitTest
{
    public class TexasHoldemGameTest
    {
        [Fact]
        public void TexasHoldemGame_Continue_AdvancesStagesAndDealsCommunityCards()
        {
            var game = new TexasHoldemGame(2);

            Assert.Equal(TexasHoldemStage.PreFlop, game.Stage);
            Assert.Equal(2, game.PlayersCards.Count);
            Assert.Empty(game.CommunityCards);

            var flop = game.Continue();
            Assert.Equal(TexasHoldemStage.Flop, game.Stage);
            Assert.Equal(3, flop.Count);
            Assert.Equal(3, game.CommunityCards.Count);

            var turn = game.Continue();
            Assert.Equal(TexasHoldemStage.Turn, game.Stage);
            Assert.Equal(4, turn.Count);
            Assert.Equal(4, game.CommunityCards.Count);

            var river = game.Continue();
            Assert.Equal(TexasHoldemStage.River, game.Stage);
            Assert.Equal(5, river.Count);
            Assert.Equal(5, game.CommunityCards.Count);

            var complete = game.Continue();
            Assert.Equal(TexasHoldemStage.Complete, game.Stage);
            Assert.Equal(5, complete.Count);
        }

        [Fact]
        public void TexasHoldemGame_GetBestHands_ReturnsRankedResultsAfterRiver()
        {
            var game = new TexasHoldemGame(2);
            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();

            var hands = game.GetBestHands();

            Assert.Equal(3, hands.Count);
            Assert.Contains(hands, hand => hand.Key == 0);
            Assert.Contains(hands, hand => hand.Key == 1);
            Assert.Contains(hands, hand => hand.Key == 2);
        }

        [Fact]
        public void TexasHoldemGame_WithKnownDeck_BurnsAndDealsCommunityCardsInOrder()
        {
            var game = new TexasHoldemGame(5, PokerHandTestHelper.CreateOrderedDeck());

            Assert.Equal(new Card(2, SuitEnum.Clubs), game.PlayersCards[1].FirstCard);
            Assert.Equal(new Card(3, SuitEnum.Hearts), game.PlayersCards[1].SecondCard);
            Assert.Equal(new Card(2, SuitEnum.Hearts), game.PlayersCards[2].FirstCard);
            Assert.Equal(new Card(3, SuitEnum.Spades), game.PlayersCards[2].SecondCard);
            Assert.Equal(new Card(2, SuitEnum.Spades), game.PlayersCards[3].FirstCard);
            Assert.Equal(new Card(3, SuitEnum.Diamonds), game.PlayersCards[3].SecondCard);
            Assert.Equal(new Card(2, SuitEnum.Diamonds), game.PlayersCards[4].FirstCard);
            Assert.Equal(new Card(4, SuitEnum.Clubs), game.PlayersCards[4].SecondCard);
            Assert.Equal(new Card(3, SuitEnum.Clubs), game.PlayersCards[5].FirstCard);
            Assert.Equal(new Card(4, SuitEnum.Hearts), game.PlayersCards[5].SecondCard);

            var flop = game.Continue();
            Assert.Equal(new Card(5, SuitEnum.Clubs), flop[0]);
            Assert.Equal(new Card(5, SuitEnum.Hearts), flop[1]);
            Assert.Equal(new Card(5, SuitEnum.Spades), flop[2]);
            Assert.Equal(3, flop.Count);
            Assert.Equal(3, game.CommunityCards.Count);

            var turn = game.Continue();
            Assert.Equal(new Card(6, SuitEnum.Clubs), turn[3]);
            Assert.Equal(4, turn.Count);

            var river = game.Continue();
            Assert.Equal(new Card(6, SuitEnum.Spades), river[4]);
            Assert.Equal(5, river.Count);
            Assert.Equal(TexasHoldemStage.River, game.Stage);
        }

        [Fact]
        public void TexasHoldemGame_GetWinnerPlayer_UsesRankingFromBaseGame()
        {
            var game = new TexasHoldemGame(5, PokerHandTestHelper.CreateOrderedDeck());
            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();

            var winner = game.GetWinnerPlayer();
            Assert.Equal(0, winner);
        }
    }
}

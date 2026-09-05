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
        public void TexasHoldemGame_GetBestHands_EqualPairHandsTieAfterComplete()
        {
            var deck = new CardDeck(
            [
                new Card(9, SuitEnum.Clubs),
                new Card(9, SuitEnum.Diamonds),
                new Card(9, SuitEnum.Hearts),
                new Card(9, SuitEnum.Spades),
                new Card(2, SuitEnum.Clubs),
                new Card(2, SuitEnum.Diamonds),
                new Card(13, SuitEnum.Clubs),
                new Card(12, SuitEnum.Diamonds),
                new Card(11, SuitEnum.Spades),
                new Card(7, SuitEnum.Clubs),
                new Card(2, SuitEnum.Hearts),
                new Card(4, SuitEnum.Clubs),
                new Card(6, SuitEnum.Spades),
            ]);
            var game = new TexasHoldemGame(2, deck);

            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();

            var hands = game.GetBestHands();
            var playerOneHand = hands.Single(hand => hand.Key == 1).Value;
            var playerTwoHand = hands.Single(hand => hand.Key == 2).Value;

            Assert.Equal(HandRankingEnum.Pair, playerOneHand.HandRanking);
            Assert.Equal(playerOneHand.HandRanking, playerTwoHand.HandRanking);
            Assert.False(playerOneHand > playerTwoHand);
            Assert.False(playerTwoHand > playerOneHand);
            Assert.False(playerOneHand < playerTwoHand);
            Assert.False(playerTwoHand < playerOneHand);
        }

        [Fact]
        public void TexasHoldemGame_GetBestHands_ReturnsValidHandsAfterFlop()
        {
            var game = new TexasHoldemGame(2, PokerHandTestHelper.CreateOrderedDeck());

            _ = game.Continue();

            var hands = game.GetBestHands();

            Assert.Equal(TexasHoldemStage.Flop, game.Stage);
            Assert.Equal(2, hands.Count);
            Assert.All(hands, hand => Assert.Equal(5, hand.Value.Cards.Length));
            Assert.Contains(hands, hand => hand.Key == 1);
            Assert.Contains(hands, hand => hand.Key == 2);
        }

        [Fact]
        public void TexasHoldemGame_GetBestHands_ReturnsValidHandsAfterTurn()
        {
            var game = new TexasHoldemGame(2, PokerHandTestHelper.CreateOrderedDeck());

            _ = game.Continue();
            _ = game.Continue();

            var hands = game.GetBestHands();

            Assert.Equal(TexasHoldemStage.Turn, game.Stage);
            Assert.Equal(2, hands.Count);
            Assert.All(hands, hand => Assert.Equal(5, hand.Value.Cards.Length));
            Assert.Contains(hands, hand => hand.Key == 1);
            Assert.Contains(hands, hand => hand.Key == 2);
        }

        [Fact]
        public void TexasHoldemGame_WithKnownDeck_BurnsAndDealsCommunityCardsInOrder()
        {
            var deck = new CardDeck();
            var cards = deck.Cards.ToArray();
            var p1c1 = cards[0];
            var p2c1 = cards[1];
            var p3c1 = cards[2];
            var p4c1 = cards[3];
            var p5c1 = cards[4];
            var p1c2 = cards[5];
            var p2c2 = cards[6];
            var p3c2 = cards[7];
            var p4c2 = cards[8];
            var p5c2 = cards[9];
            var flop1 = cards[12];
            var flop2 = cards[13];
            var flop3 = cards[14];
            var turnExpected = cards[16];
            var riverExpected = cards[18];
            var game = new TexasHoldemGame(5, deck);

            Assert.Equal(p1c1, game.PlayersCards[1].FirstCard);
            Assert.Equal(p1c2, game.PlayersCards[1].SecondCard);

            Assert.Equal(p2c1, game.PlayersCards[2].FirstCard);
            Assert.Equal(p2c2, game.PlayersCards[2].SecondCard);

            Assert.Equal(p3c1, game.PlayersCards[3].FirstCard);
            Assert.Equal(p3c2, game.PlayersCards[3].SecondCard);

            Assert.Equal(p4c1, game.PlayersCards[4].FirstCard);
            Assert.Equal(p4c2, game.PlayersCards[4].SecondCard);
            
            Assert.Equal(p5c1, game.PlayersCards[5].FirstCard);
            Assert.Equal(p5c2, game.PlayersCards[5].SecondCard);
            Assert.Equal(TexasHoldemStage.PreFlop, game.Stage);

            var flop = game.Continue();
            Assert.Equal(3, flop.Count);
            Assert.Equal(3, game.CommunityCards.Count);            
            Assert.Equal(TexasHoldemStage.Flop, game.Stage);
            Assert.Equal(flop1, flop[0]);
            Assert.Equal(flop2, flop[1]);
            Assert.Equal(flop3, flop[2]);

            var turn = game.Continue();
            Assert.Equal(4, turn.Count);
            Assert.Equal(TexasHoldemStage.Turn, game.Stage);
            Assert.Equal(turnExpected, turn[3]);

            var river = game.Continue();
            Assert.Equal(5, river.Count);
            Assert.Equal(TexasHoldemStage.River, game.Stage);
            Assert.Equal(riverExpected, river[4]);
        }

        [Fact]
        public void TexasHoldemGame_DeckLimit_AllowsHighestPlayerCountBeforeExhaustion()
        {
            var deck = new CardDeck();
            var game = new TexasHoldemGame(21, deck);

            Assert.Equal(21, game.PlayersCards.Count);
            Assert.Equal(10, deck.Count);

            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();

            Assert.Equal(1, deck.Count);
            Assert.Throws<InvalidOperationException>(() => new TexasHoldemGame(22));
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

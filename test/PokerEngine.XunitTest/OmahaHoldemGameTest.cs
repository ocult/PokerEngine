using PokerEngine.Domain.Models;
using PokerEngine.Domain.OmahaHoldem;
using Xunit;

namespace PokerEngine.XunitTest
{
    public class OmahaHoldemGameTest
    {
        [Fact]
        public void OmahaHoldemGame_Continue_AdvancesStagesAndDealsCommunityCards()
        {
            var game = new OmahaHoldemGame(2);

            Assert.Equal(OmahaHoldemStage.PreFlop, game.Stage);
            Assert.Equal(2, game.PlayersCards.Count);
            Assert.Empty(game.CommunityCards);

            var flop = game.Continue();
            Assert.Equal(OmahaHoldemStage.Flop, game.Stage);
            Assert.Equal(3, flop.Count);
            Assert.Equal(3, game.CommunityCards.Count);

            var turn = game.Continue();
            Assert.Equal(OmahaHoldemStage.Turn, game.Stage);
            Assert.Equal(4, turn.Count);
            Assert.Equal(4, game.CommunityCards.Count);

            var river = game.Continue();
            Assert.Equal(OmahaHoldemStage.River, game.Stage);
            Assert.Equal(5, river.Count);
            Assert.Equal(5, game.CommunityCards.Count);

            var complete = game.Continue();
            Assert.Equal(OmahaHoldemStage.Complete, game.Stage);
            Assert.Equal(5, complete.Count);
        }

        [Fact]
        public void OmahaHoldemGame_GetBestHands_ReturnsRankedResultsAfterRiver()
        {
            var game = new OmahaHoldemGame(2);
            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();

            var hands = game.GetBestHands();

            Assert.Equal(2, hands.Count);
            Assert.Contains(hands, hand => hand.Key == 1);
            Assert.Contains(hands, hand => hand.Key == 2);
        }

        [Fact]
        public void OmahaHoldemGame_GetBestHands_EqualPairHandsTieAfterComplete()
        {
            var deck = new CardDeck(
            [
                new Card(8, SuitEnum.Clubs),
                new Card(8, SuitEnum.Diamonds),
                new Card(8, SuitEnum.Hearts),
                new Card(8, SuitEnum.Spades),
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
            var game = new OmahaHoldemGame(2, deck);

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
        public void OmahaHoldemGame_GetBestHands_ReturnsValidHandsAfterFlop()
        {
            var game = new OmahaHoldemGame(2, PokerHandTestHelper.CreateOrderedDeck());

            _ = game.Continue();

            var hands = game.GetBestHands();

            Assert.Equal(OmahaHoldemStage.Flop, game.Stage);
            Assert.Equal(2, hands.Count);
            Assert.All(hands, hand => Assert.Equal(5, hand.Value.Cards.Length));
            Assert.Contains(hands, hand => hand.Key == 1);
            Assert.Contains(hands, hand => hand.Key == 2);
        }

        [Fact]
        public void OmahaHoldemGame_GetBestHands_ReturnsValidHandsAfterTurn()
        {
            var game = new OmahaHoldemGame(2, PokerHandTestHelper.CreateOrderedDeck());

            _ = game.Continue();
            _ = game.Continue();

            var hands = game.GetBestHands();

            Assert.Equal(OmahaHoldemStage.Turn, game.Stage);
            Assert.Equal(2, hands.Count);
            Assert.All(hands, hand => Assert.Equal(5, hand.Value.Cards.Length));
            Assert.Contains(hands, hand => hand.Key == 1);
            Assert.Contains(hands, hand => hand.Key == 2);
        }

        [Fact]
        public void OmahaHoldemGame_WithKnownDeck_BurnsAndDealsCommunityCardsInOrder()
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
            
            var p1c3 = cards[10];
            var p2c3 = cards[11];
            var p3c3 = cards[12];
            var p4c3 = cards[13];
            var p5c3 = cards[14];
            
            var p1c4 = cards[15];
            var p2c4 = cards[16];
            var p3c4 = cards[17];
            var p4c4 = cards[18];
            var p5c4 = cards[19];
            
            var flop1 = cards[22];
            var flop2 = cards[23];
            var flop3 = cards[24];
            
            var turnExpected = cards[26];
            var riverExpected = cards[28];
            
            var game = new OmahaHoldemGame(5, deck);

            Assert.Equal(p1c1, game.PlayersCards[1].FirstCard);
            Assert.Equal(p1c2, game.PlayersCards[1].SecondCard);
            Assert.Equal(p1c3, game.PlayersCards[1].ThirdCard);
            Assert.Equal(p1c4, game.PlayersCards[1].FourthCard);

            Assert.Equal(p2c1, game.PlayersCards[2].FirstCard);
            Assert.Equal(p2c2, game.PlayersCards[2].SecondCard);
            Assert.Equal(p2c3, game.PlayersCards[2].ThirdCard);
            Assert.Equal(p2c4, game.PlayersCards[2].FourthCard);

            Assert.Equal(p3c1, game.PlayersCards[3].FirstCard);
            Assert.Equal(p3c2, game.PlayersCards[3].SecondCard);
            Assert.Equal(p3c3, game.PlayersCards[3].ThirdCard);
            Assert.Equal(p3c4, game.PlayersCards[3].FourthCard);

            Assert.Equal(p4c1, game.PlayersCards[4].FirstCard);
            Assert.Equal(p4c2, game.PlayersCards[4].SecondCard);
            Assert.Equal(p4c3, game.PlayersCards[4].ThirdCard);
            Assert.Equal(p4c4, game.PlayersCards[4].FourthCard);
            
            Assert.Equal(p5c1, game.PlayersCards[5].FirstCard);
            Assert.Equal(p5c2, game.PlayersCards[5].SecondCard);
            Assert.Equal(p5c3, game.PlayersCards[5].ThirdCard);
            Assert.Equal(p5c4, game.PlayersCards[5].FourthCard);
            Assert.Equal(OmahaHoldemStage.PreFlop, game.Stage);

            var flop = game.Continue();
            Assert.Equal(3, flop.Count);
            Assert.Equal(3, game.CommunityCards.Count);            
            Assert.Equal(OmahaHoldemStage.Flop, game.Stage);
            Assert.Equal(flop1, flop[0]);
            Assert.Equal(flop2, flop[1]);
            Assert.Equal(flop3, flop[2]);

            var turn = game.Continue();
            Assert.Equal(4, turn.Count);
            Assert.Equal(OmahaHoldemStage.Turn, game.Stage);
            Assert.Equal(turnExpected, turn[3]);

            var river = game.Continue();
            Assert.Equal(5, river.Count);
            Assert.Equal(OmahaHoldemStage.River, game.Stage);
            Assert.Equal(riverExpected, river[4]);
        }

        [Fact]
        public void OmahaHoldemGame_DeckLimit_AllowsHighestPlayerCountBeforeExhaustion()
        {
            var deck = new CardDeck();
            var game = new OmahaHoldemGame(10, deck);

            Assert.Equal(10, game.PlayersCards.Count);
            Assert.Equal(12, deck.Count);

            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();

            Assert.Equal(3, deck.Count);
            Assert.Throws<InvalidOperationException>(() => new OmahaHoldemGame(11));
        }

        [Fact]
        public void OmahaHoldemGame_GetWinnerPlayer_UsesRankingFromBaseGame()
        {
            var game = new OmahaHoldemGame(5, PokerHandTestHelper.CreateOrderedDeck());
            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();

            var winner = game.GetWinnerPlayer();
            Assert.Equal(3, winner);
        }

        [Fact]
        public void OmahaHoldemGame_RejectsInvalidPlayerCounts()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new OmahaHoldemGame(0));
            Assert.Throws<InvalidOperationException>(() => new OmahaHoldemGame(22));
        }

        [Fact]
        public void OmahaHoldemPlayerCards_ExposesCardsAndValidatesIndex()
        {
            var firstCard = new Card("AC");
            var secondCard = new Card("KH");
            var thirdCard = new Card("QD");
            var fourthCard = new Card("JD");
            var cards = new OmahaHoldemPlayerCards(firstCard, secondCard, thirdCard, fourthCard);

            Assert.Equal(new[] { firstCard, secondCard, thirdCard, fourthCard }, cards.Cards);
            Assert.Equal(firstCard, cards[0]);
            Assert.Equal(secondCard, cards[1]);
            Assert.Equal(thirdCard, cards[2]);
            Assert.Equal(fourthCard, cards[3]);
            Assert.Throws<ArgumentOutOfRangeException>(() => cards[4]);
        }

        [Fact]
        public void OmahaHoldemGame_GetBestHands_RequiresAtLeastAFlop()
        {
            var game = new OmahaHoldemGame(1, new CardDeck(false));

            Assert.Throws<InvalidOperationException>(() => game.GetBestHands());
        }

        [Fact]
        public void OmahaHoldemGame_ContinueAfterCompleteKeepsFinalCommunityCards()
        {
            var game = new OmahaHoldemGame(1, new CardDeck(false));

            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();
            var complete = game.Continue();
            var afterComplete = game.Continue();

            Assert.Equal(OmahaHoldemStage.Complete, game.Stage);
            Assert.Equal(complete, afterComplete);
            Assert.Equal(5, afterComplete.Count);
        }

        [Fact]
        public void OmahaHoldemGame_ContinueRejectsAnExhaustedDeck()
        {
            var game = new OmahaHoldemGame(1, new CardDeck(new[]
            {
                new Card("AC"),
                new Card("KH"),
                new Card("QS"),
                new Card("JD"),
            }));

            Assert.Throws<InvalidOperationException>(() => game.Continue());
        }
    }
}

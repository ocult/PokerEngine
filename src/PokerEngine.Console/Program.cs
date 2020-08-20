using System;
using System.Collections.Generic;
using System.Linq;
using PokerEngine.Domain.Models;
using MSC = System.Console;

namespace PokerEngine.Console
{
    class Program
    {
        private static CardDeck _deck;
        static void Main(string[] args)
        {
            string cards = null;
            _deck = new CardDeck();
            _deck.PowerShuffle();

            if (args != null && args.Length > 0)
            {
                cards = args.Length > 1 ? string.Join(',', args) : args[0];
                MSC.WriteLine($"Your cards are {cards}");
            }
            ReadCards(cards);
        }

        static void ReadCards(string cards = null)
        {
            if (cards is null)
            {
                MSC.WriteLine("What's yours cards?");
                cards = MSC.ReadLine();
            }
            cards = cards.ToUpperInvariant();

            if (cards.StartsWith("QUIT") || cards.StartsWith("EXIT") || cards == "Q")
            {
                return;
            }
            try
            {
                if (cards.StartsWith("TEXAS"))
                {
                    var strPlayers = string.Join("", cards.Skip(6)).Trim();
                    if (!ushort.TryParse(strPlayers, out var players))
                    {
                        throw new ArgumentException(nameof(players));
                    }
                    TexasHoldem(players);
                    ReadCards();
                    return;
                }
                else if (cards.StartsWith("RANDOM"))
                {
                    var strPlayers = string.Join("", cards.Skip(7)).Trim();
                    if (!ushort.TryParse(strPlayers, out var players))
                    {
                        var pickedCards = _deck.Pick(5);
                        cards = PokerHand.GetCardsString(pickedCards);
                    }
                    else
                    {
                        RandomCards(players);
                        ReadCards();
                        return;
                    }
                }
                var hand = new PokerHand(cards);
                MSC.WriteLine($"You have {hand}");
                ReadCards();
            }
            catch (Exception e)
            {
                if (_deck.Count < 5)
                {
                    _deck = new CardDeck();
                    _deck.PowerShuffle();
                }
                MSC.WriteLine(e);
                ReadCards();
            }
        }

        private static void TexasHoldem(ushort players)
        {
            var playersCards = new Dictionary<ushort, IEnumerable<Card>>();

            for (ushort i = 1; i <= players; i++)
            {
                playersCards.Add(i, new List<Card> { _deck.Pick() });
            }
            for (ushort i = 1; i <= players; i++)
            {
                playersCards[i] = playersCards[i].Append(_deck.Pick());
                MSC.WriteLine($"Player #{i} have [{playersCards[i].ElementAt(0)}, {playersCards[i].ElementAt(1)}] in hand");
            }
            MSC.Read();
            _deck.Pick();
            _deck.Pick();
            var tableCards = new List<Card>
            {
                _deck.Pick(),
                _deck.Pick(),
                _deck.Pick()
            };
            MSC.WriteLine($"Table flop is [{tableCards[0]}, {tableCards[1]}, {tableCards[2]}]");
            MSC.ReadLine();
            _deck.Pick();
            tableCards.Add(_deck.Pick());
            MSC.WriteLine($"Table turn is {tableCards[3]}");
            MSC.ReadLine();
            _deck.Pick();
            tableCards.Add(_deck.Pick());
            MSC.WriteLine($"Table river is {tableCards[4]}");
            MSC.ReadLine();

            var hands = new Dictionary<ushort, PokerHand>();
            for (ushort i = 1; i <= players; i++)
            {
                var possibleHands = new List<PokerHand>();
                for (ushort c = 0; c < 5; c++)
                {
                    var card1 = c != 0 ? tableCards[0] : playersCards[i].ElementAt(0);
                    var card2 = c != 1 ? tableCards[1] : playersCards[i].ElementAt(0);
                    var card3 = c != 2 ? tableCards[2] : playersCards[i].ElementAt(0);
                    var card4 = c != 3 ? tableCards[3] : playersCards[i].ElementAt(0);
                    var card5 = c != 4 ? tableCards[4] : playersCards[i].ElementAt(0);
                    possibleHands.Add(new PokerHand(card1, card2, card3, card4, card5));

                    card1 = c != 0 ? tableCards[0] : playersCards[i].ElementAt(1);
                    card2 = c != 1 ? tableCards[1] : playersCards[i].ElementAt(1);
                    card3 = c != 2 ? tableCards[2] : playersCards[i].ElementAt(1);
                    card4 = c != 3 ? tableCards[3] : playersCards[i].ElementAt(1);
                    card5 = c != 4 ? tableCards[4] : playersCards[i].ElementAt(1);
                    possibleHands.Add(new PokerHand(card1, card2, card3, card4, card5));
                }

                for (ushort c = 0; c < 3; c++)
                {
                    var card1 = playersCards[i].ElementAt(0);
                    var card2 = playersCards[i].ElementAt(1);

                    var card3 = tableCards[c];
                    var card4 = tableCards[c + 1];
                    var card5 = tableCards[c + 2];

                    possibleHands.Add(new PokerHand(card1, card2, card3, card4, card5));
                }

                var bestHand = possibleHands.OrderBy(h => h).First();
                MSC.WriteLine($"A player #{i} best possible hand is {bestHand}");
                hands.Add(i, bestHand);
            }
            hands.Add(0, new PokerHand(tableCards.ToArray()));
            var playersHands = hands.OrderBy((a) => a.Value).ToList();

            MSC.ReadLine();
            PrintWinner(playersHands);
            _deck = new CardDeck();
            _deck.PowerShuffle();
        }

        private static void RandomCards(ushort players)
        {
            var hands = new Dictionary<ushort, PokerHand>();
            for (ushort i = 1; i <= players; i++)
            {
                var playerHand = new PokerHand(_deck.Pick(5).ToArray());
                hands.Add(i, playerHand);
            }
            var playersHands = hands.OrderBy((a) => a.Value).ToList();
            PrintWinner(playersHands);
        }

        private static void PrintWinner(List<KeyValuePair<ushort, PokerHand>> playersHands)
        {
            var win = playersHands.FirstOrDefault().Key;
            MSC.WriteLine(win == 0 ? "The table winner" : $"The winner is player #{win}".ToUpperInvariant());
            MSC.WriteLine($"Ranked players hands:");
            foreach (var item in playersHands)
            {
                MSC.WriteLine(item.Key == 0 ? $"Table has {item.Value}" : $"Player #{item.Key} has {item.Value}");
            }
        }
    }
}

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

            if (cards == "quit" || cards == "exit" || cards == "q")
            {
                return;
            }
            try
            {
                if (cards.StartsWith("random"))
                {
                    var strPlayers = string.Join("", cards.Skip(7)).Trim();
                    MSC.WriteLine($"{strPlayers} are parsed");
                    if (!ushort.TryParse(strPlayers, out var players))
                    {
                        var pickedCards = _deck.Pick(5);
                        cards = PokerHand.GetCardsString(pickedCards);
                    }
                    else
                    {
                        var hands = new Dictionary<ushort, PokerHand>();
                        for (ushort i = 1; i <= players; i++)
                        {
                            var playerHand = new PokerHand(_deck.Pick(5).ToArray());
                            MSC.WriteLine($"Player #{i} haves {playerHand}");
                            hands.Add(i, playerHand);
                            cards = null;
                        }
                        var playersHands = hands.OrderBy((a) => a.Value).ToList();
                        var win = playersHands.FirstOrDefault().Key;
                        MSC.WriteLine($"WINNER: Player #{win}");
                        MSC.WriteLine($"Ranked:");
                        foreach (var item in playersHands)
                        {
                            MSC.WriteLine($"Player #{item.Key} haves {item.Value}");
                        }
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
    }
}

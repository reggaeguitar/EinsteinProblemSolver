using System;
using System.Collections.Generic;
using System.Linq;

namespace EinsteinRiddleSolver
{
    class Program
    {
        private static int NumHouses = 5;

        static void Main()
        {
            var rules = new List<Func<List<House>, bool>>
            {
                // The Englishman lives in the house with red walls.
                houses => houses.Single(x => x.Nationality == Nationality.English).Color == Color.Red,
                // The Swede keeps dogs. 
                houses => houses.Single(x => x.Nationality == Nationality.Swedish).Pet == Pet.Dogs,
                // The Dane drinks tea. 
                houses => houses.Single(x => x.Nationality == Nationality.Danish).Drink == Drink.Tea,
                // The house with green walls is just to the left of the house with white walls. 
                houses =>
                    houses.Single(x => x.Color == Color.White).Position -
                    houses.Single(x => x.Color == Color.Green).Position == 1,
                // The owner of the house with green walls drinks coffee. 
                houses => houses.Single(x => x.Color == Color.Green).Drink == Drink.Coffee,
                // The man who smokes Pall Mall keeps birds.
                houses => houses.Single(x => x.Cigar == Cigar.PallMall).Pet == Pet.Birds,
                // The owner of the house with yellow walls smokes Dunhills.
                houses => houses.Single(x => x.Color == Color.Yellow).Cigar == Cigar.Dunhill,               
                // The Blend smoker has a neighbor who keeps cats
                houses =>
                    CheckNeighborCondition(houses, 
                        houses => houses.Single(x => x.Cigar == Cigar.Blend), 
                        house => house.Pet == Pet.Cats),
                // The man who smokes Blue Masters drinks beer. 
                houses => houses.Single(x => x.Cigar == Cigar.BlueMasters).Drink == Drink.Beer,

                // The man who keeps horses lives next to the Dunhill smoker. 
                houses =>
                    CheckNeighborCondition(houses,
                        houses => houses.Single(x => x.Pet == Pet.Horses),
                        house => house.Cigar == Cigar.Dunhill),

                // The German smokes Prince. 
                houses => houses.Single(x => x.Nationality == Nationality.German).Cigar == Cigar.Prince,

                // The Blend smoker has a neighbor who drinks water. 
                houses =>
                    CheckNeighborCondition(houses,
                        houses => houses.Single(x => x.Cigar == Cigar.Blend),
                        house => house.Drink == Drink.Water),
            };
            List<House> housesSolved = SolveEinsteinRiddle(rules);
            var solution = housesSolved.Single(x => x.Pet == Pet.Fish);
            Console.WriteLine(solution);
        }

        class FiveDigitBase120Number
        {
            private const int Max = 119;
            public List<int> Digits { get; set; }
            public FiveDigitBase120Number Next()
            {
                // biggest number already
                if (Digits.All(x => x == Max)) return null;
                
                var copy = new List<int>(Digits);
                for (int i = 5 - 1; i >= 0; i--)
                {
                    if (Digits[i] == Max)
                    {
                        copy[i] = 0;
                        continue;
                    };
                    ++copy[i];
                    return new FiveDigitBase120Number { Digits = copy };
                }
                throw new Exception("Should not have gotten to this point.");
            }

            public override string ToString() => string.Join('-', Digits);
        }

        private static List<House> SolveEinsteinRiddle(List<Func<List<House>, bool>> facts)
        {
            var str = "12345";
            PermutationGenerator.Permute(str, 0, str.Length - 1);
            var permutations = PermutationGenerator.Permutations;
            // brute force check all combinations and test them until a combination works
            // todo need to iterate through all combinations
            // like 12345, 12345, 12345, 12345, 12345
            // then 12345, 12345, 12345, 12345, 21345
            // etc
            // 1 1 1 1 1
            // 1 1 1 1 2
            // 1 1 1 1 3
            // ...
            // 1 1 1 1 120
            // 1 1 1 2 120
            // ...
            // 120 120 120 120 120 
            var number = new FiveDigitBase120Number { Digits = new List<int> { 0, 0, 0, 0, 0 } };
            var count = 1;
            do
            {
                var currentHouses = GetHouses(number, permutations);
                if (currentHouses != null && PassRules(currentHouses, facts))
                {
                    return currentHouses;
                }
                number = number.Next();
                ++count;
                if (count % 1000000 == 0)
                {
                    Console.WriteLine(number);
                }
                //Console.WriteLine(number);
            } while (number != null);
            return null;
        }

        private static bool PassRules(List<House> currentHouses, List<Func<List<House>, bool>> rules)
        {
            var passesAllRules = rules.All(fact => fact(currentHouses));
            return passesAllRules;
        }

        private static List<House> GetHouses(FiveDigitBase120Number number, List<string> permutations)
        {
            var houses = Enumerable.Range(0, 5).Select(x => new House { Position = x }).ToList();
            // hardcoded facts
            // The Norwegian lives in the first house.
            // The Norwegian lives next to the house with blue walls.
            // The man in the center house drinks milk. 
            // The house with green walls is just to the left of the house with white walls. 


            var color = permutations[number.Digits[0]];
            Assign<Color>(houses, color, (house, color) => house.Color = color);

            if (houses[1].Color != Color.Blue) return null;
            if (houses[0].Color == Color.White || houses[2].Color == Color.White) return null;
            if (houses[0].Color == Color.Red) return null;
            if (houses[4].Color == Color.Green) return null;

            var nationality = permutations[number.Digits[1]];
            Assign<Nationality>(houses, nationality, (house, nationality) => house.Nationality = nationality);

            if (houses[0].Nationality != Nationality.Norwegian) return null;

            var drink = permutations[number.Digits[2]];
            Assign<Drink>(houses, drink, (house, drink) => house.Drink = drink);

            if (houses[2].Drink != Drink.Milk) return null;

            var cigar = permutations[number.Digits[3]];
            Assign<Cigar>(houses, cigar, (house, cigar) => house.Cigar = cigar);

            var pet = permutations[number.Digits[4]];
            Assign<Pet>(houses, pet, (house, pet) => house.Pet = pet);

            // swede has dogs, first house is norwegian
            if (houses[0].Pet == Pet.Dogs) return null;

            return houses;
        }

        private static void Assign<T>(List<House> ret, string values, Action<House, T> assigner) where T: struct, Enum
        {
            if (ret.Count != values.Length) throw new Exception("Should be equal.");
            var enumValues = Enum.GetValues<T>();

            for (int i = 0; i < ret.Count; i++)
            {
                var value = values[i] - '0';
                assigner(ret[i], enumValues[value]);
            }
        }

        private static bool CheckNeighborCondition(List<House> houses, 
            Func<List<House>, House> houseSelector, 
            Func<House, bool> validationFunc)
        {
            var curHouse = houseSelector(houses);
            var curHousePosition = curHouse.Position;
            var neighborsOrNeighbor = GetNeighbors(houses, curHousePosition);
            var ret = neighborsOrNeighbor.Any(validationFunc);
            return ret;
        }

        private static List<House> GetNeighbors(List<House> houses, int curHousePosition)
        {
            // edges
            if (curHousePosition == 0) return houses.Where(x => x.Position == 1).ToList();
            if (curHousePosition == NumHouses - 1) return houses.Where(x => x.Position == NumHouses - 2).ToList();
            return houses.Where(x => Math.Abs(curHousePosition - x.Position) == 1).ToList();
        }
    }

    // Code basically copied/pasted from https://www.geeksforgeeks.org/write-a-c-program-to-print-all-permutations-of-a-given-string/
    static class PermutationGenerator
    {
        public static List<string> Permutations = new();

        public static void Permute(string str,
                               int l, int r)
        {
            if (l == r)
            {                
                Permutations.Add(str);
            }
            else
            {
                for (int i = l; i <= r; i++)
                {
                    str = Swap(str, l, i);
                    Permute(str, l + 1, r);
                    str = Swap(str, l, i);
                }
            }
        }
        private static string Swap(string a,
                            int i, int j)
        {
            char temp;
            char[] charArray = a.ToCharArray();
            temp = charArray[i];
            charArray[i] = charArray[j];
            charArray[j] = temp;
            string s = new(charArray);
            return s;
        }
    }


    enum Color { Unknown, Red, Green, White, Yellow, Blue };

    enum Nationality { Unknown, English, Swedish, Danish, German, Norwegian };

    enum Drink { Unknown, Tea, Coffee, Milk, Beer, Water };

    enum Cigar { Unknown, PallMall, Dunhill, Blend, BlueMasters, Prince };

    enum Pet { Unknown, Dogs, Birds, Cats, Horses, Fish };

    class Fact
    {
        public int Position { get; set; }
        public Color Color { get; set; }
        public Nationality Nationality { get; set; }
        public Drink Drink { get; set; }
        public Cigar Cigar { get; set; }
        public Pet Pet { get; set; }
        public Func<List<House>, bool> VerificationFunc { get; set; }
    }

    // todo convert this to record type?
    class House
    {
        public int Position { get; set; }
        public Color Color { get; set; }
        public Nationality Nationality { get; set; }
        public Drink Drink { get; set; }
        public Cigar Cigar { get; set; }
        public Pet Pet { get; set; }

        public override string ToString() =>
            $"Position: {Position}, Color: {Color}, Nationality: {Nationality}, Drink: {Drink}, Cigar: {Cigar}, Pet: {Pet}";
        
    }
}

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
            var houses = Enumerable.Range(0, NumHouses).Select(x => new House { Position = x }).ToList();
            var facts = new List<Fact>
            {
                new Fact { Nationality = Nationality.English, Color = Color.Red },
                new Fact { Nationality = Nationality.Swedish, Pet = Pet.Dogs },
                new Fact { Nationality = Nationality.Danish, Drink = Drink.Tea },
                // The house with green walls is just to the left of the house with white walls. 
                new Fact { VerificationFunc = houses =>
                    houses.Single(x => x.Color == Color.White).Position -
                    houses.Single(x => x.Color == Color.Green).Position == 1 },

                new Fact { Color = Color.Green, Drink = Drink.Coffee },
                new Fact { Cigar = Cigar.PallMall, Pet = Pet.Birds },
                new Fact { Color = Color.Yellow, Cigar = Cigar.Dunhill },
                new Fact { Color = Color.Yellow, Cigar = Cigar.Dunhill },
                new Fact { Position = 3, Drink = Drink.Milk },
                new Fact { Position = 0, Nationality = Nationality.Norwegian },
                // The Blend smoker has a neighbor who keeps cats
                new Fact { VerificationFunc = houses =>
                    CheckNeighborCondition(houses, 
                        houses => houses.Single(x => x.Cigar == Cigar.Blend), 
                        house => house.Pet == Pet.Cats) },

                new Fact { Cigar = Cigar.BlueMasters, Drink = Drink.Beer },
                // The man who keeps horses lives next to the Dunhill smoker. 
                new Fact { VerificationFunc = houses =>
                    CheckNeighborCondition(houses,
                        houses => houses.Single(x => x.Pet == Pet.Horses),
                        house => house.Cigar == Cigar.Dunhill) },

                new Fact { Nationality = Nationality.German, Cigar = Cigar.Prince },
                // The Norwegian lives next to the house with blue walls. 
                new Fact { VerificationFunc = houses =>
                    CheckNeighborCondition(houses,
                        houses => houses.Single(x => x.Nationality == Nationality.Norwegian),
                        house => house.Color == Color.Blue) },

                // The Blend smoker has a neighbor who drinks water. 
                new Fact { VerificationFunc = houses =>
                    CheckNeighborCondition(houses,
                        houses => houses.Single(x => x.Cigar == Cigar.Blend),
                        house => house.Drink == Drink.Water) },
            };
            List<House> housesSolved = SolveEinsteinRiddle(houses, facts);
            var solution = housesSolved.Single(x => x.Pet == Pet.Fish);
            Console.WriteLine(solution);
        }

        class FiveDigitBase120Number
        {
            public List<int> Digits { get; set; }
            public FiveDigitBase120Number Next()
            {
                // biggest number already
                if (Digits.All(x => x == 120)) return null;
                
                var copy = new List<int>(Digits);
                for (int i = 5 - 1; i >= 0; i--)
                {
                    if (Digits[i] == 120) continue;
                    ++copy[i];
                    return new FiveDigitBase120Number { Digits = copy };
                }
                throw new Exception("Should not have gotten to this point.");
            }

            public override string ToString() =>
                $"{Digits[0]}{Digits[1]}{Digits[2]}{Digits[3]}{Digits[4]}";
        }

        private static List<House> SolveEinsteinRiddle(List<House> houses, List<Fact> facts)
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
            var number = new FiveDigitBase120Number { Digits = new List<int> { 1, 1, 1, 1, 1 } };
            do 
            {
                var currentHouses = GetHouses(number, permutations);
                if (PassRules(currentHouses, facts))
                {
                    return currentHouses;
                }
                number = number.Next();
            }  while (number != null);            
            return null;
        }

        private static bool PassRules(List<House> currentHouses, List<Fact> facts)
        {
            throw new NotImplementedException();
        }

        private static List<House> GetHouses(FiveDigitBase120Number number, List<string> permutations)
        {
            var houses = Enumerable.Range(0, 5).Select(x => new House { Position = x }).ToList();
            var color = permutations[number.Digits[0]];
            Assign<Color>(houses, color, (house, color) => house.Color = color);

            var nationality = permutations[number.Digits[1]];
            Assign<Nationality>(houses, nationality, (house, nationality) => house.Nationality = nationality);

            var drink = permutations[number.Digits[2]];
            Assign<Drink>(houses, drink, (house, drink) => house.Drink = drink);

            var cigar = permutations[number.Digits[3]];
            Assign<Cigar>(houses, cigar, (house, cigar) => house.Cigar = cigar);

            var pet = permutations[number.Digits[4]];
            Assign<Pet>(houses, pet, (house, pet) => house.Pet = pet);

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
                Console.WriteLine(str);
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

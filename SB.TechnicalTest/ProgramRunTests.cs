using System;
using System.Diagnostics;
using SB.CoreTest;

namespace SB.TechnicalTest
{
    interface IBuilding
    {
        public int NumberFloors { get; }
        public int TotalDrops { get; }
        public bool DropMarble(int floorNumber);
        public void Reset();
    }

    class BuildingMock : IBuilding
    {
        public BuildingMock(int numberFloors, int highestSafeFloor)
        {
            NumberFloors = numberFloors;
            HighestSafeFloor = highestSafeFloor;
        }
        public int NumberFloors { get; }
        private int HighestSafeFloor { get; }
        public int TotalDrops { get; private set; }
        public bool DropMarble(int floorNumber)
        {
            bool flag = floorNumber <= HighestSafeFloor;
            TotalDrops++;
            return flag;
        }
        public void Reset()
        {
            TotalDrops = 0;
        }
    }

    static class AssertHelper
    {
        internal static void AssertGteq(this (int, int) result, bool isBetter)
            => Debug.Assert(isBetter == (result.Item1 >= result.Item2), $"{isBetter} {(result.Item1 >= result.Item2)}");
    }

    static class ProgramRunTests
    {
        internal static void GO()
        {
            AssertEq(setup: (0, 0), expect: (0, 0, 0)).AssertGteq(true);
            AssertEq(setup: (1, 0), expect: (0, 1, 1)).AssertGteq(true);
            AssertEq(setup: (1, 1), expect: (1, 1, 1)).AssertGteq(true);
            AssertEq(setup: (2, 0), expect: (0, 1, 1)).AssertGteq(true);
            AssertEq(setup: (2, 1), expect: (1, 2, 2)).AssertGteq(true);
            AssertEq(setup: (2, 2), expect: (2, 2, 2)).AssertGteq(true);
            AssertEq(setup: (3, 0), expect: (0, 1, 2)).AssertGteq(false);
            AssertEq(setup: (3, 1), expect: (1, 2, 2)).AssertGteq(true);
            AssertEq(setup: (3, 2), expect: (2, 3, 2)).AssertGteq(true);
            AssertEq(setup: (3, 3), expect: (3, 3, 2)).AssertGteq(true);
            AssertEq(setup: (4, 0), expect: (0, 1, 2)).AssertGteq(false);
            AssertEq(setup: (4, 1), expect: (1, 2, 2)).AssertGteq(true);
            AssertEq(setup: (4, 2), expect: (2, 3, 2)).AssertGteq(true);
            AssertEq(setup: (4, 3), expect: (3, 4, 3)).AssertGteq(true);
            AssertEq(setup: (4, 4), expect: (4, 4, 3)).AssertGteq(true);
            AssertEq(setup: (100, 73), expect: (73, 74, 7)).AssertGteq(true);
            Console.WriteLine("Done");
        }

        private static (int, int) AssertEq((int, int) setup, (int, int, int) expect)
        {
            var (p1, p2) = setup;
            var (a1, a2, a3) = RunTest(new BuildingMock(p1, p2));
            var (e1, e2, e3) = expect;
            Debug.Assert($"{a1} {a2} {a3}" == $"{e1} {e2} {e3}", $"{p1} {p2} | {a1} {a2} {a3}");
            return (a2, a3);
        }

        private static (int r1, int r2, int r3) RunTest<T>(T obj) where T : IBuilding
        {
            int h1 = Attempt1(obj);
            int t1 = obj.TotalDrops;

            obj.Reset();

            int h2 = Attempt2(obj);
            int t2 = obj.TotalDrops;
            if (h1 != h2) throw new InvalidOperationException("h1 != h2");
            return (h1, t1, t2);
        }

        private static int Attempt1<T>(T obj) where T : IBuilding
        {
            var i = 0;
            while (++i <= obj.NumberFloors && obj.DropMarble(i))
            {
                // Console.WriteLine("Hit");
            }
            return i - 1;
        }

        private static int Attempt2<T>(T obj) where T : IBuilding
        {
            int numberFloors = obj.NumberFloors;
            if (numberFloors == 0) return 0;
            if (numberFloors == 1) return obj.DropMarble(1) ? 1 : 0;
            int maxTrue = 0, minFalse = numberFloors + 1;
            bool isSearchUp = true;
            do
            {
                int floor = (maxTrue + minFalse) / 2;
                isSearchUp = obj.DropMarble(floor);
                if (isSearchUp)
                {
                    maxTrue = floor;
                }
                else
                {
                    minFalse = floor;
                }
            } while (AnsNotFound(minFalse, maxTrue));
            return maxTrue;
            static bool AnsNotFound(int minFalse, int maxTrue)
                => (minFalse - maxTrue) != 1;
        }
    }
}

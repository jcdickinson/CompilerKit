using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Sdk;

namespace CompilerKit.Collections.Generic
{
    public class DependencyTreeTests
    {
        private void AssertScc(IEnumerable<IReadOnlyList<char>> actual, params string[] expected)
        {
            var i = -1;
            var expectedList = new StringBuilder();
            var actualList = new StringBuilder();
            var valid = true;

            expectedList.AppendLine();
            actualList.AppendLine();

            // NB: reverse
            using (var actualEnum = actual.Reverse().GetEnumerator())
            {
                while (actualEnum.MoveNext())
                {
                    var actualScc = new HashSet<char>(actualEnum.Current);
                    foreach (var c in actualScc) actualList.Append(c);
                    actualList.AppendLine();

                    if (++i < expected.Length)
                    {
                        var expectedScc = new HashSet<char>(expected[i]);
                        expectedList.AppendLine(expected[i]);

                        actualScc.SymmetricExceptWith(expectedScc);
                        valid &= actualScc.Count == 0;
                    }
                    else
                    {
                        valid = false;
                    }
                }

                i++;
                for (; i < expected.Length; i++)
                {
                    valid = false;
                    expectedList.AppendLine(expected[i]);
                }

                if (!valid)
                {
                    throw new AssertActualExpectedException(expectedList.ToString(), actualList.ToString(), "Assert.Scc() Failure");
                }
            }
        }

        [Fact(DisplayName = "DependencyTree ResolveMembers should pass scenario 1.")]
        public void DependencyTree_ResolveDependencies_Scenario1()
        {
            // A
            // ! \
            // B - C

            var dt = new DependencyTree<char>();
            dt.Add('A', "B");
            dt.Add('B', "C");
            dt.Add('C', "A");

            AssertScc(dt.ResolveDependencies(), "ABC");
        }

        [Fact(DisplayName = "DependencyTree ResolveMembers should pass scenario 2.")]
        public void DependencyTree_ResolveDependencies_Scenario2()
        {
            // A
            // ! \
            // B - C <- D

            var dt = new DependencyTree<char>();
            dt.Add('A', "B");
            dt.Add('B', "C");
            dt.Add('C', "A");
            dt.Add('D', "C");

            AssertScc(dt.ResolveDependencies(), "D", "ABC");
        }

        [Fact(DisplayName = "DependencyTree ResolveMembers should pass scenario 3.")]
        public void DependencyTree_ResolveDependencies_Scenario3()
        {
            // A
            // ! \
            // B - C <- D    E <- F

            var dt = new DependencyTree<char>();
            dt.Add('A', "B");
            dt.Add('B', "C");
            dt.Add('C', "A");
            dt.Add('D', "C");
            dt.Add('F', "E");

            AssertScc(dt.ResolveDependencies(), "F", "E", "D", "ABC");
        }

        [Fact(DisplayName = "DependencyTree ResolveMembers should pass scenario 4.")]
        public void DependencyTree_ResolveDependencies_Scenario4()
        {
            // A
            // ! \
            // B - C <- D <- E <- F

            var dt = new DependencyTree<char>();
            dt.Add('A', "B");
            dt.Add('B', "C");
            dt.Add('C', "A");
            dt.Add('D', "C");
            dt.Add('E', "D");
            dt.Add('F', "E");

            AssertScc(dt.ResolveDependencies(), "F", "E", "D", "ABC");
        }

        [Fact(DisplayName = "DependencyTree ResolveMembers should pass scenario 5.")]
        public void DependencyTree_ResolveDependencies_Scenario5()
        {
            // A              ___
            // ! \           /   \.
            // B - C <- D <- E <- F

            var dt = new DependencyTree<char>();
            dt.Add('A', "B");
            dt.Add('B', "C");
            dt.Add('C', "A");
            dt.Add('D', "C");
            dt.Add('E', "FD");
            dt.Add('F', "E");

            AssertScc(dt.ResolveDependencies(), "EF", "D", "ABC");
        }

        [Fact(DisplayName = "DependencyTree ResolveMembers should pass scenario 6.")]
        public void DependencyTree_ResolveDependencies_Scenario6()
        {
            // A
            // ! \           /---\.
            // B - C <- D <- E <- F
            //          ^\
            //            G

            var dt = new DependencyTree<char>();
            dt.Add('A', "B");
            dt.Add('B', "C");
            dt.Add('C', "A");
            dt.Add('D', "CG");
            dt.Add('E', "FD");
            dt.Add('F', "E");

            AssertScc(dt.ResolveDependencies(), "EF", "D", "G", "ABC");
        }

        [Fact(DisplayName = "DependencyTree ResolveMembers should pass scenario 7.")]
        public void DependencyTree_ResolveDependencies_Scenario7()
        {
            // A
            // ! \           /---\.
            // B - C <- D <- E <- F
            //          ^\ /^
            //            G

            var dt = new DependencyTree<char>();
            dt.Add('A', "B");
            dt.Add('B', "C");
            dt.Add('C', "A");
            dt.Add('D', "C");
            dt.Add('E', "FD");
            dt.Add('F', "E");
            dt.Add('G', "DE");

            AssertScc(dt.ResolveDependencies(), "G", "EF", "D", "ABC");
        }

        [Fact(DisplayName = "DependencyTree ResolveMembers should pass scenario 8.")]
        public void DependencyTree_ResolveDependencies_Scenario8()
        {
            // A
            // ! \           /---\.
            // B - C <- D <- E <- F
            //          ^\ ./
            //            G

            var dt = new DependencyTree<char>();
            dt.Add('A', "B");
            dt.Add('B', "C");
            dt.Add('C', "A");
            dt.Add('D', "C");
            dt.Add('E', "FGD");
            dt.Add('F', "E");
            dt.Add('G', "D");

            AssertScc(dt.ResolveDependencies(), "EF", "G", "D", "ABC");
        }

        [Fact(DisplayName = "DependencyTree ResolveMembers should pass scenario 9.")]
        public void DependencyTree_ResolveDependencies_Scenario9()
        {
            // A
            // ! \           /---\.
            // B - C <- D <- E <- F
            //     ^\ ./
            //       G

            var dt = new DependencyTree<char>();
            dt.Add('A', "B");
            dt.Add('B', "C");
            dt.Add('C', "A");
            dt.Add('D', "G");
            dt.Add('F', "E");
            dt.Add('E', "FD");
            dt.Add('G', "C");

            AssertScc(dt.ResolveDependencies(), "EF", "D", "G", "ABC");
        }
    }
}

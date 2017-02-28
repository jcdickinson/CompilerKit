using Xunit;

namespace CompilerKit.Collections.Generic
{
    public class DisjointSetTests
    {
        [Fact(DisplayName = "DisjointSet should return a single union for a single strongly connected component")]
        public void DisjointSet_Union_One_SCC()
        {
            var ds = new DisjointSet<int>();
            ds.Union(1, 2);
            ds.Union(2, 2);
            ds.Union(5, 2);
            ds.Union(6, 7);
            ds.Union(7, 2);

            Assert.Equal(1, ds[1]);
            Assert.Equal(1, ds[2]);
            Assert.Equal(1, ds[5]);
            Assert.Equal(1, ds[6]);
            Assert.Equal(1, ds[7]);
        }

        [Fact(DisplayName = "DisjointSet should return a two unions for two strongly connected components")]
        public void DisjointSet_Union_Two_SCCs()
        {
            var ds = new DisjointSet<int>();
            ds.Union(0, 4);
            ds.Union(1, 2);
            ds.Union(2, 2);
            ds.Union(3, 3);
            ds.Union(4, 3);
            ds.Union(5, 2);
            ds.Union(6, 7);
            ds.Union(7, 2);

            Assert.Equal(1, ds[1]);
            Assert.Equal(1, ds[2]);
            Assert.Equal(1, ds[5]);
            Assert.Equal(1, ds[6]);
            Assert.Equal(1, ds[7]);

            Assert.Equal(0, ds[0]);
            Assert.Equal(0, ds[4]);
            Assert.Equal(0, ds[3]);
        }
    }
}

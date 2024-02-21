using Arch_TL.BLL;
using Xunit;

namespace Arch_TL.TEST
{
    public class MathTest
    {
        [Theory]
        [InlineData(2, 9, -1, -7, 1)]
        [InlineData(2, 1, 1)]
        [InlineData(0)]
        [InlineData(0, 1)]
        public void SumTest(int expected, params int[] elements)
        {
            int result = ArchMath.Sum(elements);
            Assert.Equal(expected, result);
        }
        
        [Fact]
        public void SumTestFact()
        {
            int result = ArchMath.Sum(1 + 2, 3);
            Assert.Equal(6, result);
        }
    }
}
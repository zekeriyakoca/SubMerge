using Xunit;

namespace SubMerge.Func.Test
{
    public class SampleTests
    {
        [Fact]
        public void FailingTest()
        {
            Assert.True(false);
        }
        [Fact]
        public void PassingTest()
        {
            Assert.True(true);
        }
    }
}
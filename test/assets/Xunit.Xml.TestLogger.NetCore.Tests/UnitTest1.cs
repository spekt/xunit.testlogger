using System;
using Xunit;

namespace Xunit.Xml.TestLogger.NetCore.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void PassTest11()
        {
        }

        [Fact]
        public void FailTest11()
        {
            Assert.False(true);
        }

        [Fact(Skip="Skipped")]
        public void SkipTest11()
        {
        }
    }

    public class UnitTest2
    {
        [Fact]
        public void PAssTest21()
        {
            Assert.Equal(2, 2);
        }

        [Fact]
        public void FailTest22()
        {
            Assert.False(true);
        }
    }

    public class UnitTest3
    {
        [Theory]
        [InlineData("Head\x80r")]    // See issue #25
        public void TestInvalidName(string input)
        {
        }
    }
}

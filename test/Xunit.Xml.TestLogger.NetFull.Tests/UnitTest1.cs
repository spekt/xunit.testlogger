using System;
using Xunit;

namespace Xunit.Xml.TestLogger.NetFull.Tests
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
}

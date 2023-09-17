using System;
using Xunit;

namespace Xunit.Xml.TestLogger.NetCore.Tests
{
    public class ParentUnitNestedTest3331
    {
        [Fact]
        public void PassTest33311()
        {
        }
    }

    public class ParentUnitNestedTest3332
    {
        public class ChildUnitNestedTest3332 : ParentUnitNestedTest3331
        {
            [Fact]
            public void PassTest33321()
            {
                Assert.Equal(2, 2);
            }
        }
    }
}

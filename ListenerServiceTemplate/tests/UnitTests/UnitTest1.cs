using ListenerServiceTemplate.Domain;

namespace UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var obj = new TestCaseHandler();
            var result = obj.Test("testing");
            //Assert
            Assert.Equal("testing", result);
        }
    }
}
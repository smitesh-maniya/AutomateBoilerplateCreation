using FastEndpoints;
using Microsoft.AspNetCore.Http;
using ApiServiceTemplate.Domain;

namespace UnitTests.Domain
{
    public class UnitTest1
    {
        [Fact]
        public async Task TestEndpoint_Returns_Result()
        {
            var ep = Factory.Create<TestCaseHandler>(ctx => { });
            var req = new EmptyRequest();
            await ep.HandleAsync(req, new CancellationToken());

            Assert.Equal(StatusCodes.Status200OK, ep.HttpContext.Response.StatusCode);
        }
    }
}
using TradeX.Application.Clients.Tests.Fixtures;

namespace TradeX.Application.Clients.Tests.Features.UseCases
{
    public class Tests : BaseFixture
    {
        [Fact]
        public async void ShowAssertTrue()
        {            
            Assert.True(await Task.FromResult(true));
        }
    }
}
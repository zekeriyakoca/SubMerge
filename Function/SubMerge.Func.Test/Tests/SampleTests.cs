using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace SubMerge.Func.Test
{
    public class SampleTests
    {
        [Fact]
        public async void health_check_should_return_success()
        {
            var request = TestFactory.CreateHttpRequest();
            var logger = TestFactory.CreateLogger(LoggerTypes.Null);

            var response = (OkObjectResult)HealthCheck.Run(request, logger);

            Assert.Equal(response.Value, "I've survived your bugs!");
        }
    }
}
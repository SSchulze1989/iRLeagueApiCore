using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Client.Results
{
    public class HttpExtensionsTests
    {
        private const string testString = "TestMessage";
        private const long testValue = 42;

        [Fact]
        public async Task ShouldReturnActionResult()
        {
            var content = new StringContent(TestContent.AsJson());
            var testMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = content
            };

            var result = await testMessage.ToClientActionResultAsync<TestContent>();
            Assert.True(result.Success);
            Assert.Equal(testString, result.Content.String);
            Assert.Equal(testValue, result.Content.Value);
        }

        [Fact] async Task ShouldReturnNoContentActionResult()
        {
            var testMessge = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.NoContent,
                Content = null
            };

            var result = await testMessge.ToClientActionResultAsync<TestContent>();
            Assert.True(result.Success);
            Assert.Equal(HttpStatusCode.NoContent, result.HttpStatusCode);
            Assert.Null(result.Content);
        }

        [Fact]
        public async Task ShouldReturnBadRequestActionResult()
        {
            var response = new BadRequestResponse()
            {
                Status = "Bad Request",
                Errors = new ValidationError[] { new ValidationError() { Error = "TestError", Property = "String", Value = 42 } },
            };
            var content = new StringContent(JsonConvert.SerializeObject(response));
            var testMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = content
            };

            var result = await testMessage.ToClientActionResultAsync<TestContent>();
            Assert.False(result.Success);
            Assert.Equal(HttpStatusCode.BadRequest, result.HttpStatusCode);
            Assert.Null(result.Content);
            Assert.Single(result.Errors);
            var error = Assert.IsType<ValidationError>(result.Errors.First());
            Assert.Equal("TestError", error.Error);
            Assert.Equal("String", error.Property);
        }

        [Fact]
        public async Task ShouldReturnForbiddenActionResult()
        {
            var testMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = default
            };

            var result = await testMessage.ToClientActionResultAsync<TestContent>();
            Assert.False(result.Success);
            Assert.Equal(HttpStatusCode.Forbidden, result.HttpStatusCode);
            Assert.Null(result.Content);
        }

        private class TestContent
        {
            public string String { get; set; } = testString;
            public long Value { get; set; } = testValue;

            public static string AsJson()
            {
                return JsonConvert.SerializeObject(new TestContent());
            }
        }
    }
}

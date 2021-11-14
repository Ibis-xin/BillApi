using BillApi.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using NUnit.Framework;

namespace BillApi.Tests.Helper
{
    [TestFixture]
    public class BasicAuthTests
    {
        private HttpRequest _request;

        [SetUp]
        public void Setup()
        {
            _request = Substitute.For<HttpRequest>();
        }

        [Test]
        public void BasicAuthParse_WhenCorrectBasicAuth_ReturnsCorrectUserData()
        {
            const string userIdIs123AndPasswordIs321 = "Basic MTIzOjMyMQ==";
            const string userId = "123";
            const string password = "321";

            _request.Headers["Authorization"].Returns(new StringValues(userIdIs123AndPasswordIs321));
            
            var result = new BasicAuth().Parse(_request);

            Assert.That(result.userId, Is.EqualTo(userId));
            Assert.That(result.password, Is.EqualTo(password));
        }

        [Test]
        public void BasicAuthParse_WhenNullOrEmptyBasicAuth_ReturnsEmptyUserData()
        {
            const string nullBasicAuth = "";
            _request.Headers["Authorization"].Returns(new StringValues(nullBasicAuth));

            var result = new BasicAuth().Parse(_request);

            Assert.That(result.userId, Is.EqualTo(string.Empty));
            Assert.That(result.password, Is.EqualTo(string.Empty));
        }
    }
}
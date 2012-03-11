using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;
using Match = System.Text.RegularExpressions.Match;

namespace System.Web.Helpers.Test
{
    public class AntiForgeryWorkerTest
    {
        private static string _antiForgeryTokenCookieName = AntiForgeryData.GetAntiForgeryTokenName("/SomeAppPath");
        private const string _serializedValuePrefix = @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""Creation: ";
        private const string _someValueSuffix = @", Value: some value, Salt: some other salt, Username: username"" />";
        private readonly Regex _randomFormValueSuffixRegex = new Regex(@", Value: (?<value>[A-Za-z0-9/\+=]{24}), Salt: some other salt, Username: username"" />$");
        private readonly Regex _randomCookieValueSuffixRegex = new Regex(@", Value: (?<value>[A-Za-z0-9/\+=]{24}), Salt: ");

        [Fact]
        public void Serializer_DefaultValueIsAntiForgeryDataSerializer()
        {
            Assert.Same(typeof(AntiForgeryDataSerializer), new AntiForgeryWorker().Serializer.GetType());
        }

        [Fact]
        public void GetHtml_ReturnsFormFieldAndSetsCookieValueIfDoesNotExist()
        {
            // Arrange
            AntiForgeryWorker worker = new AntiForgeryWorker()
            {
                Serializer = new DummyAntiForgeryTokenSerializer()
            };
            var context = CreateContext();

            // Act
            string formValue = worker.GetHtml(context, "some other salt", null, null).ToHtmlString();

            // Assert
            Assert.True(formValue.StartsWith(_serializedValuePrefix), "Form value prefix did not match.");

            Match formMatch = _randomFormValueSuffixRegex.Match(formValue);
            string formTokenValue = formMatch.Groups["value"].Value;

            HttpCookie cookie = context.Response.Cookies[_antiForgeryTokenCookieName];
            Assert.NotNull(cookie);
            Assert.True(cookie.HttpOnly, "Cookie should have HTTP-only flag set.");
            Assert.True(String.IsNullOrEmpty(cookie.Domain), "Domain should not have been set.");
            Assert.Equal("/", cookie.Path);

            Match cookieMatch = _randomCookieValueSuffixRegex.Match(cookie.Value);
            string cookieTokenValue = cookieMatch.Groups["value"].Value;

            Assert.Equal(formTokenValue, cookieTokenValue);
        }

        [Fact]
        public void GetHtml_SetsCookieDomainAndPathIfSpecified()
        {
            // Arrange
            AntiForgeryWorker worker = new AntiForgeryWorker()
            {
                Serializer = new DummyAntiForgeryTokenSerializer()
            };
            var context = CreateContext();

            // Act
            string formValue = worker.GetHtml(context, "some other salt", "theDomain", "thePath").ToHtmlString();

            // Assert
            Assert.True(formValue.StartsWith(_serializedValuePrefix), "Form value prefix did not match.");

            Match formMatch = _randomFormValueSuffixRegex.Match(formValue);
            string formTokenValue = formMatch.Groups["value"].Value;

            HttpCookie cookie = context.Response.Cookies[_antiForgeryTokenCookieName];
            Assert.NotNull(cookie);
            Assert.True(cookie.HttpOnly, "Cookie should have HTTP-only flag set.");
            Assert.Equal("theDomain", cookie.Domain);
            Assert.Equal("thePath", cookie.Path);

            Match cookieMatch = _randomCookieValueSuffixRegex.Match(cookie.Value);
            string cookieTokenValue = cookieMatch.Groups["value"].Value;

            Assert.Equal(formTokenValue, cookieTokenValue);
        }

        [Fact]
        public void GetHtml_ReusesCookieValueIfExistsAndIsValid()
        {
            // Arrange
            AntiForgeryWorker worker = new AntiForgeryWorker()
            {
                Serializer = new DummyAntiForgeryTokenSerializer()
            };
            var context = CreateContext("2001-01-01:some value:some salt:username");

            // Act
            string formValue = worker.GetHtml(context, "some other salt", null, null).ToHtmlString();

            // Assert
            Assert.True(formValue.StartsWith(_serializedValuePrefix), "Form value prefix did not match.");
            Assert.True(formValue.EndsWith(_someValueSuffix), "Form value suffix did not match.");
            Assert.Equal(0, context.Response.Cookies.Count);
        }

        [Fact]
        public void GetHtml_CreatesNewCookieValueIfCookieExistsButIsNotValid()
        {
            // Arrange
            AntiForgeryWorker worker = new AntiForgeryWorker()
            {
                Serializer = new DummyAntiForgeryTokenSerializer()
            };
            var context = CreateContext("invalid");

            // Act
            string formValue = worker.GetHtml(context, "some other salt", null, null).ToHtmlString();

            // Assert
            Assert.True(formValue.StartsWith(_serializedValuePrefix), "Form value prefix did not match.");

            Match formMatch = _randomFormValueSuffixRegex.Match(formValue);
            string formTokenValue = formMatch.Groups["value"].Value;

            HttpCookie cookie = context.Response.Cookies[_antiForgeryTokenCookieName];
            Assert.NotNull(cookie);
            Assert.True(cookie.HttpOnly, "Cookie should have HTTP-only flag set.");
            Assert.True(String.IsNullOrEmpty(cookie.Domain), "Domain should not have been set.");
            Assert.Equal("/", cookie.Path);

            Match cookieMatch = _randomCookieValueSuffixRegex.Match(cookie.Value);
            string cookieTokenValue = cookieMatch.Groups["value"].Value;

            Assert.Equal(formTokenValue, cookieTokenValue);
        }

        [Fact]
        public void Validate_ThrowsIfCookieMissing()
        {
            Validate_Helper(null, "2001-01-01:some other value:the real salt:username");
        }

        [Fact]
        public void Validate_ThrowsIfCookieValueDoesNotMatchFormValue()
        {
            Validate_Helper("2001-01-01:some value:the real salt:username", "2001-01-01:some other value:the real salt:username");
        }

        [Fact]
        public void Validate_ThrowsIfFormSaltDoesNotMatchAttributeSalt()
        {
            Validate_Helper("2001-01-01:some value:some salt:username", "2001-01-01:some value:some other salt:username");
        }

        [Fact]
        public void Validate_ThrowsIfFormValueMissing()
        {
            Validate_Helper("2001-01-01:some value:the real salt:username", null);
        }

        [Fact]
        public void Validate_ThrowsIfUsernameInFormIsIncorrect()
        {
            Validate_Helper("2001-01-01:value:salt:username", "2001-01-01:value:salt:different username");
        }

        private static void Validate_Helper(string cookieValue, string formValue, string username = "username")
        {
            // Arrange
            //ValidateAntiForgeryTokenAttribute attribute = GetAttribute();
            var context = CreateContext(cookieValue, formValue, username);

            AntiForgeryWorker worker = new AntiForgeryWorker()
            {
                Serializer = new DummyAntiForgeryTokenSerializer()
            };

            // Act & Assert
            Assert.Throws<HttpAntiForgeryException>(
                delegate
                {
                    //attribute.OnAuthorization(authContext);
                    worker.Validate(context, "the real salt");
                }, "A required anti-forgery token was not supplied or was invalid.");
        }

        private static HttpContextBase CreateContext(string cookieValue = null, string formValue = null, string username = "username")
        {
            HttpCookieCollection requestCookies = new HttpCookieCollection();
            if (!String.IsNullOrEmpty(cookieValue))
            {
                requestCookies.Set(new HttpCookie(_antiForgeryTokenCookieName, cookieValue));
            }
            NameValueCollection formCollection = new NameValueCollection();
            if (!String.IsNullOrEmpty(formValue))
            {
                formCollection.Set(AntiForgeryData.GetAntiForgeryTokenName(null), formValue);
            }

            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(c => c.Request.ApplicationPath).Returns("/SomeAppPath");
            mockContext.Setup(c => c.Request.Cookies).Returns(requestCookies);
            mockContext.Setup(c => c.Request.Form).Returns(formCollection);
            mockContext.Setup(c => c.Response.Cookies).Returns(new HttpCookieCollection());
            mockContext.Setup(c => c.User.Identity.IsAuthenticated).Returns(true);
            mockContext.Setup(c => c.User.Identity.Name).Returns(username);

            return mockContext.Object;
        }

        internal class DummyAntiForgeryTokenSerializer : AntiForgeryDataSerializer
        {
            public override string Serialize(AntiForgeryData token)
            {
                return String.Format(CultureInfo.InvariantCulture, "Creation: {0}, Value: {1}, Salt: {2}, Username: {3}",
                                     token.CreationDate, token.Value, token.Salt, token.Username);
            }

            public override AntiForgeryData Deserialize(string serializedToken)
            {
                if (serializedToken == "invalid")
                {
                    throw new HttpAntiForgeryException();
                }
                string[] parts = serializedToken.Split(':');
                return new AntiForgeryData()
                {
                    CreationDate = DateTime.Parse(parts[0], CultureInfo.InvariantCulture),
                    Value = parts[1],
                    Salt = parts[2],
                    Username = parts[3]
                };
            }
        }
    }
}

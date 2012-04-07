// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web;
using System.Web.Helpers;
using System.Web.Helpers.Test;
using System.Web.TestUtil;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace Microsoft.Web.Helpers.Test
{
    public class TwitterTest
    {
        /// <summary>
        ///A test for Profile
        ///</summary>
        [Fact]
        public void Profile_ReturnsValidData()
        {
            // Arrange
            string twitterUserName = "my-userName";
            int width = 100;
            int height = 100;
            string backgroundShellColor = "my-backgroundShellColor";
            string shellColor = "my-shellColor";
            string tweetsBackgroundColor = "tweetsBackgroundColor";
            string tweetsColor = "tweets-color";
            string tweetsLinksColor = "tweets Linkcolor";
            bool scrollBar = false;
            bool loop = false;
            bool live = false;
            bool hashTags = false;
            bool timestamp = false;
            bool avatars = false;
            var behavior = "all";
            int searchInterval = 10;

            // Act
            string actual = Twitter.Profile(twitterUserName, width, height, backgroundShellColor, shellColor, tweetsBackgroundColor, tweetsColor, tweetsLinksColor,
                                            5, scrollBar, loop, live, hashTags, timestamp, avatars, behavior, searchInterval).ToString();

            // Assert
            var json = GetTwitterJSObject(actual);
            Assert.Equal(json.type, "profile");
            Assert.Equal(json.interval, 1000 * searchInterval);
            Assert.Equal(json.width.ToString(), width.ToString());
            Assert.Equal(json.height.ToString(), height.ToString());
            Assert.Equal(json.theme.shell.background, backgroundShellColor);
            Assert.Equal(json.theme.shell.color, shellColor);
            Assert.Equal(json.theme.tweets.background, tweetsBackgroundColor);
            Assert.Equal(json.theme.tweets.color, tweetsColor);
            Assert.Equal(json.theme.tweets.links, tweetsLinksColor);
            Assert.Equal(json.features.scrollbar, scrollBar);
            Assert.Equal(json.features.loop, loop);
            Assert.Equal(json.features.live, live);
            Assert.Equal(json.features.hashtags, hashTags);
            Assert.Equal(json.features.avatars, avatars);
            Assert.Equal(json.features.behavior, behavior.ToLowerInvariant());
            Assert.True(actual.Contains(".setUser('my-userName')"));
        }

        [Fact]
        public void ProfileJSEncodesParameters()
        {
            // Arrange
            string twitterUserName = "\"my-userName\'";
            string backgroundShellColor = "\\foo";
            string shellColor = "<shellColor>\\";
            string tweetsBackgroundColor = "<tweetsBackgroundColor";
            string tweetsColor = "<tweetsColor>";
            string tweetsLinksColor = "<tweetsLinkColor>";

            // Act
            string actual = Twitter.Profile(twitterUserName, 100, 100, backgroundShellColor, shellColor, tweetsBackgroundColor, tweetsColor, tweetsLinksColor).ToString();

            // Assert
            Assert.True(actual.Contains("background: '\\\\foo"));
            Assert.True(actual.Contains("color: '\\u003cshellColor\\u003e\\\\"));
            Assert.True(actual.Contains("background: '\\u003ctweetsBackgroundColor"));
            Assert.True(actual.Contains("color: '\\u003ctweetsColor\\u003e"));
            Assert.True(actual.Contains("links: '\\u003ctweetsLinkColor\\u003e"));
            Assert.True(actual.Contains(".setUser('\\\"my-userName\\u0027')"));
        }

        [Fact]
        public void Search_ReturnsValidData()
        {
            // Arrange
            string search = "awesome-search-term";
            int width = 100;
            int height = 100;
            string title = "cust-title";
            string caption = "some caption";
            string backgroundShellColor = "background-shell-color";
            string shellColor = "shellColorValue";
            string tweetsBackgroundColor = "tweetsBackgroundColor";
            string tweetsColor = "tweetsColorVal";
            string tweetsLinksColor = "tweetsLinkColor";
            bool scrollBar = false;
            bool loop = false;
            bool live = true;
            bool hashTags = false;
            bool timestamp = true;
            bool avatars = true;
            bool topTweets = true;
            var behavior = "default";
            int searchInterval = 10;

            // Act
            string actual = Twitter.Search(search, width, height, title, caption, backgroundShellColor, shellColor, tweetsBackgroundColor, tweetsColor, tweetsLinksColor, scrollBar,
                                           loop, live, hashTags, timestamp, avatars, topTweets, behavior, searchInterval).ToString();

            // Assert
            var json = GetTwitterJSObject(actual);
            Assert.Equal(json.type, "search");
            Assert.Equal(json.search, search);
            Assert.Equal(json.interval, 1000 * searchInterval);
            Assert.Equal(json.title, title);
            Assert.Equal(json.subject, caption);
            Assert.Equal(json.width.ToString(), width.ToString());
            Assert.Equal(json.height.ToString(), height.ToString());
            Assert.Equal(json.theme.shell.background, backgroundShellColor);
            Assert.Equal(json.theme.shell.color, shellColor);
            Assert.Equal(json.theme.tweets.background, tweetsBackgroundColor);
            Assert.Equal(json.theme.tweets.color, tweetsColor);
            Assert.Equal(json.theme.tweets.links, tweetsLinksColor);
            Assert.Equal(json.features.scrollbar, scrollBar);
            Assert.Equal(json.features.loop, loop);
            Assert.Equal(json.features.live, live);
            Assert.Equal(json.features.hashtags, hashTags);
            Assert.Equal(json.features.avatars, avatars);
            Assert.Equal(json.features.toptweets, topTweets);
            Assert.Equal(json.features.behavior, behavior.ToLowerInvariant());
        }

        [Fact]
        public void SearchJavascriptEncodesParameters()
        {
            // Arrange
            string search = "<script>";
            int width = 100;
            int height = 100;
            string title = "'title'";
            string caption = "<caption>";
            string backgroundShellColor = "\\foo";
            string shellColor = "<shellColor>\\";
            string tweetsBackgroundColor = "<tweetsBackgroundColor";
            string tweetsColor = "<tweetsColor>";
            string tweetsLinksColor = "<tweetsLinkColor>";

            // Act
            string actual = Twitter.Search(search, width, height, title, caption, backgroundShellColor, shellColor, tweetsBackgroundColor, tweetsColor, tweetsLinksColor).ToString();

            // Assert
            Assert.True(actual.Contains("search: '\\u003cscript\\u003e'"));
            Assert.True(actual.Contains("title: '\\u0027title\\u0027'"));
            Assert.True(actual.Contains("subject: '\\u003ccaption\\u003e'"));
            Assert.True(actual.Contains("background: '\\\\foo"));
            Assert.True(actual.Contains("color: '\\u003cshellColor\\u003e\\\\"));
            Assert.True(actual.Contains("background: '\\u003ctweetsBackgroundColor"));
            Assert.True(actual.Contains("color: '\\u003ctweetsColor\\u003e"));
            Assert.True(actual.Contains("links: '\\u003ctweetsLinkColor\\u003e"));
        }

        [Fact]
        public void SearchWithInvalidArgs_ThrowsArgumentException()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => Twitter.Search(null).ToString(), "searchQuery");
        }

        [Fact]
        public void ProfileWithInvalidArgs_ThrowsArgumentException()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => Twitter.Profile(null).ToString(), "userName");
        }

        [Fact]
        public void SearchRendersRendersValidXhtml()
        {
            string result = "<html> <head> \n <title> </title> \n </head> \n <body> \n" +
                            Twitter.Search("any<>term") +
                            "\n </body> \n </html>";
            HtmlString htmlResult = new HtmlString(result);
            XhtmlAssert.Validate1_1(htmlResult);
        }

        [Fact]
        public void SearchEncodesSearchTerms()
        {
            // Act 
            string result = Twitter.Search("'any term'", backgroundShellColor: "\"bad-color").ToString();

            // Assert
            Assert.True(result.Contains(@"background: '\""bad-color',"));
            //Assert.True(result.Contains(@"search: @"'\u0027any term\u0027'","));
        }

        [Fact]
        public void ProfileRendersRendersValidXhtml()
        {
            string result = "<html> <head> \n <title> </title> \n </head> \n <body> \n" +
                            Twitter.Profile("any<>Name") +
                            "\n </body> \n </html>";
            HtmlString htmlResult = new HtmlString(result);
            XhtmlAssert.Validate1_1(htmlResult);
        }

        [Fact]
        public void ProfileEncodesSearchTerms()
        {
            // Act 
            string result = Twitter.Profile("'some user'", backgroundShellColor: "\"malformed-color").ToString();

            // Assert
            Assert.True(result.Contains(@"background: '\""malformed-color'"));
            Assert.True(result.Contains("setUser('\\u0027some user\\u0027')"));
        }

        [Fact]
        public void TweetButtonWithDefaultAttributes()
        {
            // Arrange
            string expected = @"<a href=""http://twitter.com/share"" class=""twitter-share-button"" data-count=""vertical"">Tweet</a><script type=""text/javascript"" src=""http://platform.twitter.com/widgets.js""></script>";

            // Act
            string result = Twitter.TweetButton().ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(result, expected);
        }

        [Fact]
        public void TweetButtonWithSpeicifedAttributes()
        {
            // Arrange
            string expected = @"<a href=""http://twitter.com/share"" class=""twitter-share-button"" data-text=""tweet-text"" data-url=""http://www.microsoft.com"" data-via=""userName"" data-related=""related-userName:rel-desc"" data-count=""none"">my-share-text</a><script type=""text/javascript"" src=""http://platform.twitter.com/widgets.js""></script>";

            // Act
            string result = Twitter.TweetButton("none", shareText: "my-share-text", tweetText: "tweet-text", url: "http://www.microsoft.com", language: "en", userName: "userName", relatedUserName: "related-userName", relatedUserDescription: "rel-desc").ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(result, expected);
        }

        [Fact]
        public void TweetButtonHtmlEncodesValues()
        {
            // Arrange
            string expected = @"<a href=""http://twitter.com/share"" class=""twitter-share-button"" data-text=""&lt;tweet-text>"" data-url=""&lt;http://www.microsoft.com>"" data-via=""&lt;userName>"" data-related=""&lt;related-userName>:&lt;rel-desc>"" data-count=""none"">&lt;Tweet</a><script type=""text/javascript"" src=""http://platform.twitter.com/widgets.js""></script>";
            // Act
            string result = Twitter.TweetButton("none", shareText: "<Tweet", tweetText: "<tweet-text>", url: "<http://www.microsoft.com>", language: "en", userName: "<userName>", relatedUserName: "<related-userName>", relatedUserDescription: "<rel-desc>").ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(result, expected);
        }

        [Fact]
        public void FollowButtonWithDefaultParameters()
        {
            // Arrange
            string expected = @"<a href=""http://www.twitter.com/my-twitter-userName""><img src=""http://twitter-badges.s3.amazonaws.com/follow_me-a.png"" alt=""Follow my-twitter-userName on Twitter""/></a>";

            // Act
            string result = Twitter.FollowButton("my-twitter-userName").ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(result, expected);
        }

        [Fact]
        public void FollowButtonWithSpecifiedParmeters()
        {
            // Arrange
            string expected = @"<a href=""http://www.twitter.com/my-twitter-userName""><img src=""http://twitter-badges.s3.amazonaws.com/t_logo-b.png"" alt=""Follow my-twitter-userName on Twitter""/></a>";

            // Act
            string result = Twitter.FollowButton("my-twitter-userName", "t_logo", "b").ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(result, expected);
        }

        [Fact]
        public void FollowButtonEncodesParameters()
        {
            // Arrange
            string expected = @"<a href=""http://www.twitter.com/http%3a%2f%2fmy-twitter-userName%3cscript""><img src=""http://twitter-badges.s3.amazonaws.com/t_logo-b.png"" alt=""Follow http://my-twitter-userName&lt;script on Twitter""/></a>";

            // Act
            string result = Twitter.FollowButton("http://my-twitter-userName<script", "t_logo", "b").ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(result, expected);
        }

        [Fact]
        public void FavesReturnsValidData()
        {
            // Arrange
            string twitterUserName = "my-userName";
            string title = "my-title";
            string caption = "my-caption";
            int width = 100;
            int height = 100;
            string backgroundShellColor = "my-backgroundShellColor";
            string shellColor = "my-shellColor";
            string tweetsBackgroundColor = "tweetsBackgroundColor";
            string tweetsColor = "tweets-color";
            string tweetsLinksColor = "tweets Linkcolor";
            int numTweets = 4;
            bool scrollBar = false;
            bool loop = false;
            bool live = false;
            bool hashTags = false;
            bool timestamp = false;
            bool avatars = false;
            var behavior = "all";
            int searchInterval = 10;

            // Act
            string actual = Twitter.Faves(twitterUserName, width, height, title, caption, backgroundShellColor, shellColor, tweetsBackgroundColor, tweetsColor, tweetsLinksColor,
                                          numTweets, scrollBar, loop, live, hashTags, timestamp, avatars, "all", searchInterval).ToString();

            // Assert
            var json = GetTwitterJSObject(actual);
            Assert.Equal(json.type, "faves");
            Assert.Equal(json.interval, 1000 * searchInterval);
            Assert.Equal(json.width.ToString(), width.ToString());
            Assert.Equal(json.height.ToString(), height.ToString());
            Assert.Equal(json.title, title);
            Assert.Equal(json.subject, caption);
            Assert.Equal(json.theme.shell.background, backgroundShellColor);
            Assert.Equal(json.theme.shell.color, shellColor);
            Assert.Equal(json.theme.tweets.background, tweetsBackgroundColor);
            Assert.Equal(json.theme.tweets.color, tweetsColor);
            Assert.Equal(json.theme.tweets.links, tweetsLinksColor);
            Assert.Equal(json.features.scrollbar, scrollBar);
            Assert.Equal(json.features.loop, loop);
            Assert.Equal(json.features.live, live);
            Assert.Equal(json.features.hashtags, hashTags);
            Assert.Equal(json.features.avatars, avatars);
            Assert.Equal(json.features.behavior, behavior.ToLowerInvariant());
            Assert.True(actual.Contains(".setUser('my-userName')"));
        }

        [Fact]
        public void FavesJavascriptEncodesParameters()
        {
            // Arrange
            string twitterUserName = "<my-userName>";
            string title = "<my-title>";
            string caption = "<my-caption>";
            int width = 100;
            int height = 100;
            string backgroundShellColor = "<my-backgroundShellColor>";
            string shellColor = "<my-shellColor>";
            string tweetsBackgroundColor = "<tweetsBackgroundColor>";
            string tweetsColor = "<tweets-color>";
            string tweetsLinksColor = "<tweets Linkcolor>";

            // Act
            string actual = Twitter.Faves(twitterUserName, width, height, title, caption, backgroundShellColor, shellColor, tweetsBackgroundColor, tweetsColor, tweetsLinksColor).ToString();

            // Assert
            Assert.True(actual.Contains("title: '\\u003cmy-title\\u003e'"));
            Assert.True(actual.Contains("subject: '\\u003cmy-caption\\u003e'"));
            Assert.True(actual.Contains("background: '\\u003cmy-backgroundShellColor\\u003e'"));
            Assert.True(actual.Contains("color: '\\u003cmy-shellColor\\u003e'"));
            Assert.True(actual.Contains("background: '\\u003ctweetsBackgroundColor\\u003e'"));
            Assert.True(actual.Contains("color: '\\u003ctweets-color\\u003e"));
            Assert.True(actual.Contains("links: '\\u003ctweets Linkcolor\\u003e'"));
            Assert.True(actual.Contains(".setUser('\\u003cmy-userName\\u003e')"));
        }

        [Fact]
        public void FavesRendersRendersValidXhtml()
        {
            string result = "<html> <head> \n <title> </title> \n </head> \n <body> \n" +
                            Twitter.Faves("any<>Name") +
                            "\n </body> \n </html>";
            HtmlString htmlResult = new HtmlString(result);
            XhtmlAssert.Validate1_1(htmlResult);
        }

        [Fact]
        public void ListReturnsValidData()
        {
            // Arrange
            string twitterUserName = "my-userName";
            string list = "my-list";
            string title = "my-title";
            string caption = "my-caption";
            int width = 100;
            int height = 100;
            string backgroundShellColor = "my-backgroundShellColor";
            string shellColor = "my-shellColor";
            string tweetsBackgroundColor = "tweetsBackgroundColor";
            string tweetsColor = "tweets-color";
            string tweetsLinksColor = "tweets Linkcolor";
            int numTweets = 4;
            bool scrollBar = false;
            bool loop = false;
            bool live = false;
            bool hashTags = false;
            bool timestamp = false;
            bool avatars = false;
            var behavior = "all";
            int searchInterval = 10;

            // Act
            string actual = Twitter.List(twitterUserName, list, width, height, title, caption, backgroundShellColor, shellColor, tweetsBackgroundColor, tweetsColor, tweetsLinksColor,
                                         numTweets, scrollBar, loop, live, hashTags, timestamp, avatars, "all", searchInterval).ToString();

            // Assert
            var json = GetTwitterJSObject(actual);
            Assert.Equal(json.type, "list");
            Assert.Equal(json.interval, 1000 * searchInterval);
            Assert.Equal(json.width.ToString(), width.ToString());
            Assert.Equal(json.height.ToString(), height.ToString());
            Assert.Equal(json.title, title);
            Assert.Equal(json.subject, caption);
            Assert.Equal(json.theme.shell.background, backgroundShellColor);
            Assert.Equal(json.theme.shell.color, shellColor);
            Assert.Equal(json.theme.tweets.background, tweetsBackgroundColor);
            Assert.Equal(json.theme.tweets.color, tweetsColor);
            Assert.Equal(json.theme.tweets.links, tweetsLinksColor);
            Assert.Equal(json.features.scrollbar, scrollBar);
            Assert.Equal(json.features.loop, loop);
            Assert.Equal(json.features.live, live);
            Assert.Equal(json.features.hashtags, hashTags);
            Assert.Equal(json.features.avatars, avatars);
            Assert.Equal(json.features.behavior, behavior.ToLowerInvariant());
            Assert.True(actual.Contains(".setList('my-userName', 'my-list')"));
        }

        [Fact]
        public void ListJavascriptEncodesParameters()
        {
            // Arrange
            string twitterUserName = "<my-userName>";
            string list = "<my-list>";
            string title = "<my-title>";
            string caption = "<my-caption>";
            int width = 100;
            int height = 100;
            string backgroundShellColor = "<my-backgroundShellColor>";
            string shellColor = "<my-shellColor>";
            string tweetsBackgroundColor = "<tweetsBackgroundColor>";
            string tweetsColor = "<tweets-color>";
            string tweetsLinksColor = "<tweets Linkcolor>";

            // Act
            string actual = Twitter.List(twitterUserName, list, width, height, title, caption, backgroundShellColor, shellColor, tweetsBackgroundColor, tweetsColor, tweetsLinksColor).ToString();

            // Assert
            Assert.True(actual.Contains("title: '\\u003cmy-title\\u003e'"));
            Assert.True(actual.Contains("subject: '\\u003cmy-caption\\u003e'"));
            Assert.True(actual.Contains("background: '\\u003cmy-backgroundShellColor\\u003e'"));
            Assert.True(actual.Contains("color: '\\u003cmy-shellColor\\u003e'"));
            Assert.True(actual.Contains("background: '\\u003ctweetsBackgroundColor\\u003e'"));
            Assert.True(actual.Contains("color: '\\u003ctweets-color\\u003e"));
            Assert.True(actual.Contains("links: '\\u003ctweets Linkcolor\\u003e'"));
            Assert.True(actual.Contains(".setList('\\u003cmy-userName\\u003e', '\\u003cmy-list\\u003e')"));
        }

        [Fact]
        public void ListRendersRendersValidXhtml()
        {
            string result = "<html> <head> \n <title> </title> \n </head> \n <body> \n" +
                            Twitter.List("any<>Name", "my-list") +
                            "\n </body> \n </html>";
            HtmlString htmlResult = new HtmlString(result);
            XhtmlAssert.Validate1_1(htmlResult);
        }

        private static dynamic GetTwitterJSObject(string twitterOutput)
        {
            const string startString = "Widget(";
            int start = twitterOutput.IndexOf(startString) + startString.Length, end = twitterOutput.IndexOf(')');
            return Json.Decode(twitterOutput.Substring(start, end - start));
        }
    }
}

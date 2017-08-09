﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using NUglify.Html;
using Xunit;

namespace WebOptimizer.Test.Processors
{
    public class HtmlMinifierTest
    {
        [Theory2]
        [InlineData("<p class=\"foo\">", "<p class=foo>")]
        [InlineData("<p class=\"foo\"><!-- comment -->", "<p class=foo>")]
        public async Task MinifyHtml_DefaultSettings(string input, string output)
        {
            var minifier = new HtmlMinifier(new HtmlSettings());
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = new Dictionary<string, byte[]> { { "", output.AsByteArray() } };

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal(output, context.Object.Content.First().Value.AsString());
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext()));
        }

        [Theory2]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("<!-- comment -->")]
        [InlineData("   <!-- --> ")]
        [InlineData("\r\n  \t \r \n")]
        public async Task MinifyHtml_EmptyContent_Success(string input)
        {
            var minifier = new HtmlMinifier(new HtmlSettings());
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = new Dictionary<string, byte[]> { { "", input.AsByteArray() } };

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal("", context.Object.Content.First().Value.AsString());
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext()));
        }

        [Fact2]
        public async Task MinifyHtml_CustomSettings_Success()
        {
            var settings = new HtmlSettings { RemoveComments = false};
            var minifier = new HtmlMinifier(settings);
            var context = new Mock<IAssetContext>().SetupAllProperties();
            context.Object.Content = new Dictionary<string, byte[]> { { "", "\r\n<!-- foo -->\r\n".AsByteArray() } };

            await minifier.ExecuteAsync(context.Object);

            Assert.Equal("<!-- foo -->", context.Object.Content.First().Value.AsString());
            Assert.Equal("", minifier.CacheKey(new DefaultHttpContext()));
        }

        [Fact2]
        public void AddHtmlBundle_DefaultSettings_Success()
        {
            var pipeline = new AssetPipeline();
            var asset = pipeline.AddHtmlBundle("/foo.html", "file1.html", "file2.html");

            Assert.Equal("/foo.html", asset.Route);
            Assert.Equal("text/html; charset=UTF-8", asset.ContentType);
            Assert.Equal(2, asset.SourceFiles.Count());
            Assert.Equal(2, asset.Processors.Count);
        }

        [Fact2]
        public void AddHtmlBundle_CustomSettings_Success()
        {
            var settings = new HtmlSettings();
            var pipeline = new AssetPipeline();
            var asset = pipeline.AddHtmlBundle("/foo.html", settings, "file1.css", "file2.css");

            Assert.Equal("/foo.html", asset.Route);
            Assert.Equal("text/html; charset=UTF-8", asset.ContentType);
            Assert.Equal(2, asset.SourceFiles.Count());
            Assert.Equal(2, asset.Processors.Count);
        }

        [Fact2]
        public void AddHtmlFiles_DefaultSettings_Success()
        {
            var pipeline = new AssetPipeline();
            var asset = pipeline.MinifyHtmlFiles().First();

            Assert.Equal("/**/*.html", asset.Route);
            Assert.Equal("text/html; charset=UTF-8", asset.ContentType);
            Assert.Equal(1, asset.SourceFiles.Count());
            Assert.Equal(1, asset.Processors.Count);
        }
    }
}

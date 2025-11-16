using CycleManager.Tests.Integration.Helpers;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace CycleManager.Tests.Integration.Manager
{
    public class NewsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public NewsTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }

        [Fact]
        public async Task Index_ShouldShowListOfNews()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.NewsItems.Add(new NewsItem { Title = "News A", Message = "Message A", DatePosted = DateTime.Today, IsActive = true });
            db.NewsItems.Add(new NewsItem { Title = "News B", Message = "Message B", DatePosted = DateTime.Today, IsActive = true });
            await db.SaveChangesAsync();

            var client = _factory.CreateClient();

            var response = await client.GetAsync("/News");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("News A");
            html.Should().Contain("News B");
        }

        [Fact]
        public async Task Create_ShouldAddNewsItem()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var get = await client.GetAsync("/News/Create");
            get.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await get.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Title"] = "My News",
                ["Message"] = "This is a test",
                ["DatePosted"] = DateTime.Today.ToString("yyyy-MM-dd"),
                ["IsActive"] = "true"
            };

            var post = await client.PostAsync("/News/Create", new FormUrlEncodedContent(form));
            post.StatusCode.Should().Be(HttpStatusCode.Redirect);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var news = db.NewsItems.Single();
            news.Title.Should().Be("My News");
            news.Message.Should().Be("This is a test");
        }

        [Fact]
        public async Task Edit_ShouldUpdateNewsItem()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var item = new NewsItem { Title = "Old Title", Message = "Old Message", DatePosted = DateTime.Today, IsActive = true };
            db.NewsItems.Add(item);
            await db.SaveChangesAsync();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var get = await client.GetAsync($"/News/Edit/{item.Id}");
            get.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await get.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Id"] = item.Id.ToString(),
                ["Title"] = "New Title",
                ["Message"] = "New Message",
                ["DatePosted"] = DateTime.Today.ToString("yyyy-MM-dd"),
                ["IsActive"] = "true"
            };

            var post = await client.PostAsync($"/News/Edit/{item.Id}", new FormUrlEncodedContent(form));
            post.StatusCode.Should().Be(HttpStatusCode.Redirect);

            using var newsScope = _factory.Services.CreateScope();
            db = newsScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updated = db.NewsItems.Single();
            updated.Title.Should().Be("New Title");
            updated.Message.Should().Be("New Message");
        }

        [Fact]
        public async Task Delete_ShouldRemoveNewsItem()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var item = new NewsItem { Title = "ToDelete", Message = "Msg", DatePosted = DateTime.Today, IsActive = true };
            db.NewsItems.Add(item);
            await db.SaveChangesAsync();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var get = await client.GetAsync($"/News/Delete/{item.Id}");
            get.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await get.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Id"] = item.Id.ToString()
            };

            var post = await client.PostAsync($"/News/Delete/{item.Id}", new FormUrlEncodedContent(form));
            post.StatusCode.Should().Be(HttpStatusCode.Redirect);

            using var newsScope = _factory.Services.CreateScope();
            db = newsScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.NewsItems.Should().BeEmpty();
        }

        [Fact]
        public async Task Details_InvalidId_ShouldReturnNotFound()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/News/Edit/9999"); // niet bestaand
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}

using CycleManager.Tests.Integration.Helpers;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace CycleManager.Tests.Integration.Manager
{
    public class ConfigurationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ConfigurationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }

        // =====================================
        //   CONFIGURATIES  (CRUD)
        // =====================================

        [Fact]
        public async Task Index_ShouldShowListOfConfiguraties()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Configurations.Add(new Configuration { ConfigurationType = "Config A" });
            db.Configurations.Add(new Configuration { ConfigurationType = "Config B" });
            await db.SaveChangesAsync();

            var client = _factory.CreateClient();

            var response = await client.GetAsync("/Configurations");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("Config A");
            html.Should().Contain("Config B");
        }

        [Fact]
        public async Task Create_ShouldAddNewConfiguratie()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var get = await client.GetAsync("/Configurations/Create");
            get.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await get.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["ConfigurationName"] = "MyConfig"
            };

            var post = await client.PostAsync("/Configurations/Create", new FormUrlEncodedContent(form));
            post.StatusCode.Should().Be(HttpStatusCode.Redirect);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Configurations.Count().Should().Be(1);
            db.Configurations.Single().ConfigurationType.Should().Be("MyConfig");
        }

        [Fact]
        public async Task Edit_ShouldUpdateConfiguratie()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cfg = new Configuration { ConfigurationType = "Old" };
            db.Configurations.Add(cfg);
            await db.SaveChangesAsync();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var get = await client.GetAsync($"/Configurations/Edit/{cfg.Id}");
            get.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await get.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Id"] = cfg.Id.ToString(),
                ["ConfigurationName"] = "NewName"
            };

            var post = await client.PostAsync($"/Configurations/Edit/{cfg.Id}", new FormUrlEncodedContent(form));
            post.StatusCode.Should().Be(HttpStatusCode.Redirect);

            using (var newscope = _factory.Services.CreateScope())
            {
                db = newscope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var updated = db.Configurations.Single();
                updated.ConfigurationType.Should().Be("NewName");
            }
        }

        [Fact]
        public async Task Delete_ShouldRemoveConfiguratie()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cfg = new Configuration { ConfigurationType = "ToDelete" };
            db.Configurations.Add(cfg);
            await db.SaveChangesAsync();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var get = await client.GetAsync($"/Configurations/Delete/{cfg.Id}");
            get.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await get.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Id"] = cfg.Id.ToString()
            };

            var post = await client.PostAsync($"/Configurations/Delete/{cfg.Id}", new FormUrlEncodedContent(form));
            post.StatusCode.Should().Be(HttpStatusCode.Redirect);

            using (var newscope = _factory.Services.CreateScope())
            {
                db = newscope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Configurations.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task Details_ShouldShowItems()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cfg = new Configuration { ConfigurationType = "CFG" };
            db.Configurations.Add(cfg);

            db.ConfigurationItems.Add(new ConfigurationItem
            {
                Position = 1,
                Score = 50,
                Configuration = cfg
            });

            db.ConfigurationItems.Add(new ConfigurationItem
            {
                Position = 2,
                Score = 30,
                Configuration = cfg
            });

            await db.SaveChangesAsync();

            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/Configurations/Details/{cfg.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("1");
            html.Should().Contain("50");
            html.Should().Contain("2");
            html.Should().Contain("30");
        }

        [Fact]
        public async Task Create_ShouldFail_WhenNameIsEmpty()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var get = await client.GetAsync("/Configurations/Create");
            var html = await get.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["ConfigurationName"] = ""   // invalid
            };

            var post = await client.PostAsync("/Configurations/Create", new FormUrlEncodedContent(form));

            // Model invalid → zelfde view opnieuw → 200 OK
            post.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Configurations.Should().BeEmpty();
        }

        [Fact]
        public async Task Edit_ShouldFail_WhenNameIsEmpty()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cfg = new Configuration { ConfigurationType = "Original" };
            db.Configurations.Add(cfg);
            await db.SaveChangesAsync();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var get = await client.GetAsync($"/Configurations/Edit/{cfg.Id}");
            var html = await get.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Id"] = cfg.Id.ToString(),
                ["ConfigurationName"] = ""    // invalid
            };

            var post = await client.PostAsync($"/Configurations/Edit/{cfg.Id}", new FormUrlEncodedContent(form));

            post.StatusCode.Should().Be(HttpStatusCode.OK);

            using var newscope = _factory.Services.CreateScope();
            db = newscope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Configurations.Single().ConfigurationType.Should().Be("Original");
        }

        [Fact]
        public async Task Details_ShouldReturnNotFound_ForInvalidId()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/Configurations/Details/99999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Edit_ShouldReturnNotFound_ForInvalidId()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/Configurations/Edit/99999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_ForInvalidId()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/Configurations/Delete/99999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


        // =====================================
        //  CONFIGURATIE ITEMS (CRUD)
        // =====================================

        [Fact]
        public async Task CreateItem_ShouldAddItem()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cfg = new Configuration { ConfigurationType = "CFG" };
            db.Configurations.Add(cfg);
            await db.SaveChangesAsync();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var get = await client.GetAsync($"/ConfigurationItems/Create?configId={cfg.Id}");
            get.StatusCode.Should().Be(HttpStatusCode.OK);
            var html = await get.Content.ReadAsStringAsync();

            var token = TokenHelper.ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["ConfigurationId"] = cfg.Id.ToString(),
                ["Position"] = "3",
                ["Score"] = "25"
            };

            var post = await client.PostAsync("/ConfigurationItems/Create", new FormUrlEncodedContent(form));
            post.StatusCode.Should().Be(HttpStatusCode.Redirect);

            using (var newscope = _factory.Services.CreateScope())
            {
                db = newscope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.ConfigurationItems.Count().Should().Be(1);

                var item = db.ConfigurationItems.Single();
                item.Position.Should().Be(3);
                item.Score.Should().Be(25);
            }
        }

        [Fact]
        public async Task EditItem_ShouldUpdateItem()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cfg = new Configuration { ConfigurationType = "CFG" };
            var item = new ConfigurationItem { Configuration = cfg, Position = 1, Score = 10 };

            db.Configurations.Add(cfg);
            db.ConfigurationItems.Add(item);
            await db.SaveChangesAsync();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var get = await client.GetAsync($"/ConfigurationItems/Edit/{item.Id}");
            get.StatusCode.Should().Be(HttpStatusCode.OK);
            var html = await get.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Id"] = item.Id.ToString(),
                ["ConfigurationId"] = cfg.Id.ToString(),
                ["Position"] = "2",
                ["Score"] = "40"
            };

            var post = await client.PostAsync($"/ConfigurationItems/Edit/{item.Id}", new FormUrlEncodedContent(form));
            post.StatusCode.Should().Be(HttpStatusCode.Redirect);

            using (var newscope = _factory.Services.CreateScope())
            {
                db = newscope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var updated = db.ConfigurationItems.Single();
                updated.Position.Should().Be(2);
                updated.Score.Should().Be(40);
            }
        }

        [Fact]
        public async Task DeleteItem_ShouldRemoveItem()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cfg = new Configuration { ConfigurationType = "CFG" };
            var item = new ConfigurationItem { Configuration = cfg, Position = 1, Score = 20 };

            db.Configurations.Add(cfg);
            db.ConfigurationItems.Add(item);
            await db.SaveChangesAsync();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var get = await client.GetAsync($"/ConfigurationItems/Delete/{item.Id}");
            get.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await get.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Id"] = item.Id.ToString()
            };

            var post = await client.PostAsync($"/ConfigurationItems/Delete/{item.Id}", new FormUrlEncodedContent(form));
            post.StatusCode.Should().Be(HttpStatusCode.Redirect);

            using (var newscope = _factory.Services.CreateScope())
            {
                db = newscope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.ConfigurationItems.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task CreateItem_ShouldFail_WhenPositionAlreadyExists()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cfg = new Configuration { ConfigurationType = "CFG" };
            db.Configurations.Add(cfg);

            db.ConfigurationItems.Add(new ConfigurationItem { Configuration = cfg, Position = 1, Score = 10 });
            await db.SaveChangesAsync();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var get = await client.GetAsync($"/ConfigurationItems/Create?configId={cfg.Id}");
            var html = await get.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["ConfigurationId"] = cfg.Id.ToString(),
                ["Position"] = "1",   // duplicate!
                ["Score"] = "50"
            };

            var post = await client.PostAsync("/ConfigurationItems/Create", new FormUrlEncodedContent(form));

            // Duplicate → ModelState error → zelfde view
            post.StatusCode.Should().Be(HttpStatusCode.OK);

            using var newscope = _factory.Services.CreateScope();
            db = newscope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.ConfigurationItems.Count().Should().Be(1);
            db.ConfigurationItems.Single().Score.Should().Be(10);
        }

        [Fact]
        public async Task EditItem_ShouldFail_WhenPositionAlreadyExists()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cfg = new Configuration { ConfigurationType = "CFG" };
            var item1 = new ConfigurationItem { Configuration = cfg, Position = 1, Score = 10 };
            var item2 = new ConfigurationItem { Configuration = cfg, Position = 2, Score = 20 };

            db.Configurations.Add(cfg);
            db.ConfigurationItems.AddRange(item1, item2);
            await db.SaveChangesAsync();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var get = await client.GetAsync($"/ConfigurationItems/Edit/{item2.Id}");
            var html = await get.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Id"] = item2.Id.ToString(),
                ["ConfigurationId"] = cfg.Id.ToString(),
                ["Position"] = "1",   // already used by item1
                ["Score"] = "999"
            };

            var post = await client.PostAsync($"/ConfigurationItems/Edit/{item2.Id}", new FormUrlEncodedContent(form));

            post.StatusCode.Should().Be(HttpStatusCode.OK);

            using var newscope = _factory.Services.CreateScope();
            db = newscope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var updated2 = db.ConfigurationItems.Single(i => i.Id == item2.Id);
            updated2.Position.Should().Be(2);  // unchanged
            updated2.Score.Should().Be(20);
        }

        [Fact]
        public async Task EditItem_ShouldReturnNotFound_ForInvalidId()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/ConfigurationItems/Edit/99999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

    }
}
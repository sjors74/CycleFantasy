using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using WebCycleManager.Controllers;
using WebCycleManager.Models;

namespace CycleManager.Tests.Unit.Manager
{
    public class RatingControllerTests
    {
        private readonly Mock<IScraperService> _mockScraperService;
        private readonly Mock<IRatingService> _mockRatingService;
        private readonly RatingController _controller;

        public RatingControllerTests()
        {
            _mockScraperService = new Mock<IScraperService>();
            _mockRatingService = new Mock<IRatingService>();

            _controller = new RatingController(
                _mockScraperService.Object,
                _mockRatingService.Object);
        }

        // -------------------------------------------
        // INDEX
        // -------------------------------------------
        [Fact]
        public async Task Index_ReturnsView_WithRatings()
        {
            var ratings = new List<CompetitorRating>
            {
                new()
                {
                    CompetitorId = 1,
                    RatingCategoryId = 1,
                    Rating = 8,
                    Competitor = new Competitor
                    {
                        FirstName = "Wout",
                        LastName = "Test"
                    },
                    RatingCategory = new RatingCategory
                    {
                        RatingCategoryId = 1,
                        Name = "GC"
                    }
                }
            };

            var categories = new List<RatingCategory>
            {
                new()
                {
                    RatingCategoryId = 1,
                    Name = "GC"
                }
            };

            _mockRatingService
                .Setup(s => s.GetRatings())
                .ReturnsAsync(ratings);

            _mockRatingService
                .Setup(s => s.GetRatingCategories())
                .ReturnsAsync(categories);

            var result = await _controller.Index(null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RatingsIndexViewModel>(viewResult.Model);

            Assert.Single(model.Ratings);
            Assert.Equal(1, model.Ratings.First().CompetitorId);
            Assert.Equal("Wout", model.Ratings.First().FirstName);
            Assert.Equal("Test", model.Ratings.First().LastName);
            Assert.Equal("GC", model.Ratings.First().Category);
            Assert.Equal(8, model.Ratings.First().Rating);
            Assert.Single(model.Categories);
            Assert.Equal(1, model.Page);
            Assert.Equal(50, model.PageSize);
            Assert.Equal(1, model.TotalCount);
        }

        [Fact]
        public async Task Index_FiltersBySearch()
        {
            var ratings = new List<CompetitorRating>
            {
                new()
                {
                    CompetitorId = 1,
                    Rating = 8,
                    Competitor = new Competitor
                    {
                        FirstName = "Wout",
                        LastName = "Test"
                    },
                    RatingCategory = new RatingCategory
                    {
                        Name = "GC"
                    }
                },
                new()
                {
                    CompetitorId = 2,
                    Rating = 7,
                    Competitor = new Competitor
                    {
                        FirstName = "Mathieu",
                        LastName = "Other"
                    },
                    RatingCategory = new RatingCategory
                    {
                        Name = "GC"
                    }
                }
            };

            _mockRatingService
                .Setup(s => s.GetRatings())
                .ReturnsAsync(ratings);

            _mockRatingService
                .Setup(s => s.GetRatingCategories())
                .ReturnsAsync(new List<RatingCategory>());

            var result = await _controller.Index("Wout", null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RatingsIndexViewModel>(viewResult.Model);

            Assert.Single(model.Ratings);
            Assert.Equal(1, model.Ratings.First().CompetitorId);
            Assert.Equal("Wout", model.Search);
            Assert.Equal(1, model.TotalCount);
        }

        [Fact]
        public async Task Index_FiltersByRatingCategory()
        {
            var ratings = new List<CompetitorRating>
            {
                new()
                {
                    CompetitorId = 1,
                    RatingCategoryId = 1,
                    Rating = 8,
                    Competitor = new Competitor
                    {
                        FirstName = "Wout",
                        LastName = "Test"
                    },
                    RatingCategory = new RatingCategory
                    {
                        Name = "GC"
                    }
                },
                new()
                {
                    CompetitorId = 2,
                    RatingCategoryId = 2,
                    Rating = 7,
                    Competitor = new Competitor
                    {
                        FirstName = "Mathieu",
                        LastName = "Other"
                    },
                    RatingCategory = new RatingCategory
                    {
                        Name = "KOM"
                    }
                }
            };

            _mockRatingService
                .Setup(s => s.GetRatings())
                .ReturnsAsync(ratings);

            _mockRatingService
                .Setup(s => s.GetRatingCategories())
                .ReturnsAsync(new List<RatingCategory>());

            var result = await _controller.Index(null, 2);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RatingsIndexViewModel>(viewResult.Model);

            Assert.Single(model.Ratings);
            Assert.Equal(2, model.Ratings.First().CompetitorId);
            Assert.Equal(2, model.RatingCategoryId);
            Assert.Equal(1, model.TotalCount);
        }

        [Fact]
        public async Task Index_SortsByRatingDescending_ThenLastName()
        {
            var ratings = new List<CompetitorRating>
            {
                new()
                {
                    CompetitorId = 1,
                    Rating = 7,
                    Competitor = new Competitor
                    {
                        FirstName = "A",
                        LastName = "Z"
                    },
                    RatingCategory = new RatingCategory
                    {
                        Name = "GC"
                    }
                },
                new()
                {
                    CompetitorId = 2,
                    Rating = 9,
                    Competitor = new Competitor
                    {
                        FirstName = "B",
                        LastName = "B"
                    },
                    RatingCategory = new RatingCategory
                    {
                        Name = "GC"
                    }
                },
                new()
                {
                    CompetitorId = 3,
                    Rating = 9,
                    Competitor = new Competitor
                    {
                        FirstName = "C",
                        LastName = "A"
                    },
                    RatingCategory = new RatingCategory
                    {
                        Name = "GC"
                    }
                }
            };

            _mockRatingService
                .Setup(s => s.GetRatings())
                .ReturnsAsync(ratings);

            _mockRatingService
                .Setup(s => s.GetRatingCategories())
                .ReturnsAsync(new List<RatingCategory>());

            var result = await _controller.Index(null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RatingsIndexViewModel>(viewResult.Model);

            Assert.Equal(3, model.Ratings.Count);
            Assert.Equal(3, model.Ratings[0].CompetitorId);
            Assert.Equal(2, model.Ratings[1].CompetitorId);
            Assert.Equal(1, model.Ratings[2].CompetitorId);
        }

        [Fact]
        public async Task Index_SetsPageAndTotalCount()
        {
            var ratings = Enumerable.Range(1, 55)
                .Select(i => new CompetitorRating
                {
                    CompetitorId = i,
                    Rating = i,
                    Competitor = new Competitor
                    {
                        FirstName = $"First{i}",
                        LastName = $"Last{i}"
                    },
                    RatingCategory = new RatingCategory
                    {
                        Name = "GC"
                    }
                })
                .ToList();

            _mockRatingService
                .Setup(s => s.GetRatings())
                .ReturnsAsync(ratings);

            _mockRatingService
                .Setup(s => s.GetRatingCategories())
                .ReturnsAsync(new List<RatingCategory>());

            var result = await _controller.Index(page: 2, search: null, ratingCategoryId: null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RatingsIndexViewModel>(viewResult.Model);

            Assert.Equal(2, model.Page);
            Assert.Equal(50, model.PageSize);
            Assert.Equal(55, model.TotalCount);
            Assert.Equal(5, model.Ratings.Count);
        }

        // -------------------------------------------
        // RUN RATING SCRAPE
        // -------------------------------------------
        [Fact]
        public async Task RunRatingScrape_RunsScrapeAndRedirects()
        {
            // Arrange
            _mockScraperService
                .Setup(s => s.RunRatingsScrapeAsync())
                .Returns(Task.CompletedTask);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            // Act
            var result = await _controller.RunRatingScrape();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);

            _mockScraperService.Verify(
                s => s.RunRatingsScrapeAsync(),
                Times.Once);

            Assert.Equal(
                "Rating scrape uitgevoerd.",
                _controller.TempData["Success"]);
        }
    }
}
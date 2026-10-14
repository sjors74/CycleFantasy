using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresqlMigrationForReal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConfigurationType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CountryNameLong = table.Column<string>(type: "text", nullable: false),
                    CountryNameShort = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.CountryId);
                });

            migrationBuilder.CreateTable(
                name: "DeelnemerStageScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameCompetitorEventId = table.Column<int>(type: "integer", nullable: false),
                    StageId = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeelnemerStageScores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    DatePosted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RatingCategories",
                columns: table => new
                {
                    RatingCategoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MaxPages = table.Column<int>(type: "integer", nullable: false),
                    RefreshOrder = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingCategories", x => x.RatingCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "ScrapeCompetitorRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Source = table.Column<string>(type: "text", nullable: false),
                    RatingCategoryId = table.Column<int>(type: "integer", nullable: false),
                    RatingCategoryCode = table.Column<string>(type: "text", nullable: false),
                    CompetitorName = table.Column<string>(type: "text", nullable: false),
                    ProfileUrl = table.Column<string>(type: "text", nullable: true),
                    Rating = table.Column<decimal>(type: "numeric", nullable: false),
                    RatingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Processed = table.Column<bool>(type: "boolean", nullable: false),
                    CompetitorId = table.Column<int>(type: "integer", nullable: true),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapeCompetitorRatings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScrapedCompetitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RiderName = table.Column<string>(type: "text", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    CountryShortName = table.Column<string>(type: "text", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapedCompetitors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeasonYears",
                columns: table => new
                {
                    SeasonYearId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonYears", x => x.SeasonYearId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    ConfigurationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigurationItems_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationItemSpecials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Question = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    ConfigurationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationItemSpecials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigurationItemSpecials_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventName = table.Column<string>(type: "text", nullable: false),
                    EventYear = table.Column<int>(type: "integer", nullable: false),
                    EventCode = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Slogan = table.Column<string>(type: "text", nullable: true),
                    CountryCode = table.Column<string>(type: "text", nullable: true),
                    ColorName = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CanSubscribe = table.Column<bool>(type: "boolean", nullable: false),
                    ShowPodium = table.Column<bool>(type: "boolean", nullable: false),
                    ConfigurationId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_Events_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Competitors",
                columns: table => new
                {
                    CompetitorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    PcsName = table.Column<string>(type: "text", nullable: false),
                    PcsScraperName = table.Column<string>(type: "text", nullable: true),
                    CyclingFlashScraperName = table.Column<string>(type: "text", nullable: true),
                    CyclingFlashLastScraped = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CountryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competitors", x => x.CompetitorId);
                    table.ForeignKey(
                        name: "FK_Competitors_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrentTeamName = table.Column<string>(type: "text", nullable: false),
                    CountryId = table.Column<int>(type: "integer", nullable: true),
                    PcsName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.TeamId);
                    table.ForeignKey(
                        name: "FK_Teams_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId");
                });

            migrationBuilder.CreateTable(
                name: "RatingScrapeProgress",
                columns: table => new
                {
                    RatingScrapeProgressId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RatingCategoryId = table.Column<int>(type: "integer", nullable: false),
                    LastPage = table.Column<int>(type: "integer", nullable: false),
                    LastScrapeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorCount = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingScrapeProgress", x => x.RatingScrapeProgressId);
                    table.ForeignKey(
                        name: "FK_RatingScrapeProgress_RatingCategories_RatingCategoryId",
                        column: x => x.RatingCategoryId,
                        principalTable: "RatingCategories",
                        principalColumn: "RatingCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameCompetitorsEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TeamName = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    EventId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCompetitorsEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameCompetitorsEvent_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameCompetitorsEvent_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StageDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StageName = table.Column<string>(type: "text", nullable: false),
                    StageOrder = table.Column<int>(type: "integer", nullable: false),
                    StartLocation = table.Column<string>(type: "text", nullable: false),
                    FinishLocation = table.Column<string>(type: "text", nullable: false),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    NoScore = table.Column<bool>(type: "boolean", nullable: false),
                    NoScoreDescription = table.Column<string>(type: "text", nullable: true),
                    ScrapeStatus = table.Column<int>(type: "integer", nullable: false),
                    LastScrapeAttempt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSuccessfulScrape = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stages_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompetitorRatings",
                columns: table => new
                {
                    CompetitorRatingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompetitorId = table.Column<int>(type: "integer", nullable: false),
                    RatingCategoryId = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<decimal>(type: "numeric", nullable: false),
                    RatingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitorRatings", x => x.CompetitorRatingId);
                    table.ForeignKey(
                        name: "FK_CompetitorRatings_Competitors_CompetitorId",
                        column: x => x.CompetitorId,
                        principalTable: "Competitors",
                        principalColumn: "CompetitorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitorRatings_RatingCategories_RatingCategoryId",
                        column: x => x.RatingCategoryId,
                        principalTable: "RatingCategories",
                        principalColumn: "RatingCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventTeam",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTeam", x => new { x.EventId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_EventTeam_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTeam_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamYear",
                columns: table => new
                {
                    TeamYearId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    SeasonYearId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamYear", x => x.TeamYearId);
                    table.ForeignKey(
                        name: "FK_TeamYear_SeasonYears_SeasonYearId",
                        column: x => x.SeasonYearId,
                        principalTable: "SeasonYears",
                        principalColumn: "SeasonYearId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamYear_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeelnemerScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameCompetitorEventId = table.Column<int>(type: "integer", nullable: false),
                    TotalScore = table.Column<int>(type: "integer", nullable: false),
                    LaatsteStageScore = table.Column<int>(type: "integer", nullable: false),
                    LaatsteStageId = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeelnemerScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeelnemerScores_GameCompetitorsEvent_GameCompetitorEventId",
                        column: x => x.GameCompetitorEventId,
                        principalTable: "GameCompetitorsEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompetitorInTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompetitorId = table.Column<int>(type: "integer", nullable: false),
                    TeamYearId = table.Column<int>(type: "integer", nullable: false),
                    IsNationalChampion = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitorInTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitorInTeams_Competitors_CompetitorId",
                        column: x => x.CompetitorId,
                        principalTable: "Competitors",
                        principalColumn: "CompetitorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitorInTeams_TeamYear_TeamYearId",
                        column: x => x.TeamYearId,
                        principalTable: "TeamYear",
                        principalColumn: "TeamYearId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompetitorsInEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompetitorInTeamId = table.Column<int>(type: "integer", nullable: false),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    EventNumber = table.Column<int>(type: "integer", nullable: false),
                    OutOfCompetition = table.Column<bool>(type: "boolean", nullable: false),
                    InSelectie = table.Column<bool>(type: "boolean", nullable: false),
                    RemovedFromStartList = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitorsInEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitorsInEvent_CompetitorInTeams_CompetitorInTeamId",
                        column: x => x.CompetitorInTeamId,
                        principalTable: "CompetitorInTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitorsInEvent_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameCompetitorEventPicks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameCompetitorEventId = table.Column<int>(type: "integer", nullable: false),
                    CompetitorsInEventId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCompetitorEventPicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameCompetitorEventPicks_CompetitorsInEvent_CompetitorsInEv~",
                        column: x => x.CompetitorsInEventId,
                        principalTable: "CompetitorsInEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameCompetitorEventPicks_GameCompetitorsEvent_GameCompetito~",
                        column: x => x.GameCompetitorEventId,
                        principalTable: "GameCompetitorsEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StageId = table.Column<int>(type: "integer", nullable: false),
                    CompetitorInEventId = table.Column<int>(type: "integer", nullable: false),
                    ConfigurationItemId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Results_CompetitorsInEvent_CompetitorInEventId",
                        column: x => x.CompetitorInEventId,
                        principalTable: "CompetitorsInEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Results_ConfigurationItems_ConfigurationItemId",
                        column: x => x.ConfigurationItemId,
                        principalTable: "ConfigurationItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Results_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "Stages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScrapedSpecialResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StageId = table.Column<int>(type: "integer", nullable: false),
                    QuestionType = table.Column<int>(type: "integer", nullable: false),
                    BibNumber = table.Column<int>(type: "integer", nullable: false),
                    CompetitorInEventId = table.Column<int>(type: "integer", nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapedSpecialResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScrapedSpecialResults_CompetitorsInEvent_CompetitorInEventId",
                        column: x => x.CompetitorInEventId,
                        principalTable: "CompetitorsInEvent",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScrapedStageResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    StageId = table.Column<int>(type: "integer", nullable: false),
                    BibNumber = table.Column<int>(type: "integer", nullable: false),
                    RiderName = table.Column<string>(type: "text", nullable: false),
                    TeamName = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MatchedCompetitorInEventId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapedStageResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScrapedStageResults_CompetitorsInEvent_MatchedCompetitorInE~",
                        column: x => x.MatchedCompetitorInEventId,
                        principalTable: "CompetitorsInEvent",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SpecialResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StageId = table.Column<int>(type: "integer", nullable: false),
                    CompetitorInEventId = table.Column<int>(type: "integer", nullable: false),
                    SpecialId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialResults_CompetitorsInEvent_CompetitorInEventId",
                        column: x => x.CompetitorInEventId,
                        principalTable: "CompetitorsInEvent",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SpecialResults_ConfigurationItemSpecials_SpecialId",
                        column: x => x.SpecialId,
                        principalTable: "ConfigurationItemSpecials",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SpecialResults_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "Stages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeelnemerPickScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameCompetitorEventPickId = table.Column<int>(type: "integer", nullable: false),
                    TotalScore = table.Column<int>(type: "integer", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeelnemerPickScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeelnemerPickScores_GameCompetitorEventPicks_GameCompetitor~",
                        column: x => x.GameCompetitorEventPickId,
                        principalTable: "GameCompetitorEventPicks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeelnemerStagePickScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameCompetitorEventPickId = table.Column<int>(type: "integer", nullable: false),
                    StageId = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeelnemerStagePickScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeelnemerStagePickScores_GameCompetitorEventPicks_GameCompe~",
                        column: x => x.GameCompetitorEventPickId,
                        principalTable: "GameCompetitorEventPicks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeelnemerStagePickScores_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "Stages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeelnemerStagePickSpecialScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameCompetitorEventPickId = table.Column<int>(type: "integer", nullable: false),
                    StageId = table.Column<int>(type: "integer", nullable: false),
                    QuestionType = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeelnemerStagePickSpecialScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeelnemerStagePickSpecialScores_GameCompetitorEventPicks_Ga~",
                        column: x => x.GameCompetitorEventPickId,
                        principalTable: "GameCompetitorEventPicks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorInTeams_CompetitorId",
                table: "CompetitorInTeams",
                column: "CompetitorId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorInTeams_TeamYearId",
                table: "CompetitorInTeams",
                column: "TeamYearId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorRatings_CompetitorId_RatingCategoryId_RatingDate",
                table: "CompetitorRatings",
                columns: new[] { "CompetitorId", "RatingCategoryId", "RatingDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorRatings_RatingCategoryId",
                table: "CompetitorRatings",
                column: "RatingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Competitors_CountryId",
                table: "Competitors",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorsInEvent_CompetitorInTeamId",
                table: "CompetitorsInEvent",
                column: "CompetitorInTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorsInEvent_EventId",
                table: "CompetitorsInEvent",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationItems_ConfigurationId",
                table: "ConfigurationItems",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationItemSpecials_ConfigurationId",
                table: "ConfigurationItemSpecials",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerPickScores_GameCompetitorEventPickId",
                table: "DeelnemerPickScores",
                column: "GameCompetitorEventPickId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerScores_GameCompetitorEventId",
                table: "DeelnemerScores",
                column: "GameCompetitorEventId");

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerStagePickScores_GameCompetitorEventPickId_StageId",
                table: "DeelnemerStagePickScores",
                columns: new[] { "GameCompetitorEventPickId", "StageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerStagePickScores_StageId",
                table: "DeelnemerStagePickScores",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerStagePickSpecialScores_GameCompetitorEventPickId",
                table: "DeelnemerStagePickSpecialScores",
                column: "GameCompetitorEventPickId");

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerStageScores_GameCompetitorEventId_StageId",
                table: "DeelnemerStageScores",
                columns: new[] { "GameCompetitorEventId", "StageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_ConfigurationId",
                table: "Events",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTeam_TeamId",
                table: "EventTeam",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorEventPicks_CompetitorsInEventId",
                table: "GameCompetitorEventPicks",
                column: "CompetitorsInEventId");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorEventPicks_GameCompetitorEventId",
                table: "GameCompetitorEventPicks",
                column: "GameCompetitorEventId");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorsEvent_EventId_UserId_TeamName",
                table: "GameCompetitorsEvent",
                columns: new[] { "EventId", "UserId", "TeamName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorsEvent_UserId",
                table: "GameCompetitorsEvent",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RatingScrapeProgress_RatingCategoryId",
                table: "RatingScrapeProgress",
                column: "RatingCategoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Results_CompetitorInEventId",
                table: "Results",
                column: "CompetitorInEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_ConfigurationItemId",
                table: "Results",
                column: "ConfigurationItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_StageId",
                table: "Results",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapedSpecialResults_CompetitorInEventId",
                table: "ScrapedSpecialResults",
                column: "CompetitorInEventId");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapedStageResults_MatchedCompetitorInEventId",
                table: "ScrapedStageResults",
                column: "MatchedCompetitorInEventId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialResults_CompetitorInEventId",
                table: "SpecialResults",
                column: "CompetitorInEventId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialResults_SpecialId",
                table: "SpecialResults",
                column: "SpecialId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialResults_StageId",
                table: "SpecialResults",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_Stages_EventId",
                table: "Stages",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_CountryId",
                table: "Teams",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamYear_SeasonYearId",
                table: "TeamYear",
                column: "SeasonYearId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamYear_TeamId",
                table: "TeamYear",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CompetitorRatings");

            migrationBuilder.DropTable(
                name: "DeelnemerPickScores");

            migrationBuilder.DropTable(
                name: "DeelnemerScores");

            migrationBuilder.DropTable(
                name: "DeelnemerStagePickScores");

            migrationBuilder.DropTable(
                name: "DeelnemerStagePickSpecialScores");

            migrationBuilder.DropTable(
                name: "DeelnemerStageScores");

            migrationBuilder.DropTable(
                name: "EventTeam");

            migrationBuilder.DropTable(
                name: "NewsItems");

            migrationBuilder.DropTable(
                name: "RatingScrapeProgress");

            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "ScrapeCompetitorRatings");

            migrationBuilder.DropTable(
                name: "ScrapedCompetitors");

            migrationBuilder.DropTable(
                name: "ScrapedSpecialResults");

            migrationBuilder.DropTable(
                name: "ScrapedStageResults");

            migrationBuilder.DropTable(
                name: "SpecialResults");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "GameCompetitorEventPicks");

            migrationBuilder.DropTable(
                name: "RatingCategories");

            migrationBuilder.DropTable(
                name: "ConfigurationItems");

            migrationBuilder.DropTable(
                name: "ConfigurationItemSpecials");

            migrationBuilder.DropTable(
                name: "Stages");

            migrationBuilder.DropTable(
                name: "CompetitorsInEvent");

            migrationBuilder.DropTable(
                name: "GameCompetitorsEvent");

            migrationBuilder.DropTable(
                name: "CompetitorInTeams");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Competitors");

            migrationBuilder.DropTable(
                name: "TeamYear");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "SeasonYears");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}

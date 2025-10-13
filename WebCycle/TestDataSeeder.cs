using Domain.Context;
using Domain.Models;

namespace WebCycle.Services
{
    public static class TestDataSeeder
    {
        public static void Seed(ApplicationDbContext db)
        {
            if (db.Events.Any()) return;

            db.Events.AddRange(
                new Event
                {
                    EventName = "Tour de Test",
                    StartDate = DateTime.Now.AddDays(1),
                    EndDate = DateTime.Now.AddDays(15),
                    IsActive = true,
                    Stages = new List<Stage>
                    {
                        new Stage
                        {
                            StageDate = DateTime.UtcNow.AddDays(1),
                            StageName = "Proloog",
                            StageOrder = 1,
                            StartLocation = "Startville",
                            FinishLocation = "Finishburg"
                        }
                    }
                }
                ,
                new Event
                {
                    EventName = "Cycle Classic",
                    StartDate = DateTime.Now.AddDays(3),
                    EndDate = DateTime.Now.AddDays(10),
                    IsActive = true,
                    Stages = new List<Stage>
                    {
                        new Stage
                        {
                            StageDate = DateTime.UtcNow.AddDays(1),
                            StageName = "Proloog",
                            StageOrder = 1,
                            StartLocation = "Startville",
                            FinishLocation = "Finishburg"
                        },
                        new Stage
                        {
                            StageDate = DateTime.UtcNow.AddDays(1),
                            StageName = "1",
                            StageOrder = 1,
                            StartLocation = "Finishburg",
                            FinishLocation = "SomewhereElst"
                        }
                    }
                }
            );

            db.SaveChanges();
            Console.WriteLine("[API] Seeded test events for E2E");
        }
    }
}

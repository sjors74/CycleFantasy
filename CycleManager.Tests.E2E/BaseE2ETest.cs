using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CycleManager.Tests.E2E;

[TestClass]
public abstract class BaseE2ETest
{
    protected static AppFixture Fixture = null;
    protected IBrowser Browser => Fixture.Browser;
    protected IPage Page = null!;
    protected IBrowserContext Context = null;

    public string WebBaseUrl => Fixture.WebBaseUrl;

    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static async Task ClassInit(TestContext _)
    {
        Fixture = new AppFixture();
        await Fixture.InitializeAsync();
    }

    [TestInitialize]
    public async Task TestInit()
    {
        await Fixture.EnsureRunningAsync();
        Context = await Browser.NewContextAsync(new() { IgnoreHTTPSErrors = true });
        Page = await Context.NewPageAsync();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        if (Page != null)
            await Page.CloseAsync();

        if (Context != null)
            await Context.CloseAsync();
    }

    [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
    public static async Task ClassCleanup()
    {
        await Fixture.DisposeAsync();
    }

    protected async Task WaitForAppReadyAsync(int timeout = 30000)
    {
        var spinner = Page.Locator(".loading-spinner.active");
        try
        {
            await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = timeout });
        }
        catch
        {
            Console.WriteLine("[WaitForAppReady] Geen spinner of bleef actief — doorgaan.");
        }
    }

    protected async Task LoginAsAsync(string username)
    {
        await Page.GotoAsync($"{WebBaseUrl}/?fakeuser={username}");
        await WaitForAppReadyAsync();
    }
}
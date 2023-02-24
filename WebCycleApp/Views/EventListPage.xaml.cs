using Microsoft.Maui.Controls;
using WebCycleApp.Services;
using WebCycleApp.ViewModels;

namespace WebCycleApp.Views;

public partial class EventListPage : ContentPage
{
    IRestService _restService;

    public EventListPage(IRestService restService)
    {
        InitializeComponent();
        _restService = restService;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        //var t = await _restService.GetEventByEventId(2); //TODO: eventId is hardcoded
        //var evm = new EventsViewModel();
        //var e = new Models.Event(t.EventId, t.EventName, t.EventYear, t.StartDate, t.EndDate);
        //evm.EventsCollection.Add(e);
        //BindingContext = evm;

        var t = await _restService.GetRandomCompetitorListByEventId(7, 15); //TODO: eventId is hardcoded
        var cvm = new CompetitorsViewModel();
        foreach (var item in t)
        {
            cvm.CompetitorsCollection.Add(new Models.Competitor { CompetitorId = item.CompetitorId, CompetitorName = item.CompetitorName, CountryId = item.CountryId, TeamId = item.TeamId });
        }
        BindingContext = cvm;
    }
}
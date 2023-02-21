using Microsoft.Maui.Controls;
using WebCycleApp.Services;

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
		this.
        collectionView.ItemsSource = await _restService.GetActiveEvents();
    }
}
namespace CycleManager.Services.Interfaces
{
    public interface IConfigurationService
    {
        Task<IEnumerable<Domain.Models.Configuration>> GetAllConfigurations();
        
    }
}

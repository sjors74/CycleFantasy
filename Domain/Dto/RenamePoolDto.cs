namespace CycleManager.Domain.Dto
{
    public class RenamePoolDto
    {
        public int DeelnemerId { get; set; }
        public string NieuweNaam { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;  
    }
}

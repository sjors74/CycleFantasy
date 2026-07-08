namespace CycleManager.Domain.Dto
{
    public class EtappeResultaatDto
    {
        public List<EtappeUitslagDto> Uitslag { get; set; } = [];
        public List<EtappeSpecialDto> Specials { get; set; } = [];
    }
}

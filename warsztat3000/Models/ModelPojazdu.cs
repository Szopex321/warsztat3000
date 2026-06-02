using System.ComponentModel.DataAnnotations.Schema;

namespace warsztat3000.Models
{
    public class ModelPojazdu
    {
        public int Id { get; set; }
        public int MarkaPojazduId { get; set; }

        [ForeignKey("MarkaPojazduId")]
        public MarkaPojazdu MarkaPojazdu { get; set; }

        public string Nazwa { get; set; } = string.Empty;

        public override string ToString() => Nazwa;
    }
}

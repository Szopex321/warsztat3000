using System.Collections.Generic;

namespace warsztat3000.Models
{
    public class MarkaPojazdu
    {
        public int Id { get; set; }
        public string Nazwa { get; set; } = string.Empty;
        public List<ModelPojazdu> Modele { get; set; } = new();

        public override string ToString() => Nazwa;
    }
}

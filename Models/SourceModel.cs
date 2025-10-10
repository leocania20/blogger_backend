namespace blogger_backend.Models
{
    public class SourceModel
    {
         public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string URL { get; set; } = null!;
        public string Type { get; set; } = null!;
        public bool Active { get; set; } = true;

        public ICollection<ArticlesModel> Articles { get; set; } = new List<ArticlesModel>();

    }
}
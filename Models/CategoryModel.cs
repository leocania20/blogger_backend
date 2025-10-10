namespace blogger_backend.Models
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Tag { get; set; } = null!;
        public bool Active { get; set; } = true;

        public ICollection<ArticlesModel> Articles { get; set; } = new List<ArticlesModel>();
 
    }
}
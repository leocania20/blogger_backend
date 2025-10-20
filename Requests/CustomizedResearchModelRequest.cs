namespace blogger_backend.Models
{
    public record CustomizedResearchRequest(
        int UserId,       
        int? CategoryId,     
        int? AuthorId,         
        int? SourceId          
    );
    public class CustomizedResearchBulkRequest
    {
        public List<int>? Categories { get; set; }
        public List<int>? Authors { get; set; }
        public List<int>? Sources { get; set; }
    }

}

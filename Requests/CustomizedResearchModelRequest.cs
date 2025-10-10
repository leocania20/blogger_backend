namespace blogger_backend.Models
{
    public record CustomizedResearchRequest(
        int UserId,       
        int? CategoryId,     
        int? AuthorId,         
        int? SourceId          
    );
}

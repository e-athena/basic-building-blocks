namespace Athena.Infrastructure.Lucene.Dto;

public class SearchDocDto
{
    public int DocId { get; set; }


    public Document Document { get; set; }


    public float Score { get; set; }
}
namespace iRLeagueApiCore.Common.Models;

[DataContract]
public sealed class PaginatedResultModel<T>
{
    [DataMember]
    public T Data { get; set; }
    [DataMember]
    public int Page { get; set; }
    [DataMember] 
    public int PageSize { get; set; }
    [DataMember] 
    public int PageCount { get; set; }

    public  PaginatedResultModel(T data, int page, int pageSize, int pageCount)
    {
        Data = data;
        Page = page;
        PageSize = pageSize;
        PageCount = pageCount;
    }
}

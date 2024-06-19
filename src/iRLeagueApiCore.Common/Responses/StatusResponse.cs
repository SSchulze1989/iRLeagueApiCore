namespace iRLeagueApiCore.Common.Responses;
public struct StatusResponse
{
    public string Status { get; set; }
    public readonly bool Success => Status is "success" or "Success";
    public string Message { get; set; }
}

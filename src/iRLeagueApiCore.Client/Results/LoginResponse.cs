namespace iRLeagueApiCore.Client.Results
{
    public struct LoginResponse
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}

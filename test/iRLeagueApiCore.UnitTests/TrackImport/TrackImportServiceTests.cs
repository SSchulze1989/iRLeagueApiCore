using iRLeagueApiCore.TrackImport.Service;
using Microsoft.Extensions.Configuration;

namespace iRLeagueApiCore.UnitTests.TrackImport;

public sealed class TrackImportServiceTests
{
    private const string testUserName = "CLunky@iracing.Com";
    private const string testPassword = "MyPassWord";
    private const string testPasswordEncoded = "xGKecAR27ALXNuMLsGaG0v5Q9pSs2tZTZRKNgmHMg+Q=";

    [Fact]
    public void EncodePassword_ShouldEncodeTestHash_WhenUsingTestPassword()
    {
        string userName = testUserName;
        string password = testPassword;
        string expected = testPasswordEncoded;

        var encoded = TrackImportService.EncodePassword(userName, password);

        encoded.Should().Be(expected);
    }
}

using Aydsko.iRacingData;
using iRLeagueApiCore.TrackImport.Models;
using System.Security.Cryptography;
using System.Text;

namespace iRLeagueApiCore.TrackImport.Service;

public sealed class TrackImportService
{
    public const double m2km = 1.60934;

    private readonly HttpClient httpClient;
    private readonly IDataClient dataClient;

    public TrackImportService(IDataClient dataClient, HttpClient httpClient)
    {
        this.dataClient = dataClient;
        this.httpClient = httpClient;
    }

    public async Task<IEnumerable<Aydsko.iRacingData.Tracks.Track>> GetTracksData(CancellationToken cancellationToken = default)
    {
        return (await dataClient.GetTracksAsync(cancellationToken)).Data;
    }

    public async Task<IReadOnlyDictionary<string, Aydsko.iRacingData.Tracks.TrackAssets>> GetTrackAssets(CancellationToken cancellationToken = default)
    {
        return (await dataClient.GetTrackAssetsAsync(cancellationToken)).Data;
    }

    public async Task<TrackMapLayersModel> LoadTrackSvgs(TrackAssetsModel trackAssets, CancellationToken cancellationToken = default)
    {
        var svgLayers = new TrackMapLayersModel
        {
            active = await GetSvg(trackAssets.track_map + trackAssets.track_map_layers.active, cancellationToken),
            background = await GetSvg(trackAssets.track_map + trackAssets.track_map_layers.background, cancellationToken),
            pitroad = await GetSvg(trackAssets.track_map + trackAssets.track_map_layers.pitroad, cancellationToken),
            inactive = await GetSvg(trackAssets.track_map + trackAssets.track_map_layers.inactive, cancellationToken),
            start_finish = await GetSvg(trackAssets.track_map + trackAssets.track_map_layers.start_finish, cancellationToken),
            turns = await GetSvg(trackAssets.track_map + trackAssets.track_map_layers.turns, cancellationToken)
        };

        return svgLayers;
    }

    private async Task<string> GetSvg(string link, CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetAsync(link, cancellationToken);
        if (result.IsSuccessStatusCode == false)
        {
            throw new Exception($"Result was: {result.StatusCode}");
        }
        return await result.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
    }

    public static string EncodePassword(string username, string password)
    {
        var stringData = $"{password}{username.ToLower()}";
        var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(stringData));
        return Convert.ToBase64String(bytes);
    }
}

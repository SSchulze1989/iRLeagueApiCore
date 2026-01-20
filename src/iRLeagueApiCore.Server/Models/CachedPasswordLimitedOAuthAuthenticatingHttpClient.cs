using Aydsko.iRacingData;
using Microsoft.Extensions.Caching.Memory;

namespace iRLeagueApiCore.Server.Models;

public class CachedPasswordLimitedOAuthAuthenticatingHttpClient : PasswordLimitedOAuthAuthenticatingHttpClient
{
    private readonly IMemoryCache memoryCache;

    public CachedPasswordLimitedOAuthAuthenticatingHttpClient(HttpClient httpClient, 
        iRacingDataClientOptions options, TimeProvider timeProvider, IMemoryCache memoryCache) : base(httpClient, options, timeProvider)
    {
        this.memoryCache = memoryCache;
    }

    protected override async Task<RequestTokenResult> RequestTokenAsync(CancellationToken cancellationToken = default)
    {
        // check if token is stored in cache
        if (memoryCache.TryGetValue<RequestTokenResult>(CacheKeys.IracingOauthTokenKey, out var cachedToken) 
            && cachedToken is not null)
        {
            return cachedToken;
        }

        // if not request new token from iRacing
        var tokenResult = await base.RequestTokenAsync(cancellationToken);

        // store token on success
        if (tokenResult.Token is not null)
        {
            // clear refresh token for security reasons
            var token = tokenResult.Token;
            token = new OAuthTokenResponse(token.AccessToken, token.TokenType, token.ExpiresInSeconds, null, 0, token.Scope);
            tokenResult = new RequestTokenResult(token, tokenResult.ExpiresAt, DateTimeOffset.Now);

            // store in cache
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(tokenResult.ExpiresAt);
            memoryCache.Set(CacheKeys.IracingOauthTokenKey, tokenResult, cacheEntryOptions);
        }
        return tokenResult;
    }
}

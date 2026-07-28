using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using WebAccount.Interfaces;

namespace StudentAccountTSTU.Services;

public class HttpService : IHttpService, IDisposable {
    private readonly CookieContainer cookieContainer;
    private readonly HttpClient client;

    public HttpService() {
        cookieContainer = new();
        client = new HttpClient(
            new HttpClientHandler {
                CookieContainer   = cookieContainer,
                UseCookies        = true,
                AllowAutoRedirect = false,
            }
        );

        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en;q=0.8"); 
    }

    public async Task<string> GetPageAsync(string url) {        
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) {
            throw new HttpRequestException($"Некорректная или относительная ссылка: {url}");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await client.SendAsync(request);

        if ((int)response.StatusCode is >= 300 and < 400 && response.Headers.Location is not null) {
            var redirectUrl = response.Headers.Location.IsAbsoluteUri ? response.Headers.Location.ToString() : new Uri(new Uri(url), response.Headers.Location).ToString();
            return await GetPageAsync(redirectUrl);
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<HttpResponseMessage> PostFormAsync(string url, Dictionary<string, List<string>> payload, string? referer = null) {
        var pairs = new List<KeyValuePair<string, string>>();
        foreach (var (key, values) in payload)
            foreach (var value in values)
                pairs.Add(new KeyValuePair<string, string>(key, value));

        using var content = new FormUrlEncodedContent(pairs);
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

        if (referer is not null) {
            request.Headers.Referrer = new Uri(referer);
            request.Headers.Add("Origin", new Uri(referer).GetLeftPart(UriPartial.Authority));
        }

        return await client.SendAsync(request);
    }

    public async Task<Stream> GetStreamAsync(string url) {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await client.SendAsync(request);

        if ((int)response.StatusCode is >= 300 and < 400 && response.Headers.Location is not null) {
            var redirectUrl = response.Headers.Location.IsAbsoluteUri
                ? response.Headers.Location.ToString()
                : new Uri(new Uri(url), response.Headers.Location).ToString();
            return await GetStreamAsync(redirectUrl);
        }

        response.EnsureSuccessStatusCode();

        var memoryStream = new MemoryStream();
        await response.Content.CopyToAsync(memoryStream);

        memoryStream.Position = 0;
        return memoryStream;
    }

    public void Dispose() => client.Dispose();
}

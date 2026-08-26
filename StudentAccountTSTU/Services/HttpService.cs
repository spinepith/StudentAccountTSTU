using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using WebAccount.Interfaces;

namespace StudentAccountTSTU.Services;

public class HttpService : IHttpService, IDisposable {
    private const string ProxyUrl = "https://student-account-tstu.pages.dev/proxy";
    //private const string ProxyUrl = "http://127.0.0.1:8788/proxy";

    private readonly CookieContainer cookieContainer;
    private readonly HttpClient client;
    private readonly bool isBowser;

    public CancellationTokenSource Cts { get; set; } = new();

    public HttpService() {
        isBowser = OperatingSystem.IsBrowser();
        cookieContainer = new();

        if (isBowser) {
            client = new HttpClient();
        }
        else {
            client = new HttpClient(
                new HttpClientHandler {
                    CookieContainer   = cookieContainer,
                    UseCookies        = true,
                    AllowAutoRedirect = false,
                }
            );
        }

        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en;q=0.8"); 
    }

    public async Task<string> GetPageAsync(string url) {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new HttpRequestException($"Некорректная или относительная ссылка: {url}");

        using var request = PrepareRequest(HttpMethod.Get, url);
        using var response = await client.SendAsync(request, Cts.Token);

        ProcessProxyResponse(response, url);

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

        var content = new FormUrlEncodedContent(pairs);
        var request = PrepareRequest(HttpMethod.Post, url, content);

        if (referer is not null) {
            request.Headers.Referrer = new Uri(referer);
            request.Headers.Add("Origin", new Uri(referer).GetLeftPart(UriPartial.Authority));
        }

        var response = await client.SendAsync(request, Cts.Token);
        ProcessProxyResponse(response, url);

        return response;
    }

    public async Task<Stream> GetStreamAsync(string url) {
        var request = PrepareRequest(HttpMethod.Get, url);
        var response = await client.SendAsync(request, Cts.Token);

        ProcessProxyResponse(response, url);

        if ((int)response.StatusCode is >= 300 and < 400 && response.Headers.Location is not null) {
            var redirectUrl = response.Headers.Location.IsAbsoluteUri ? response.Headers.Location.ToString() : new Uri(new Uri(url), response.Headers.Location).ToString();
            return await GetStreamAsync(redirectUrl);
        }

        response.EnsureSuccessStatusCode();

        var memoryStream = new MemoryStream();
        await response.Content.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public void Dispose() => client.Dispose();

    private HttpRequestMessage PrepareRequest(HttpMethod method, string url, HttpContent? content = null) {
        var targetUrl = isBowser ? ProxyUrl : url;
        var request = new HttpRequestMessage(method, targetUrl);

        if (content is not null)
            request.Content = content;

        if (isBowser) {
            request.Headers.Add("X-Target-Url", url);
            var cookies = cookieContainer.GetCookieHeader(new Uri(url));
            if (!string.IsNullOrEmpty(cookies))
                request.Headers.Add("X-Custom-Cookie", cookies);
        }

        return request;
    }

    private void ProcessProxyResponse(HttpResponseMessage response, string originalUrl) {
        if (!isBowser)
            return;

        if (response.Headers.TryGetValues("X-Proxy-Status", out var statuses) && int.TryParse(statuses.First(), out var statusCode))
            response.StatusCode = (HttpStatusCode)statusCode;

        if (response.Headers.TryGetValues("X-Proxy-Location", out var locs))
            response.Headers.Location = new Uri(locs.First(), UriKind.RelativeOrAbsolute);

        if (response.Headers.TryGetValues("X-Proxy-Set-Cookie", out var cookieValues)) {
            var json = cookieValues.FirstOrDefault();
            if (json is not null) {
                var cookieArray = System.Text.Json.JsonSerializer.Deserialize<string[]>(json);
                if (cookieArray is not null) {
                    var uri = new Uri(originalUrl);
                    foreach (var cookieString in cookieArray) {
                        try {
                            cookieContainer.SetCookies(uri, cookieString);
                        }
                        catch { }
                    }
                }
            }
        }
    }
}

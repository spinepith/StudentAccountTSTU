const PROD_ORIGIN = "https://student-account-tstu.pages.dev";

const ALLOWED_ORIGINS = [
  PROD_ORIGIN,
  "http://localhost:8788",
  "http://127.0.0.1:8788",
  "http://localhost:5235",
  "http://127.0.0.1:5235"
];

const ALLOWED_HOSTNAMES = [
  "tstu.ru",
  "web-iais.admin.tstu.ru",
  "82.179.146.170"
];

export default {
  async fetch(request, env, ctx) {
    const url = new URL(request.url);

    if (url.pathname !== "/proxy") {
      const assetResponse = await env.ASSETS.fetch(request);
      const headers = new Headers(assetResponse.headers);

      headers.set("Cross-Origin-Opener-Policy", "same-origin");
      headers.set("Cross-Origin-Embedder-Policy", "require-corp");

      return new Response(assetResponse.body, {
        status: assetResponse.status,
        statusText: assetResponse.statusText,
        headers
      });
    }

    const origin = request.headers.get("Origin") ?? "";
    const referer = request.headers.get("Referer") ?? "";

    const isPagesDev = origin.endsWith(".student-account-tstu.pages.dev") || origin === PROD_ORIGIN;
    const isFromOurSite = !origin || ALLOWED_ORIGINS.includes(origin) || isPagesDev || ALLOWED_ORIGINS.some(o => referer.startsWith(o)) || referer.includes(".student-account-tstu.pages.dev");
    const responseOrigin = (ALLOWED_ORIGINS.includes(origin) || isPagesDev) ? origin : PROD_ORIGIN;

    if (request.method === "OPTIONS") {
      return new Response(null, {
        status: isFromOurSite ? 204 : 403,
        headers: {
          "Access-Control-Allow-Origin": responseOrigin,
          "Access-Control-Allow-Methods": "*",
          "Access-Control-Allow-Headers": "*",
          "Access-Control-Max-Age": "86400"
        }
      });
    }

    if (!isFromOurSite) {
      return new Response("Forbidden: unauthorized origin", { status: 403 });
    }

    const targetUrl = request.headers.get("X-Target-Url");
    if (!targetUrl) return new Response("Missing X-Target-Url", { status: 400 });

    let parsedTarget;
    try {
      parsedTarget = new URL(targetUrl);
    } catch {
      return new Response("Invalid X-Target-Url", { status: 400 });
    }
    const hostname = parsedTarget.hostname.toLowerCase();
    const isAllowed =
      ALLOWED_HOSTNAMES.includes(hostname) ||
      hostname.endsWith(".tstu.ru");

    if (!isAllowed) {
      return new Response("Forbidden: domain not allowed", { status: 403 });
    }

    const newHeaders = new Headers(request.headers);
    newHeaders.delete("X-Target-Url");
    newHeaders.delete("Host");
    newHeaders.delete("Origin");
    newHeaders.delete("Referer");

    const customCookie = newHeaders.get("X-Custom-Cookie");
    if (customCookie) {
      newHeaders.set("Cookie", customCookie);
      newHeaders.delete("X-Custom-Cookie");
    }

    let proxyResponse;
    try {
      proxyResponse = await fetch(targetUrl, {
        method: request.method,
        headers: newHeaders,
        body: request.method !== "GET" && request.method !== "HEAD" ? request.body : null,
        redirect: "manual"
      });
    } catch (err) {
      throw new Error("Proxy Fetch Error: " + err.message);
    }

    const responseHeaders = new Headers(proxyResponse.headers);
    responseHeaders.set("Access-Control-Allow-Origin", responseOrigin);
    responseHeaders.set("Access-Control-Expose-Headers", "*");

    if (typeof responseHeaders.getSetCookie === "function") {
      const setCookies = responseHeaders.getSetCookie();
      if (setCookies && setCookies.length > 0) {
        responseHeaders.set("X-Proxy-Set-Cookie", JSON.stringify(setCookies));
      }
    }

    const location = responseHeaders.get("Location");
    if (location) {
      responseHeaders.set("X-Proxy-Location", location);
      responseHeaders.delete("Location");
    }

    let status = proxyResponse.status;
    if (status >= 300 && status < 400) {
      responseHeaders.set("X-Proxy-Status", status.toString());
      status = 200;
    }

    return new Response(proxyResponse.body, {
      status: status,
      statusText: status === 200 ? "OK" : proxyResponse.statusText,
      headers: responseHeaders
    });
  }
};

import type { NextRequest } from "next/server";

// Прокси /api/* -> ASP.NET Core API. Адрес бэкенда читается в рантайме,
// поэтому один и тот же образ работает и локально, и в docker-compose.
const API_URL = process.env.API_INTERNAL_URL ?? "http://localhost:5273";

const FORWARDED_REQUEST_HEADERS = ["authorization", "content-type", "accept", "accept-language"];

async function proxy(request: NextRequest, { params }: RouteContext<"/api/[...path]">) {
  const { path } = await params;
  const target = `${API_URL}/api/${path.join("/")}${request.nextUrl.search}`;

  const headers = new Headers();
  for (const name of FORWARDED_REQUEST_HEADERS) {
    const value = request.headers.get(name);
    if (value) headers.set(name, value);
  }

  const hasBody = !["GET", "HEAD"].includes(request.method);
  const response = await fetch(target, {
    method: request.method,
    headers,
    body: hasBody ? await request.arrayBuffer() : undefined,
    cache: "no-store",
  });

  const responseHeaders = new Headers();
  const contentType = response.headers.get("content-type");
  if (contentType) responseHeaders.set("content-type", contentType);

  return new Response(response.body, { status: response.status, headers: responseHeaders });
}

export { proxy as GET, proxy as POST, proxy as PUT, proxy as PATCH, proxy as DELETE };

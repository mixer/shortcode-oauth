import * as http from 'http';
import * as https from 'https';
import * as url from 'url';

const { version } = require('../package.json');

/**
 * Returns a promise that resolves after the given duration.
 */
export async function delay(duration: number): Promise<void> {
  return new Promise<void>(resolve => setTimeout(resolve, duration)); // tslint:disable-line
}

export type HttpMethod = 'get' | 'post' | 'put' | 'patch';

/**
 * HttpResponse is returned from the IRequester.
 */
export interface IHttpResponse extends http.ClientResponse {
  text: string;
  statusCode: number;
  json?: any;
}

/**
 * IRequester is an interface for a type that makes HTTP requests.
 */
export interface IRequester {
  json(method: HttpMethod, path: string, body?: object): Promise<IHttpResponse>;
}

/**
 * NativeRequester is an IRequester implementation that uses Node's built-in
 * http module.
 */
export class NativeRequester implements IRequester {
  /**
   * URL to make requests against.
   */
  private readonly url: url.UrlWithStringQuery;

  constructor(
    baseUrl = 'https://mixer.com/api/v1/',
    private readonly defaultHeaders: http.OutgoingHttpHeaders = {},
  ) {
    this.url = url.parse(baseUrl);
  }

  /**
   * @inheritDoc
   */
  public json(method: HttpMethod, path: string, body?: object): Promise<IHttpResponse> {
    const request = this.url.protocol === 'http' ? http.request : https.request;
    const options: http.RequestOptions = {
      method,
      hostname: this.url.hostname,
      protocol: this.url.protocol,
      port: this.url.port,
      path: url.resolve(this.url.path || '/', path),
      headers: {
        'user-agent': `NodeJS Shortcode Client/${version}`,
        ...this.defaultHeaders,
      },
    };

    if (body !== undefined) {
      options.headers!['content-type'] = 'application/json';
    }

    return new Promise((resolve, reject) => {
      const req = request(options, res => {
        let data = Buffer.from([]);
        res.on('data', chunk => (data = Buffer.concat([chunk, data])));
        res.on('end', () => {
          const text = data.toString('utf8');
          let json: any;
          try {
            json = JSON.parse(text);
          } catch (e) {
            // ignored
          }

          resolve(Object.assign(res, { json, text }) as any);
        });
      });

      req.on('error', reject);

      if (body !== undefined) {
        req.write(JSON.stringify(body));
      }

      req.end();
    });
  }
}

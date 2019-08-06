import { ShortCodeAccessDeniedError, ShortCodeExpireError, UnexpectedHttpError } from './errors';
import { delay, IRequester, NativeRequester } from './util';

// note: this is a functional port of the Python version here:
// https://github.com/mixer/interactive-python/blob/master/interactive_python/oauth.py

export interface IShortcodeCreateResponse {
  code: string;
  expires_in: number;
  handle: string;
}

export interface IOAuthTokenData {
  accessToken: string;
  refreshToken: string;
  scopes: string[];
  expiresAt: Date;
}

/**
 * OAuthTokens is the handle for OAuth access and refresh tokens granted by
 * the user.
 */
export class OAuthTokens {
  /**
   * Returns OAuth tokens created from a response from the token endpoint as
   * described in https://tools.ietf.org/html/rfc6749#section-5.1.
   */
  public static fromTokenResponse(res: any, scopes: string[]) {
    return new OAuthTokens({
      accessToken: res.access_token,
      refreshToken: res.refresh_token,
      expiresAt: new Date(Date.now() + res.expires_in * 1000),
      scopes,
    });
  }

  constructor(public readonly data: IOAuthTokenData) {}

  /**
   * Returns a header that can be used in making http requests.
   */
  public header(): { Authorization: string } {
    return { Authorization: `Bearer ${this.data.accessToken}` };
  }

  /**
   * Returns whether the oauth tokens are expired.
   */
  public expired(): boolean {
    return this.data.expiresAt.getTime() < Date.now();
  }

  /**
   * Returns whether all of the provided scopes have been granted by this set
   * of OAuth tokens.
   */
  public granted(scopes: string[]): boolean {
    return !scopes.some(s => this.data.scopes.indexOf(s) === -1);
  }
}

/**
 * OAuthShortCode is the shortcode handle returned by the `OAuthClient`. See
 * documentation on that class for more information and usage examples.
 */
export class OAuthShortCode {
  /**
   * Interval on which to poll to see if the shortcode was accepted.
   */
  private static checkInterval = 2000;

  /**
   * The OAuth shortcode to display to the user.
   */
  public readonly code: string;

  /**
   * The shortcode handle for polling.
   */
  public readonly handle: string;

  /**
   * Return the number of seconds until the shortcode expires.
   */
  public readonly expiresIn: number;

  /**
   * Gets the time at which this shortcode expires.
   */
  public readonly expiresAt: Date;

  private expired: Promise<void>;

  constructor(
    private readonly clientId: string,
    private readonly scopes: string[],
    private readonly fetcher: IRequester,
    response: IShortcodeCreateResponse,
    private readonly clientSecret?: string,
  ) {
    this.handle = response.handle;
    this.expiresIn = response.expires_in;
    this.code = response.code;
    this.expired = delay(this.expiresIn * 1000);
    this.expiresAt = new Date(this.expiresIn * 1000 + Date.now());
  }

  /**
   * Returns whether this shortcode has expired.
   */
  public hasExpired(): boolean {
    return Date.now() > this.expiresAt.getTime();
  }

  /**
   * Returns a promise that resolves once the user authorizes your app,
   * or throws if the token expires or is denied.
   */
  public async waitForAccept(): Promise<OAuthTokens> {
    const result = await this.poll();
    if (result) {
      return result;
    }

    await Promise.race([
      delay(OAuthShortCode.checkInterval),
      this.expired.then(() => {
        throw new ShortCodeExpireError();
      }),
    ]);

    return this.waitForAccept();
  }

  /**
   * Polls the server to see if the shortcode has been authorized. Returns
   * the set of access and refresh tokens if so, or undefined if it's not
   * been authorized yet. Throws an appropriate error if those shortcode
   * has expired or been denied.
   */
  public async poll(): Promise<OAuthTokens | void> {
    const res = await this.fetcher.json('get', `oauth/shortcode/check/${this.handle}`);
    switch (res.statusCode) {
      case 200:
        return this.getTokens(res.json);
      case 204:
        return undefined;
      case 403:
        throw new ShortCodeAccessDeniedError();
      case 404:
        throw new ShortCodeExpireError();
      default:
        throw new UnexpectedHttpError(res);
    }
  }

  private async getTokens({ code }: { code: string }): Promise<OAuthTokens> {
    const res = await this.fetcher.json('post', 'oauth/token', {
      client_id: this.clientId,
      client_secret: this.clientSecret,
      grant_type: 'authorization_code',
      code,
    });

    if (res.statusCode >= 300) {
      throw new UnexpectedHttpError(res);
    }

    return OAuthTokens.fromTokenResponse(res.json, this.scopes);
  }
}

/**
 * IOAuthOptions are passed into the OAuth client for creating
 * and retrieving grants.
 */
export interface IOAuthOptions {
  /**
   * OAuth client ID.
   */
  clientId: string;
  /**
   * OAuth client secret, if any.
   */
  clientSecret?: string;
  /**
   * A list of permissions to request. A full list can be found here:
   * https://dev.mixer.com/reference/oauth/index.html#oauth_scopes
   */
  scopes: string[];

  /**
   * OAuth host address, defaults to https://mixer.com/api/v1
   */
  host?: string;
}

/**
 * OAuthClient is a clien interface for handling shortcode grants and
 * reauthorizing access tokens.
 *
 * ```
 * const client = new OAuthClient(options);
 * const attempt = () => {
 *   return client.getCode()
 *     .then(code => {
 *       console.log(`Go to mixer.com/go and enter ${code.code}`);
 *       return code.waitForAccept();
 *     })
 *     .catch(err => {
 *       if (err instanceof ShortCodeExpireError) {
 *         return attempt(); // loop!
 *       }
 *
 *       throw err;
 *     });
 * };
 *
 * attempt().then(tokens => console.log(`Access token: ${tokens.access}`));
 * ```
 */
export class OAuthClient {
  constructor(
    private readonly options: IOAuthOptions,
    private readonly fetcher: IRequester = new NativeRequester(options.host),
  ) {}

  /**
   * Starts a flow to get an oauth code.
   */
  public async getCode(): Promise<OAuthShortCode> {
    const res = await this.fetcher.json('post', 'oauth/shortcode', {
      client_id: this.options.clientId,
      client_secret: this.options.clientSecret,
      scope: this.options.scopes.join(' '),
    });

    if (res.statusCode >= 300) {
      throw new UnexpectedHttpError(res);
    }

    return new OAuthShortCode(this.options.clientId, this.options.scopes, this.fetcher, res.json, this.options.clientSecret);
  }

  /**
   * Refreshes the token set and returns a new set of tokens.
   */
  public async refresh(tokens: OAuthTokens): Promise<OAuthTokens> {
    const res = await this.fetcher.json('post', 'oauth/token', {
      grant_type: 'refresh_token',
      refresh_token: tokens.data.refreshToken,
      client_id: this.options.clientId,
      client_secret: this.options.clientSecret,
    });

    if (res.statusCode >= 300) {
      throw new UnexpectedHttpError(res);
    }

    return OAuthTokens.fromTokenResponse(res.json, tokens.data.scopes);
  }
}

import { IHttpResponse } from './util';

/**
 * ShortCodeError is the error type thrown when anything goes wrong in shortcode oauth.
 */
export class ShortCodeError extends Error {}

/**
 * ShortCodeExpireError is raised from waitForAccept() if the shortcode expires
 * before the user accepts it. Callers should handle this error.
 */
export class ShortCodeExpireError extends ShortCodeError {
  constructor() {
    super('Shortcode handle has expired');
  }
}

/**
 * Exception raised when the user denies access to the client in shortcode OAuth.
 */
export class ShortCodeAccessDeniedError extends ShortCodeError {
  constructor() {
    super('User has denied access');
  }
}

/**
 * UnexpectedHttpError is raised when we get an unexpected status code
 * from Mixer.
 */
export class UnexpectedHttpError extends Error {
  constructor(public readonly res: IHttpResponse) {
    super(
      `Unexpected status code ${res.statusCode} ${res.statusMessage} from ${res.url}: ${res.text}`,
    );
  }

  public cause() {
    return this.res;
  }
}

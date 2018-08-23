import { use } from 'chai';
import * as sinon from 'sinon';

import { OAuthTokens } from '../src/shortcode';
import { IRequester } from '../src/util';

use(require('chai-as-promised'));

/**
 * Mock implementation of IRequester
 */
export class MockRequester implements IRequester {
  public json = sinon.stub();
}

export function createExpiredOAuthTokens() {
  return new OAuthTokens({
    accessToken: 'expired_access_token',
    refreshToken: 'expired_refresh_token',
    scopes: ['foo', 'bar'],
    expiresAt: new Date(Date.now() - 1),
  });
}

export function createValidOAuthTokens() {
  return new OAuthTokens({
    accessToken: 'access_token',
    refreshToken: 'refresh_token',
    scopes: ['foo', 'bar'],
    expiresAt: new Date(Date.now() + 10000),
  });
}

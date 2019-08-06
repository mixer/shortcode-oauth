import { expect } from 'chai';
import * as sinon from 'sinon';

import { ShortCodeAccessDeniedError, ShortCodeExpireError } from './errors';
import { createExpiredOAuthTokens, createValidOAuthTokens, MockRequester } from './setup.test';
import { OAuthClient, OAuthTokens } from './shortcode';

const scopes = ['foo', 'bar'];

describe('shortcode oauth', () => {
  let requester: MockRequester;
  let clock: sinon.SinonFakeTimers;
  let oauth: OAuthClient;
  beforeEach(() => {
    clock = sinon.useFakeTimers();
    requester = new MockRequester();
    oauth = new OAuthClient(
      {
        clientId: 'clientId',
        clientSecret: 'clientSecret',
        scopes,
      },
      requester,
    );
  });

  const mockTokenResponse = {
    expires_in: 60000,
    access_token: 'access_token',
    refresh_token: 'refresh_token',
  };

  const expectTokensToBeMadeFromMock = (tokens: OAuthTokens) => {
    expect(tokens.data).to.deep.equal({
      expiresAt: new Date(Date.now() + mockTokenResponse.expires_in * 1000),
      accessToken: 'access_token',
      refreshToken: 'refresh_token',
      scopes,
    });
  };

  afterEach(() => {
    clock.restore();
  });

  describe('oauth tokens', () => {
    let tokens: OAuthTokens;

    beforeEach(() => {
      tokens = createValidOAuthTokens();
    });

    it('detects granted permissions correctly', () => {
      expect(tokens.granted([])).to.be.true;
      expect(tokens.granted(['foo'])).to.be.true;
      expect(tokens.granted(['foo', 'other'])).to.be.false;
      expect(tokens.granted(['other'])).to.be.false;
    });

    it('detects expiration correctly', () => {
      expect(tokens.expired()).to.be.false;
      clock.tick(tokens.data.expiresAt.getTime() - Date.now() + 1);
      expect(tokens.expired()).to.be.true;
    });

    it('creates headers', () => {
      expect(tokens.header()).to.deep.equal({ Authorization: 'Bearer access_token' });
    });
  });

  describe('token refresh', () => {
    it('refreshes tokens successfully', async () => {
      requester.json
        .withArgs('post', 'oauth/token', {
          grant_type: 'refresh_token',
          refresh_token: 'expired_refresh_token',
          client_id: 'clientId',
          client_secret: 'clientSecret',
        })
        .resolves({
          json: mockTokenResponse,
          statusCode: 200,
        });

      const newTokens = await oauth.refresh(createExpiredOAuthTokens());
      expectTokensToBeMadeFromMock(newTokens);
    });
  });

  describe('granting', () => {
    const giveStatus = (statusCode: number) => {
      return requester.json.withArgs('get', 'oauth/shortcode/check/sc_handle').resolves({
        statusCode,
        json: { code: 'oauth_authorization_code' },
      });
    };

    beforeEach(() => {
      requester.json
        .withArgs('post', 'oauth/token', {
          client_id: 'clientId',
          client_secret: 'clientSecret',
          grant_type: 'authorization_code',
          code: 'oauth_authorization_code',
        })
        .resolves({
          statusCode: 200,
          json: mockTokenResponse,
        });

      requester.json
        .withArgs('post', 'oauth/shortcode', {
          client_id: 'clientId',
          client_secret: 'clientSecret',
          scope: scopes.join(' '),
        })
        .resolves({
          statusCode: 200,
          json: { code: 'ABC123', handle: 'sc_handle' },
        });
    });

    it('detects expired handles', async () => {
      const code = await oauth.getCode();
      expect(code.code).to.equal('ABC123');
      giveStatus(404);

      await expect(code.waitForAccept()).to.eventually.be.rejectedWith(ShortCodeExpireError);
    });

    it('detects rejected handles', async () => {
      const code = await oauth.getCode();
      expect(code.code).to.equal('ABC123');
      giveStatus(403);

      await expect(code.waitForAccept()).to.eventually.be.rejectedWith(ShortCodeAccessDeniedError);
    });
    it('loops and eventually resolves otherwise', async () => {
      const code = await oauth.getCode();
      expect(code.code).to.equal('ABC123');

      giveStatus(204).onFirstCall();
      giveStatus(200).onSecondCall();
      expectTokensToBeMadeFromMock(await code.waitForAccept());
    });
  });
});

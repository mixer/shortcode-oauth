const { ShortCodeExpireError, OAuthClient } = require('./');

const client = new OAuthClient({
  clientId: 'a2c0f4df2b184b14196aa826548deec10e53f571f9f59f09',
  scopes: ['interactive:robot:self'],
});

const attempt = () =>
  client
    .getCode()
    .then(code => {
      console.log(`Go to mixer.com/go and enter ${code.code}`);
      return code.waitForAccept();
    })
    .catch(err => {
      if (err instanceof ShortCodeExpireError) {
        return attempt(); // loop!
      }

      throw err;
    });

attempt().then(tokens => console.log(`Token data`, tokens.data));

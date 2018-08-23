const { ShortCodeExpireError, OAuthClient } = require('./');

const client = new OAuthClient({
  clientId: '<Your OAuth token here!>',
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

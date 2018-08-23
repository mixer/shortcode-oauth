# @mixer/shortcode-auth

```
npm install --save @mixer/shortcode-oauth
```

This is an example of Mixer shortcode OAuth in Node.js. An example can be found in `example.js` in this directory. For more information about OAuth in general, check out the [OAuth reference page](https://dev.mixer.com/reference/oauth/index.html) on the Mixer developer site.

```js
const { ShortCodeExpireError, OAuthClient } = require('@mixer/shortcode-oauth');

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

```

Example:

```
Go to mixer.com/go and enter E2LM8U
Token data { accessToken: '<snip>',
  refreshToken: '<snip>',
  expiresAt: 2018-08-23T06:56:49.169Z,
  scopes: [ 'interactive:robot:self' ] }
```

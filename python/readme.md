# mixer_shortcode

```
pip install mixer_shotcode
```

This is an example of Mixer shortcode OAuth in Python. An example can be found in `example.py` in this directory. For more information about OAuth in general, check out the [OAuth reference page](https://dev.mixer.com/reference/oauth/index.html) on the Mixer developer site.

```py
from mixer_shortcode import OAuthClient, ShortCodeAccessDeniedError, ShortCodeTimeoutError
import asyncio

my_client_id = '<Your OAuth token here!>'
scopes = ['interactive:robot:self']

async def get_access_token(client):
    code = await client.get_code()
    print("Go to mixer.com/go and enter {}".format(code.code))

    try:
        return await code.accepted()
    except ShortCodeAccessDeniedError:
        print("The user denied access to our client")
    except ShortCodeTimeoutError:
        print("Yo, you're too slow! Let's try again...")
        return await get_access_token(client)

async def run():
    async with OAuthClient(my_client_id, scopes=scopes) as client:
        token = await get_access_token(client)
        print("Access token: {}".format(token.access))

asyncio.get_event_loop().run_until_complete(run())
```

Example:

```
> python example.py
Go to mixer.com/go and enter KJ3NA6
Access token: <snip>
```

# shortcode-oauth

This repository contains several clients for the Mixer shortcode OAuth flow. This is an alternative OAuth-based flow for applications to avoid the need for opening or embedding browsers, or requiring keyboard input on the client device.

Several implementations are available:

 - [`@mixer/shortcode-auth` in Node.js](https://github.com/mixer/shortcode-oauth/tree/master/nodejs)
 - [`Microsoft.Mixer.ShortcodeOAuth` in C#](https://github.com/mixer/shortcode-oauth/tree/master/csharp) (.NET standard)
 - [`mixer_shortcode` in Python](https://github.com/mixer/shortcode-oauth/tree/master/python)

![](./demo.gif)

### Quickstart

 1. Register an OAuth application on the [Mixer Lab](http://mixer.com/lab>). If you're developing an integration which will run on users' computers, you should not request a client secret.
 2. Find the client for your language above.
 3. Follow the instructions/example code on its readme to get going!

### Specification

Shortcode OAuth was first described in the [Interactive protocol specification](https://dev.mixer.com/reference/interactive/protocol/protocol.pdf). The relevant sections are below.

```
         │
         ▼
┌─────────────────┐             ┌─────────────────────────────┐
│  Have existing  │             │ Call POST /oauth/shortcode  │
│  access_token?  │────no──────▶│ with desired permissions +  │─────┐
└─────────────────┘             │       client details        │     │
         │                      └─────────────────────────────┘     ▼
        yes                                    ▲   ┌────────────────────────────────┐
         │                                     │   │ Direct user to beam.pro/go to  │
         │                                     │   │     enter six-digit `code`     │
         │                                     │   └────────────────────────────────┘
         ▼                                404 status                │          ┌───────┐
┌─────────────────┐                     (code expired)              ▼          │       204 status
│ Is it close to  │                            │    ┌──────────────────────────▼────┐ (not entered
│    expiring?    │──────yes────┐              │    │           Poll GET            │  │  yet)
└─────────────────┘             │              └────│/oauth/shortcode/check/{handle}│──┘
         │                      ▼                   └─────────────────────────────┬─┘
         │           ┌─────────────────────┐               │                      │
        no           │Use the refresh_token│          200 status             403 status
         │           │  to get new tokens  │               │               (denied access)
         ▼           └─────────────────────┘               ▼                      │
┌─────────────────┐             │            ┌──────────────────────────┐         ▼
│     Use the     │             │            │Exchange for access_token,│  ┌─────────────┐
│ access_token to │◀────────────┴────────────│      refresh_token       │  │ Show Error  │
│    handshake    │                          └──────────────────────────┘  └─────────────┘
└─────────────────┘
```

In order to build an OAuth shortcode-based application without using one of these ready-made clients, you should follow these steps:

 0. Register an OAuth application on the [Mixer Lab](http://mixer.com/lab>). If you're developing an integration which will run on users' computers, you should not request a client secret.
 1. Call [`POST /oauth/shortcode`](https://dev.mixer.com/rest.html#oauth_shortcode_post>) with your `client_id`, `client_secret` (if any) and space-delimited `scope` you want in the request body. This will typically look something like this::

    ```http
    POST /api/v1/oauth/shortcode HTTP/1.1
    Accept: application/json, */*
    Host: mixer.com

    {
        "client_id": "fooclient",
        "scope": "interactive:robot:self"
    }

    HTTP/1.1 200 OK
    Content-Type: application/json; charset=utf-8

    {
        "code": "8WPVHT",
        "expires_in": 120,
        "handle": "Lc7eBcB78d5gZmqHOajMH3QnmFPrxLGr"
    }
    ```

 2. Display the short six-digit `code` to the user and prompt them to enter it on [mixer.com/go](https://mixer.com/go). You can view a user's perspective of this process [here](https://dev.mixer.com/img/reference/interactive/link-demo.gif).
 3. Continuously poll [`GET /oauth/shortcode/check/{handle}`](https://dev.mixer.com/rest.html#oauth_shortcode_check__handle__get). It will give you one of a few statuses back:

    - `204 No Content` indicates we're still waiting on the user to enter the code.
    - `403 Forbidden` indicates the user denied your requested permissions.
    - `404 Not Found` indicates that the handle is invalid or expired.
    - `200 OK` is returned once the user has granted your application access. The response body will contain an `code`.

    A poll which results in a 200 response might look something like this::

      ```http
      GET /api/v1/oauth/shortcode/check/Lc7eBcB78d5gZ... HTTP/1.1
      Accept: application/json, */*
      Host: mixer.com

      HTTP/1.1 200 OK
      Content-Type: application/json; charset=utf-8

      {
          "code": "r2BTMKCZJJyIuoNE"
      }
      ```


 4. The client app should use this code as an OAuth Authorization Code which it can [exchange](https://tools.ietf.org/html/rfc6749#section-4.1.3) for access and refresh tokens. The `redirect_uri` in the exchange request is not required and will be ignored, but you must present a valid `client_id`. A request to exchange the tokens might look something like this::

    ```http
    POST /api/v1/oauth/token HTTP/1.1
    Accept: application/json, */*
    Content-Type: application/json
    Host: mixer.com

    {
        "client_id": "fooclient",
        "code": "r2BTMKCZJJyIuoNE",
        "grant_type": "authorization_code"
    }

    HTTP/1.1 200 OK
    Content-Type: application/json; charset=utf-8

    {
        "access_token": "pHktaORPcQGejnz48rJQdDWh1AJpevs \
            TWnvKrZW5z2HP3lgEqhp9gzje1YfblIO2",
        "expires_in": 21599,
        "refresh_token": "HzCZSviiueoWsfcT6kh6d4n7SHUnfK \
            cFTRIOyHkgykjaCSIT5ctTqUKNTXfWsxfg",
        "token_type": "Bearer"
    }
    ```

 5. The response will include an `access_token`, which you should send in the Authorization header when you connect to Interactive, prefixed with `Bearer ` per standard OAuth behavior. In the above example, the client would then connect to Interactive and present the header `Authorization: Bearer pHktaORPcQGejnz48rJQdDWh1AJpevsTWnvKrZW5z2...`

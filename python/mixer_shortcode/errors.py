class ShortCodeError(Exception):
    """Base exception raised when some unexpected event occurs in the shortcode
    OAuth flow."""
    pass


class UnknownShortCodeError(ShortCodeError):
    """Exception raised when an unknown error happens while running shortcode
    OAuth.
    """
    pass


class ShortCodeAccessDeniedError(ShortCodeError):
    """Exception raised when the user denies access to the client in shortcode
    OAuth."""
    pass


class ShortCodeTimeoutError(ShortCodeError):
    """Exception raised when the shortcode expires without being accepted."""
    pass

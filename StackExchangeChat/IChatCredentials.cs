﻿namespace StackExchangeChat
{
    public interface IChatCredentials
    {
        string AcctCookie { get; set; }
        string AcctCookieExpiry { get; set; }

        string Email { get; set; }
        string Password { get; set; }
    }
}

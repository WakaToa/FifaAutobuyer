using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public enum FUTErrorCode : ushort
    {
        ExpiredSession = 401,
        Forbidden = 403,
        NotFound = 404,
        Conflict = 409,
        CaptchaTriggered = 458,
        CaptchaTriggered2 = 459,
        BadRequest = 460,
        PermissionDenied = 461,
        NotEnoughCredit = 470,
        DestinationFull = 473,
        NoSuchCardExists = 475,
        InvalidDeck = 477,
        NoSuchTradeExists = 478,
        InvalidCookie = 482,
        InternalServerError = 500,
        ServiceUnavailable = 503,


        JsonSerializationException = 600,

        UnknownEMailProvider = 700,
        WrongEMailPassword = 701,
        TwoFactorFailed = 702,
        NoRedirectToWebApp = 703,
        NoEaswID = 704,
        NoUserAccounts = 705,
        NoSessionID = 706,
        NoPhishingToken = 707,
        GMXBlocked = 708,
        WrongLoginData = 709,
        WrongSecurityAnswer = 710,
        WrongEMailCode = 711,
        AccountBanned = 712,
        LoginFailureUrlError = 713,
        ServerMaintenance = 714,

        NoAccessToken = 750,


        ProxyException = 800,
        HttpRequestException = 801,
        RequestException = 802,
        CaptchaException = 803
    }
}

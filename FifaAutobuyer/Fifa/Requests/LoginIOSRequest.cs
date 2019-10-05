using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FifaAutobuyer.Database;
using FifaAutobuyer.Fifa.Captcha;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Extensions;
using FifaAutobuyer.Fifa.Http;
using FifaAutobuyer.Fifa.MailService;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Responses;
using FifaAutobuyer.Fifa.Services;
using FifaAutobuyer.Fifa.Services.GoogleAuthenticator;

namespace FifaAutobuyer.Fifa.Requests
{
    public class LoginIOSRequest : FUTRequestBase, IFUTRequest<LoginResponse>
    {
        public LoginIOSRequest(FUTAccount account, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login) : base(account, rpmManager, rpmManagerSearch, login)
        {
            _mailClient = MailClientFactory.Create(account.EMail, account.EMailPassword);
            FUTAccount.FUTPlatform = new FUTPlatform();
            FUTAccount.FUTPlatform.SKU = Constants.SKUIOS;
        }

        private IMailClient _mailClient;

        public async Task<LoginResponse> PerformRequestAsync()
        {
            try
            {
                var resp = new LoginResponse();
                if (_mailClient == null && String.IsNullOrEmpty(FUTAccount.BackupCode1) && String.IsNullOrEmpty(FUTAccount.BackupCode2) && String.IsNullOrEmpty(FUTAccount.BackupCode3) && String.IsNullOrEmpty(FUTAccount.GoogleAuthCode))
                {
                    resp.Code = FUTErrorCode.UnknownEMailProvider;
                    resp.Message = "Unknown EMailprovider";
                    return resp;
                }
                HttpClient.RemoveRequestHeader(NonStandardHttpHeaders.PhishingToken);
                HttpClient.RemoveRequestHeader(NonStandardHttpHeaders.SessionId);
                UpdateStatistic("Loading Cookies and Enviromentvariables...");
                HttpClient.SetCookieContainer(CookieManager.GetCookieContainer(FUTAccount, false));
                UpdateStatistic("Grabbing Loginpage...");
                var loggedIn = await GetLoginPageAsync().ConfigureAwait(false);
                var loggedInString = await loggedIn.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!loggedIn.RequestMessage.RequestUri.AbsoluteUri.Contains("companion/auth.html"))
                {
                    await Task.Delay(2000).ConfigureAwait(false);
                    UpdateStatistic("Loging in to Origin...");
                    loggedIn = await LoginAsync(loggedIn.RequestMessage.RequestUri.AbsoluteUri).ConfigureAwait(false);
                    loggedInString = await loggedIn.Content.ReadAsStringAsync().ConfigureAwait(false);

                }


                var codeSentTimestamp = DateTime.Now.Subtract(new TimeSpan(0, 2, 0));
                if (loggedInString.Contains("var redirectUri = 'https://signin.ea.com:443/p/web2/login?execution="))
                {
                    var redirect = Regex.Match(loggedInString, "'(.*?)';").Groups[1].Value;
                    var red2 = Regex.Match(loggedInString, "redirectUri \\+ \"(.*?)\";").Groups[1].Value;
                    loggedIn = await HttpClient.GetAsync(redirect + red2).ConfigureAwait(false);
                    loggedInString = await loggedIn.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                if (loggedInString.Contains("general-error") && !loggedInString.Contains("panel-profile-upgrade"))
                {
                    resp.Code = Models.FUTErrorCode.WrongLoginData;
                    return resp;
                }

                if (loggedInString.Contains("panel-forewordMessage") && loggedInString.Contains("btn-continue"))
                {
                    UpdateStatistic("Accepting TFACreation...");
                    loggedIn = await AcceptTwoFactorAsync(loggedIn.RequestMessage.RequestUri.AbsoluteUri);
                    loggedInString = await loggedIn.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                if (loggedInString.Contains("btnTFACreate") && loggedInString.Contains("tfa_type") && loggedInString.Contains("EMAIL"))
                {
                    UpdateStatistic("Create EMail TFA...");
                    loggedIn = await CreateEMailTwoFactorAsync(loggedIn.RequestMessage.RequestUri.AbsoluteUri);
                    loggedInString = await loggedIn.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                if (loggedInString.Contains("tfa-login-link") && loggedInString.Contains("btnSendCode"))
                {
                    if (!String.IsNullOrEmpty(FUTAccount.GoogleAuthCode))
                    {
                        loggedIn = await SelectGoogleAuthenticatorAsync(loggedIn.RequestMessage.RequestUri.AbsoluteUri).ConfigureAwait(false);
                        loggedInString = await loggedIn.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        loggedIn = await SelectEMailTwoFactorAsync(loggedIn.RequestMessage.RequestUri.AbsoluteUri).ConfigureAwait(false);
                        loggedInString = await loggedIn.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                }
                var twoFactorCode = "";
                if (loggedInString.Contains("oneTimeCode"))
                {
                    if (!String.IsNullOrEmpty(FUTAccount.BackupCode1))
                    {
                        twoFactorCode = FUTAccount.BackupCode1;
                        FUTAccount.BackupCode1 = "";
                        FUTAccount.SaveChanges();

                    }
                    else if (!String.IsNullOrEmpty(FUTAccount.BackupCode2))
                    {
                        twoFactorCode = FUTAccount.BackupCode2;
                        FUTAccount.BackupCode2 = "";
                        FUTAccount.SaveChanges();
                    }
                    else if (!String.IsNullOrEmpty(FUTAccount.BackupCode2))
                    {
                        twoFactorCode = FUTAccount.BackupCode2;
                        FUTAccount.BackupCode3 = "";
                        FUTAccount.SaveChanges();
                    }
                    else if (!string.IsNullOrEmpty(FUTAccount.GoogleAuthCode))
                    {
                        twoFactorCode = QuickEAAuthenticator.GenerateAuthCode(FUTAccount.GoogleAuthCode);
                    }
                    else
                    {
                        var resendUri = loggedIn.RequestMessage.RequestUri.AbsoluteUri + "&_eventId=resend";
                        var resendUri2 = "https://signin.ea.com" + loggedInString.GetRegexBetween(" < a id=\"resend_code_link\" href=\"", "\"");
                        UpdateStatistic("Waiting for TwoFactor Code...");
                        twoFactorCode = await WaitForTwoFactorCode(codeSentTimestamp, resendUri).ConfigureAwait(false);
                    }
                    if (twoFactorCode == "WrongUserPassword")
                    {
                        resp.Code = Models.FUTErrorCode.WrongEMailPassword;
                        return resp;
                    }
                    if (twoFactorCode == "GMXBlocked")
                    {
                        resp.Code = Models.FUTErrorCode.GMXBlocked;
                        return resp;
                    }
                    if (twoFactorCode.StartsWith("EXC"))
                    {
                        resp.Code = Models.FUTErrorCode.TwoFactorFailed;
                        resp.Message = twoFactorCode.Remove(0, 3);
                        return resp;
                    }
                    if (twoFactorCode == "000000" || twoFactorCode == "")
                    {
                        resp.Code = Models.FUTErrorCode.TwoFactorFailed;
                        resp.Message = "Couldn't get twoFactorCode";
                        return resp;
                    }
                    await Task.Delay(2000).ConfigureAwait(false);
                    loggedIn = await FillInTwoFactorAsync(loggedIn.RequestMessage.RequestUri.AbsoluteUri, twoFactorCode).ConfigureAwait(false);
                    loggedInString = await loggedIn.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                if (loggedInString.Contains("Eingegebener Code nicht korrekt"))
                {
                    resp.Code = FUTErrorCode.WrongEMailCode;
                    resp.Message = "Wrong email code!";
                    return resp;
                }
                if (!loggedIn.RequestMessage.RequestUri.AbsoluteUri.Contains("auth.html"))
                {
                    resp.Code = Models.FUTErrorCode.NoRedirectToWebApp;
                    resp.Message = "No redirect to mobile-app";
                    return resp;
                }
                UpdateStatistic("Grabbing EASW ID...");

                var authCode = loggedIn.RequestMessage.RequestUri.Query.Remove(0, 6);
                var tmpBearer = await GetBearerAuthCodeAsync(authCode);
                var bearer = Regex.Match(tmpBearer, "\"access_token\" : \"(.*?)\",").Groups[1].Value;
                var easw = await GetEaswAsync(bearer);
                var easwID = easw.pid.pidId.ToString();
                if (string.IsNullOrEmpty(easwID))
                {
                    CookieManager.DeleteCookieContainer(FUTAccount, false);
                    resp.Code = Models.FUTErrorCode.NoEaswID;
                    resp.Message = "No easwID";
                    return resp;
                }
                resp.NucleusID = easwID;

                HttpClient.AddRequestHeader(NonStandardHttpHeaders.NucleusId, easwID);
                HttpClient.AddRequestHeader(NonStandardHttpHeaders.EmbedError, "true");
                UpdateStatistic("Grabbing Shards...");
                var shards = await IsServerOnline();
                if (!shards)
                {
                    resp.Code = Models.FUTErrorCode.ServerMaintenance; ;
                    resp.Message = "Server offline";
                    return resp;
                }

                HttpClient.AddRequestHeader(NonStandardHttpHeaders.SessionId, "");
                UpdateStatistic("Grabbing UserAccounts...");
                var userAccounts = await BruteforcePlatformAsync().ConfigureAwait(false);
                if (userAccounts == null || userAccounts.userAccountInfo == null || userAccounts.userAccountInfo.personas == null || userAccounts.userAccountInfo.personas.FirstOrDefault() == null)
                {
                    resp.Code = Models.FUTErrorCode.NoUserAccounts;
                    resp.Message = "No userAccounts";
                    return resp;
                }
                var nucleusName = userAccounts.userAccountInfo.personas.FirstOrDefault().personaName;
                var personaID = userAccounts.userAccountInfo.personas.FirstOrDefault().personaId.ToString();
                resp.NucleusName = nucleusName;
                resp.PersonaID = personaID;

                authCode = await GetAuthCodeAsync(bearer);
                authCode = Regex.Match(authCode, "{\"code\":\"(.*?)\"}").Groups[1].Value;
                if (string.IsNullOrEmpty(authCode))
                {
                    resp.Code = Models.FUTErrorCode.NoSessionID;
                    resp.Message = "No authCode";
                    return resp;
                }
                UpdateStatistic("Authing on Utas...");
                var authed = await AuthAsync(authCode, personaID).ConfigureAwait(false);
                if (authed == null || string.IsNullOrEmpty(authed.sid))
                {
                    resp.Code = Models.FUTErrorCode.NoSessionID;
                    resp.Message = "No sid";
                    return resp;
                }
                resp.SessionID = authed.sid;
                HttpClient.AddRequestHeader(NonStandardHttpHeaders.SessionId, authed.sid);
                await Task.Delay(3000).ConfigureAwait(false);
                UpdateStatistic("Grabbing SecurityQuestion...");
                var question = await QuestionAsync().ConfigureAwait(false);
                if (question.Code == FUTErrorCode.CaptchaTriggered || question.Code == FUTErrorCode.CaptchaTriggered2)
                {
                    var futproxy = FUTAccount.GetFUTProxy();
                    var solver = new CaptchaSolver(futproxy);
                    var result = await solver.DoAntiCaptcha();
                    if (result.errorId == 0)
                    {
                        await SolveCaptchaAsync(result.solution.token);
                        question = await QuestionAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        resp.Code = Models.FUTErrorCode.CaptchaException;
                        resp.Message = "Captcha failed! " + result.errorDescription;
                        return resp;
                    }
                }
                if (!string.IsNullOrEmpty(question.token))
                {
                    HttpClient.AddRequestHeader(NonStandardHttpHeaders.PhishingToken, question.token);
                    resp.PhishingToken = question.token;

                    HttpClient.RemoveRequestHeader(NonStandardHttpHeaders.NucleusId);
                    HttpClient.RemoveRequestHeader(NonStandardHttpHeaders.Route);
                    CollectAllCookies();
                    return resp;
                }
                UpdateStatistic("Validating SecurityQuestion...");
                var validate = await ValidateAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(validate?.token))
                {
                    if (validate.code == "461")
                    {
                        resp.Code = Models.FUTErrorCode.WrongSecurityAnswer;
                        resp.Message = "Wrong SecurityAnswer";
                        return resp;
                    }
                    resp.Code = Models.FUTErrorCode.NoPhishingToken;
                    resp.Message = "No phishingtoken";
                    return resp;
                }
                //await QuestionAsync().ConfigureAwait(false);

                UpdateStatistic("Login success...");
                HttpClient.AddRequestHeader(NonStandardHttpHeaders.PhishingToken, validate.token);
                resp.PhishingToken = validate.token;

                HttpClient.RemoveRequestHeader(NonStandardHttpHeaders.Route);
                CollectAllCookies();
                return resp;
            }
            catch (Exception e)
            {
                var resp = new LoginResponse();
                resp.Message = e.ToString();
                resp.Code = FUTErrorCode.BadRequest;
                return resp;
            }
        }

        public void UpdateStatistic(string data)
        {
            var stat = new FUTAccountStatistic();
            stat.EMail = FUTAccount.EMail;
            stat.LastActionData = data;
            stat.LastActionTimestamp = Helper.CreateTimestamp();
            stat.Update();
        }

        private async Task<HttpResponseMessage> AcceptTwoFactorAsync(string uri)
        {
            var stringContent = new StringContent("_eventId=submit");
            stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var loginPage = await HttpClient.PostAsync(uri, stringContent).ConfigureAwait(false);
            return loginPage;
        }

        private async Task<HttpResponseMessage> CreateEMailTwoFactorAsync(string uri)
        {
            var stringContent = new StringContent("tfa_type=&twofactorType=EMAIL&country=0&phoneNumber=&_eventId=submit&appDevice=IPHONE");
            stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var loginPage = await HttpClient.PostAsync(uri, stringContent).ConfigureAwait(false);
            return loginPage;
        }

        private async Task<HttpResponseMessage> GetLoginPageAsync()
        {
            var loginPage = await HttpClient.GetAsync($"https://accounts.ea.com/connect/auth?client_id=FIFA-18-MOBILE-COMPANION&response_type=code&display=web2/login&scope=basic.identity+offline+signin&locale=en_US&machineProfileKey={GetMachineProfileKey()}").ConfigureAwait(false);
            return loginPage;
        }

        private async Task<HttpResponseMessage> LoginAsync(string uri)
        {
            var stringContent = new StringContent("email=" + FUTAccount.EMail + "&password=" + FUTAccount.Password + "&country=DE&phoneNumber=&passwordForPhone=&_rememberMe=on&rememberMe=on&_eventId=submit&gCaptchaResponse=&isPhoneNumberLogin=false&isIncompletePhone=");
            stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var loginPage = await HttpClient.PostAsync(uri, stringContent).ConfigureAwait(false);
            return loginPage;
        }

        private async Task<HttpResponseMessage> FillInTwoFactorAsync(string uri, string code)
        {
            var stringContent = new StringContent("oneTimeCode=" + code + "&_trustThisDevice=on&trustThisDevice=on&_eventId=submit");
            stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var loginPage = await HttpClient.PostAsync(uri, stringContent).ConfigureAwait(false);
            return loginPage;
        }

        private async Task<HttpResponseMessage> SelectGoogleAuthenticatorAsync(string uri)
        {
            var stringContent = new StringContent("codeType=APP&_eventId=submit");
            stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var loginPage = await HttpClient.PostAsync(uri, stringContent).ConfigureAwait(false);
            return loginPage;
        }

        private async Task<HttpResponseMessage> SelectEMailTwoFactorAsync(string uri)
        {
            var stringContent = new StringContent("codeType=EMAIL&_eventId=submit");
            stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var loginPage = await HttpClient.PostAsync(uri, stringContent).ConfigureAwait(false);
            return loginPage;
        }

        private async Task SolveCaptchaAsync(string captchaResult)
        {
            var httpValidationResponse = await HttpClient.PostAsync(FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/captcha/fun/validate", new StringContent($"{{\"funCaptchaToken\":\"{captchaResult}\"}}"));
            string validationResponse = await httpValidationResponse.Content.ReadAsStringAsync();
        }

        private async Task<UserAccountsResponse> BruteforcePlatformAsync()
        {
            var ps4 = await GetUserAccountsAsync(Constants.RoutePS4);
            if (ps4.Item1)
            {
                FUTAccount.FUTPlatform.GameSKU = Constants.GameSKUPS4;
                FUTAccount.FUTPlatform.Route = Constants.RoutePS4;
                FUTAccount.FUTPlatform.PersonaPlatform = Constants.PersonaPlatformPS4;
                HttpClient.AddRequestHeader(NonStandardHttpHeaders.Route, Constants.RoutePS4);
                return ps4.Item2;
            }
            var xb1 = await GetUserAccountsAsync(Constants.RouteXB1);
            if (xb1.Item1)
            {
                FUTAccount.FUTPlatform.GameSKU = Constants.GameSKUXB1;
                FUTAccount.FUTPlatform.Route = Constants.RouteXB1;
                FUTAccount.FUTPlatform.PersonaPlatform = Constants.PersonaPlatformXB1;
                HttpClient.AddRequestHeader(NonStandardHttpHeaders.Route, Constants.RouteXB1);
                return xb1.Item2;
            }
            return null;
        }

        private async Task<Tuple<bool, UserAccountsResponse>> GetUserAccountsAsync(string route)
        {
            HttpClient.AddRequestHeader(NonStandardHttpHeaders.Route, route);
            var loginPage = await HttpClient.GetAsync(route + "/ut/game/fifa18/user/accountinfo?filterConsoleLogin=true&sku=" + FUTAccount.FUTPlatform.SKU + "&returningUserGameYear=2017&_=" + Helper.CreateTimestamp()).ConfigureAwait(false);
            HttpClient.RemoveRequestHeader(NonStandardHttpHeaders.Route);
            var response = await Deserialize<UserAccountsResponse>(loginPage).ConfigureAwait(false);
            if (response.HasError)
            {
                return new Tuple<bool, UserAccountsResponse>(false, null);
            }
            return new Tuple<bool, UserAccountsResponse>(true, response);
        }

        private async Task<PidsMeResponse> GetEaswAsync(string auth)
        {
            HttpClient.AddRequestHeader("Authorization", "Bearer " + auth);
            var loginPage = await HttpClient.GetAsync("https://gateway.ea.com/proxy/identity/pids/me").ConfigureAwait(false);
            HttpClient.RemoveRequestHeader("Authorization");
            return await Deserialize<PidsMeResponse>(loginPage).ConfigureAwait(false);
        }

        private async Task<string> GetBearerAuthCodeAsync(string code)
        {
            var loginPage = await HttpClient.PostAsync("https://accounts.ea.com/connect/token?grant_type=authorization_code&code=" + code + "&client_id=FIFA-18-MOBILE-COMPANION&client_secret=XPlwap1hbbuarJC2qzmgP4XZv1hBqf3D6Rrw4OSa7RqrnkYt60NVdiAI8xpGgRGJoVNUkW6DwzhWyYxx", new StringContent(" ", Encoding.UTF8, "application/x-www-form-urlencoded")).ConfigureAwait(false);
            return await loginPage.Content.ReadAsStringAsync();
        }

        private async Task<string> GetAuthCodeAsync(string accessToken)
        {
            var loginPage = await HttpClient.GetAsync($"https://accounts.ea.com/connect/auth?client_id=FOS-SERVER&redirect_uri=nucleus:rest&response_type=code&access_token={accessToken}&machineProfileKey=A7AB10C1-BD8C-4121-92AE-AB2E9F2EB6C4").ConfigureAwait(false);
            return await loginPage.Content.ReadAsStringAsync();
        }

        private async Task<AuthResponse> AuthAsync(string authCode, string personaId)
        {
            var stringContent = new StringContent("{\"isReadOnly\":false,\"sku\":\"" + FUTAccount.FUTPlatform.SKU + "\",\"clientVersion\":27,\"locale\":\"de-DE\",\"method\":\"authcode\",\"priorityLevel\":4,\"identification\":{\"authCode\":\"" + authCode + "\",\"redirectUrl\":\"nucleus:rest\"},\"nucleusPersonaId\":" + personaId + ",\"gameSku\":\"" + FUTAccount.FUTPlatform.GameSKU + "\"}");
            stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var loginPage = await HttpClient.PostAsync(FUTAccount.FUTPlatform.Route + "/ut/auth?sku_a=FFT18&" + Helper.CreateTimestamp(), stringContent).ConfigureAwait(false);
            return await Deserialize<AuthResponse>(loginPage).ConfigureAwait(false);
        }

        private async Task<PhishingResponse> QuestionAsync()
        {
            var loginPage = await HttpClient.GetAsync(FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/phishing/question?_=" + Helper.CreateTimestamp()).ConfigureAwait(false);
            return await Deserialize<PhishingResponse>(loginPage).ConfigureAwait(false);
        }

        private async Task<ValidateResponse> ValidateAsync()
        {
            var answerHashed = FUTHasher.Hash(FUTAccount.SecurityAnswer);
            var stringContent = new StringContent(answerHashed);
            stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var loginPage = await HttpClient.PostAsync(FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/phishing/validate?answer=" + answerHashed, stringContent).ConfigureAwait(false);
            return await Deserialize<ValidateResponse>(loginPage).ConfigureAwait(false);
        }

        private async Task<bool> IsServerOnline()
        {
            var loginPage = await HttpClient.GetAsync("https://utas.mob.v4.fut.ea.com/ut/shards/v2?_=" + Helper.CreateTimestamp()).ConfigureAwait(false);
            HttpClient.RemoveRequestHeader(NonStandardHttpHeaders.Route);
            return loginPage.StatusCode == HttpStatusCode.OK;
        }

        private async Task<string> WaitForTwoFactorCode(DateTime codeSent, string resendUri)
        {
            var returnCode = "";
            var starting = DateTime.Now;
            var startingSending = DateTime.Now;
            while (returnCode == "")
            {
                if (DateTime.Now.Subtract(starting).TotalMinutes >= 5)
                {
                    returnCode = "000000";
                    break;
                }
                if (DateTime.Now.Subtract(startingSending).TotalMinutes >= 1.5)
                {
                    var resend = await HttpClient.GetAsync(resendUri).ConfigureAwait(false);
                    var resendString = resend.Content.ReadAsStringAsync().ConfigureAwait(false);
                    startingSending = DateTime.Now;
                }
                var mail = await _mailClient.GetTwoFactorCode(codeSent).ConfigureAwait(false);
                returnCode = mail;
                await Task.Delay(5000).ConfigureAwait(false);
            }
            return returnCode;
        }

        private void CollectAllCookies()
        {
            CookieManager.SaveCookieContainer(FUTAccount, HttpClient.ClientHandler.CookieContainer, false);
        }

        private string GetMachineProfileKey()
        {
            var guid = Guid.NewGuid().ToString().ToUpper();
            return guid;
        }
    }
}

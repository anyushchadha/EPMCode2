// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginController.cs" company="Eyefinity, Inc.">
//    Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//  Defines the LoginController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Http;
    using System.Web.Security;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.Enterprise.Business.Payment;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.ViewModel;

    using Anonymous = Eyefinity.PracticeManagement.Common.Api.AllowAnonymousAttribute;

    /// <summary>The login controller.</summary>
    [NoCache]
    [ValidateHttpAntiForgeryToken]
    public class LoginController : ApiController
    {
        #region Fields
        /// <summary>
        /// The default session cookie name.
        /// </summary>
        private const string DefaultSessionCookieName = "ASP.NET_SessionId";

        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("Eyefinity.PracticeManagement.Controllers.Api.LoginController");

        /// <summary>
        /// The it 2 business.
        /// </summary>
        private readonly DailyClosingIt2Manager it2Business;

        private readonly OfficeIt2Manager officeIt2Manager;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoginController" /> class.
        /// </summary>
        public LoginController()
        {
            try
            {
                this.it2Business = new DailyClosingIt2Manager();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            this.officeIt2Manager = new OfficeIt2Manager();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>The logout.</summary>
        [HttpPost]
        [Authorize]
        public void Logout()
        {
            try
            {
                const string AntiXsrfTokenKey = "__AntiXsrfToken";

                if (HttpContext.Current.Session != null)
                {
                    HttpContext.Current.Session.Abandon(); //// nuke session state
                    HttpContext.Current.Session.RemoveAll(); //// nuke session state
                }
                
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(GetSessionCookieName()) { Expires = DateTime.Now.AddDays(-1) });
                FormsAuthentication.SignOut();

                if (this.User.Identity.IsAuthenticated)
                {
                    HttpContext.Current.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                    HttpContext.Current.Response.Cache.SetValidUntilExpires(false);
                    HttpContext.Current.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                    HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    HttpContext.Current.Response.Cache.SetNoStore();
                }

                if (HttpContext.Current.Request.Cookies[AntiXsrfTokenKey] != null)
                {
                    var csrfCookie = new HttpCookie(AntiXsrfTokenKey) { Expires = DateTime.Now.AddDays(-1d) };
                    HttpContext.Current.Response.Cookies.Add(csrfCookie);
                }

                HttpContext.Current.Response.Cookies.Clear();
            }
            catch (HttpException httpException)
            {
                Logger.Error("Logout HTTP Exception occured", httpException);
            }
        }

        /// <summary>The logout.</summary>
        [HttpGet]
        [Authorize]
        public void KeepAlive()
        {
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket ticketOld = null;

            if (authCookie != null)
            {
                ticketOld = FormsAuthentication.Decrypt(authCookie.Value);
            }

            FormsAuthenticationTicket ticketNew = null;
            if (ticketOld == null)
            {
                return;
            }

            if (FormsAuthentication.SlidingExpiration)
            {
                ticketNew = FormsAuthentication.RenewTicketIfOld(ticketOld);
            }

            if (ticketNew == null)
            {
                return;
            }

            var hash = FormsAuthentication.Encrypt(ticketNew);
            if (ticketNew.IsPersistent)
            {
                authCookie.Expires = ticketNew.Expiration;
            }

            var authHelper = new AuthorizationTicketHelper();
            var token = authHelper.GetToken();
            var refreshToken = authHelper.GetRefreshToken();
            if (!string.IsNullOrEmpty(token))
            {
                var newhash = this.GetNewHashWithUpdatedToken(refreshToken, ticketNew, token);

                authCookie.Value = newhash;
            }
            else
            {
                authCookie.Value = hash;
            }

            authCookie.HttpOnly = true;
            authCookie.Secure = FormsAuthentication.RequireSSL;
            authCookie.Domain = FormsAuthentication.CookieDomain;
            authCookie.Path = FormsAuthentication.FormsCookiePath;

            HttpContext.Current.Response.Cookies.Add(authCookie);
        }

	    /// <summary>
	    /// The Reset password.
	    /// </summary>
	    /// <param name="data">
	    /// The data.
	    /// </param>
	    /// <returns>
	    /// The <see cref="LoginVm"/>.
	    /// </returns>
	    [HttpPost]
        [Anonymous]
        public LoginVm ResetPassword([FromBody] LoginVm data)
        {
            try
            {
                if (data == null)
                {
                    return null;
                }

                data.LoginName = data.LoginName.Trim();
                data.PracticeLocationId = data.PracticeLocationId.Trim();
                data.CompanyZipCode = data.CompanyZipCode.Trim();
                data.CompanyPhoneNumber = data.CompanyPhoneNumber.Trim();
                var security = new Security();

                try
                {
                    data.CompanyId = security.GetCompanyId(data.PracticeLocationId);
                }
                catch (Exception badOfficeException)
                {
                    Logger.Error($"ResetPassword: Unable to retrieve company information for office {data.PracticeLocationId}.", badOfficeException);
                    return data;
                }

                var loginName = string.Concat(data.LoginName.DecryptStringAes(), security.GetUserNamePlaceHolderValue(data.CompanyId));
                switch (data.Action)
                {
                    case "StepOne":
                        security.VerifyUser(
                            data.CompanyId,
                            loginName,
                            data.CompanyZipCode,
                            data.CompanyPhoneNumber,
                            string.Empty,
                            ref data);
                        break;
                    case "StepTwo":
                        security.VerifyUser(
                            data.CompanyId,
                            loginName,
                            data.CompanyZipCode,
                            data.CompanyPhoneNumber,
                            data.SecurityAnswer.DecryptStringAes(),
                            ref data);
                        break;
                    case "StepThree":
                        if (security.VerifyUser(
                            data.CompanyId,
                            loginName,
                            data.CompanyZipCode,
                            data.CompanyPhoneNumber,
                            data.SecurityAnswer.DecryptStringAes(),
                            ref data))
                        {
                            security.ResetPassword(loginName, data.Password.DecryptStringAes(), ref data);
                        }

                        break;
                    default:
                        data.PasswordCheckResult = "None";
                        break;
                }
            }
            catch (Exception e)
            {
	            Logger.Error(
		            data != null
			            ? $"ResetPassword: Location Id - {data.PracticeLocationId} Login Name - {data.LoginName}"
			            : $"ResetPassword: Exception message - {e.Message} ",
		            e);
            }

            return data;
        }

        /// <summary>
        /// The verify password.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="LoginVm"/>.
        /// </returns>
        [HttpPost]
        [Anonymous]
        public LoginVm ProcessPassword([FromBody] LoginVm data)
        {
            try
            {
                data.LoginName = data.LoginName.Trim();
                data.PracticeLocationId = data.PracticeLocationId.Trim();
                var security = new Security();
                try
                {
                    data.CompanyId = security.GetCompanyId(data.PracticeLocationId);
                }
                catch (Exception badOfficeException)
                {
                    Logger.Error($"ProcessPassword: Unable to retrieve company information for office {data.PracticeLocationId}.", badOfficeException);
                    return data;
                }

                if (data.CompanyId == string.Empty)
                {
                    return data;
                }

                var loginName = string.Concat(data.LoginName.DecryptStringAes(), security.GetUserNamePlaceHolderValue(data.CompanyId));
                var sessionTimeout = 0;

                switch (data.Action)
                {
                    case "Verify":
                        var id = security.VerifyPassword(loginName, data.Password.DecryptStringAes(), ref data, ref sessionTimeout);
                   
                        if (data.PasswordCheckResult == "Verified")
                        {
                            var token = string.Empty;
                            var refreshToken = string.Empty;
                            var overrideOAuthCreds = Convert.ToBoolean(ConfigurationManager.AppSettings["OverrideOAuthCreds"]);
                            var enableOAuth = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableOAuth"]);

                            if (enableOAuth)
                            {
                                Dictionary<string, string> result;
                                if (overrideOAuthCreds)
                                {
                                    var login = ConfigurationManager.AppSettings["OAuthUser"];
                                    var password = ConfigurationManager.AppSettings["OAuthPassword"];
                                    result = this.GetOAuthToken(login, password);
                                }
                                else
                                {
                                    result = this.GetOAuthToken(loginName, data.Password.DecryptStringAes());
                                }

                                if (result != null)
                                {
                                    token = result["access_token"];
                                    refreshToken = result["refresh_token"];
                                }
                            }

                            //// Use shorter names here as we want this to be light weight & try to keep it upper case to be consistent
                            //// 1. PID Practice Location ID 2. LN Login Name 3. OID Office Id 4. TO Session Timeout
                            //// 5. DCR Daily Closing Role 6. EHR Is EHR Enabled 7. TKN OAuth2 Token 8. RTKN OAuth2 Refresh Token 9. CID Company ID
                            var userData = string.Format("PID:{0}|LN:{1}|OID:{2}|TO:{3}|DCR:{4}|EHR:{5}|TKN:{6}|RTKN:{7}|CID:{8}", data.PracticeLocationId.ToUpper(), CultureInfo.CurrentCulture.TextInfo.ToTitleCase(data.LoginName.DecryptStringAes()), data.OfficeId, sessionTimeout, data.DayCloseInRole, data.IsEhrEnabled, token, refreshToken, data.CompanyId);
                            FormsAuthentication.SetAuthCookie(loginName, true);
                            var ticket = new FormsAuthenticationTicket(
                                1,
                                id,
                                DateTime.Now,
                                DateTime.Now.AddMinutes(sessionTimeout),
                                true,
                                userData,
                                FormsAuthentication.FormsCookiePath);
                            var encTicket = FormsAuthentication.Encrypt(ticket);
                            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket)
                                             {
                                                 HttpOnly = true,
                                                 Domain = FormsAuthentication.CookieDomain,
                                                 Secure = FormsAuthentication.RequireSSL,
                                                 Path = FormsAuthentication.FormsCookiePath,
                                             };
                            HttpContext.Current.Response.Cookies.Add(cookie);
                            data.RedirectUrl = this.GetRedirectUrl(data);
                        }

                        break;
                    case "Reset":
                        if (data.OldPassword.DecryptStringAes() != data.Password.DecryptStringAes())
                        {
                            security.VerifyPassword(loginName, data.OldPassword.DecryptStringAes(), ref data, ref sessionTimeout);
                            if (!data.PasswordCheckResult.Equals("NoMatch"))
                            {
                                security.ResetPassword(loginName, data.Password.DecryptStringAes(), ref data);
                            }
                        }
                        else
                        {
                            data.PasswordCheckResult = "NoMatch";
                        }

                        break;
                    case "GetPolicy":
                        security.GetPasswordPolicy(ref data);
                        break;
                    case "ResetPassword":
                        security.VerifyPassword(loginName, data.OldPassword.DecryptStringAes(), ref data, ref sessionTimeout);
                        if (!data.PasswordCheckResult.Equals("NoMatch"))
                        {
                            security.ResetPassword(loginName, data.Password.DecryptStringAes(), ref data);
                        }

                        break;
                    case "ChangeSecurityQuestion":
                        security.ChangeSecurityQuestion(loginName, ref data);
                        var redirect = data.RedirectUrl;
                        var redirectUrl = this.GetRedirectUrl(data);
                        data.RedirectUrl = redirectUrl == redirect ? redirectUrl : redirect + redirectUrl;
                        break;
                    case "GetSecurityQuestion":
                        security.GetSecurityQuestion(loginName, ref data);
                        break;
                    default:
                        data.PasswordCheckResult = "None";
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ProcessPassword: Location Id - {data?.PracticeLocationId} Login Name - {data?.LoginName}", e);
            }

            return data;
        }

        [HttpGet]
        [Anonymous]
        public bool IsAutomationOn(bool isBrowserIe)
        {
            if (!isBrowserIe)
            {
                return true;
            }

            var result = string.Empty;
            try
            {
                result = System.Configuration.ConfigurationManager.AppSettings["TestAutomation"];
            }
            catch (Exception e)
            {
                Logger.Error("IsAutomationOn: " + e);
            }

            return result.Contains("on");
        }

        [HttpPost]
        [Anonymous]
        public String GetCmsContent()
        {
            var content = String.Empty;
            try
            {
                // get the url from properties, then connect and retrieve data
                var url = ConfigurationManager.AppSettings["MagnoliaURL_Login"];
                content = this.GetCmsMessaging(url);
            }
            catch (Exception e)
            {
                Logger.Error("LoginController:GetCmsContent(): " + e);
            }

            return content;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Returns a String containing HTML data from a CMS system
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetCmsMessaging(string url)
        {
            var result = String.Empty;
            if (!String.IsNullOrEmpty(url))
            {
                try
                {
                    // connect and get the data
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/html"));
                    var response = client.GetAsync(new Uri(string.Format(url))).Result;

                    // read and parse the object if we were successful
                    if (response.IsSuccessStatusCode)
                    {
                        result = response.Content.ReadAsStringAsync().Result;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("LoginController:GetCmsMessaging(): " + e);
                }
            }

            return result;
        }

        /// <summary>
        /// Get OAuth 2 Token
        /// </summary>
        /// <param name="refreshToken">Refresh Token</param>
        /// <returns>token</returns>
        public Dictionary<string, string> RefreshToken(string refreshToken)
        {
            var oauthUrl = ConfigurationManager.AppSettings["OAuthUrl"];
            var base64ClientIdPlusSecret = ConfigurationManager.AppSettings["Base64ClientIdPlusSecret"];

            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, oauthUrl);
                    client.BaseAddress = new Uri(oauthUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var keyValues = new List<KeyValuePair<string, string>>
										{
											new KeyValuePair<string, string>("grant_type", "refresh_token"),
											new KeyValuePair<string, string>("refresh_token", refreshToken)
										};

                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64ClientIdPlusSecret);
                    request.Headers.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618; InfoPath.2; OfficeLiveConnector.1.3; OfficeLivePatch.0.0)");
                    request.Content = new FormUrlEncodedContent(keyValues);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    var response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return new JsonParser().Parse<Dictionary<string, string>>(response.Content.ReadAsStreamAsync().Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to refresh Token. Exception - ", ex);
            }

            return null;
        }

        /// <summary>
        /// Revise Authentication Ticket with New Values
        /// </summary>
        /// <param name="refreshToken">Refresh OAuth Token Value</param>
        /// <param name="ticketNew">New auth Ticket </param>
        /// <param name="token">OAuth Token</param>
        /// <returns></returns>
        public string GetNewHashWithUpdatedToken(string refreshToken, FormsAuthenticationTicket ticketNew, string token)
        {
            var result = this.RefreshToken(refreshToken);
            var newToken = string.Empty;
            var newRefreshToken = string.Empty;

            if (result != null)
            {
                newToken = result["access_token"];
                newRefreshToken = result["refresh_token"];
            }

            var updatedUserData = ticketNew.UserData.Replace(
                $"|TKN:{token}|RTKN:{refreshToken}",
                $"|TKN:{newToken}|RTKN:{newRefreshToken}");

            var tempTicket = new FormsAuthenticationTicket(
                1,
                ticketNew.Name,
                DateTime.Now,
                ticketNew.Expiration,
                true,
                updatedUserData,
                FormsAuthentication.FormsCookiePath);
            var newhash = FormsAuthentication.Encrypt(tempTicket);
            return newhash;
        }

        /// <summary>
        /// The get session cookie name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GetSessionCookieName()
        {
            var sessionStateSection = ConfigurationManager.GetSection("system.web/sessionState") as SessionStateSection;
            return string.IsNullOrEmpty(sessionStateSection?.CookieName) ? DefaultSessionCookieName : sessionStateSection.CookieName;
        }

        /// <summary>
        /// The get redirect url.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetRedirectUrl(LoginVm data)
        {
            const string DefaultUrl = "Home/Index";
            try
            {
                var result = this.it2Business.GetPreviousUnclosedBusinessDaysInfo(data.PracticeLocationId);
                var redirectUrl = data.RedirectUrl.ToLower().Contains("logout") ? DefaultUrl : data.RedirectUrl;

                if (string.IsNullOrEmpty(data.SecurityQuestion))
                {
                    redirectUrl = "Common/SecurityQuestion?redirect=1";
                }
                else if (result.UnclosedDaysCount >= 1 && data.DayCloseInRole)
                {
                    redirectUrl = "Common/DailyClosing";
                }

                var redirecturi = new Uri(redirectUrl.ToString(CultureInfo.InvariantCulture), UriKind.RelativeOrAbsolute);

                if (redirecturi.IsAbsoluteUri)
                {
                    if (!redirecturi.Host.Equals(this.Request.RequestUri.Host))
                    {
                        return VirtualPathUtility.ToAbsolute(DefaultUrl);
                    }
                }

                if (redirectUrl.Contains("SecurityQuestion"))
                {
                    return VirtualPathUtility.ToAbsolute("~/" + redirectUrl);
                }

                if (redirectUrl.Contains("Common/DailyClosing"))
                {
                    return VirtualPathUtility.ToAbsolute("~/" + redirectUrl);
                }
                else
                {
                    return VirtualPathUtility.ToAbsolute(redirectUrl);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return VirtualPathUtility.ToAbsolute(DefaultUrl);
            }
        }

        /// <summary>
        /// Get OAuth 2 Token
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="password">login</param>
        /// <returns>token</returns>
        private Dictionary<string, string> GetOAuthToken(string userName, string password)
        {
            try
            {
                var oauthUrl = ConfigurationManager.AppSettings["OAuthUrl"];
                var clientId = ConfigurationManager.AppSettings["ClientId"];
                var base64ClientIdPlusSecret = ConfigurationManager.AppSettings["Base64ClientIdPlusSecret"];

                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, oauthUrl);
                    client.BaseAddress = new Uri(oauthUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var keyValues = new List<KeyValuePair<string, string>>
										{
											new KeyValuePair<string, string>("grant_type", "password"),
											new KeyValuePair<string, string>("username", userName),
											new KeyValuePair<string, string>("password", password),
											new KeyValuePair<string, string>("client_id", clientId)
										};

                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64ClientIdPlusSecret);
                    request.Headers.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618; InfoPath.2; OfficeLiveConnector.1.3; OfficeLivePatch.0.0)");
                    request.Content = new FormUrlEncodedContent(keyValues);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    var response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return new JsonParser().Parse<Dictionary<string, string>>(response.Content.ReadAsStreamAsync().Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to retrieve Token. Exception - ", ex);
            }

            return null;
        }

        #endregion
    }
}
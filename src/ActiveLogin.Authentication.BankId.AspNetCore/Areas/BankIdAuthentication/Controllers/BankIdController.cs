﻿using System;
using ActiveLogin.Authentication.BankId.Api.UserMessage;
using ActiveLogin.Authentication.BankId.AspNetCore.Areas.BankIdAuthentication.Models;
using ActiveLogin.Authentication.BankId.AspNetCore.DataProtection;
using ActiveLogin.Authentication.BankId.AspNetCore.Models;
using ActiveLogin.Authentication.BankId.AspNetCore.UserMessage;
using ActiveLogin.Authentication.Common.Serialization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace ActiveLogin.Authentication.BankId.AspNetCore.Areas.BankIdAuthentication.Controllers
{
    [Area(BankIdAuthenticationConstants.AreaName)]
    [Route("/[area]/[action]")]
    public class BankIdController : Controller
    {
        private readonly IAntiforgery _antiforgery;
        private readonly IBankIdUserMessageLocalizer _bankIdUserMessageLocalizer;
        private readonly IBankIdLoginOptionsProtector _loginOptionsProtector;

        public BankIdController(
            IAntiforgery antiforgery,
            IBankIdUserMessageLocalizer bankIdUserMessageLocalizer,
            IBankIdLoginOptionsProtector loginOptionsProtector)
        {
            _antiforgery = antiforgery;
            _bankIdUserMessageLocalizer = bankIdUserMessageLocalizer;
            _loginOptionsProtector = loginOptionsProtector;
        }
    
        public ActionResult Login(string returnUrl, string loginOptions, string orderRef)
        {
            if (!Url.IsLocalUrl(returnUrl))
            {
                throw new Exception(BankIdAuthenticationConstants.InvalidReturnUrlErrorMessage);
            }

            var unprotectedLoginOptions = _loginOptionsProtector.Unprotect(loginOptions);
            var antiforgeryTokens = _antiforgery.GetAndStoreTokens(HttpContext);

            var viewModel = GetLoginViewModel(returnUrl, loginOptions, unprotectedLoginOptions, antiforgeryTokens, orderRef);
            return View(viewModel);
        }
        
        private BankIdLoginViewModel GetLoginViewModel(string returnUrl, string loginOptions, BankIdLoginOptions unprotectedLoginOptions, AntiforgeryTokenSet antiforgeryTokens, string orderRef = null)
        {
            var loginScriptOptions = new BankIdLoginScriptOptions
            {
                RefreshIntervalMs = BankIdAuthenticationDefaults.StatusRefreshIntervalMs,

                InitialStatusMessage = _bankIdUserMessageLocalizer.GetLocalizedString(MessageShortName.RFA13),
                UnknownErrorMessage = _bankIdUserMessageLocalizer.GetLocalizedString(MessageShortName.RFA22),

                BankIdInitializeApiUrl = Url.Action(nameof(BankIdApiController.InitializeAsync), "BankIdApi"),
                BankIdStatusApiUrl = Url.Action(nameof(BankIdApiController.StatusAsync), "BankIdApi")
            };

            return new BankIdLoginViewModel
            {
                ReturnUrl = returnUrl,

                AutoLogin = unprotectedLoginOptions.IsAutoLogin(),
                PersonalIdentityNumber = unprotectedLoginOptions.PersonalIdentityNumber?.To12DigitString() ?? string.Empty,
                OrderRef = orderRef,

                LoginOptions = loginOptions,
                UnprotectedLoginOptions = unprotectedLoginOptions,

                AntiXsrfRequestToken = antiforgeryTokens.RequestToken,
                LoginScriptOptions = loginScriptOptions,
                LoginScriptOptionsJson = SystemRuntimeJsonSerializer.Serialize(loginScriptOptions)
            };
        }
    }
}
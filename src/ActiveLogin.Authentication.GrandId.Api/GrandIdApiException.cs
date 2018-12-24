﻿using System;
using System.Net.Http;
using ActiveLogin.Authentication.GrandId.Api.Models;

namespace ActiveLogin.Authentication.GrandId.Api
{
    /// <summary>
    /// Exception that wraps any error returned by the GrandID API.
    /// </summary>
    public class GrandIdApiException : HttpRequestException
    {
        internal GrandIdApiException(ErrorObject error)
            : this(error.Code, error.Message)
        { }

        private GrandIdApiException(string errorCodeString, string details)
          : base($"{errorCodeString}: {details}", null)
        {
            ErrorCode = Enum.TryParse<ErrorCode>(errorCodeString, true, out var errorCode) ? errorCode : ErrorCode.Unknown;
            Details = details;
        }

        /// <summary>
        /// The category of error.
        /// </summary>
        public ErrorCode ErrorCode { get; }

        /// <summary>
        /// Details about the error.
        /// </summary>
        public string Details { get; }
    }
}

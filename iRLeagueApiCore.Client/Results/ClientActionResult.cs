﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Results
{
    public struct ClientActionResult<T>
    {
        public ClientActionResult(T content) : this(true, "", content, HttpStatusCode.OK)
        { }

        public ClientActionResult(bool success, string message, T content, HttpStatusCode httpStatusCode, IEnumerable<object> errors = null)
        {
            Success = success;
            Message = message;
            Content = content;
            HttpStatusCode = httpStatusCode;
            Errors = errors ?? new object[0];
        }

        public bool Success { get; }
        public string Message { get; }
        public T Content { get; }
        public HttpStatusCode HttpStatusCode { get; }
        public IEnumerable<object> Errors { get; }
    }

    public struct NoContent
    {
        public static NoContent Value => new NoContent();
    }
}

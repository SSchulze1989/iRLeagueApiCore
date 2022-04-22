using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Results
{
    public struct ClientActionResult<T>
    {
        public ClientActionResult(T content, HttpStatusCode httpStatusCode) : this(true, "Success", "", content, httpStatusCode)
        { }

        public ClientActionResult(bool success, string status, string message, T content, HttpStatusCode httpStatusCode, IEnumerable<object> errors = null)
        {
            Success = success;
            Status = status;
            Message = message;
            Content = content;
            HttpStatusCode = httpStatusCode;
            Errors = errors ?? new object[0];
        }

        public bool Success { get; }
        public string Status { get; }
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

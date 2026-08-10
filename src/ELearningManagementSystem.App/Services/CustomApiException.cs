using System;
using System.Net;

namespace ELearningManagementSystem.App.Services;

public class CustomApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public CustomApiException(HttpStatusCode statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}

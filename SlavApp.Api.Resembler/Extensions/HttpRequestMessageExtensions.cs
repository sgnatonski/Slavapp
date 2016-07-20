using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Http.Controllers;

namespace SlavApp.Api.Resembler.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static IPAddress GetClientIpAddress(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return IPAddress.Parse(((HttpContextBase)request.Properties["MS_HttpContext"]).Request.UserHostAddress);
            }

            if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                return IPAddress.Parse(((RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name]).Address);
            }

            return IPAddress.None;
        }
    }
}
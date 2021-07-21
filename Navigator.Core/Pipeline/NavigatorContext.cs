using System;
using System.Collections.Generic;
using System.Security.Claims;
using Immaterium;
using Navigator.Core;

namespace Navigator.Pipeline
{
    public class NavigatorContextRequest
    {
        public ImmateriumMessage RawMessage { get; internal set; }
        public ImmateriumHeaderCollection Headers => RawMessage.Headers;
        public object Body => RawMessage.Body;
    }

    public class NavigatorContextResponse
    {
        public ImmateriumMessage RawMessage { get; set; }
        public ImmateriumHeaderCollection Headers => RawMessage.Headers;
        public object Body
        {
            get => RawMessage.Body;
            set => RawMessage.Body = value;
        }
    }

    public class NavigatorContext
    {
        public NavigatorContextRequest Request { get; }

        public NavigatorContextResponse Response { get; }

        public IServiceProvider ServiceProvider { get; internal set; }

        public bool ResponseRequired { get; }

        public ClaimsPrincipal User { get; set; }

        public List<ClaimsPrincipal> AuxUsers { get; private set; } = new List<ClaimsPrincipal>();

        public bool IsEvent { get; internal set; }


        internal ControllerAction ControllerAction { get; set; }
        internal object[] RequestParameters { get; set; }
        internal object ResponseObject { get; set; }

        public NavigatorContext(ImmateriumMessage rawMessage, object objectBody)
        {
            Request = new NavigatorContextRequest()
            {
                RawMessage = rawMessage
            };

            if (rawMessage.Type != ImmateriumMessageType.Request) return;

            ResponseRequired = true;
            Response = new NavigatorContextResponse { RawMessage = rawMessage.CreateReply() };
        }

    }
}

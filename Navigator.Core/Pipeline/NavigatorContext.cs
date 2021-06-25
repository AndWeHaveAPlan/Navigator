using System;
using System.Collections.Generic;
using System.Security.Claims;
using Immaterium;
using Navigator.Core;

namespace Navigator.Pipeline
{
    public class NavigatorContext
    {
        public ImmateriumMessage Request { get; }

        public ImmateriumMessage Response { get; }

        public IServiceProvider ServiceProvider { get; internal set; }

        public bool ResponseRequired { get; }

        public ClaimsPrincipal User { get; set; }

        public List<ClaimsPrincipal> AuxUsers { get; private set; } = new List<ClaimsPrincipal>();

        public bool IsEvent { get; internal set; }


        internal ControllerAction ControllerAction { get; set; }
        internal object[] RequestParameters { get; set; }
        internal object ResponseObject { get; set; }

        public NavigatorContext(ImmateriumMessage request)
        {
            Request = request;

            if (request.Type == ImmateriumMessageType.StrictRequest)
            {
                ResponseRequired = true;
                Response = request.CreateResponse();
            }
        }

    }
}

#region Disclaimer/Info

///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at Google Code at http://code.google.com/p/subtext/
// The development mailing list is at subtext@googlegroups.com 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////

#endregion

using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Subtext.Framework.Providers;
using Subtext.Framework.Routing;
using Subtext.Infrastructure;

namespace Subtext.Framework
{
    public interface ISubtextContext
    {
        Blog Blog { get; }
        ObjectRepository Repository { get; }
        RequestContext RequestContext { get; }
        HttpContextBase HttpContext { get; }
        BlogUrlHelper UrlHelper { get; }
        IPrincipal User { get; }
        ICache Cache { get; }
        IDependencyResolver ServiceLocator { get; }
    }
}
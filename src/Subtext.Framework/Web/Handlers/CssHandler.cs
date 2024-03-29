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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using Subtext.Extensibility.Web;
using Subtext.Framework.UI.Skinning;

namespace Subtext.Framework.Web.Handlers
{
    public class CssHandler : BaseHttpHandler
    {
        private static readonly StyleSheetElementCollectionRenderer StyleRenderer =
            new StyleSheetElementCollectionRenderer(new SkinEngine());

        protected new bool IsReusable
        {
            get { return false; }
        }

        protected override bool RequiresAuthentication
        {
            get { return false; }
        }

        protected override string ContentMimeType
        {
            get { return "text/css"; }
        }

        protected override void HandleRequest(HttpContext context)
        {
            context.Response.ContentEncoding = Encoding.UTF8;

            string skinName = context.Request.Params["name"];
            string skinMedia = context.Request.Params["media"];
            string skinTitle = context.Request.Params["title"];
            string skinConditional = context.Request.Params["conditional"];

            var styles =
                (List<StyleDefinition>)
                StyleRenderer.GetStylesToBeMerged(skinName, skinMedia, skinTitle, skinConditional);

            //Append all styles into one file

            context.Response.Write(string.Format("/*{0}", Environment.NewLine));
            foreach(StyleDefinition style in styles)
            {
                context.Response.Write(style + Environment.NewLine);
            }
            context.Response.Write(string.Format("*/{0}", Environment.NewLine));

            foreach(StyleDefinition style in styles)
            {
                context.Response.Write(Environment.NewLine + "/* " + style + " */" + Environment.NewLine);
                string path = context.Server.MapPath(style.Href);
                if(File.Exists(path))
                {
                    string cssFile = File.ReadAllText(path);
                    // Normalize path.
                    cssFile = cssFile.Replace("url(../images", "url(../../images");
                    cssFile = cssFile.Replace("url(../Images", "url(../../Images");
                    if(!String.IsNullOrEmpty(style.Media) && styles.Count > 1)
                    {
                        context.Response.Write("@media " + style.Media + "{\r\n");
                        context.Response.Write(cssFile);
                        context.Response.Write("\r\n}");
                    }
                    else
                    {
                        context.Response.Write(cssFile);
                    }
                }
                else
                {
                    context.Response.Write(Environment.NewLine + "/* CSS file at " + path +
                                           " doesn't exist so cannot be included in the merged CSS file. */" +
                                           Environment.NewLine);
                }
            }

            SetHeaders(styles, context);
        }


        private static void SetHeaders(IEnumerable<StyleDefinition> styles, HttpContext context)
        {
            foreach(StyleDefinition style in styles)
            {
                context.Response.AddFileDependency(context.Server.MapPath(style.Href));
            }

            context.Response.Cache.VaryByParams["name"] = true;
            context.Response.Cache.VaryByParams["media"] = true;
            context.Response.Cache.VaryByParams["title"] = true;
            context.Response.Cache.VaryByParams["conditional"] = true;

            context.Response.Cache.SetValidUntilExpires(true);
            // Client-side caching
            context.Response.Cache.SetLastModifiedFromFileDependencies();
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
        }

        protected override void SetResponseCachePolicy(HttpCachePolicy cache)
        {
            return;
        }

        protected override bool ValidateParameters(HttpContext context)
        {
            string skinName = context.Request.Params["name"];
            if(String.IsNullOrEmpty(skinName))
            {
                return false;
            }
            return true;
        }
    }
}
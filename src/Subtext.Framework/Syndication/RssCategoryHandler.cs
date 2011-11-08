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
using System.Linq;
using Subtext.Framework.Components;
using Subtext.Framework.Data;
using Subtext.Framework.Routing;

namespace Subtext.Framework.Syndication
{
    public class RssCategoryHandler : EntryCollectionHandler<Entry>
    {
        protected LinkCategory Category;
        ICollection<Entry> _posts;

        public RssCategoryHandler(ISubtextContext subtextContext)
            : base(subtextContext)
        {
        }

        protected override BaseSyndicationWriter SyndicationWriter
        {
            get
            {
                return new CategoryWriter(new StringWriter(), _posts, Category,
                                          Url.CategoryUrl(Category).ToFullyQualifiedUrl(Blog), SubtextContext);
            }
        }

        /// <summary>
        /// Returns true if the feed is the main feed.  False for category feeds and comment feeds.
        /// </summary>
        protected override bool IsMainfeed
        {
            get { return false; }
        }

        protected override ICollection<Entry> GetFeedEntries()
        {
            if (Category == null)
            {
                Category = Cacher.SingleCategory(SubtextContext);
            }

            if (Category != null && _posts == null)
            {
                _posts = Cacher.GetEntriesByCategory(10, Category.Id, SubtextContext);
            }

            return _posts;
        }

        /// <summary>
        /// Builds the feed using delta encoding if it's true.
        /// </summary>
        /// <returns></returns>
        protected override CachedFeed BuildFeed()
        {
            CachedFeed feed = null;

            _posts = GetFeedEntries();

            if (_posts != null && _posts.Count > 0)
            {
                feed = new CachedFeed();
                var cw = new CategoryWriter(new StringWriter(), _posts, Category,
                                            Url.CategoryUrl(Category).ToFullyQualifiedUrl(Blog), SubtextContext);
                feed.LastModifiedUtc = _posts.First().DateCreatedUtc;
                feed.Xml = cw.Xml;
            }
            return feed;
        }

        /// <summary>
        /// Gets the item created date.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected override DateTime GetItemCreatedDateUtc(Entry item)
        {
            return item.DateCreatedUtc;
        }
    }
}
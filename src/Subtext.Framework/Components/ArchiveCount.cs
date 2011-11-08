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

namespace Subtext.Framework.Components
{
    /// <summary>
    /// DTO style class for representing a count of archived posts.
    /// </summary>
    [Serializable]
    public class ArchiveCount
    {
        // This is the date in the Blog's timezone, not UTC
        public DateTime Date { get; set; }

        public int Count { get; set; }
    }
}
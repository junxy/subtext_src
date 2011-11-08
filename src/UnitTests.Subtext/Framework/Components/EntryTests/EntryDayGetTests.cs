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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using MbUnit.Framework;
using Subtext.Extensibility;
using Subtext.Framework;
using Subtext.Framework.Components;
using Subtext.Framework.Configuration;
using Subtext.Framework.Data;
using Subtext.Framework.Web.HttpModules;

namespace UnitTests.Subtext.Framework.Components.EntryTests
{
    [TestFixture]
    public class EntryDayGetTests
    {
        [SetUp]
        public void Setup()
        {
            var repository = new DatabaseObjectProvider();
            string hostname = UnitTestHelper.GenerateUniqueString();
            repository.CreateBlog("", "username", "password", hostname, string.Empty);
            UnitTestHelper.SetHttpContextWithBlogRequest(hostname, string.Empty);
            BlogRequest.Current.Blog = repository.GetBlog(hostname, string.Empty);
        }

        [Test]
        [RollBack2]
        public void GetSingleDayReturnsDayWithEnclosure()
        {
            //Create some entries.
            var repository = new DatabaseObjectProvider();
            Entry entryZero = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-zero", "body-zero");
            Thread.Sleep(100);
            Entry entryOne = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-one", "body-one");
            Thread.Sleep(100);
            Entry entryTwo = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-two", "body-two");

            //Persist entries.
            UnitTestHelper.Create(entryZero);
            UnitTestHelper.Create(entryOne);
            UnitTestHelper.Create(entryTwo);

            //Add Enclosure
            Enclosure enc = UnitTestHelper.BuildEnclosure("Nothing to see here.", "httP://blablabla.com", "audio/mp3",
                                                          entryZero.Id, 12345678, true, true);
            repository.Create(enc);

            //Get EntryDay
            EntryDay entries = repository.GetEntryDay(DateTime.UtcNow);

            //Test outcome
            Assert.AreEqual(3, entries.Count, "Expected to find three entries.");

            Assert.AreEqual(entries.First().Id, entryTwo.Id, "Ordering is off.");
            Assert.AreEqual(entries.ElementAt(1).Id, entryOne.Id, "Ordering is off.");
            Assert.AreEqual(entries.ElementAt(2).Id, entryZero.Id, "Ordering is off.");


            Assert.IsNull(entries.First().Enclosure, "Entry should not have enclosure.");
            Assert.IsNull(entries.ElementAt(1).Enclosure, "Entry should not have enclosure.");
            Assert.IsNotNull(entries.ElementAt(2).Enclosure, "Entry should have enclosure.");
            UnitTestHelper.AssertEnclosures(enc, entries.ElementAt(2).Enclosure);
        }

        [Test]
        [RollBack2]
        public void GetBlogPostsReturnsAllPostsIfPostConfigNoneSpecified()
        {
            // Create four entries.
            var repository = new DatabaseObjectProvider();
            Entry entryZero = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-zero", "body-zero");
            entryZero.IsActive = entryZero.IncludeInMainSyndication = true;
            Entry entryOne = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-one", "body-one");
            entryOne.IsActive = entryOne.IncludeInMainSyndication = true;
            Entry entryTwo = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-two", "body-two");
            entryTwo.IsActive = false;
            Entry entryThree = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-three", "body-zero");
            entryThree.IncludeInMainSyndication = true;
            entryThree.IsActive = true;
            entryThree.DateCreatedUtc = DateTime.UtcNow.AddDays(10);
            entryThree.DatePublishedUtc = DateTime.UtcNow.AddDays(10);

            //Persist entries.
            UnitTestHelper.Create(entryZero);
            Thread.Sleep(500);
            UnitTestHelper.Create(entryOne);
            Thread.Sleep(500);
            UnitTestHelper.Create(entryTwo);
            Thread.Sleep(500);
            UnitTestHelper.Create(entryThree);



            Assert.IsTrue(entryThree.DatePublishedUtc > DateTime.UtcNow);

            //Get EntryDay
            ICollection<EntryDay> entryList = repository.GetBlogPostsForHomePage(10, PostConfig.None).ToList();

            //Test outcome
            Assert.AreEqual(3, entryList.Count);

            foreach (var entryDay in entryList)
            {
                Console.WriteLine("Entry Day: " + entryDay.Count + " " + entryDay.BlogDay);
                foreach (var item in entryDay)
                {
                    Console.WriteLine("\tEntry: " + item.Id);
                }
            }

            Assert.AreEqual(1, entryList.First().Count);
            Assert.AreEqual(2, entryList.ElementAt(1).Count);
            Assert.AreEqual(1, entryList.ElementAt(2).Count); // One of these don't have a date syndicated.
        }

        [Test]
        [RollBack2]
        public void GetBlogPostsReturnsActiveOnlyAndNoneInFuture()
        {
            //Create some entries.
            var repository = new DatabaseObjectProvider();
            Entry entryZero = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-zero", "body-zero");
            entryZero.IsActive = entryZero.IncludeInMainSyndication = true;
            Entry entryOne = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-one", "body-one");
            entryOne.IsActive = entryOne.IncludeInMainSyndication = true;
            entryOne.DateCreatedUtc = DateTime.UtcNow.AddDays(-1);
            entryOne.DatePublishedUtc = DateTime.UtcNow.AddDays(-1);
            Entry entryTwo = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-two", "body-two");
            entryTwo.IsActive = false;
            Entry entryThree = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-three", "body-zero");
            entryThree.IsActive = entryThree.IncludeInMainSyndication = true;
            entryThree.DateCreatedUtc.AddDays(-2);
            entryThree.DatePublishedUtc = DateTime.UtcNow.AddDays(10);

            //Persist entries.
            UnitTestHelper.Create(entryZero);
            Thread.Sleep(500);
            UnitTestHelper.Create(entryOne);
            Thread.Sleep(500);
            UnitTestHelper.Create(entryTwo);
            Thread.Sleep(500);
            UnitTestHelper.Create(entryThree);
            Assert.IsTrue(entryThree.DatePublishedUtc > DateTime.UtcNow);

            //Get EntryDay
            ICollection<EntryDay> entryList = repository.GetBlogPostsForHomePage(10, PostConfig.IsActive).ToList();

            //Test outcome
            Assert.AreEqual(2, entryList.Count);
            Assert.AreEqual(1, entryList.First().Count);
            Assert.AreEqual(1, entryList.ElementAt(1).Count);
        }

        [Test]
        [RollBack2]
        public void GetBlogPostsReturnsDaysWithEnclosure()
        {
            //Create some entries.
            Entry entryZero = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-zero", "body-zero");
            Thread.Sleep(500);
            Entry entryOne = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-one", "body-one");
            Thread.Sleep(500);
            Entry entryTwo = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-two", "body-two");
            entryTwo.IsActive = false;
            Thread.Sleep(500);
            Entry entryThree = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-three", "body-three");

            //Persist entries.
            UnitTestHelper.Create(entryZero);
            UnitTestHelper.Create(entryOne);
            UnitTestHelper.Create(entryTwo);
            UnitTestHelper.Create(entryThree);

            Assert.IsTrue(entryZero.DateCreatedUtc < entryOne.DateCreatedUtc);
            Assert.IsTrue(entryOne.DateCreatedUtc < entryTwo.DateCreatedUtc);
            Assert.IsTrue(entryTwo.DateCreatedUtc < entryThree.DateCreatedUtc);
            var repository = new DatabaseObjectProvider();

            //Add Enclosure
            Enclosure enc = UnitTestHelper.BuildEnclosure("Nothing to see here.", "httP://blablabla.com", "audio/mp3",
                                                          entryZero.Id, 12345678, true, true);
            repository.Create(enc);

            //Get EntryDay
            ICollection<EntryDay> entryList = repository.GetBlogPostsForHomePage(10, PostConfig.IsActive).ToList();

            Collection<Entry> entries = entryList.First();
            //Test outcome
            Assert.AreEqual(1, entryList.Count, "Expected to find one entry day.");

            Assert.AreEqual(3, entries.Count, "Expected to find three entries.");

            Assert.IsNull(entries.First().Enclosure, "Entry should not have enclosure.");
            Assert.IsNull(entries.ElementAt(1).Enclosure, "Entry should not have enclosure.");
            Assert.IsNotNull(entries.ElementAt(2).Enclosure, "Entry should have enclosure.");
            UnitTestHelper.AssertEnclosures(enc, entries.ElementAt(2).Enclosure);
        }

        [Test]
        [RollBack2]
        public void GetHomePageEntriesReturnsDaysWithEnclosure()
        {
            //Create some entries.
            Entry entryZero = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-zero", "body-zero");
            entryZero.DatePublishedUtc = DateTime.UtcNow.AddDays(-1);
            entryZero.IsActive = entryZero.IncludeInMainSyndication = true;
            Thread.Sleep(100);
            Entry entryOne = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-one", "body-one");
            entryOne.DatePublishedUtc = DateTime.UtcNow.AddDays(-1);
            entryOne.IsActive = entryOne.IncludeInMainSyndication = true;
            Thread.Sleep(100);
            Entry entryTwo = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-two", "body-two");
            entryTwo.DisplayOnHomePage = false;
            Thread.Sleep(100);
            Entry entryThree = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-two", "body-two");
            entryThree.IsActive = entryThree.IncludeInMainSyndication = true;

            //Persist entries.
            UnitTestHelper.Create(entryZero);
            UnitTestHelper.Create(entryOne);
            UnitTestHelper.Create(entryTwo);
            UnitTestHelper.Create(entryThree);
            var repository = new DatabaseObjectProvider();

            //Add Enclosure
            Enclosure enc = UnitTestHelper.BuildEnclosure("Nothing to see here.", "httP://blablabla.com", "audio/mp3",
                                                          entryZero.Id, 12345678, true, true);
            repository.Create(enc);

            //Get EntryDay
            ICollection<EntryDay> entryList = repository.GetBlogPostsForHomePage(10, PostConfig.DisplayOnHomepage | PostConfig.IsActive).ToList();

            var days = new EntryDay[2];
            entryList.CopyTo(days, 0);

            //Test outcome
            Assert.AreEqual(2, entryList.Count, "Expected to find two days.");

            EntryDay entries = days[1];
            Assert.AreEqual(2, entries.Count, "Expected to find two entries.");

            Assert.AreEqual(entries.First().Id, entryOne.Id, "Ordering is off.");
            Assert.AreEqual(entries.ElementAt(1).Id, entryZero.Id, "Ordering is off.");

            Assert.IsNull(entries.First().Enclosure, "Entry should not have enclosure.");
            Assert.IsNotNull(entries.ElementAt(1).Enclosure, "Entry should have enclosure.");
            UnitTestHelper.AssertEnclosures(enc, entries.ElementAt(1).Enclosure);
        }

        [Test]
        [RollBack2]
        public void GetPostsByCategoryIDReturnsDaysWithEnclosure()
        {
            //Create Category
            int blogId = Config.CurrentBlog.Id;
            int categoryId = UnitTestHelper.CreateCategory(blogId, "Test Category");

            //Create four entries.
            Entry entryZero = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-zero", "body-zero");
            entryZero.DatePublishedUtc = DateTime.UtcNow.AddDays(-1);
            Thread.Sleep(100);
            Entry entryOne = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-one", "body-one");
            entryOne.DatePublishedUtc = DateTime.UtcNow.AddDays(-1);
            Thread.Sleep(100);
            Entry entryTwo = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-two", "body-two");
            entryTwo.DatePublishedUtc = DateTime.UtcNow.AddDays(-1);
            Thread.Sleep(100);
            Entry entryThree = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-three", "body-two");
            entryThree.DatePublishedUtc = DateTime.UtcNow;

            //Associate Category
            entryZero.Categories.Add("Test Category");
            entryOne.Categories.Add("Test Category");
            entryThree.Categories.Add("Test Category");


            //Persist entries.
            UnitTestHelper.Create(entryZero);
            UnitTestHelper.Create(entryOne);
            UnitTestHelper.Create(entryTwo);
            UnitTestHelper.Create(entryThree);
            var repository = new DatabaseObjectProvider();

            //Add Enclosure
            Enclosure enc = UnitTestHelper.BuildEnclosure("Nothing to see here.", "httP://blablabla.com", "audio/mp3",
                                                          entryZero.Id, 12345678, true, true);
            repository.Create(enc);

            //Get EntryDay
            ICollection<EntryDay> entryList = repository.GetBlogPostsByCategoryGroupedByDay(10, categoryId).ToList();

            var days = new EntryDay[2];
            entryList.CopyTo(days, 0);

            //Test outcome
            Assert.AreEqual(2, entryList.Count, "Expected to find two days.");

            EntryDay entries = days[1];
            Assert.AreEqual(2, entries.Count, "Expected to find two entries.");

            Assert.AreEqual(entries.First().Id, entryOne.Id, "Ordering is off.");
            Assert.AreEqual(entries.ElementAt(1).Id, entryZero.Id, "Ordering is off.");

            Assert.IsNull(entries.First().Enclosure, "Entry should not have enclosure.");
            Assert.IsNotNull(entries.ElementAt(1).Enclosure, "Entry should have enclosure.");
            UnitTestHelper.AssertEnclosures(enc, entries.ElementAt(1).Enclosure);
        }

        [Test]
        [RollBack2]
        public void GetPostsByMonthReturnsDaysWithEnclosure()
        {
            //Create some entries.
            Entry entryZero = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-zero", "body-zero");
            Thread.Sleep(100);
            Entry entryOne = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-one", "body-one");
            Thread.Sleep(100);
            Entry entryTwo = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-two", "body-two");
            Thread.Sleep(100);
            Entry entryThree = UnitTestHelper.CreateEntryInstanceForSyndication("me", "title-two", "body-two");
            entryThree.DateCreatedUtc = DateTime.UtcNow.AddDays(1);

            //Persist entries.
            UnitTestHelper.Create(entryZero);
            UnitTestHelper.Create(entryOne);
            UnitTestHelper.Create(entryTwo);
            UnitTestHelper.Create(entryThree);
            var repository = new DatabaseObjectProvider();

            //Add Enclosure
            Enclosure enc = UnitTestHelper.BuildEnclosure("Nothing to see here.", "httP://blablabla.com", "audio/mp3",
                                                          entryZero.Id, 12345678, true, true);
            repository.Create(enc);

            //Get EntryDay
            //ICollection<EntryDay> entryList = Entries.GetPostsByMonth(DateTime.UtcNow.Month, DateTime.Now.Year);

            //EntryDay[] days = new EntryDay[2];
            //entryList.CopyTo(days, 0);

            ////Test outcome
            //Assert.AreEqual(2, entryList.Count, "Expected to find two days.");

            //EntryDay entries = days[1];
            //Assert.AreEqual(3, entries.Count, "Expected to find three entries.");

            //Assert.AreEqual(entries.First().Id, entryTwo.Id, "Ordering is off.");
            //Assert.AreEqual(entries.ElementAt(1).Id, entryOne.Id, "Ordering is off.");
            //Assert.AreEqual(entries.ElementAt(2).Id, entryZero.Id, "Ordering is off.");

            //Assert.IsNull(entries.First().Enclosure, "Entry should not have enclosure.");
            //Assert.IsNull(entries.ElementAt(1).Enclosure, "Entry should not have enclosure.");
            //Assert.IsNotNull(entries.ElementAt(2).Enclosure, "Entry should have enclosure.");
            //UnitTestHelper.AssertEnclosures(enc, entries.ElementAt(2).Enclosure);
        }
    }
}
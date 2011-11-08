﻿using System;
using MbUnit.Framework;
using Moq;
using Subtext.Framework;
using Subtext.Framework.Security;

namespace UnitTests.Subtext.Framework.SecurityHandling
{
    [TestFixture]
    public class AccountServiceTests
    {
        [Test]
        public void UpdatePassword_WithEmptyPassword_ThrowsArgumentNullException()
        {
            // Arrange
            var context = new Mock<ISubtextContext>();
            var accountService = new AccountService(context.Object);

            // Act, Assert
            UnitTestHelper.AssertThrows<ArgumentNullException>(() =>
                accountService.UpdatePassword(null));
            UnitTestHelper.AssertThrows<ArgumentNullException>(() =>
                accountService.UpdatePassword(""));
        }

        [Test]
        public void UpdatePassword_WithNonEmptyPassword_HashesPassword()
        {
            // Arrange
            var blog = new Blog { IsPasswordHashed = true };
            var context = new Mock<ISubtextContext>();
            context.Setup(c => c.Repository.UpdateBlog(blog));
            context.Setup(c => c.Blog).Returns(blog);
            var accountService = new AccountService(context.Object);

            // Act
            accountService.UpdatePassword("newPass");

            // Assert
            string expected = SecurityHelper.HashPassword("newPass");
            Assert.AreEqual(expected, blog.Password);
        }

    }
}

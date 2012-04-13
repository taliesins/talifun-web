using NUnit.Framework;

namespace Talifun.Web.Tests
{
    public static class AssertionExtensions
    {
        public static void ShouldBeOfType<T>(this object target)
        {
            Assert.That(target, Is.InstanceOf<T>());
        }

        public static void ShouldEqual(this object actual, object expected)
        {
            Assert.That(actual, Is.EqualTo(expected));
        }

        public static void ShouldNotBeNull(this object target)
        {
            Assert.That(target, Is.Not.Null);
        }

        public static void ShouldBeNull(this object target)
        {
            Assert.That(target, Is.Null);
        }

        public static void ShouldBeNullOrEmptyString(this string target)
        {
            Assert.That(string.IsNullOrEmpty(target));
        }

        public static void ShouldHaveCount(this object target, int expectedCount)
        {
            Assert.That(target, Has.Count.EqualTo(expectedCount));
        }

        public static void ShouldContain(this string target, string expectedString)
        {
            Assert.That(target.Contains(expectedString));
        }
    }
}

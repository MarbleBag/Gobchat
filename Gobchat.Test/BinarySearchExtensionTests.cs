using NUnit.Framework;
using System;

namespace Gobchat.Test
{
    public class BinarySearchExtensionTests
    {
        private Gobchat.Core.Util.Extension.BinarySearchExtension.DistanceTo<int, int> distanceDelegate;

        [SetUp]
        public void Setup()
        {
            distanceDelegate = (obj, value) => obj - value;
        }

        [Test]
        public void BinarySearchClosest_EmptyArray()
        {
            var array = new int[] { };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchClosest(array, 5, distanceDelegate);
            Assert.AreEqual(-1, result);
        }

        [Test]
        public void BinarySearchClosest_OnlyElement()
        {
            var array = new int[] { 5 };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchClosest(array, 5, distanceDelegate);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void BinarySearchClosest_Contained_LowEnd()
        {
            var array = new int[] { 5, 9 };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchClosest(array, 5, distanceDelegate);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void BinarySearchClosest_Contained_HighEnd()
        {
            var array = new int[] { 2, 5 };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchClosest(array, 5, distanceDelegate);
            Assert.AreEqual(1, result);
        }

        public void BinarySearchClosest_Contained_OddElements()
        {
            var array = new int[] { 2, 5, 9 };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchClosest(array, 5, distanceDelegate);
            Assert.AreEqual(1, result);
        }

        public void BinarySearchClosest_Contained_EvenElements()
        {
            var array = new int[] { 2, 3, 5, 9 };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchClosest(array, 3, distanceDelegate);
            Assert.AreEqual(1, result);
        }

        [Test]
        public void BinarySearchClosest_OutOfBounds_high()
        {
            var array = new int[] { 1, 4, 7 };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchClosest(array, 9, distanceDelegate);
            Assert.AreEqual(2, result);
        }

        [Test]
        public void BinarySearchClosest_OutOfBounds_Low()
        {
            var array = new int[] { 1, 4, 7 };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchClosest(array, -2, distanceDelegate);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void BinarySearchClosest_TwoValues()
        {
            var array = new int[] { 2, 4, 8, 15 };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchClosest(array, 3, distanceDelegate);
            Assert.That(result, Is.EqualTo(1).Or.EqualTo(2));
        }

        [Test]
        public void BinarySearchClosest_OneValue_Higher()
        {
            var array = new int[] { 1, 4, 8, 15 };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchClosest(array, 7, distanceDelegate);
            Assert.AreEqual(2, result);
        }

        [Test]
        public void BinarySearchClosest_OneValue_Lower()
        {
            var array = new int[] { 1, 4, 8, 15 };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchClosest(array, 5, distanceDelegate);
            Assert.AreEqual(1, result);
        }

        [Test]
        public void BinarySearchUpper_Equal()
        {
            var array = new int[] { 1, 3, 5, 7 };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchUpper(array, 5, distanceDelegate);
            Assert.AreEqual(2, result);
        }

        [Test]
        public void BinarySearchUpper_Higher()
        {
            var array = new int[] { 1, 3, 5, 7 };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchUpper(array, 4, distanceDelegate);
            Assert.AreEqual(2, result);
        }

        [Test]
        public void BinarySearchLower_Equal()
        {
            var array = new int[] { 1, 3, 5, 7 };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchLower(array, 3, distanceDelegate);
            Assert.AreEqual(1, result);
        }

        [Test]
        public void BinarySearchLower_Lower()
        {
            var array = new int[] { 1, 3, 5, 7 };
            Array.Sort(array);

            var result = Gobchat.Core.Util.Extension.BinarySearchExtension.BinarySearchLower(array, 4, distanceDelegate);
            Assert.AreEqual(1, result);
        }
    }
}
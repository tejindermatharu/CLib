using CacheLib;
using System.Diagnostics;

namespace CacheTest
{
    public class CacheLibTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            //Lets create with a low number to ease testing
            CustomCache.Initialise(3);
            CustomCacheWithConcurrentDictionary.Initialise(3);
        }

        [TearDown]
        public void DerivedTearDown() 
        {
            var cache = CustomCache.GetInstance;
            cache.RemoveAllFromCache();
        }

        [Test]
        public void WhennAddCacheItems_ThenGetValueFromCache_ShouldReturnCorrectValue()
        {
            var cache = CustomCache.GetInstance;

            var list = new List<string>() { "one", "two", "three" };
            var list2 = new List<string>() { "four", "five", "six" };
            string notifyMessage = null;

            CacheNotifyMediator.GetInstance().CacheItemRemovedChanged += (s, e) => {

                notifyMessage = CacheRemovedNotification(e.Key);
            };

            cache.AddReplace("Test", list);

            var test2Array = new int[] { 1, 2 };

            cache.AddReplace("Test2", test2Array);

            cache.AddReplace("Test3", "test");

            cache.TryGetValue<int[]>("Test2", out int[] myval);

            Assert.IsTrue(Enumerable.SequenceEqual(test2Array, myval));
        }

        [Test]
        public void WhennAddCacheItemsThatExceedLimit_ShouldAllowNewItemsToBeAdded()
        {
            var cache = CustomCache.GetInstance;

            var list = new List<string>() { "one", "two", "three" };
            var list2 = new List<string>() { "four", "five", "six" };
            string notifyMessage = null;

            CacheNotifyMediator.GetInstance().CacheItemRemovedChanged += (s, e) => {

                notifyMessage = CacheRemovedNotification(e.Key);
            };

            cache.AddReplace("Test", list);

            cache.AddReplace("Test2", "test");

            cache.AddReplace("Test3", "newval");

            var testArray = new int[] { 1, 2 };

            cache.AddReplace("Test4", testArray);

            cache.TryGetValue<string>("Test3", out string myval);
            cache.TryGetValue<int[]>("Test4", out int[] myval2);

            Assert.That(myval, Is.EqualTo("newval"));
            Assert.IsTrue(Enumerable.SequenceEqual(testArray, myval2));
        }

        [Test]
        public void WhennAddCacheItemsThatExceedLimit_ShouldNotifyUser()
        {
            var cache = CustomCache.GetInstance;

            var list = new List<string>() {"one", "two","three" }; 
            var list2 = new List<string>() {"four", "five","six" };
            string notifyMessage = null;

            CacheNotifyMediator.GetInstance().CacheItemRemovedChanged += (s, e) => {

                notifyMessage =  CacheRemovedNotification(e.Key);
            };

            cache.AddReplace("Test", list);

            cache.AddReplace("Test2", list2);

            cache.AddReplace("Test3", new int[] {1,2});

            cache.AddReplace("Test4","test");

            Assert.That(notifyMessage, Is.EqualTo("Item with key Test was removed from the cache"));
        }

        [Test]
        public void WhenAddingThreeItemsUnderMaxCapacity_ThenRunningAddReplaceInParallel_ShouldHaveCorrectNumberOfItemsInCache()
        {
            var cache = CustomCache.GetInstance;

            Parallel.For(0,100, i =>
            {
                cache.AddReplace("Apple", i);
                cache.AddReplace("Pear", i);
                cache.AddReplace("Orange", i);
            });

            Assert.That(cache.GetItemsCount, Is.LessThanOrEqualTo(3));
        }

        [Test]
        public void WhenAddingThreeItemsOverMaxCapacity_ThenRunningAddReplaceInParallel_ShouldHaveCorrectNumberOfItemsInCache()
        {
            var cache = CustomCache.GetInstance;
            for (int i = 0; i < 20; i++)
            {
                Parallel.For(0, 100, i =>
                {
                    cache.AddReplace("Apple", i);
                    cache.AddReplace("Pear", i);
                    cache.AddReplace("Orange", i);
                    cache.AddReplace("Plum", i);
                    cache.AddReplace("Peach", i);
                });
            }

            Assert.That(cache.GetItemsCount, Is.LessThanOrEqualTo(3));
        }

        [Test]
        public void WhenAddingItemsOverMaxCapacityAndTryGetCalled_ThenRunningAddReplaceInParallel_ShouldHaveCorrectNumberOfItemsInCache()
        {
            var cache = CustomCacheWithConcurrentDictionary.GetInstance;

            Parallel.For(0, 10, i =>
            {
                cache.AddReplace($"Apple{i}", i);
                cache.TryGetValue<int>($"Apple{ i}", out int myval1);
                cache.AddReplace($"Pear{i}", i);
                cache.TryGetValue<int>($"Pear{i}", out int myval2);
                cache.AddReplace($"Orange{i}", i);
                cache.TryGetValue<int>($"Orange{i}", out int myval3);
                cache.AddReplace($"Plum{i}", i);
                cache.TryGetValue<int>($"Plum{i}", out int myval4);
            });

            Assert.That(cache.GetItemsCount, Is.LessThanOrEqualTo(3));            
        }

        [Test]
        public void WhenAddingItemsOverMaxCapacityAndTryGetCalledInDifferentOrder_ThenRunningAddReplaceInParallel_ShouldHaveCorrectNumberOfItemsInCache()
        {
            var cache = CustomCache.GetInstance;

            Parallel.For(0, 100, i =>
            {
                cache.AddReplace($"Apple{i}", i);
                cache.TryGetValue<int>($"Orange{i}", out int myval3);
                cache.AddReplace($"Pear{i}", i);
                cache.TryGetValue<int>($"Plum{i}", out int myval4);
                cache.AddReplace($"Orange{i}", i);
                cache.TryGetValue<int>($"Apple{i}", out int myval1);
                cache.AddReplace($"Plum{i}", i);
                cache.TryGetValue<int>($"Pear{i}", out int myval2);

            });

            Assert.That(cache.GetItemsCount, Is.LessThanOrEqualTo(3));
        }

        [Test]
        public void BenmarkingTestToCompareWithConcurrentDictionaryCache()
        {
            var cache = CustomCache.GetInstance;
            var timer = new Stopwatch();
            timer.Start();

            for (int i = 0; i < 200; i++)
            {
                Parallel.For(0, 1000, i =>
                {
                    cache.AddReplace($"Apple{i}", i);
                    cache.TryGetValue<int>($"Orange{i}", out int myval3);
                    cache.AddReplace($"Pear{i}", i);
                    cache.TryGetValue<int>($"Plum{i}", out int myval4);
                    cache.AddReplace($"Orange{i}", i);
                    cache.TryGetValue<int>($"Apple{i}", out int myval1);
                    cache.AddReplace($"Plum{i}", i);
                    cache.TryGetValue<int>($"Pear{i}", out int myval2);

                });

                Assert.That(cache.GetItemsCount, Is.LessThanOrEqualTo(3));
            }

            TimeSpan timeTaken = timer.Elapsed;
        }

        [Test]
        public void BenmarkingTestToCompareWithNonConcurrentDictionaryCache()
        {
            var cache = CustomCacheWithConcurrentDictionary.GetInstance;
            var timer = new Stopwatch();
            timer.Start();

            for (int i = 0; i < 200; i++)
            {
                Parallel.For(0, 1000, i =>
                {
                    cache.AddReplace($"Apple{i}", i);
                    cache.TryGetValue<int>($"Orange{i}", out int myval3);
                    cache.AddReplace($"Pear{i}", i);
                    cache.TryGetValue<int>($"Plum{i}", out int myval4);
                    cache.AddReplace($"Orange{i}", i);
                    cache.TryGetValue<int>($"Apple{i}", out int myval1);
                    cache.AddReplace($"Plum{i}", i);
                    cache.TryGetValue<int>($"Pear{i}", out int myval2);

                });

                Assert.That(cache.GetItemsCount, Is.LessThanOrEqualTo(3));
            }

            TimeSpan timeTaken = timer.Elapsed;
        }


        /// <summary>
        /// Event handler to receive notification when something was removed from the cache
        /// </summary>
        /// <param name="key"></param>
        public string CacheRemovedNotification(string key)
        {
            var message = $"Item with key {key} was removed from the cache";
            Console.WriteLine(message);

            return message;
        }
    }
}
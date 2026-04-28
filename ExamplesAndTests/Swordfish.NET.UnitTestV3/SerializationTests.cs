using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#if NETCOREAPP
using System.Text.Json;
#endif

namespace Swordfish.NET.UnitTestV3
{
    [TestClass]
    public class SerializationTests
    {
#if NETCOREAPP
        private T RoundTripSerialization<T>(T collection) where T : class
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(collection,
                        new JsonSerializerOptions { WriteIndented = false });
            return JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(bytes));
        }
#else
        private T RoundTripSerialization<T>(T collection) where T : class
        {
            var serializer = new BinaryFormatter();
            var stream = new MemoryStream();
            serializer.Serialize(stream, collection);
            stream.Position = 0;
            return serializer.Deserialize(stream) as T;
        }
#endif
        [TestMethod]
        public void ConcurrentObservableCollectionSerializationTest()
        {
            var collection = new ConcurrentObservableCollection<string>();
            for (int i = 0; i < 10; i++)
                collection.Add("TestItem" + (i + 1).ToString());

            collection = RoundTripSerialization(collection);

            for (int i = 0; i < 10; i++)
                Assert.AreEqual("TestItem" + (i + 1).ToString(), collection[i]);
        }

        [TestMethod]
        public void ConcurrentObservableDictionarySerializationTest()
        {
            var collection = new ConcurrentObservableDictionary<string, int>();
            for (int i = 0; i < 10; i++)
                collection.Add("TestItem" + (i + 1).ToString(), i);

            collection = RoundTripSerialization(collection);

            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, collection["TestItem" + (i + 1).ToString()]);
        }

        [TestMethod]
        public void ConcurrentObservableSortedDictionarySerializationTest()
        {
            var collection = new ConcurrentObservableSortedDictionary<string, int>();
            for (int i = 0; i < 10; i++)
                collection.Add("TestItem" + (i + 1).ToString(), i);

            collection = RoundTripSerialization(collection);

            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, collection["TestItem" + (i + 1).ToString()]);
        }

        [TestMethod]
        public void ConcurrentObservableHashSetSerializationTest()
        {
            var collection = new ConcurrentObservableHashSet<string>();
            for (int i = 0; i < 10; i++)
                collection.Add("TestItem" + (i + 1).ToString());

            collection = RoundTripSerialization(collection);

            Assert.AreEqual(10, collection.Count);
            for (int i = 0; i < 10; i++)
                Assert.IsTrue(collection.Contains("TestItem" + (i + 1).ToString()));
        }
#if !NETCOREAPP
        [TestMethod]
        public void UnthrottledActionSerializationTest()
        {
            var collection = new ConcurrentObservableCollection<string>(controlledAction: new UnthrottledAction());
            collection = RoundTripSerialization(collection);
            Assert.AreEqual(typeof(UnthrottledAction), collection.GetControlledAction().GetType());
        }

        [TestMethod]
        public void ThrottledActionWithWaitSerializationTest()
        {
            var collection = new ConcurrentObservableCollection<string>(controlledAction: new ThrottledActionWithWait());
            collection = RoundTripSerialization(collection);
            Assert.AreEqual(typeof(ThrottledActionWithWait), collection.GetControlledAction().GetType());
        }

        [TestMethod]
        public void ThrottledActionTaskDelaySerializationTest()
        {
            var collection = new ConcurrentObservableCollection<string>(controlledAction: new ThrottledActionTaskDelay());
            collection = RoundTripSerialization(collection);
            Assert.AreEqual(typeof(ThrottledActionTaskDelay), collection.GetControlledAction().GetType());
        }

#endif

    }
}

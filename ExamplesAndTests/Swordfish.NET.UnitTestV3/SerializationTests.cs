﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Swordfish.NET.UnitTestV3
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void ConcurrentObservableCollectionSerializationTest()
        {
            var serializer = new BinaryFormatter();
            var stream = new MemoryStream();
            var collection = new ConcurrentObservableCollection<string>();
            for (int i = 0; i < 10; i++)
                collection.Add("TestItem" + (i + 1).ToString());
            serializer.Serialize(stream, collection);
            stream.Position = 0;
            collection = serializer.Deserialize(stream) as ConcurrentObservableCollection<string>;
            for (int i = 0; i < 10; i++)
                Assert.AreEqual("TestItem" + (i + 1).ToString(), collection[i]);
        }

        [TestMethod]
        public void ConcurrentObservableDictionarySerializationTest()
        {
            var serializer = new BinaryFormatter();
            var stream = new MemoryStream();
            var collection = new ConcurrentObservableDictionary<string, int>();
            for (int i = 0; i < 10; i++)
                collection.Add("TestItem" + (i + 1).ToString(), i);
            serializer.Serialize(stream, collection);
            stream.Position = 0;
            collection = serializer.Deserialize(stream) as ConcurrentObservableDictionary<string, int>;
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, collection["TestItem" + (i + 1).ToString()]);
        }

        [TestMethod]
        public void ConcurrentObservableSortedDictionarySerializationTest()
        {
            var serializer = new BinaryFormatter();
            var stream = new MemoryStream();
            var collection = new ConcurrentObservableSortedDictionary<string, int>();
            for (int i = 0; i < 10; i++)
                collection.Add("TestItem" + (i + 1).ToString(), i);
            serializer.Serialize(stream, collection);
            stream.Position = 0;
            collection = serializer.Deserialize(stream) as ConcurrentObservableSortedDictionary<string, int>;
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, collection["TestItem" + (i + 1).ToString()]);
        }

        [TestMethod]
        public void ConcurrentObservableHashSetSerializationTest()
        {
            var serializer = new BinaryFormatter();
            var stream = new MemoryStream();
            var collection = new ConcurrentObservableHashSet<string>();
            for (int i = 0; i < 10; i++)
                collection.Add("TestItem" + (i + 1).ToString());
            serializer.Serialize(stream, collection);
            stream.Position = 0;
            collection = serializer.Deserialize(stream) as ConcurrentObservableHashSet<string>;
            Assert.AreEqual(10, collection.Count);
            for (int i = 0; i < 10; i++)
                Assert.IsTrue(collection.Contains("TestItem" + (i + 1).ToString()));
        }
    }
}

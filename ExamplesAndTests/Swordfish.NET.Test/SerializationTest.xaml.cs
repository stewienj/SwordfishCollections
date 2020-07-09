using Swordfish.NET.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;

namespace Swordfish.NET.Test
{
    /// <summary>
    /// Interaction logic for SerializationTest.xaml
    /// </summary>
    public partial class SerializationTest : UserControl
    {
        public SerializationTest()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Messages.Add("Testing BinaryFormatter...");
            var list = new ConcurrentObservableCollection<string>();
            var dictionary = new ConcurrentObservableDictionary<string, string>();
            var sortedDictionary = new ConcurrentObservableSortedDictionary<string, string>();
            InitializeList(list, 20);
            TestSerializingObject(list);
            InitializeDictionary(dictionary, 20);
            TestSerializingObject(dictionary);
            InitializeDictionary(sortedDictionary, 20);
            TestSerializingObject(sortedDictionary);
            Messages.Add("Done");
        }

        private void TestSerializingObject(object source)
        {
            Messages.Add($"Serializing {source.GetType().Name}");
            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            try
            {
                formatter.Serialize(stream, source);
                Messages.Add("Success!");
            }
            catch (SerializationException e)
            {
                Messages.Add("Failed to serialize. Reason: " + e.Message);
            }
            finally
            {
                stream.Close();
            }
        }

        public void InitializeDictionary(IDictionary<string, string> dictionary, int elementCount)
        {
            Messages.Add($"Adding {elementCount} elements to {dictionary.GetType().Name}");
            for (int i = 0; i < elementCount; ++i)
            {
                string key = $"key {i}".PadLeft(3, '0');
                string value = $"value {i}".PadLeft(3, '0');
                dictionary.Add(key, value);
            }
        }

        public void InitializeList(IList<string> list, int elementCount)
        {
            Messages.Add($"Adding {elementCount} elements to {list.GetType().Name}");
            for (int i = 0; i < elementCount; ++i)
            {
                string value = $"value {i}".PadLeft(3, '0');
                list.Add(value);
            }
        }

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();
    }
}

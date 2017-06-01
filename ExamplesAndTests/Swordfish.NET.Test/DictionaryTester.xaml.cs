using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Concurrent;
using System.Threading;

namespace Swordfish.NET.Test {
  /// <summary>
  /// This user control is used for testing the observable dictionary class
  /// ... and its derived classes
  /// </summary>
  public partial class DictionaryTester : UserControl {

    // ************************************************************************
    // Private Fields
    // ************************************************************************
    #region Private Fields

    /// <summary>
    /// The list being tested
    /// </summary>
    private IDictionary<string, string> list;
    /// <summary>
    /// A dictionary mapping how many times a key has been updated
    /// </summary>
    private Dictionary<string, int> versions;
    /// <summary>
    /// Random number generator for generating list items
    /// </summary>
    private Random random;
    /// <summary>
    /// List of actions to be taken on the list being tested
    /// </summary>
    private BlockingCollection<Action> actions;
    /// <summary>
    /// Flag indicating if values should be added to the collection
    /// concurrently or not
    /// </summary>
    private bool concurrent = false;

    int lastKeyValueSelectedIndex = -1;
    int lastKeySelectedIndex = -1;

    #endregion Private Fields

    // ************************************************************************
    // Public Methods
    // ************************************************************************
    #region Public Methods

    /// <summary>
    /// Default constructor
    /// </summary>
    public DictionaryTester() {
      InitializeComponent();

      actions = new BlockingCollection<Action>();
      Thread collectionUpdater = new Thread(
        delegate() {
          while (true) {
            foreach (Action d in actions.GetConsumingEnumerable()) {
              d();
            }
          }
        }
      );
      collectionUpdater.IsBackground = true;
      collectionUpdater.Start();

    }

    /// <summary>
    /// Initializes the control wih the observable list passed in, and randomly
    /// creates the number of values passed in to populate the list.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="elementCount"></param>
    public void InitializeList(IDictionary<string, string> list, int elementCount, bool concurrent) {
      this.list = list;
      this.random = new Random(100);
      this.versions = new Dictionary<string, int>();
      this.concurrent = concurrent;
      for (int i = 0; i < elementCount; ++i) {
        string key = GetUniqueRandomKey();
        string value = GetValue(key, 0);
        list.Add(key, value);
        versions[key] = 0;
      }
      KeyValueList.ItemsSource = list;
      ValueList.ItemsSource = list;
      KeyList.ItemsSource = list;
      LastActionMessage.Text = "Action: Initialized Lists";
    }

    #endregion Public Methods

    // ************************************************************************
    // Private Methods
    // ************************************************************************
    #region Private Methods

    /// <summary>
    /// Adds a dictionary modification action to the list of actions if
    /// concurrent, or just executes the action if not.
    /// </summary>
    /// <param name="action"></param>
    private void AddAction(Action action) {
      if (concurrent) {
        actions.Add(action);
      } else {
        action();
      }
    }

    /// <summary>
    /// Creates a random key
    /// </summary>
    /// <returns></returns>
    private string GetRandomKey() {
      int index = random.Next(100);
      return "key" + index.ToString().PadLeft(3, '0');
    }

    /// <summary>
    /// Creates a unique random key
    /// </summary>
    /// <returns></returns>
    private string GetUniqueRandomKey() {
      string key = null;
      do {
        key = GetRandomKey();
      } while (versions.ContainsKey(key));
      return key;
    }

    /// <summary>
    /// Creates a value string for the key and version number passed in
    /// </summary>
    /// <param name="key"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    private static string GetValue(string key, int version) {
      return key.Replace("key", "value") + "-" + version.ToString();
    }

    #endregion Private Methods

    // ************************************************************************
    // Event Handlers
    // ************************************************************************
    #region Event Handlers

    /// <summary>
    /// Handles when the Delete button under the Key-Value list is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KeyValueRemove_Click(object sender, RoutedEventArgs e) {
      
      // Check if an item is selected
      if (KeyValueList.SelectedItem != null) {

        // Retain the selected indicies as the selection gets lost on an update
        lastKeyValueSelectedIndex = this.KeyValueList.SelectedIndex;
        lastKeySelectedIndex = this.KeyList.SelectedIndex;

        // Get the selected item
        KeyValuePair<string, string> pair = (KeyValuePair<string, string>)KeyValueList.SelectedItem;
        
        // Exectue the collection update on a separate thread by adding this
        // annoymous delegate to our delegate queue.
        AddAction(delegate() {
          list.Remove(pair);
          versions.Remove(pair.Key);
        });

        // Update the status at the bottom to reflect the last action
        LastActionMessage.Text = "Action: Removed " + pair.ToString();

      }
    }

    /// <summary>
    /// Handles when the Delete button under the Key list is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KeyRemove_Click(object sender, RoutedEventArgs e) {

      // Check if an item is selected
      if (KeyList.SelectedItem != null) {

        // Retain the selected indicies as the selection gets lost on an update
        lastKeyValueSelectedIndex = this.KeyValueList.SelectedIndex;
        lastKeySelectedIndex = this.KeyList.SelectedIndex;

        // Get the selected item
        string key = ((KeyValuePair<string,string>)KeyList.SelectedItem).Key;

        // Exectue the collection update on a separate thread by adding this
        // annoymous delegate to our delegate queue.
        AddAction(delegate() {
          list.Remove(key);
          versions.Remove(key);
        });

        // Update the status at the bottom to reflect the last action
        LastActionMessage.Text = "Action: Removed " + key.ToString();

      }
    }

    /// <summary>
    /// Handles when the Update button under the key list is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KeyUpdate_Click(object sender, RoutedEventArgs e) {

      // Check if an item is selected
      if (KeyList.SelectedItem != null) {

        // Retain the selected indicies as the selection gets lost on an update
        lastKeyValueSelectedIndex = this.KeyValueList.SelectedIndex;
        lastKeySelectedIndex = this.KeyList.SelectedIndex;

        // Get the selected item
        string key = ((KeyValuePair<string, string>)KeyList.SelectedItem).Key;
        string value = GetValue(key, ++versions[key]);

        // Exectue the collection update on a separate thread by adding this
        // annoymous delegate to our delegate queue.
        AddAction(delegate() {
          list[key] = value;
        });

        // Update the status at the bottom to reflect the last action
        LastActionMessage.Text = "Action: Updated " + key.ToString();

      }
    }

    /// <summary>
    /// Handles when the Add New button is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddNew_Click(object sender, RoutedEventArgs e) {

      // Don't want to propagate an invalid index after an item could
      // be inserted before the current selected index so clear these

      lastKeyValueSelectedIndex = -1;
      lastKeySelectedIndex = -1;

      // Get the key and value to add
      string key = GetUniqueRandomKey();
      string value = GetValue(key, 0);

      // Exectue the collection update on a separate thread by adding this
      // annoymous delegate to our delegate queue.
      AddAction(delegate() {
        list.Add(key, value);
        versions[key] = 0;
      });

      // Update the status at the bottom to reflect the last action
      LastActionMessage.Text = "Action: Added " + key.ToString();
    }

    /// <summary>
    /// Handles when the Add New Or Update button is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddNewOrUpdate_Click(object sender, RoutedEventArgs e) {

      // Get a random key to create or update depending on whether it
      // already exists or not.
      string key = GetRandomKey();

      // Check if the key exists
      if (versions.ContainsKey(key)) {

        // Retain the selected indicies as the selection gets lost on an update
        lastKeyValueSelectedIndex = this.KeyValueList.SelectedIndex;
        lastKeySelectedIndex = this.KeyList.SelectedIndex;

        // Increment the value referenced by the key
        string value = GetValue(key, ++versions[key]);

        // Exectue the collection update on a separate thread by adding this
        // annoymous delegate to our delegate queue.
        AddAction(delegate() {
          list[key] = value;
        });

      } else {

        // Don't want to propagate an invalid index after an item could
        // be inserted before the current selected index so clear these
        lastKeyValueSelectedIndex = -1;
        lastKeySelectedIndex = -1;

        // Create a new value
        string value = GetValue(key, 0);
        versions.Add(key, 0);

        // Exectue the collection update on a separate thread by adding this
        // annoymous delegate to our delegate queue.
        AddAction(delegate() {
          list[key] = value;
        });

      }

      // Update the status at the bottom to reflect the last action
      LastActionMessage.Text = "Action: Added Or Updated " + key.ToString();
    }

    /// <summary>
    /// Handles retaining the selection when the list is updated as it loses the selection
    /// </summary>
    private void KeyValueList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (this.KeyValueList.SelectedItem == null) {
        if (this.lastKeyValueSelectedIndex > -1 && this.KeyValueList.Items.Count > 0) {
          this.KeyValueList.SelectedIndex = Math.Min(this.lastKeyValueSelectedIndex, this.KeyValueList.Items.Count - 1);
        }
      }
    }

    /// <summary>
    /// Handles retaining the selection when the list is updated as it loses the selection
    /// </summary>
    private void KeyList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (this.KeyList.SelectedItem == null) {
        if (this.lastKeySelectedIndex > -1 && this.KeyList.Items.Count > 0) {
          this.KeyList.SelectedIndex = Math.Min(this.lastKeySelectedIndex, this.KeyList.Items.Count - 1);
        }
      }
    }


    #endregion Event Handlers
  }
}

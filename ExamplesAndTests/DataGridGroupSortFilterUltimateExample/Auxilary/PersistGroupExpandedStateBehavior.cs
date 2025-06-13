using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DataGridGroupSortFilterUltimateExample.Auxilary;

/// <summary>
/// Based on the following stackoverflow answer, but modified to stop visual glitches,
/// tidied up, and added some comments.
/// https://stackoverflow.com/a/15924044/242220
///
/// </summary>
/// <example>
///
///   <Expander IsExpanded="True" Background="#FF112255" BorderBrush="#FF002255" Foreground="#FFEEEEEE" BorderThickness="1,1,1,5">
///     <i:Interaction.Behaviors>
///       <aux:PersistGroupExpandedStateBehavior GroupKey = "{Binding Name}" />
///     </ i:Interaction.Behaviors>
///  </Expander>
///  
///</example>
public class PersistGroupExpandedStateBehavior : Behavior<Expander>
{
  // **************************************************************************
  // Dependency Properties
  // **************************************************************************
  #region Dependency Properties

  /// <summary>
  /// The key identifing the  group 
  /// </summary>
  public object GroupKey
  {
    get => GetValue(GroupKeyProperty);
    set => SetValue(GroupKeyProperty, value);
  }

  public static readonly DependencyProperty GroupKeyProperty = DependencyProperty.Register(
      "GroupKey",
      typeof(object),
      typeof(PersistGroupExpandedStateBehavior),
      new PropertyMetadata(default(object)));

  /// <summary>
  /// A property that gets attached to the parent Items Control, used for storing state by group name
  /// </summary>
  private static readonly DependencyProperty ExpandedStateStoreProperty =
      DependencyProperty.RegisterAttached(
          "ExpandedStateStore",
          typeof(IDictionary<object, bool>),
          typeof(PersistGroupExpandedStateBehavior),
          new PropertyMetadata(default(IDictionary<object, bool>)));

  #endregion Dependency Properties

  // **************************************************************************
  // Protected Methods
  // **************************************************************************
  #region Protected Methods

  /// <summary>
  /// Does initialization when attached to an Expander
  /// </summary>
  protected override void OnAttached()
  {
    base.OnAttached();

    if (GetStoredState() is bool expanded)
    {
      AssociatedObject.IsExpanded = expanded;
    }
    SetStoredState(AssociatedObject.IsExpanded);

    /// Handle when the expander is expanded or collapsed during intialization,
    /// and override its state wtih the stored value.
    /// This is to stop the IsExpanded status bouncing when first attached 
    AssociatedObject.Expanded += OnExpandedOrCollapsedInitial;
    AssociatedObject.Collapsed += OnExpandedOrCollapsedInitial;

    // Pass on to the next dispatcher frame, after the control has been initialized
    // and its state is stabilized. Now we can store state changes by the user.
    SynchronizationContext.Current.Post(d =>
    {
      // Remove the handlers that stopped a bouncing effect on initialization
      AssociatedObject.Expanded -= OnExpandedOrCollapsedInitial;
      AssociatedObject.Collapsed -= OnExpandedOrCollapsedInitial;

      // Now store state changes by the user
      AssociatedObject.Expanded += OnExpandedOrCollapsed;
      AssociatedObject.Collapsed += OnExpandedOrCollapsed;
    }, null);
  }

  /// <summary>
  /// Removes event handlers when removed from an Expander
  /// </summary>
  protected override void OnDetaching()
  {
    // Remove all possible event handlers (first 2 might already be removed)
    AssociatedObject.Expanded -= OnExpandedOrCollapsedInitial;
    AssociatedObject.Collapsed -= OnExpandedOrCollapsedInitial;
    AssociatedObject.Expanded -= OnExpandedOrCollapsed;
    AssociatedObject.Collapsed -= OnExpandedOrCollapsed;

    base.OnDetaching();
  }

  #endregion Protected Methods

  // **************************************************************************
  // Private Methods
  // **************************************************************************
  #region Private Methods

  /// <summary>
  /// Gets the parent 
  /// </summary>
  /// <returns></returns>
  private ItemsControl FindItemsControl()
  {
    DependencyObject current = AssociatedObject;
    while (current is not null && current is not ItemsControl)
    {
      current = VisualTreeHelper.GetParent(current);
    }
    return current as ItemsControl;
  }

  /// <summary>
  /// Gets the stored state for this group
  /// </summary>
  private bool? GetStoredState()
  {
    var dict = GetExpandedStateStore();
    if (!dict.ContainsKey(GroupKey))
    {
      return null;
    }
    return dict[GroupKey];
  }

  /// <summary>
  /// Sets the stored state for this group
  /// </summary>
  private void SetStoredState(bool expanded)
  {
    var dict = GetExpandedStateStore();
    dict[GroupKey] = expanded;
  }

  /// <summary>
  /// Gets the dictionary that is attached to the parent items control.
  /// This dictionary is used to store the expanded state by group key.
  /// </summary>
  private IDictionary<object, bool> GetExpandedStateStore()
  {
    ItemsControl itemsControl = FindItemsControl();

#if DEBUG
    // Check if we are attached to an items control, and if not the blow up the
    // whole UI... but only if compiled as debug
    if (itemsControl == null)
    {
      throw new Exception(
          "Behavior needs to be attached to an Expander that is contained inside an ItemsControl");
    }
#endif

    // Check if a dictionary is already attached to the parent ItemsControl,
    // and if not then create a new dictionary and attach it to the parent ItemsControl
    if(itemsControl?.GetValue(ExpandedStateStoreProperty) is not IDictionary<object, bool> dict)
    {
      dict = new Dictionary<object, bool>();
      itemsControl.SetValue(ExpandedStateStoreProperty, dict);
    }

    return dict;
  }

  #endregion Private Methods

  // **************************************************************************
  // Event Handlers
  // **************************************************************************
  #region Event Handlers

  /// <summary>
  /// Handles when the expander is expanded or collapsed
  /// </summary>
  private void OnExpandedOrCollapsed(object sender, RoutedEventArgs e) =>
    SetStoredState(AssociatedObject.IsExpanded);

  /// <summary>
  /// Handles when the expander is expanded or collapsed during intialization,
  /// and overrides its state wtih the stored value.
  /// This is to stop the IsExpanded status bouncing when first attached 
  /// </summary>
  private void OnExpandedOrCollapsedInitial(object sender, RoutedEventArgs e)
  {
    if (GetStoredState() is bool expanded)
    {
      AssociatedObject.IsExpanded = expanded;
    }
  }

  #endregion Event Handlers
}

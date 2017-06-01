using Microsoft.Win32;
using Swordfish.NET.General;
using Swordfish.NET.WPF.Converters;
using Swordfish.NET.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Swordfish.NET.WPF.Controls {
  /// <summary>
  /// Interaction logic for BigTextFileViewer.xaml
  /// </summary>
  public partial class BigTextFileViewer : UserControl {

    private BigTextFileViewModel _viewModel = new BigTextFileViewModel();
    /// <summary>
    /// Maps the view column to the index of the data column
    /// </summary>
    private Dictionary<GridViewColumn, int> _columnToDataColumnIndex = new Dictionary<GridViewColumn, int>();

    public BigTextFileViewer() {
      this.Loaded += (s, e) => Window.GetWindow(this).Closing += (sw, ew) => _viewModel.Dispose();
      InitializeComponent();
      Binding bindingColumnCount = new Binding("ColumnCount");
      bindingColumnCount.Source = _viewModel;
      this.SetBinding(ColumnsRequiredProperty, bindingColumnCount);

      Binding bindingFileHasHeader = new Binding("FileHasHeader");
      bindingFileHasHeader.Source = _viewModel;
      this.SetBinding(FileHasHeaderProperty, bindingFileHasHeader);
      _viewModel.CountChanged += (s, e) =>
      {
        RowCount = _viewModel.RowCount;
      };
      _gridViewControl.Columns.CollectionChanged += (s, e) =>
      {
        for (int i = 0; i < _gridViewControl.Columns.Count; ++i)
        {
          int dataIndex;
          if (_columnToDataColumnIndex.TryGetValue(_gridViewControl.Columns[i], out dataIndex))
          {
            // subtract 1 because the first column in the row number column
            _viewModel.ColumnDisplayIndex[dataIndex] = i-1;
          }
        }
      };
    }


    private void InputFileDrop(object sender, DragEventArgs e)
    {
      try
      {
        if (e.Data is DataObject && ((DataObject)e.Data).ContainsFileDropList())
        {
          Filename = ((DataObject)e.Data).GetFileDropList().Cast<string>().FirstOrDefault();
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }
    }

    private void InputFileDragEnter(object sender, DragEventArgs e)
    {
      var dropPossible = e.Data != null && ((DataObject)e.Data).ContainsFileDropList();
      if (dropPossible)
      {
        e.Effects = DragDropEffects.Copy;
      }
    }

    private void InputFileDragOver(object sender, DragEventArgs e)
    {
      e.Handled = true;
    }

    private void LoadFile_Click(object sender, RoutedEventArgs e) {
      OpenFileDialog dialog = new OpenFileDialog();
      if (dialog.ShowDialog() == true) {
        Filename = dialog.FileName;
      }
    }

    private void RefitColumns_Click(object sender, RoutedEventArgs e) {
      AutoSizeAllColumns();
    }

    private void AutoSizeAllColumns() {
      BigTextFileViewer control = (BigTextFileViewer)this;
      var view = control.lvLineList.View as GridView;
      if (view != null) {
        foreach (var column in view.Columns) {
          if (double.IsNaN(column.Width)) {
            // Reset the column width
            column.Width = column.ActualWidth;
            column.Width = double.NaN;
          }
        }
      }
    }

    public BigTextFileViewModel ViewModel
    {
      get
      {
        return _viewModel;
      }
    }


    public bool BinaryMode
    {
      get { return (bool)GetValue(BinaryModeProperty); }
      set { SetValue(BinaryModeProperty, value); }
    }

    // Using a DependencyProperty as the backing store for BinaryMode.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BinaryModeProperty =
        DependencyProperty.Register("BinaryMode", typeof(bool), typeof(BigTextFileViewer), new PropertyMetadata(false, (d, e) => {
          bool? useBinary = e.NewValue as bool?;
          if (useBinary.HasValue)
          {
            try
            {
              BigTextFileViewer viewer = (BigTextFileViewer)d;
              viewer._viewModel.BinaryMode = useBinary.Value;
            }
            catch (Exception ex)
            {
              System.Diagnostics.Debug.WriteLine(ex.Message);
            }
          }
        }));


    public string Filename
    {
      get { return (string)GetValue(FilenameProperty); }
      set { SetValue(FilenameProperty, value); }
    }

    // Using a DependencyProperty as the backing store for FileToLoad.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FilenameProperty =
        DependencyProperty.Register("Filename", typeof(string), typeof(BigTextFileViewer), new PropertyMetadata(null, (d, e) => {
          if (e.NewValue != null) {
            try
            {
              string newFilename = e.NewValue.ToString();
              if (!string.IsNullOrWhiteSpace(newFilename)) {
                BigTextFileViewer viewer = (BigTextFileViewer)d;
                viewer._viewModel.OpenFile(newFilename, viewer.MaxRows);
              }
            }
            catch (Exception ex)
            {
              MessageBox.Show("Error Opening File", ex.Message);
            }
          }
        }));

    public LinesProvider LinesProvider
    {
      get { return (LinesProvider)GetValue(LinesProviderProperty); }
      set { SetValue(LinesProviderProperty, value); }
    }

    // Using a DependencyProperty as the backing store for LinesProvider.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty LinesProviderProperty =
        DependencyProperty.Register("LinesProvider", typeof(LinesProvider), typeof(BigTextFileViewer), new PropertyMetadata(null, (d, e) =>
        {
          BigTextFileViewer control = (BigTextFileViewer)d;
          LinesProvider linesProvider = e.NewValue as LinesProvider;
          control._viewModel.OpenProvider(linesProvider);
        }));


    public int ColumnsRequired {
      get { return (int)GetValue(ColumnsRequiredProperty); }
      set { SetValue(ColumnsRequiredProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ColumnsRequired.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ColumnsRequiredProperty =
        DependencyProperty.Register("ColumnsRequired", typeof(int), typeof(BigTextFileViewer), new PropertyMetadata(0, (d, e) => {
          BigTextFileViewer control = (BigTextFileViewer)d;
          int? columnsRequired =  e.NewValue as int?;
          var view = control.lvLineList.View as GridView;
          if (columnsRequired.HasValue && view!=null){

            var columns = view.Columns;
            //var headers = control._viewModel.Headers;

            // Add 1 because we have a row number column at the far left
            while (columns.Count > columnsRequired.Value + 1) {
              control._columnToDataColumnIndex.Remove(columns.Last());
              columns.RemoveAt(columns.Count-1);
            }
            while (columns.Count < columnsRequired.Value + 1) {
              // First column is th row number
              int columnNo = columns.Count-1;
              GridViewColumn column = new GridViewColumn();
              control._columnToDataColumnIndex.Add(column, columnNo);
              StackPanel headerStack = new StackPanel();
              TextBlock headerText = new TextBlock();
              headerText.TextAlignment = TextAlignment.Center;
              headerText.HorizontalAlignment = HorizontalAlignment.Center;
              CheckBox headerCheckBox = new CheckBox();
              headerCheckBox.HorizontalAlignment = HorizontalAlignment.Center;
              headerCheckBox.Checked += (s, checkedArgs) => control._viewModel.ColumnsSelected[columnNo] = true;
              headerCheckBox.Unchecked += (s, uncheckedArgs) => control._viewModel.ColumnsSelected[columnNo] = false;

              Binding checkBoxVisiblity = new Binding("ColumnSelectorsVisible");
              checkBoxVisiblity.Converter = VisibilityConverter.Instance;
              headerCheckBox.SetBinding(CheckBox.VisibilityProperty, checkBoxVisiblity);

              headerStack.Children.Add(headerText);
              headerStack.Children.Add(headerCheckBox);

              column.Header = headerStack;
              Binding binding = new Binding(string.Format("Data.ColumnsSafe[{0}]", columnNo));
              column.DisplayMemberBinding = binding;
              columns.Add(column);
              column.Width = double.NaN;

              control.SetColumnHeaderText(columnNo);
            }
          }
        }));



    public bool FileHasHeader
    {
      get { return (bool)GetValue(FileHasHeaderProperty); }
      set { SetValue(FileHasHeaderProperty, value); }
    }

    // Using a DependencyProperty as the backing store for FileHasHeader.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FileHasHeaderProperty =
        DependencyProperty.Register("FileHasHeader", typeof(bool), typeof(BigTextFileViewer), new PropertyMetadata(false, (d, e) => {
          BigTextFileViewer control = (BigTextFileViewer)d;
          BigTextFileViewModel viewModel = control._viewModel;
          bool? hasHeader = e.NewValue as bool?;
          if (!hasHeader.HasValue)
          {
            return;
          }
          viewModel.FileHasHeader = hasHeader.Value;
          var view = control.lvLineList.View as GridView;
          var columns = view.Columns;
          for (int i = 1; i < columns.Count; ++i)
          {
            control.SetColumnHeaderText(i-1);
          }
        }));

    private void SetColumnHeaderText(int columnNumber)
    {
      var view = lvLineList.View as GridView;
      if (view != null)
      {
        var columns = view.Columns;
        var headers = _viewModel.Headers;
        // First columm is the row number so need to offset
        StackPanel stackPanel = columns[columnNumber+1].Header as StackPanel;
        TextBlock headerText = stackPanel.Children.OfType<TextBlock>().FirstOrDefault();
        if (headerText != null)
        {
          headerText.Text = (_viewModel.FileHasHeader && headers.Length > columnNumber) ? headers[columnNumber] : (columnNumber+1).ToString();
        }
      }
    }


    public bool ControlsVisible
    {
      get { return (bool)GetValue(ControlsVisibleProperty); }
      set { SetValue(ControlsVisibleProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ControlsVisible.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ControlsVisibleProperty =
        DependencyProperty.Register("ControlsVisible", typeof(bool), typeof(BigTextFileViewer), new PropertyMetadata(true, (d,e)=> {
          BigTextFileViewer control = (BigTextFileViewer)d;
          bool? controlsVisible = e.NewValue as bool?;
          if (controlsVisible == true)
          {
            control.buttonsGrid.Visibility = System.Windows.Visibility.Visible;
          }
          else if (controlsVisible == false)
          {
            control.buttonsGrid.Visibility = System.Windows.Visibility.Collapsed;
          }
        }));



    public int MaxRows
    {
      get { return (int)GetValue(MaxRowsProperty); }
      set { SetValue(MaxRowsProperty, value); }
    }

    // Using a DependencyProperty as the backing store for MaxRows.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MaxRowsProperty =
        DependencyProperty.Register("MaxRows", typeof(int), typeof(BigTextFileViewer), new PropertyMetadata(Int32.MaxValue));




    public int RowCount
    {
      get { return (int)GetValue(RowCountProperty); }
      set { SetValue(RowCountProperty, value); }
    }

    // Using a DependencyProperty as the backing store for RowCount.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RowCountProperty =
        DependencyProperty.Register("RowCount", typeof(int), typeof(BigTextFileViewer), new PropertyMetadata(0));


  }
}

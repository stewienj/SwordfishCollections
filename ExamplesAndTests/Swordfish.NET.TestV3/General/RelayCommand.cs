using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Swordfish.NET.WPF.ViewModel
{
  /// <summary>
  /// Preferable to use RelayCommandFactory to generate these
  /// </summary>
  public class RelayCommand : ICommand
  {
    #region Fields

    readonly Action<object> execute;
    readonly Predicate<object> canExecute;

    #endregion // Fields

    #region Constructors

    public RelayCommand(Action<object> execute)
      : this(execute, null)
    {
    }

    public RelayCommand(Action<object> execute, Predicate<object> canExecute)
    {
      if (execute == null)
        throw new ArgumentNullException("execute");

      this.execute = execute;
      this.canExecute = canExecute;
    }
    #endregion // Constructors

    #region ICommand Members

    [DebuggerStepThrough]
    public bool CanExecute(object parameter)
    {
      return canExecute == null ? true : canExecute(parameter);
    }

    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public void Execute(object parameter)
    {
      execute(parameter);
    }

    #endregion // ICommand Members
  }


  /// <summary>
  /// Preferred way of creating RelayCommand objects
  /// </summary>
  /// <example>
  ///   private RelayCommandFactory _testCommand = new RelayCommandFactory();
  ///   public ICommand TestCommand
  ///   {
  ///     get
  ///     {
  ///       return _testCommand.GetCommandAsync(async () => await Task.Run(() =>
  ///       {
  ///         for (int i = 0; i < 100; ++i)
  ///         {
  ///           Thread.Sleep(100);
  ///           Progress = i / 99.0;
  ///         }
  ///       }));
  ///     }
  ///   }
  /// </example>
  public class RelayCommandFactory
  {
    private RelayCommand _relayCommand = null;
    private bool _isRunning = false;

    public RelayCommandFactory()
    {

    }

    /// <summary>
    /// Gets the command and blocks execution while the command is running
    /// </summary>
    /// <param name="execute"></param>
    /// <param name="canExecute"></param>
    /// <returns></returns>
    public ICommand GetCommand(Action<object> execute, Predicate<object> canExecute)
    {
      if (_relayCommand == null)
      {
        _relayCommand = new RelayCommand(
          param =>
          {
            _isRunning = true;
            CommandManager.InvalidateRequerySuggested();
            execute(param);
            _isRunning = false;
            CommandManager.InvalidateRequerySuggested();
          },
          param =>
          {
            return !_isRunning && (canExecute!= null ? canExecute(param) : true);
          });
      }
      return _relayCommand;
    }

    public ICommand GetCommand(Action execute, Predicate<object> canExecute)
    {
      return GetCommand(param => execute(), canExecute);
    }
    public ICommand GetCommand(Action<object> execute, Func<bool> canExecute)
    {
      return GetCommand(execute, param => canExecute());
    }
    public ICommand GetCommand(Action execute, Func<bool> canExecute)
    {
      return GetCommand(param => execute(), param => canExecute());
    }
    public ICommand GetCommand(Action<object> execute)
    {
      return GetCommand(execute, (Predicate<object>)null);
    }
    public ICommand GetCommand(Action execute)
    {
      return GetCommand(param => execute(), (Predicate<object>)null);
    }

    public ICommand GetCommandAsync(Func<object, Task> execute, Predicate<object> canExecute)
    {
      if (_relayCommand == null)
      {
        _relayCommand = new RelayCommand(
          param =>
          {
            var context = SynchronizationContext.Current;
            _isRunning = true;
            CommandManager.InvalidateRequerySuggested();
            execute(param).ContinueWith((t) =>
            {
              _isRunning = false;
              if (context != null)
              {
                context.Post(x => CommandManager.InvalidateRequerySuggested(), null);
              }
            });
          },
          param =>
          {
            return !_isRunning && (canExecute != null ? canExecute(param) : true);
          });
      }
      return _relayCommand;
    }

    public ICommand GetCommandAsync(Func<Task> execute, Predicate<object> canExecute)
    {
      return GetCommandAsync(param => execute(), canExecute);
    }
    public ICommand GetCommandAsync(Func<object, Task> execute, Func<bool> canExecute)
    {
      return GetCommandAsync(execute, param => canExecute());
    }
    public ICommand GetCommandAsync(Func<Task> execute, Func<bool> canExecute)
    {
      return GetCommandAsync(param => execute(), param => canExecute());
    }
    public ICommand GetCommandAsync(Func<object, Task> execute)
    {
      return GetCommandAsync(execute, (Predicate<object>)null);
    }
    public ICommand GetCommandAsync(Func<Task> execute)
    {
      return GetCommandAsync(param => execute(), (Predicate<object>)null);
    }

  }

  public class RelayCommandTest : NotifyPropertyChanged 
  {
    private static Lazy<RelayCommandTest> _instance = new Lazy<RelayCommandTest>(true);
    public static RelayCommandTest Instance
    {
      get
      {
        return _instance.Value;
      }
    }

    private RelayCommandFactory _testCommand = new RelayCommandFactory();
    public ICommand TestCommand
    {
      get
      {
        return _testCommand.GetCommandAsync(async () => await Task.Run(() =>
        {
          for (int i = 0; i < 100; ++i)
          {
            Thread.Sleep(100);
            Progress = i / 99.0;
          }
        }));
      }
    }

    private double _progress = 0;
    public double Progress
    {
      get
      {
        return _progress;
      }
      set
      {
        SetValue(ref _progress, value);
      }
    }
  }
}

using Swordfish.NET.Collections;
using Swordfish.NET.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Swordfish.NET.Demo.ViewModels
{
  public class ConcurrentObservableCollectionStressTestViewModel : ConcurrentObservableTestBaseViewModel<string>
  {
    private bool _stopStressTest = true;
    public ConcurrentObservableCollectionStressTestViewModel()
    {
    }

    public ConcurrentObservableCollection<string> StressTestCollection1 { get; } = new ConcurrentObservableCollection<string>(false);
    public ConcurrentObservableCollection<string> StressTestCollection2 { get; } = new ConcurrentObservableCollection<string>(true);

    private RelayCommandFactory _startStressTestCommand1 = new RelayCommandFactory();
    public ICommand StartStressTestCommand1
    {
      get
      {
        return _startStressTestCommand1.GetCommandAsync(async () => await StressTestCollection(StressTestCollection1));
      }
    }

    private RelayCommandFactory _startStressTestCommand2 = new RelayCommandFactory();
    public ICommand StartStressTestCommand2
    {
      get
      {
        return _startStressTestCommand2.GetCommandAsync(async () => await StressTestCollection(StressTestCollection2));
      }
    }

    private async Task StressTestCollection(ConcurrentObservableCollection<string> collection)
    {
      _stopStressTest = false;
      await Task.Factory.StartNew(() =>
      {
        while (!_stopStressTest)
        {
          collection.AddRange(Enumerable.Range(1, 1000).Select(x => $"{x}").ToList());
          while (collection.Count > 0)
          {
            collection.RemoveLast();
          }
          Thread.Sleep(200);
        }
      });
    }

    private RelayCommandFactory _stopStressTestCommand = new RelayCommandFactory();
    public ICommand StopStressTestCommand
    {
      get
      {
        return _stopStressTestCommand.GetCommand(() => _stopStressTest = true);
      }
    }
  }
}

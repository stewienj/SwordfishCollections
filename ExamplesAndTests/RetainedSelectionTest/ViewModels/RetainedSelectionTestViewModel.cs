using RetainedSelectionTest.Data;
using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RetainedSelectionTest.ViewModels
{
    public class RetainedSelectionTestViewModel : ExtendedNotifyPropertyChanged
    {
        private System.Threading.Timer _addItemTimer;

        public RetainedSelectionTestViewModel()
        {
            _addItemTimer = new System.Threading.Timer(AddRandomItemFromTimer, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            for(int i=0; i<5; ++i)
            {
                TestCollection.Add(Person.GetNewRandom());
            }
        }

        /// <summary>
        /// This method is called by the timer delegate. Adds a random item.
        /// </summary>
        private void AddRandomItemFromTimer(Object stateInfo)
        {
            TestCollection.Add(Person.GetNewRandom());
        }

        /// <summary>
        /// The main source source collection. You bind to TestCollection.CollectionView in your view.
        /// </summary>
        public ConcurrentObservableCollection<Person> TestCollection { get; } = new ConcurrentObservableCollection<Person>();

        private Person _person = null;
        public Person SelectedPerson
        {
            get => _person;
            set => SetProperty(ref _person, value);
        }

        private bool _continuouslyAddItems = false;
        public bool ContinuouslyAddItems
        {
            get => _continuouslyAddItems;
            set
            {
                SetProperty(ref _continuouslyAddItems, value);
                if (_continuouslyAddItems)
                {
                    _addItemTimer.Change(0, 1000);
                }
                else
                {
                    _addItemTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                }
            }
        }


        private RelayCommandFactory _addRandomItemCommand = new RelayCommandFactory();
        public ICommand AddRandomItemCommand => _addRandomItemCommand.GetCommand(() => TestCollection.Add(Person.GetNewRandom()));

    }
}

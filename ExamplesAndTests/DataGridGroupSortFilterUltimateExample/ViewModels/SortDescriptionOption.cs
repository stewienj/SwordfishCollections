using DataGridGroupSortFilterUltimateExample.Auxilary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridGroupSortFilterUltimateExample.ViewModels
{
    /// <summary>
    /// Container for a sort description that lets the user toggle it on and off
    /// </summary>
    public class SortDescriptionOption : NotifyPropertyChanged
    {
        public SortDescriptionOption(bool isActive, string propertyName)
        {
            IsActive = isActive;
            PropertyName = propertyName;
        }

        public SortDescription SortDescription { get; private set; } = new SortDescription();

        private bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public string PropertyName
        {
            get => SortDescription.PropertyName;
            set => SortDescription = new SortDescription(value, SortDescription.Direction);
        }

        public bool _ascending = true;
        public bool Ascending
        {
            get => _ascending;
            set
            {
                // It's important all the data is correct before firing the property changed notifications
                if (_ascending!=value)
                {
                    _ascending = value;
                    _descending = !_ascending;
                    SortDescription = new SortDescription(SortDescription.PropertyName, _ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
                    RaisePropertyChanged(nameof(Ascending));
                    RaisePropertyChanged(nameof(Descending));
                }
            }
        }

        private bool _descending = false;
        public bool Descending
        {
            get => _descending;
            set
            {
                if (_descending != value)
                {
                    _descending = value;
                    _ascending = !_descending;
                    SortDescription = new SortDescription(SortDescription.PropertyName, _ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
                    RaisePropertyChanged(nameof(Descending));
                    RaisePropertyChanged(nameof(Ascending));
                }
            }
        }
    }
}

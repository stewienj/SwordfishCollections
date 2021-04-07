
using DataGridGroupSortFilterUltimateExample.Auxilary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DataGridGroupSortFilterUltimateExample.ViewModels
{
    /// <summary>
    /// Container for a group description that lets the user toggle it on and off
    /// </summary>
    public class GroupDescriptionOption : NotifyPropertyChanged
    {
        private bool _isActive = false;

        public GroupDescriptionOption(bool isActive, string propertyName)
        {
            IsActive = isActive;
            PropertyName = propertyName;
        }

        public PropertyGroupDescription GroupDescription { get; } = new PropertyGroupDescription();

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public string PropertyName
        {
            get => GroupDescription.PropertyName;
            private set => GroupDescription.PropertyName = value;
        }
    }
}

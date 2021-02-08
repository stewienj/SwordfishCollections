using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DGGroupSortFilterExampleConcurrent.ViewModels
{
    // Task Class
    // Requires using System.ComponentModel;
    public class ProjectDetails : INotifyPropertyChanged, IEditableObject
    {
        // The Task class implements INotifyPropertyChanged and IEditableObject
        // so that the datagrid can properly respond to changes to the
        // data collection and edits made in the DataGrid.

        // Private task data.
        private string _projectName = string.Empty;
        private string _taskName = string.Empty;
        private DateTime _dueDate = DateTime.Now;
        private bool _complete = false;

        // Data for undoing canceled edits.
        private ProjectDetails _tempTask = null;

        private static int _itemCount = 0;

        public static ProjectDetails GetNewProject()
        {
            var i = Interlocked.Increment(ref _itemCount);
            return new ProjectDetails()
            {
                ProjectName = "Project " + ((i % 3) + 1).ToString(),
                TaskName = "Task " + i.ToString(),
                DueDate = DateTime.Now.AddDays(i),
                Complete = (i % 2 == 0)
            };
        }

        public override string ToString()
        {
            return $"{ProjectName} {TaskName} Due {DueDate} Complete: {Complete}";
        }

        // Public properties.
        public string ProjectName
        {
            get => _projectName;
            set => SetProperty(ref _projectName, value);
        }

        public string TaskName
        {
            get => _taskName;
            set => SetProperty(ref _taskName, value);
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set => SetProperty(ref _dueDate, value);
        }

        public bool Complete
        {
            get => _complete;
            set => SetProperty(ref _complete, value);
        }

        // Implement INotifyPropertyChanged interface.
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T current, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(current, newValue))
            {
                current = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Implement IEditableObject interface.
        public void BeginEdit()
        {
            // Set temp task if is hasn't already been set
            _tempTask = _tempTask ?? MemberwiseClone() as ProjectDetails;
        }

        public void CancelEdit()
        {
            if (_tempTask != null)
            {
                ProjectName = _tempTask.ProjectName;
                TaskName = _tempTask.TaskName;
                DueDate = _tempTask.DueDate;
                Complete = _tempTask.Complete;
                _taskName = null;
            }
        }

        public void EndEdit()
        {
            _tempTask = null;
        }
    }
}

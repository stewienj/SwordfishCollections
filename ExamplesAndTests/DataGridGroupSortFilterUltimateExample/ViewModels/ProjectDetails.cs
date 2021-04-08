using DataGridGroupSortFilterUltimateExample.Auxilary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataGridGroupSortFilterUltimateExample.ViewModels
{
    /// <summary>
    /// ProjectDetails, and example class that is used for the items
    /// in the example collection.
    /// </summary>
    public class ProjectDetails : NotifyPropertyChanged
    {
        private string _projectName = string.Empty;
        private string _taskName = string.Empty;
        private DateTime _dueDate = DateTime.Now;
        private CompleteStatus _status = CompleteStatus.Active;

        private static int _itemCount = 0;

        public static ProjectDetails GetNewProject()
        {
            var i = Interlocked.Increment(ref _itemCount);
            return new ProjectDetails()
            {
                ProjectName = "Project " + ((i % 3) + 1).ToString(),
                TaskName = "Task " + i.ToString().PadLeft(2, '0'),
                DueDate = DateTime.Now.AddDays(i),
                Status = (i % 2 == 0) ? CompleteStatus.Complete : CompleteStatus.Active
            };
        }

        public override string ToString()
        {
            return $"{ProjectName} {TaskName} Due {DueDate} Status: {Status}";
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

        public CompleteStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

    }
}

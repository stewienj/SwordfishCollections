using DGGroupSortFilterExampleConcurrent.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGGroupSortFilterExampleConcurrent.Controls
{
    /// <summary>
    /// An optimised sorter for use with ProjectDetails objects
    /// </summary>
    public class ProjectDetailsSorter : ICustomSorter
    {
        private Func<object, object, int> _comparer = (x, y) => 0;

        private void UpdateComparer()
        {
            int upDown = SortDirection == ListSortDirection.Ascending ? 1 : -1;

            Func<ProjectDetails, ProjectDetails, int> selector = SortMemberPath switch
            {
                nameof(ProjectDetails.ProjectName) => (x, y) => upDown * Comparer<string>.Default.Compare(x.ProjectName, y.ProjectName),
                nameof(ProjectDetails.Complete) => (x, y) => upDown * Comparer<bool>.Default.Compare(x.Complete, y.Complete),
                nameof(ProjectDetails.DueDate) => (x, y) => upDown * Comparer<DateTime>.Default.Compare(x.DueDate, y.DueDate),
                nameof(ProjectDetails.TaskName) => (x, y) => upDown * Comparer<string>.Default.Compare(x.TaskName, y.TaskName),
                _ => (x, y) => 0
            };

            _comparer = (a, b) =>
            {
                if (a is ProjectDetails x && b is ProjectDetails y)
                {
                    return selector(x, y);
                }
                else
                {
                    return 0;
                }
            };
        }

        private ListSortDirection _listSortDirection = ListSortDirection.Ascending;
        public ListSortDirection SortDirection
        {
            get => _listSortDirection;
            set
            {
                _listSortDirection = value;
                UpdateComparer();
            }
        }

        private string _sortMemberPath = string.Empty;
        public string SortMemberPath
        {
            get => _sortMemberPath;
            set
            {
                _sortMemberPath = value;
                UpdateComparer();
            }
        }

        public int Compare(object x, object y) => _comparer(x, y);
    }
}

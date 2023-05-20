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
using timeTrack.ViewModels;

namespace timeTrack.Views {
  /// <summary>
  /// Interaction logic for timeTrackView.xaml
  /// </summary>
  public partial class timeTrackView : UserControl, ICloseable {
    public timeTrackView() {
      InitializeComponent();
    }

    public void Close() {
      this.Close();
    }

    private void DataGrid_SourceUpdated(object sender, DataTransferEventArgs e) {

    }

    private void timeTrackViewWindow_Loaded(object sender, RoutedEventArgs e) {

      if (ListBoxSelectableProjects.Items.Count > 0) {
        ListBoxSelectableProjects.SelectedIndex = ListBoxSelectableProjects.Items.Count - 1;
        ListBoxSelectableProjects.ScrollIntoView(ListBoxSelectableProjects.SelectedItem);
      }
    }

    private void Dtg_taskList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e) {
      // items in .\timeTrack\Model\TaskItem.cs
      switch (e.PropertyName) {
        case "TaskName":
          e.Column.Header = "Task";
          break;

        case "Start":
          e.Column.Header = "Start";
          break;

        case "DurationSession":
          e.Column.Header = "Session";
          break;

        case "DurationTaskTotalDay":
          e.Column.Header = "Task Day";
          break;

        case "DurationAllTasksDay":
          e.Column.Header = "All Tasks Day";
          break;

        default:
          break;
      }
    }
  }
}

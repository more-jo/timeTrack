namespace timeTrack.ViewModels
{
  using System;
  using System.Windows.Input;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Timers;
  using System.Windows;
  using CommunityToolkit.Mvvm.Input;
  using Microsoft.Win32;
  using timeTrack.Views;
  using Newtonsoft.Json;
  using System.Windows.Controls;

  public class TimeTrackVM : ObservableObject
  {
    #region fields

    private ObservableCollection<string> selectableProjects = new ObservableCollection<string>();
    private string selectedTaskNameItem;
    private string textBoxContentInputProjects;
    private ObservableCollection<TaskItem> taskList = new ObservableCollection<TaskItem>();
    private Stopwatch stopWatch;
    private string pathProject = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\projects.txt";
    private string pathTaskMeasurements = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\taskList.json";

    #endregion

    #region constructor

    public TimeTrackVM()
    {
      LoadProjectList();
      LoadTimeTableList();
      CreateTestData();
    }

    private void CreateTestData()
    {
      taskList.Add(new TaskItem { TaskName = "test", End = DateTime.Now, Start = DateTime.Now });
    }

    #endregion

    #region privates

    private void CloseWindowMethod()
    {
      Application.Current.Shutdown();
    }

    private void StartTimer(DataGrid tasksListViewItem)
    {
      if (string.IsNullOrEmpty(selectedTaskNameItem))
      {
        MessageBox.Show("Please select a task.");
        return;
      };

      if (stopWatch == null)
      {
        stopWatch = new Stopwatch();
      }
      
      if (stopWatch.IsRunning)
      {
        stopWatch.Stop();
        var indexOfLastItem = taskList.IndexOf(taskList.Last());
        taskList[indexOfLastItem].End = DateTime.Now;
        taskList[indexOfLastItem].Duration = stopWatch.Elapsed;
      }
      
      stopWatch.Start();
      taskList.Add(new TaskItem
      {
        TaskName = selectedTaskNameItem,
        Start = DateTime.Now,
      });

      // todo: set focus in list to last item.
      RaisePropertyChangedEvent(nameof(Tasklist));
      tasksListViewItem.Items.Refresh();
    }

    public void AppendProjectToList()
    {
      if (string.IsNullOrEmpty(textBoxContentInputProjects))
      {
        return;
      }

      selectableProjects.Add(textBoxContentInputProjects);

      TextBoxContentInputProjects = string.Empty;
    }

    private void RemoveEntryFromList()
    {
      var selectedItem = ListBoxSelectableProjectsSelectedItem;
      if (SelectableProjects.Contains(selectedItem))
      {
        SelectableProjects.Remove(selectedItem);
      }
    }

    private void EmptyProjectsList()
    {
      SelectableProjects.Clear();
    }

    private void EmptyTimeTableList()
    {
      if (stopWatch != null && stopWatch.IsRunning)
      {
        stopWatch.Stop();
        stopWatch.Reset();
      }

      taskList.Clear();

      RaisePropertyChangedEvent(nameof(Tasklist));
    }

    private void SaveProjectList()
    {
      pathProject = GetPathSaveDialog();
      if (string.IsNullOrEmpty(pathProject)) return;

      if (!File.Exists(pathProject))
      {
        using (FileStream fs = File.Create(pathProject))
        {
        }
      }

      if (!File.Exists(pathProject))
      {
        throw new AccessViolationException($"Cannot access : {pathProject}");
      }

      using (StreamWriter sw = File.CreateText(pathProject))
      {
        foreach (string selectableProject in SelectableProjects)
        {
          sw.WriteLine(selectableProject);
        }
      }
    }

    private void LoadProjectList()
    {
      if (File.Exists(pathProject))
      {
        using (StreamReader sr = File.OpenText(pathProject))
        {
          if (SelectableProjects.Count != 0)
          {
            var emptyListeUserDecision = MessageBox.Show("Shall the list be emptied?", "Empty list?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.None);

            if (emptyListeUserDecision == MessageBoxResult.Yes)
            {
              SelectableProjects.Clear();
            }
          }

          string s;
          while ((s = sr.ReadLine()) != null)
          {
            SelectableProjects.Add(s);
          }
        }
      }
    }

    private string GetPathSaveDialog()
    {
      SaveFileDialog saveFileDialog = new SaveFileDialog()
      {
        Title = "Save to",
        DefaultExt = "json",
        Filter = "json files (*.json)|*.json|All files (*.*)|*.*",
        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
      };

      saveFileDialog.ShowDialog();
      var path = saveFileDialog.FileName;

      if (string.IsNullOrEmpty(path)) return string.Empty;

      return path;
    }

    private void SelectAnotherProjectList()
    {
      pathProject = DialogFilePath();
      LoadProjectList();
    }

    private void SelectLoadAnotherTimeList()
    {
      if(stopWatch != null && stopWatch.IsRunning)
      {
        stopWatch.Stop();
        stopWatch.Reset();
      }

      pathTaskMeasurements = DialogFilePath();
      LoadTimeTableList();
    }

    private void SaveTimeTableList()
    {
      pathTaskMeasurements = GetPathSaveDialog();

      if (string.IsNullOrEmpty(pathTaskMeasurements)) return;

      if (!File.Exists(pathTaskMeasurements))
      {
        using (FileStream fs = File.Create(pathTaskMeasurements))
        {
        }
      }

      if (File.Exists(pathTaskMeasurements))
      {
        var tasListAsJson = JsonConvert.SerializeObject(taskList, Formatting.Indented);
        File.WriteAllText(pathTaskMeasurements, tasListAsJson);
      }
    }

    private void LoadTimeTableList()
    {
      if (!File.Exists(pathTaskMeasurements)) return;

      var readOutFile = File.ReadAllText(pathTaskMeasurements);
      try
      {
        var tasks = JsonConvert.DeserializeObject<ObservableCollection<TaskItem>>(readOutFile);
        taskList = tasks;
        RaisePropertyChangedEvent(nameof(Tasklist));
      }
      catch(Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private string DialogFilePath()
    {
      var openFileDialog = new OpenFileDialog()
      {
        Title = "Open file.",
        DefaultExt = "json",
        Filter = "json files (*.json)|*.json|All files (*.*)|*.*",
        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        CheckFileExists = true,
        Multiselect = false,
      };

      openFileDialog.ShowDialog();
      var path = openFileDialog.FileName;
      if (string.IsNullOrEmpty(path)) return string.Empty;

      return path;
    }

    #endregion

    #region properties
    public ICommand AddProjectCommand => new DelegateCommand(AppendProjectToList);

    public ICommand RemoveProjectEntryCommand => new DelegateCommand(RemoveEntryFromList);

    public ICommand EmptyProjectListCommand => new DelegateCommand(EmptyProjectsList);

    public ICommand ProjectListSaveCommand => new DelegateCommand(SaveProjectList);

    public ICommand SelectLoadAnotherProjectListCommand => new DelegateCommand(SelectAnotherProjectList);

    public ICommand TimeTableListSaveCommand => new DelegateCommand(SaveTimeTableList);

    public ICommand SelectAnotherTimeListCommand => new DelegateCommand(SelectLoadAnotherTimeList);

    public ICommand EmptyTimeListCommand => new DelegateCommand(EmptyTimeTableList);

    public ICommand StartTimeMeasureCommand => new RelayCommand<DataGrid>(StartTimer);

    public ICommand CloseWindowCommand => new DelegateCommand(CloseWindowMethod);

    public ObservableCollection<string> SelectableProjects
    {
      get => selectableProjects;
      set
      {
        selectableProjects = value;
        RaisePropertyChangedEvent(nameof(SelectableProjects));
      }
    }

    public ObservableCollection<TaskItem> Tasklist
    {
      get => taskList;
      set
      {
        taskList = value;
        RaisePropertyChangedEvent(nameof(Tasklist));
      }
    }

    public string ListBoxSelectableProjectsSelectedItem
    {
      get => selectedTaskNameItem;
      set
      {
        selectedTaskNameItem = value;
        RaisePropertyChangedEvent(nameof(TextBoxContentInputProjects));
      }
    }

    public string TextBoxContentInputProjects
    {
      get => textBoxContentInputProjects;
      set
      {
        textBoxContentInputProjects = value;
        RaisePropertyChangedEvent(nameof(TextBoxContentInputProjects));
      }
    }

    #endregion
  }
}
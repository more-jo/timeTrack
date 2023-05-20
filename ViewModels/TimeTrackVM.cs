namespace timeTrack.ViewModels {
  using System;
  using System.Windows.Input;
  using System.Collections.ObjectModel;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Windows;
  using CommunityToolkit.Mvvm.Input;
  using Microsoft.Win32;
  using timeTrack.Views;
  using Newtonsoft.Json;
  using System.Windows.Controls;

  public class TimeTrackVM : ObservableObject {
    #region fields

    private ObservableCollection<string> selectableProjects = new ObservableCollection<string>();
    private string selectedTaskNameItem;
    private string textBoxContentInputProjects;
    private ObservableCollection<TaskItem> taskList = new ObservableCollection<TaskItem>();
    private Stopwatch stopWatch;
    private string pathProject;
    private string pathTaskMeasurements;

    #endregion

    #region constructor

    public TimeTrackVM() {
      SetProjecFilePathToDefault();
      SetTaskListFilePathToDefault();

      LoadProjectList();
      LoadTimeTableList();
      //CreateTestData();
    }

    private void CreateTestData() {
      taskList.Add(new TaskItem { TaskName = "test", End = DateTime.Now, Start = DateTime.Now });
    }

    #endregion

    #region privates

    private void SetProjecFilePathToDefault() {
      pathProject = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\projects.json";
    }

    private void SetTaskListFilePathToDefault() {
      pathTaskMeasurements = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\taskList.json";
    }

    private void CloseWindowMethod() {
      Application.Current.Shutdown();
    }

    private void StartTimer(DataGrid tasksListDataGrid) {
      if (string.IsNullOrEmpty(selectedTaskNameItem)) {
        MessageBox.Show("Please select a task.");
        return;
      };

      if (stopWatch == null) {
        stopWatch = new Stopwatch();
      }

      if (stopWatch.IsRunning) {
        stopWatch.Stop();
        AddtimesToLastEntry();
        CalculateSumDurationTask();
        CalculateSumDurationToday();
        stopWatch.Reset();
      }

      stopWatch.Start();
      taskList.Add(new TaskItem {
        TaskName = selectedTaskNameItem,
        Start = DateTime.Now,
      });

      RaisePropertyChangedEvent(nameof(Tasklist));

      tasksListDataGrid.Items.Refresh();

      tasksListDataGrid.SelectedIndex = tasksListDataGrid.Items.Count - 1;
      tasksListDataGrid.ScrollIntoView(tasksListDataGrid.Items[tasksListDataGrid.Items.Count - 1]);
    }

    private void CalculateSumDurationToday() {
      TimeSpan timeSpan = TimeSpan.Zero;
      var indexOfLastItem = taskList.IndexOf(taskList.Last());

      foreach (TaskItem task in taskList) {
        timeSpan = timeSpan.Add(task.DurationSession);
      }

      taskList[indexOfLastItem].DurationAllTasksDay = timeSpan;
    }

    private void CalculateSumDurationTask() {
      TimeSpan timeSpan = TimeSpan.Zero;
      var indexOfLastItem = taskList.IndexOf(taskList.Last());
      var lastTaskName = taskList[indexOfLastItem].TaskName;

      foreach (TaskItem task in taskList) {
        if (task.TaskName == lastTaskName && task.Start.Day == DateTime.Now.Day) {
          timeSpan = timeSpan.Add(task.DurationSession);
        }
      }

      taskList[indexOfLastItem].DurationTaskTotalDay = timeSpan;
    }

    private void AddtimesToLastEntry() {
      var indexOfLastItem = taskList.IndexOf(taskList.Last());
      taskList[indexOfLastItem].End = DateTime.Now;
      taskList[indexOfLastItem].DurationSession = stopWatch.Elapsed;
    }

    public void AppendProjectToList() {
      if (string.IsNullOrEmpty(textBoxContentInputProjects)) {
        return;
      }

      selectableProjects.Add(textBoxContentInputProjects);

      TextBoxContentInputProjects = string.Empty;
    }

    private void RemoveEntryFromList() {
      var selectedItem = ListBoxSelectableProjectsSelectedItem;
      if (SelectableProjects.Contains(selectedItem)) {
        SelectableProjects.Remove(selectedItem);
      }
    }

    private void EmptyProjectsList() {
      SelectableProjects.Clear();
    }

    private void EmptyTimeTableList() {
      if (stopWatch != null && stopWatch.IsRunning) {
        stopWatch.Stop();
        stopWatch.Reset();
      }

      taskList.Clear();

      RaisePropertyChangedEvent(nameof(Tasklist));
    }

    private void SaveAs_ProjectList() {
      var path = GetPathSaveDialog();
      if (string.IsNullOrEmpty(path)) {
        SetProjecFilePathToDefault();
      }
      else {
        pathProject = path;
      };

      SaveProjectList();
    }

    private void SaveProjectList() {
      if (string.IsNullOrEmpty(pathProject)) {
        SetProjecFilePathToDefault();
      };

      if (!File.Exists(pathProject)) {
        using (FileStream fs = File.Create(pathProject)) {
        }
      }

      if (!File.Exists(pathProject)) {
        throw new AccessViolationException($"Cannot access : {pathProject}");
      }

      using (StreamWriter sw = File.CreateText(pathProject)) {
        foreach (string selectableProject in SelectableProjects) {
          sw.WriteLine(selectableProject);
        }
      }
    }

    private void SaveAs_TimeTableList() {
      var path = GetPathSaveDialog();
      if (string.IsNullOrEmpty(path)) {
        SetTaskListFilePathToDefault();
      }
      else {
        pathTaskMeasurements = path;
      };

      SaveTimeTableList();
    }

    private void SaveTimeTableList() {
      if (string.IsNullOrEmpty(pathTaskMeasurements)) {
        SetTaskListFilePathToDefault();
      };

      if (!File.Exists(pathTaskMeasurements)) {
        using (FileStream fs = File.Create(pathTaskMeasurements)) {
        }
      }

      if (File.Exists(pathTaskMeasurements)) {
        var tasListAsJson = JsonConvert.SerializeObject(taskList, Formatting.Indented);
        File.WriteAllText(pathTaskMeasurements, tasListAsJson);
      }
    }

    private void SelectAnotherProjectList() {
      pathProject = DialogFilePath();
      LoadProjectList();
    }

    private void LoadProjectList() {
      if (File.Exists(pathProject)) {
        using (StreamReader sr = File.OpenText(pathProject)) {
          if (SelectableProjects.Count != 0) {
            var emptyListeUserDecision = MessageBox.Show("Shall the list be emptied?", "Empty list?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.None);

            if (emptyListeUserDecision == MessageBoxResult.Yes) {
              SelectableProjects.Clear();
            }
          }

          string s;
          while ((s = sr.ReadLine()) != null) {
            SelectableProjects.Add(s);
          }
        }
      }
    }

    private string GetPathSaveDialog() {
      SaveFileDialog saveFileDialog = new SaveFileDialog() {
        Title = "Save to",
        DefaultExt = "json",
        Filter = "json files (*.json)|*.json|All files (*.*)|*.*",
        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
      };

      saveFileDialog.ShowDialog();
      var path = saveFileDialog.FileName;

      if (string.IsNullOrEmpty(path))
        return string.Empty;

      return path;
    }

    private void SelectLoadAnotherTimeList() {
      if (stopWatch != null && stopWatch.IsRunning) {
        stopWatch.Stop();
        stopWatch.Reset();
      }

      pathTaskMeasurements = DialogFilePath();
      LoadTimeTableList();
    }

    private void LoadTimeTableList() {
      if (!File.Exists(pathTaskMeasurements))
        return;

      var readOutFile = File.ReadAllText(pathTaskMeasurements);
      try {
        var tasks = JsonConvert.DeserializeObject<ObservableCollection<TaskItem>>(readOutFile);
        taskList = tasks;
        RaisePropertyChangedEvent(nameof(Tasklist));
      }
      catch (Exception ex) {
        MessageBox.Show("Error in time table list:" + Environment.NewLine + ex.Message);
      }
    }

    private string DialogFilePath() {
      var openFileDialog = new OpenFileDialog() {
        Title = "Open file.",
        DefaultExt = "json",
        Filter = "json files (*.json)|*.json|All files (*.*)|*.*",
        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        CheckFileExists = true,
        Multiselect = false,
      };

      openFileDialog.ShowDialog();
      var path = openFileDialog.FileName;
      if (string.IsNullOrEmpty(path))
        return string.Empty;

      return path;
    }

    #endregion

    #region properties

    public ICommand AddProjectCommand => new DelegateCommand(AppendProjectToList);
    public ICommand CloseWindowCommand => new DelegateCommand(CloseWindowMethod);
    public ICommand DetailCmd => new RelayCommand<TaskItem>(ShowTimeListDetails);
    public ICommand EmptyProjectListCommand => new DelegateCommand(EmptyProjectsList);
    public ICommand EmptyTimeListCommand => new DelegateCommand(EmptyTimeTableList);
    public ICommand RemoveProjectEntryCommand => new DelegateCommand(RemoveEntryFromList);
    public ICommand SaveProjectListCmd => new DelegateCommand(SaveProjectList);
    public ICommand SaveAs_ProjectListCmd => new DelegateCommand(SaveAs_ProjectList);
    public ICommand SelectAnotherTimeListCommand => new DelegateCommand(SelectLoadAnotherTimeList);
    public ICommand SelectLoadAnotherProjectListCommand => new DelegateCommand(SelectAnotherProjectList);
    public ICommand StartTimeMeasureCommand => new RelayCommand<DataGrid>(StartTimer);
    public ICommand SaveTimeTableListCmd => new DelegateCommand(SaveTimeTableList);
    public ICommand SaveAs_TimeTableListCmd => new DelegateCommand(SaveAs_TimeTableList);

    private void ShowTimeListDetails(TaskItem selectedItem) {
      DetailView dialog = new DetailView();
      (dialog.DataContext as DetailVM).GridSelectedTaskItem = selectedItem;
      dialog.ShowDialog();
    }

    public ObservableCollection<string> SelectableProjects {
      get => selectableProjects;
      set {
        selectableProjects = value;
        RaisePropertyChangedEvent(nameof(SelectableProjects));
      }
    }

    public ObservableCollection<TaskItem> Tasklist {
      get => taskList;
      set {
        taskList = value;
        RaisePropertyChangedEvent(nameof(Tasklist));
      }
    }

    public string ListBoxSelectableProjectsSelectedItem {
      get => selectedTaskNameItem;
      set {
        selectedTaskNameItem = value;
        RaisePropertyChangedEvent(nameof(TextBoxContentInputProjects));
      }
    }

    public string TextBoxContentInputProjects {
      get => textBoxContentInputProjects;
      set {
        textBoxContentInputProjects = value;
        RaisePropertyChangedEvent(nameof(TextBoxContentInputProjects));
      }
    }

    #endregion
  }
}
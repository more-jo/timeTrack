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

  public class TimeTrackVM : ObservableObject
  {
    private ObservableCollection<string> selectableProjects = new ObservableCollection<string>();
    private string selectedItem;
    private string textBoxContentInputProjects;
    private ObservableCollection<string> timeMeasured = new ObservableCollection<string>();
    private Stopwatch stopWatch;
    private string _listBoxDisplaySelectedItem;
    private string pathProject = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\projects.txt";
    private string pathTimeTable = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @".\timeTable.txt";

    public string TextBoxContentInputProjects
    {
      get => textBoxContentInputProjects;
      set
      {
        textBoxContentInputProjects = value;
        RaisePropertyChangedEvent(nameof(TextBoxContentInputProjects));
      }
    }

    public ICommand AddProjectCommand => new DelegateCommand(AppendProjectToList);

    public ICommand RemoveProjectEntryCommand => new DelegateCommand(RemoveEntryFromList);

    public ICommand EmptyProjectListCommand => new DelegateCommand(EmptyProjectsList);

    public ICommand ProjectListSaveCommand => new DelegateCommand(SaveProjectList);

    public ICommand SelectLoadAnotherProjectListCommand => new DelegateCommand(SelectLoadAnotherProjectList);

    public ICommand TimeTableListSaveCommand => new DelegateCommand(SaveTimeTableList);

    public ICommand TimeTableListLoadCommand => new DelegateCommand(LoadTimeTableList);

    public ICommand EmptyTimeListCommand => new DelegateCommand(EmptyTimeTableList);

    public ICommand StartTimeMeasureCommand => new DelegateCommand(StartTimer);

    public ICommand CloseWindowCommand => new DelegateCommand(CloseWindowMethod2);


    private void CloseWindowMethod2()
    {
      Application.Current.Shutdown();
    }

    public ObservableCollection<string> SelectableProjects
    {
      get => selectableProjects;
      set
      {
        selectableProjects = value;
        RaisePropertyChangedEvent(nameof(SelectableProjects));
      }
    }

    public ObservableCollection<string> TimeMeasured
    {
      get => timeMeasured;
      set
      {
        selectableProjects = value;
        RaisePropertyChangedEvent(nameof(TimeMeasured));
      }
    }

    public string ListBoxDisplaySelectedItem
    {
      get
      {
        if (TimeMeasured.Count > 3)
        {
          return _listBoxDisplaySelectedItem = TimeMeasured[TimeMeasured.Count - 3];
        }

        return string.Empty;
      }
      set
      {
        _listBoxDisplaySelectedItem = value;
        RaisePropertyChangedEvent(nameof(ListBoxDisplaySelectedItem));
      }
    }

    public string ListBoxSelectableProjectsSelectedItem
    {
      get => selectedItem;
      set
      {
        selectedItem = value;
        RaisePropertyChangedEvent(nameof(TextBoxContentInputProjects));
      }
    }

    public TimeTrackVM()
    {
      LoadProjectList();
      LoadTimeTableList();
    }

    private void StartTimer()
    {
      if (string.IsNullOrEmpty(selectedItem)) return;

      if (stopWatch == null)
      {
        stopWatch = new Stopwatch();
      }

      if (stopWatch.IsRunning)
      {
        stopWatch.Stop();
        timeMeasured.Add($"{"Stop:",-8}{DateTime.Now}");
        //_timeMeasured.Add($"{_stopWatch.Elapsed}");

        AddDurationToTask();
      }

      stopWatch.Start();
      timeMeasured.Add($"{"Task:",-8} {selectedItem}");
      timeMeasured.Add($"{"Start:",-8} {DateTime.Now}");

      RaisePropertyChangedEvent(nameof(ListBoxDisplaySelectedItem));
    }

    private void AddDurationToTask()
    {
      var lastIndexWithTask = GetIndexOfLastEntryWithTask();
      if (lastIndexWithTask >= 0)
      {
        var valueBefore = timeMeasured[lastIndexWithTask];
        var valueAfter = valueBefore + ": " + stopWatch.Elapsed;
        timeMeasured[lastIndexWithTask] = valueAfter;
      }

      timeMeasured.Add(string.Empty);
    }

    private int GetIndexOfLastEntryWithTask()
    {
      var lastIndex = timeMeasured.IndexOf(timeMeasured.Last());

      while (timeMeasured.Contains("Task") || lastIndex != -1)
      {
        if (timeMeasured[lastIndex].Contains("Task"))
        {
          return lastIndex;
        }

        if (lastIndex == timeMeasured.IndexOf(timeMeasured.First()))
        {
          MessageBox.Show($"No Entry found");
          throw new ArgumentOutOfRangeException("There is no entry for Task.");
        }

        lastIndex--;
      }

      return -1;
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
      TimeMeasured.Clear();
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
          if(SelectableProjects.Count != 0)
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

    private string GetFilePath()
    {
      var openFileDialog = new OpenFileDialog()
      {
        Title = "Open file.",
        DefaultExt = "txt",
        Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        CheckFileExists = true,
        Multiselect = false,
      };

      openFileDialog.ShowDialog();
      var path = openFileDialog.FileName;
      if (string.IsNullOrEmpty(path)) return string.Empty;

      return path;
    }

    private string GetPathSaveDialog()
    {
      SaveFileDialog saveFileDialog = new SaveFileDialog()
      {
        Title = "Save to",
        DefaultExt = "txt",
        Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
      };

      saveFileDialog.ShowDialog();
      var path = saveFileDialog.FileName;

      if (string.IsNullOrEmpty(path)) return string.Empty;

      return path;
    }


    private void SelectLoadAnotherProjectList()
    {
      pathProject = GetFilePath();
      LoadProjectList();
    }

    private void SaveTimeTableList()
    {     

      if (!File.Exists(pathTimeTable))
      {
        using (FileStream fs = File.Create(pathTimeTable))
        {
        }
      }

      if (File.Exists(pathTimeTable))
      {
        using (StreamWriter sw = File.CreateText(pathTimeTable))
        {
          foreach (string listEntry in TimeMeasured)
          {
            sw.WriteLine(listEntry);
          }
        }
      }
    }

    private void LoadTimeTableList()
    {
      if (File.Exists(pathTimeTable))
      {
        using (StreamReader sr = File.OpenText(pathTimeTable))
        {
          string s;
          while ((s = sr.ReadLine()) != null)
          {
            TimeMeasured.Add(s);
          }
        }
      }
    }

    //private void CloseWindow(ICloseable window)
    //{
    //  if (window != null)
    //  {
    //    window.Close();
    //  }
    //}
  }
}
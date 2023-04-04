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
    private ObservableCollection<string> _selectableProjects = new ObservableCollection<string>();
    private string _selectedItem;
    private string _textBoxContentInputProjects;
    private ObservableCollection<string> _timeMeasured = new ObservableCollection<string>();
    private Stopwatch _stopWatch;
    private string _listBoxDisplaySelectedItem;
    private string _pathProjects = @".\projects.txt";
    private string _pathTimeTable = @".\timeTable.txt";


    //public Action<ICloseable> CloseWindowCommand { get; private set; }
    //https://stackoverflow.com/questions/16172462/close-window-from-viewmodel

    public string TextBoxContentInputProjects
    {
      get => _textBoxContentInputProjects;
      set
      {
        _textBoxContentInputProjects = value;
        RaisePropertyChangedEvent(nameof(TextBoxContentInputProjects));
      }
    }

    public ICommand AddProject => new DelegateCommand(AppendProjectToList);

    public ICommand RemoveProjectEntry => new DelegateCommand(RemoveEntryFromList);
    
    public ICommand EmptyProjectList => new DelegateCommand(EmptyProjectsList);
    
    public ICommand ProjectListSave => new DelegateCommand(SaveProjectList);

    public ICommand ProjectListLoad => new DelegateCommand(LoadProjectList);

    public ICommand TimeTableListSave => new DelegateCommand(SaveTimeTableList);

    public ICommand TimeTableListLoad => new DelegateCommand(LoadTimeTableList);

    public ICommand EmptyTimeList => new DelegateCommand(EmptyTimeTableList);

    public ICommand StartTimeMeasure => new DelegateCommand(StartTimer);

    public ICommand CloseWindowCommand => new DelegateCommand(CloseWindowMethod2);
    

    private void CloseWindowMethod2()
    {
      Application.Current.Shutdown();
    }

    public ObservableCollection<string> SelectableProjects
    {
      get => _selectableProjects;
      set
      {
        _selectableProjects = value;
        RaisePropertyChangedEvent(nameof(SelectableProjects));
      }
    }

    public ObservableCollection<string> TimeMeasured
    {
      get => _timeMeasured;
      set
      {
        _selectableProjects = value;
        RaisePropertyChangedEvent(nameof(TimeMeasured));
      }
    }
    
    public string ListBoxDisplaySelectedItem 
    {
      get
      {
        if(TimeMeasured.Count > 3)
        {
          return _listBoxDisplaySelectedItem = TimeMeasured[TimeMeasured.Count - 3] ;
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
      get => _selectedItem;
      set
      {
        _selectedItem = value;
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
      if (string.IsNullOrEmpty(_selectedItem)) return;

      if (_stopWatch == null)
      {
        _stopWatch = new Stopwatch();
      }

      if (_stopWatch.IsRunning)
      {
        _stopWatch.Stop();
        _timeMeasured.Add($"{"Stop:",-8}{DateTime.Now}");
        //_timeMeasured.Add($"{_stopWatch.Elapsed}");

        AddDurationToTask();
      }

      _stopWatch.Start();
      _timeMeasured.Add($"{"Task:",-8} {_selectedItem}");
      _timeMeasured.Add($"{"Start:",-8} {DateTime.Now}");

      RaisePropertyChangedEvent(nameof(ListBoxDisplaySelectedItem));
    }

    private void AddDurationToTask()
    {
      var lastIndexWithTask = GetIndexOfLastEntryWithTask();
      if (lastIndexWithTask >= 0)
      {
        var valueBefore = _timeMeasured[lastIndexWithTask];
        var valueAfter = valueBefore + ": " + _stopWatch.Elapsed;
        _timeMeasured[lastIndexWithTask] = valueAfter;
      }

      _timeMeasured.Add(string.Empty);
    }

    private int GetIndexOfLastEntryWithTask()
    {
      var lastIndex = _timeMeasured.IndexOf(_timeMeasured.Last());

      while (_timeMeasured.Contains("Task") || lastIndex != -1)
      {
        if (_timeMeasured[lastIndex].Contains("Task"))
        {
          return lastIndex;
        }

        if (lastIndex == _timeMeasured.IndexOf(_timeMeasured.First()))
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
      if (string.IsNullOrEmpty(_textBoxContentInputProjects))
      {
        return;
      }
      
      _selectableProjects.Add(_textBoxContentInputProjects);
      
      TextBoxContentInputProjects = String.Empty;
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
      if (!File.Exists(_pathProjects))
      {
        using (FileStream fs = File.Create(_pathProjects))
        {
        }
      }

      if (!File.Exists(_pathProjects))
      {
        throw new AccessViolationException($"Cannot access : {_pathProjects}");
      }
      else
      {
        // Create a file to write to.
        using (StreamWriter sw = File.CreateText(_pathProjects))
        {
          foreach (string selectableProject in SelectableProjects)
          {
            sw.WriteLine(selectableProject);
          }
        }
      }
    }

    private void LoadProjectList()
    {
      if(File.Exists(_pathProjects)){
        using (StreamReader sr = File.OpenText(_pathProjects))
        {
          string s;
          while ((s = sr.ReadLine()) != null)
          {
            SelectableProjects.Add(s);
          }
        }
      }
    }

    private void SaveTimeTableList()
    {
      if (!File.Exists(_pathTimeTable))
      {
        using (FileStream fs = File.Create(_pathTimeTable))
        {
        }
      }

      if (File.Exists(_pathTimeTable))
      {
        using (StreamWriter sw = File.CreateText(_pathTimeTable))
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
      if (File.Exists(_pathTimeTable))
      {
        using (StreamReader sr = File.OpenText(_pathTimeTable))
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
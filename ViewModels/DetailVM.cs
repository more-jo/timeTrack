using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace timeTrack.ViewModels
{
  public class DetailVM : INotifyPropertyChanged
  {
    private TaskItem gridSelectedTaskItem;

    private void CopyToClipboard(string str)
    {
      Clipboard.SetText(str);
    }

    #region Properties

    public TaskItem GridSelectedTaskItem 
    { 
      get => gridSelectedTaskItem;
      set
      {
        gridSelectedTaskItem = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GridSelectedTaskItem))); 
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ICommand CopyToClipboardCmd => new RelayCommand<string>(CopyToClipboard);
    public ICommand CloseWindowCmd => new RelayCommand<FrameworkElement>(CloseWindowCommand);

    private void CloseWindowCommand(FrameworkElement window)
    {
      (window as Window).Close();
    }

    #endregion
  }
}

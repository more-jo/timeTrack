namespace timeTrack.ViewModels
{
  using CommunityToolkit.Mvvm.Input;
  using GalaSoft.MvvmLight;
  using Microsoft.Win32;

  public class OpenFileDialogVM : ViewModelBase
  {
    public static RelayCommand OpenCommand { get; set; }
    private string _selectedPath;
    public string SelectedPath
    {
      get => _selectedPath;
      set
      {
        _selectedPath = value;
        RaisePropertyChanged(nameof(SelectedPath));
      }
    }

    private string _defaultPath = @"c:\";

    public OpenFileDialogVM()
    {
      RegisterCommands();
    }

    public OpenFileDialogVM(string defaultPath)
    {
      _defaultPath = defaultPath;
      RegisterCommands();
    }

    private void RegisterCommands()
    {
      OpenCommand = new RelayCommand(ExecuteOpenFileDialog);
    }

    private void ExecuteOpenFileDialog()
    {
      var dialog = new OpenFileDialog { InitialDirectory = _defaultPath };
      dialog.ShowDialog();

      SelectedPath = dialog.FileName;
    }
  }
}
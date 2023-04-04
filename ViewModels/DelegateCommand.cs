namespace timeTrack.ViewModels
{
  using System;
  using System.Windows.Input;

  public class DelegateCommand : ICommand
  {
    private readonly Action _action;

    public DelegateCommand(Action action)
    {
      _action = action;
    }

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      _action();
    }

#pragma warning disable 67
    // better show user why something cannot be done
    public event EventHandler CanExecuteChanged { add { } remove { } }
#pragma warning restore 67
  }
}
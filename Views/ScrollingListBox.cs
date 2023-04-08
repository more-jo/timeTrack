namespace timeTrack.Views
{
  using System.Windows.Controls;

  public class ScrollingListBox : ListBox
  {
    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      ScrollAutomaticallyLastEntry(e);
    }

    private void ScrollAutomaticallyLastEntry(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      if (e.NewItems != null)
      {
        int newItemCount = e.NewItems.Count;

        if (newItemCount > 0)
        {
          this.ScrollIntoView(e.NewItems[newItemCount - 1]);
        }

        base.OnItemsChanged(e);
      }
    }
  }

  // todo: copy items: https://jamesmccaffrey.wordpress.com/2012/09/13/copying-a-wpf-listbox-selected-item-via-ctrl-c-and-a-context-menu/
}
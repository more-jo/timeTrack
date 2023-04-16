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
}
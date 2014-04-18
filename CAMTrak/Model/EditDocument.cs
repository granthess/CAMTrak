using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.AvalonDock.Layout;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using Xceed.Wpf.AvalonDock;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Zoombox;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Collections.Specialized;
using CAMTrak.Model.TrackItems;
using CAMTrak.Model.Scale;
using CAMTrak.MathUtils;

namespace CAMTrak.Model
{
    public class EditDocument : ViewModelBase
    {
        #region Notified Properties
        
        private double _Width;
        private void SetWidth(double value)
        {
            Set<double>("Width", ref _Width, value);
            if (null != OurBorder)
            {
                OurBorder.Width = value;
            }
        }
        public double Width { get { return _Width; } private set { SetWidth(value); } }

        private double _Height;
        private void SetHeight(double value)
        {
            Set<double>("Height", ref _Height, value);
            if (null != OurBorder)
            {
                OurBorder.Height = value;
            }
        }
        public double Height { get { return _Width; } private set { SetWidth(value); } }

        private string _Title;
        private void SetTitle(string value)
        {
            if (null != Layout)
            {
                Layout.Title = value;
            }
            Set<string>("Title", ref _Title, value);
        }
        public string Title { get { return _Title; } set { SetTitle(value); } }

        private LayoutDocument _Layout;
        private void SetLayout(LayoutDocument value)
        {
            Set<LayoutDocument>("Layout", ref _Layout, value);
        }
        public LayoutDocument Layout { get { return _Layout; } set { SetLayout(value); } }


        private bool _Changed;
        private void SetChanged(bool value)
        {
            Set<bool>("Changed", ref _Changed, value);
        }
        public bool Changed { get { return _Changed; } set { SetChanged(value); } }

        private ObservableCollection<ITrackItem> _Items;
        private void SetItems(ObservableCollection<ITrackItem> value)
        {
            Set<ObservableCollection<ITrackItem>>("Items", ref _Items, value);
        }
        public ObservableCollection<ITrackItem> Items { get { return _Items; } set { SetItems(value); } }

        private ITrackItem _CurrentItem;
        private void SetCurrentItem(ITrackItem value)
        {
            if (value != _CurrentItem)
            {
                if (_CurrentItem != null)
                {
                    _CurrentItem.IsActive = false;
                }
                if (value != null)
                {
                    value.IsActive = true;
                }
            }
            Set<ITrackItem>("CurrentItem", ref _CurrentItem, value);
        }
        public ITrackItem CurrentItem { get { return _CurrentItem; } set { SetCurrentItem(value); } }

        #endregion


        #region Live Properties
        public ContentPresenter OurView { get { return GetView(); } }
        public Zoombox OurZoombox { get { return GetZoomBox(); } }
        public Border OurBorder { get; private set; }
        public Canvas OurCanvas { get; private set; }
        #endregion


        #region Misc Properties
        public ScaleManager Scale { get; private set; }
        #endregion


        #region Constructors
        public EditDocument()
        {            
            Scale = new ScaleManager();

            Items = new ObservableCollection<ITrackItem>();
            Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Items_CollectionChanged);
            SetTitle(string.Empty);
            _Changed = true;
            
            SetWidth(1500);
            SetHeight(1200);
        }

        #endregion 
        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

           
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        ITrackItem itm = item as ITrackItem;
                        OurCanvas.Children.Add(itm.Control);
                        Canvas.SetLeft(itm.Control, itm.Left);
                        Canvas.SetTop(itm.Control, itm.Top);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    break;

                case NotifyCollectionChangedAction.Replace:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;
            }
        }



        internal void SetLayout(string Title, LayoutContent Layout)
        {
            SetLayout(Layout as LayoutDocument);
            SetTitle(Title);
            Changed = true;

            OurView.ApplyTemplate();

            Border border = new Border();
            border.BorderThickness = new Thickness(2);
            border.BorderBrush = Brushes.Black;
            border.HorizontalAlignment = HorizontalAlignment.Left;
            border.VerticalAlignment = VerticalAlignment.Top;
            border.Margin = new Thickness(5);            
            border.Width = Width;
            border.Height = Height;

            OurZoombox.Content = border;
            OurBorder = border;

            Canvas canvas = new Canvas();
            canvas.HorizontalAlignment = HorizontalAlignment.Stretch;
            canvas.VerticalAlignment = VerticalAlignment.Stretch;
            canvas.Background = Brushes.LightGray;

            OurBorder.Child = canvas;
            OurCanvas = canvas;


            GenerateSomeItems();
        }


        private void GenerateSomeItems()
        {              
            Vector2 A1 = new Vector2(50, 120);
            Vector2 A2 = new Vector2(56, 120);

            TrackItemStraight item = new TrackItemStraight(this, A1, A2);
            var x = item.Angle;
            Items.Add(item);
        }

        public ContentPresenter GetView()
        {
            if (Layout != null)
            {
                return Layout.Root.Manager.GetLayoutItemFromModel(Layout).View as ContentPresenter;
            }
            else
            {
                return null;
            }
            
        }


        public Zoombox GetZoomBox()
        {
            if (OurView != null)
            {
                return (from i in FindVisualChildren<Zoombox>(OurView, false) select i).First();
            }
            else
            {
                return null;
            }
        }

        public Border GetBorder()
        {
            if (OurZoombox != null)
            {
                return (from i in FindVisualChildren<Border>(OurZoombox, true) select i).First();
            }
            else
            {
                return null;
            }
        }

        public Canvas GetCanvas()
        {
            if (OurBorder != null)
            {
                return (from i in FindVisualChildren<Canvas>(OurBorder, true) select i).First();
            }
            else
            {
                return null;
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj, bool SearchDeep = true) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }
                    if (SearchDeep)
                    {
                        foreach (T childOfChild in FindVisualChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }                 
                }
            }
        }


    }
}

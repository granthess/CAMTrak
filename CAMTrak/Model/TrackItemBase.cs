using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace CAMTrak.Model
{
    public class TrackItemBase :   ViewModelBase, ITrackItem
    {

        private double _Width;
        private void SetWidth(double value)
        {
            Set<double>("Width", ref _Width, value);
            UpdateSize();
        }
        public double Width { get { return _Width; } set { SetWidth(value); } }

        private double _Height;
        private void SetHeight(double value)
        {
            Set<double>("Height", ref _Height, value);
            UpdateSize();
        }
        public double Height { get { return _Height; } set { SetHeight(value); } }

        private double _Left;
        private void SetLeft(double value)
        {
            Set<double>("Left", ref _Left, value);
            Canvas.SetLeft(Control, value);
        }
        public double Left { get { return _Left; } set { SetLeft(value); } }

        private double _Top;
        private void SetTop(double value)
        {
            Set<double>("Top", ref _Top, value);
            Canvas.SetTop(Control, value);
        }
        public double Top { get { return _Top; } set { SetTop(value); } }
        

        

        private UIElement _Control;
        private void SetControl(UIElement value)
        {
            Set<UIElement>("Control", ref _Control, value);
        }
        public UIElement Control { get { return _Control; } set { SetControl(value); } }

        private GeometryGroup OutlineGeometry;
        private GeometryGroup SchematicGeometry;
        private GeometryGroup DetailGeometry;

        public TrackItemBase()
        {
            Control = new ContentPresenter();
            Control.MouseDown += new System.Windows.Input.MouseButtonEventHandler(Control_MouseDown);

  

            Width = 100;
            Height = 50;
            Top = 37;
            Left = 199;
        }

        void Control_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Width += 25;
                Height += 10;
            }
            else
            {
                Top += 50;
                Left += 21;
            }
        }

        private void UpdateSize()
        {
            GenerateOutlineGeometry();
            GenerateSchematicGeometry();
            GenerateDetailGeometry();

            GeometryGroup Total = new GeometryGroup();
            Total.Children.Add(OutlineGeometry);
            Total.Children.Add(SchematicGeometry);
            Total.Children.Add(DetailGeometry);

            GeometryDrawing drawing = new GeometryDrawing(null, new Pen(Brushes.Blue, 1), Total);
                        
            DrawingImage image = new DrawingImage(drawing);
            var x = image.Width;
            

            Image img = new Image();
            img.Source = image;
            img.Stretch = Stretch.None;
            img.HorizontalAlignment = HorizontalAlignment.Left;
            img.VerticalAlignment = VerticalAlignment.Top;
            img.Width = Width+4;
            img.Height = Height+4;

            (Control as ContentPresenter).Content = img;  
        }

        public virtual void GenerateOutlineGeometry()
        {
            OutlineGeometry = new GeometryGroup();                
            OutlineGeometry.Children.Add(new RectangleGeometry(new Rect(0.0f, 0.0f, Width, Height)));
        }

        public virtual void GenerateSchematicGeometry()
        {
            SchematicGeometry = new GeometryGroup();
        }

        public virtual void GenerateDetailGeometry()
        {
            DetailGeometry = new GeometryGroup();
        }
    }
}

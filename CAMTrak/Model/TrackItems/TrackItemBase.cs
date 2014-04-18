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

namespace CAMTrak.Model.TrackItems
{
    public class TrackItemBase :   ViewModelBase, ITrackItem
    {

        private double _Width;
        private void SetWidth(double value)
        {
            Set<double>("Width", ref _Width, value);
            RegenerateGeometry();
        }
        public double Width { get { return _Width; } set { SetWidth(value); } }

        private double _Height;
        private void SetHeight(double value)
        {
            Set<double>("Height", ref _Height, value);
            RegenerateGeometry();
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

        private bool _IsActive;
        private void SetActive(bool value)
        {
            Set<bool>("IsActive", ref _IsActive, value);
            RegenerateGeometry();
        }
        public bool IsActive { get { return _IsActive; } set { SetActive(value); } }

        private UIElement _Control;
        private void SetControl(UIElement value)
        {
            Set<UIElement>("Control", ref _Control, value);
        }
        public UIElement Control { get { return _Control; } set { SetControl(value); } }

        private Drawing OutlineDrawing;
        private Drawing SchematicDrawing;
        private Drawing DetailDrawing;

        private EditDocument Parent;

        public TrackItemBase(EditDocument Parent)
        {
            this.Parent = Parent;

            Control = new ContentPresenter();
            Control.MouseDown += new System.Windows.Input.MouseButtonEventHandler(Control_MouseDown);

            Width = 100;
            Height = 50;
            Top = 37;
            Left = 199;
        }

        void Control_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Parent.CurrentItem = this;
            e.Handled = true;
        }



        private void RegenerateGeometry()
        {
            GenerateOutlineDrawing();
            GenerateSchematicDrawing();
            GenerateDetailDrawing();

            DrawingGroup Total = new DrawingGroup();
            Total.Children.Add(OutlineDrawing);
            Total.Children.Add(SchematicDrawing);
            Total.Children.Add(DetailDrawing);
                        
            DrawingImage image = new DrawingImage(Total);
                       

            Image img = new Image();
            img.Source = image;
            img.Stretch = Stretch.None;
            img.HorizontalAlignment = HorizontalAlignment.Left;
            img.VerticalAlignment = VerticalAlignment.Top;
            img.Width = Width+4;
            img.Height = Height+4;
            

            (Control as ContentPresenter).Content = img;  
        }

        
        public virtual void GenerateOutlineDrawing()
        {
            Pen OutlinePen;

            if (IsActive)
            {
                OutlinePen = new Pen(Brushes.Red, 2);
            }
            else
            {
                OutlinePen = new Pen(Brushes.DarkBlue, 1);
            }
            
            GeometryGroup OutlineGeometry = new GeometryGroup();                
            OutlineGeometry.Children.Add(new RectangleGeometry(new Rect(0.0f, 0.0f, Width, Height)));
            OutlineDrawing = new GeometryDrawing(Brushes.Pink, OutlinePen, OutlineGeometry);
        }

        public virtual void GenerateSchematicDrawing()
        {
            GeometryGroup SchematicGeometry = new GeometryGroup();            
            SchematicDrawing = new GeometryDrawing(null, new Pen(Brushes.Red, 1), SchematicGeometry);
        }

        public virtual void GenerateDetailDrawing()
        {
            GeometryGroup DetailGeometry = new GeometryGroup();
            DetailDrawing = new GeometryDrawing(null, new Pen(Brushes.Pink, 1), DetailGeometry);
        }
    }
}

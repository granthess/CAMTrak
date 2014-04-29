using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using CAMTrak.Model.CADElements.Parts;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using CAMTrak.Utilities;

namespace CAMTrak.Model.Controls.Adorners
{
    public class CADAdorner : Adorner
    {

        ObservableCollection<Position> Positions;
        VisualCollection Children;
        AdornerLayer Layer;
        

        public CADAdorner(UIElement adornedElement, ObservableCollection<Position> Positions)
            : base(adornedElement)
        {
            this.Positions = Positions;
            Children = new VisualCollection(adornedElement);
            Layer = AdornerLayer.GetAdornerLayer(adornedElement);
            Layer.Add(this);
            Visibility = Visibility.Hidden;
            AddThumbs();
        }

        protected void AddThumbs()
        {
            foreach (Position pos in Positions)
            {
                DragThumb thumb = new DragThumb(pos);
                Children.Add(thumb);
            }
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Hidden;
        }

        protected override int VisualChildrenCount { get { return Children.Count; } }
        

        protected override Visual GetVisualChild(int index)
        {
            return Children[index];
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size newsize = base.MeasureOverride(constraint);
            newsize.Height += 10;
            newsize.Width += 10;
            return newsize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {            
            
            foreach (DragThumb thumb in Children)
            {
                if (thumb.LinkedPosition.IsDragging)
                {
                    thumb.Arrange(new Rect(thumb.LinkedPosition.PreviewLocalPosition.x - 5,
                                  thumb.LinkedPosition.PreviewLocalPosition.y - 5,
                                  10, 10));
                }
                else
                {
                    thumb.Arrange(new Rect(thumb.LinkedPosition.LocalPosition.x - 5,
                        thumb.LinkedPosition.LocalPosition.y - 5,
                        10, 10));
                }
            }
            
            return finalSize;
        }
    }
}
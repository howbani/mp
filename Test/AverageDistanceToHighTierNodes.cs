using MP.Dataplane;
using MP.Intilization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MP.Test
{
    class AverageDistanceToHighTierNodes
    {
        double Diameter = 0;
        double raduis = 0;
        public AverageDistanceToHighTierNodes()
        {
            if (PublicParamerters.NetworkSquareSideLength == 0)
            {
                double sidLength = 50;
                PublicParamerters.NetworkSquareSideLength = sidLength;
                PublicParamerters.MainWindow.Canvas_SensingFeild.Width = sidLength;
                PublicParamerters.MainWindow.Canvas_SensingFeild.Height = sidLength;
            }
            // draw the circle
            // draw the diagonal
            DrawDiaogonal();
            DrawRing(25);
            
        }


        private void DrawDiaogonal()
        {
            
            Point point1 = new Point(0, 0);
            Point point2 = new Point(PublicParamerters.NetworkSquareSideLength, PublicParamerters.NetworkSquareSideLength);

            Operations.DrawLine(PublicParamerters.MainWindow.Canvas_SensingFeild, point1, point2,1);
        }

        private void DrawRing(double diameter)
        {
            Diameter = diameter;
            raduis = Diameter / 2;
            Point netCeter = GetNetworkCenter();
            Ellipse myEllipse = new Ellipse();
            myEllipse.StrokeThickness = 1;
            myEllipse.Stroke = Brushes.Black;
            myEllipse.Width = Diameter;
            myEllipse.Height = Diameter;

            double xCenter = netCeter.X - raduis;
            double yCenter = netCeter.Y - raduis;

            myEllipse.Margin = new Thickness(xCenter, yCenter, 0, 0);
            PublicParamerters.MainWindow.Canvas_SensingFeild.Children.Add(myEllipse);
        }

        private Point GetNetworkCenter()
        {
            Ellipse myEllipse = new Ellipse();
            myEllipse.StrokeThickness = 3;
            myEllipse.Stroke = Brushes.Black;
            myEllipse.Width = 3;
            myEllipse.Height = 3;

            double xCenter = (PublicParamerters.NetworkSquareSideLength / 2);
            double yCenter = (PublicParamerters.NetworkSquareSideLength / 2);

            myEllipse.Margin = new Thickness(xCenter, yCenter, 0, 0);
            PublicParamerters.MainWindow.Canvas_SensingFeild.Children.Add(myEllipse);
            return new Point(xCenter, yCenter);
        }

        public double FindDistanceToDiagonal(Point me)
        {
            Point s = new Point(0, 0);
            Point e = new Point(PublicParamerters.NetworkSquareSideLength, PublicParamerters.NetworkSquareSideLength);

           // Operations.DrawPoint(me);
            return Operations.Perpendiculardistance(me, s, e);
        }

        public double FindDistanceToRing(Point me)
        {
            double xCenter = (PublicParamerters.NetworkSquareSideLength / 2);
            double yCenter = (PublicParamerters.NetworkSquareSideLength / 2);
            Point NETcenter = new Point(xCenter, yCenter);

            double dis = Operations.DistanceBetweenTwoPoints(me, NETcenter);
            if (dis == raduis)
            {
                double re = raduis;
                // on border::
                Operations.DrawPoint(me, Brushes.Red);
                return re;
            }
            else if (dis < raduis)
            {
                // inside:
                double re = raduis - dis;
                Operations.DrawPoint(me, Brushes.Red);
                return re;
            }
            else
            {
                // outside:
                double re = dis - raduis;
                Operations.DrawPoint(me, Brushes.Black);
                return re;
            }
        }

        /// <summary>
        /// inner averag distance to the center.
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public double InnerAverageDistance(Point me)
        {
            double xCenter = (PublicParamerters.NetworkSquareSideLength / 2) ;
            double yCenter = (PublicParamerters.NetworkSquareSideLength / 2) ;
            Point NETcenter = new Point(xCenter, yCenter);

            double dis = Operations.DistanceBetweenTwoPoints(me, NETcenter);

            if (dis == raduis)
            {
                double re = raduis;
                // on border::
                Operations.DrawPoint(me,Brushes.Red);
                return re;
            }
            else if (dis < raduis)
            {
                // inside:
                double re = raduis - dis;
                Operations.DrawPoint(me, Brushes.Red);
                return re;
            }

            return 0;
        }

    }
}

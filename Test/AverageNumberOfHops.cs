using MP.Dataplane;
using MP.MergedPath.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MP.Computations.RandomvariableStream;

namespace MP.Test
{
    public class RowHops
    {
        public double SideLength { get; set; }
        public double ComRange { get; set; }
        public double AverageHops { get; set; } 
    }
    class AverageNumberOfHops
    {

        public class Theortical
        {
            /// <summary>
            /// The ANH for the Report, Request (obtain) and Re-sponse paths is the same, since these three kinds of paths forward the packets toward the diagonal line.
            /// </summary>
            public double HopsToDiagonal(double sideLingth, double ComRange)
            {

                double numerator = sideLingth / (3 * Math.Sqrt(2));
                double denominator = (2 * ComRange) / 3;
                
                return numerator / denominator; 
            }

            /// <summary>
            /// Likewise, the average number of hops for disseminating a data packet can be found by employing the same idea mentioned above. The only difference is the average ASDD distance which can be modeled as average distance between any two random points fairly distributed within 〖[a,b]〗^2. 
            /// </summary>
            public double HopsBetweenTwoRandomPoints(double sideLingth, double ComRange)
            {
                double numerator = sideLingth * (5 * Math.Log(1 + Math.Sqrt(2)) + 2 + Math.Sqrt(2));
                double denominator = 10 * ComRange;
                return numerator / denominator;
            }
        }

        public class Expermental
        {
            public void SendToDiagonal()
            {
                int index = UniformRandomVariable.GetIntValue(0, PublicParamerters.MainWindow.myNetWork.Count-1);
                Sensor sen = PublicParamerters.MainWindow.myNetWork[index];
                ObtainSinkFreshPositionMessage ob = new ObtainSinkFreshPositionMessage(sen);
            }

            public void SendRandTwoPoints()
            {
                int indexSource= UniformRandomVariable.GetIntValue(0, PublicParamerters.MainWindow.myNetWork.Count - 1);
                int indexDestination = UniformRandomVariable.GetIntValue(0, PublicParamerters.MainWindow.myNetWork.Count - 1);
                Sensor source= PublicParamerters.MainWindow.myNetWork[indexSource];
                Sensor dest = PublicParamerters.MainWindow.myNetWork[indexDestination];
                TestSendPacketsBetweenTwoNodesRandomlySelected c = new TestSendPacketsBetweenTwoNodesRandomlySelected(source, dest);

            }
        }
    }
}

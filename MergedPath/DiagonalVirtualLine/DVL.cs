using MP.Dataplane;
using MP.Intilization;
using MP.MergedPath.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MP.MergedPath.DiagonalVirtualLine
{
    /// <summary>
    /// DVL consists of a one-node-width strip of nodes along the diagonal of square or a rectangular as shown in Figure 6(a). Beside the benefits of one-node-width structure as discussed in Section 2, we used the diagonal line for two more reasons. First, the construction of DVL is easier, and  it costs a linear overhead O(n), lower than the cost required to construct a ring which costs O(n log n) if Graham scan is used, or in the worst case requires O(n^2) if Gift wrapping algorithm is used. Second, the average distance to the diagonal is smaller than that to the circumference of the ring, mathematically proven in Section 5. This means the request and response paths are shorter to the diagonal line than that to the circumference of the ring, which in turn reduces the delivery delay and minimizes the energy consumption.
    /// </summary>
    public class DVL
    {
        // diaogonal known points: x=y.
        Point point1 = new Point(0, 0);
        Point point2 = new Point(PublicParamerters.NetworkSquareSideLength, PublicParamerters.NetworkSquareSideLength);


        List<Sensor> network;
        public DVL(List<Sensor> sensors, Canvas mycanvas)
        {
            network = sensors;

            List<Sensor> two = DefineAnchorNodes();
            Sensor node1 = two[0]; // error here means you need to set the side length of the network.
            Sensor node2 = two[1];//error here means you need to set the side length of the network.

            //Operations.DrawLine(mycanvas, node1.CenterLocation, node2.CenterLocation); // draw the diaogonal line.


            DVLConstructionMessage ShareLocations = new DVLConstructionMessage(node1, node2, mycanvas);



        }
        

        /// <summary>
        /// get the nodes which are closest to the point point1 and point2.
        /// </summary>
        private List<Sensor> DefineAnchorNodes() 
        {
            List<Sensor> twoNodes = new List<Sensor>(2);
            if (network.Count >= 2)
            {
                twoNodes.Add(network[0]);
                twoNodes.Add(network[1]);
            }
            double min1 = Double.MaxValue;
            double min2 = Double.MaxValue; 
            foreach (Sensor node in network)
            {
                double cum1 = Operations.DistanceBetweenTwoPoints(point1, node.CenterLocation);
                if(cum1<= min1)
                {
                    min1 = cum1;
                    twoNodes[0] = node;
                }

                double cum2 = Operations.DistanceBetweenTwoPoints(point2, node.CenterLocation);
                if (cum2 <= min2)
                {
                    min2 = cum2;
                    twoNodes[1] = node;
                }
            }
            return twoNodes;
        }








    }
}

using MP.Dataplane;
using MP.Dataplane.NOS;
using MP.Dataplane.PacketRouter;
using MP.Intilization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MP.MergedPath.computing
{
    class GreedyRouting
    {
        private NetworkOverheadCounter counter;
        public GreedyRouting()
        {
            counter = new NetworkOverheadCounter();
        }

        public Sensor Greedy3(Sensor ni, Packet packet)
        {
            List<CoordinationEntry> coordinationEntries = new List<CoordinationEntry>();
            Sensor sj = null;
            double sum = 0;
            foreach (Sensor nj in ni.NeighborsTable)
            {
                double Norangle;
                if (packet.Destination != null)
                {
                    Norangle = Operations.AngleDotProdection(ni.CenterLocation, nj.CenterLocation, packet.Destination.CenterLocation);
                    packet.Branch.EndPoint = packet.Destination.CenterLocation;
                    packet.Branch.StartPoint = ni.CenterLocation;
                }

                Norangle = Operations.AngleDotProdection(ni.CenterLocation, nj.CenterLocation, packet.Branch.EndPoint);
                double dj = Operations.DistanceBetweenTwoPoints(nj.CenterLocation, packet.Branch.EndPoint);

                if (Norangle < 0.5)
                {
                    double aggregatedValue = dj;
                    sum += aggregatedValue;
                    coordinationEntries.Add(new CoordinationEntry() { Priority = aggregatedValue, Sensor = nj }); // candidaite
                }
            }
            // coordination"..... here
            sj = counter.CoordinateGetMin(coordinationEntries, packet, sum);
            return sj;
        }
    }
}

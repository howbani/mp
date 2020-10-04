using MP.Dataplane;
using MP.Dataplane.NOS;
using MP.Dataplane.PacketRouter;
using MP.Intilization;
using MP.MergedPath.computing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP.Test
{
    class TestSendPacketsBetweenTwoNodesRandomlySelected
    {
        private NetworkOverheadCounter counter;
        public TestSendPacketsBetweenTwoNodesRandomlySelected(Sensor source, Sensor dist )
        {
            counter = new NetworkOverheadCounter();

            Packet packet = GeneragtePacket(source, dist);
            SendPacket(source, packet);
        }

        private Packet GeneragtePacket(Sensor sender, Sensor Dist)
        {
            PublicParamerters.NumberofGeneratedPackets += 1;
            Packet pck = new Packet();
            pck.Source = sender;
            pck.Path = "" + sender.ID;
            pck.Destination = Dist; // has no destination.
            pck.PacketType = PacketType.Data;
            pck.Branch = null;
            pck.PID = PublicParamerters.NumberofGeneratedPackets;
            pck.TimeToLive = Convert.ToInt16((Operations.DistanceBetweenTwoPoints(sender.CenterLocation, Dist.CenterLocation) / (PublicParamerters.CommunicationRangeRadius / 3)));
            counter.IncreasePacketsCounter(sender, PacketType.Data);
            return pck;
        }

        public void SendPacket(Sensor sender, Packet pck)
        {
            if (pck.PacketType == PacketType.Data)
            {
                // neext hope:
                Sensor Reciver = SelectNextHop(sender, pck);
                if (Reciver != null)
                {
                    // overhead:
                    counter.ComputeOverhead(pck, EnergyConsumption.Transmit, sender, Reciver);
                    counter.Animate(sender, Reciver, pck);
                    //:
                    RecivePacket(Reciver, pck);
                }
                else
                {
                    counter.DropPacket(pck, sender, PacketDropedReasons.Loop);
                }
            }
        }

        private void RecivePacket(Sensor Reciver, Packet packt)
        {
            packt.Path += ">" + Reciver.ID;
            packt.ReTransmissionTry = 0;

            if (Reciver.ID == packt.Destination.ID) // packet is recived.
            {
                counter.SuccessedDeliverdPacket(packt);
                counter.ComputeOverhead(packt, EnergyConsumption.Recive, null, Reciver);
                counter.DisplayRefreshAtReceivingPacket(Reciver);
            }
            else
            {
                // compute the overhead:
                counter.ComputeOverhead(packt, EnergyConsumption.Recive, null, Reciver);
                if (packt.Hops <= packt.TimeToLive)
                {
                    SendPacket(Reciver, packt);
                }
                else
                {
                    counter.DropPacket(packt, Reciver, PacketDropedReasons.TimeToLive);
                }
            }
        }

        public Sensor SelectNextHop(Sensor ni, Packet packet)
        {
            List<CoordinationEntry> coordinationEntries = new List<CoordinationEntry>();
            double sum = 0;
            foreach (Sensor nj in ni.NeighborsTable)
            {
                double pj = Operations.Perpendiculardistance(nj.CenterLocation, packet.Source.CenterLocation, packet.Destination.CenterLocation);
                double pi = Operations.Perpendiculardistance(ni.CenterLocation, packet.Source.CenterLocation, packet.Destination.CenterLocation);
                double npj = pj / (pi + PublicParamerters.CommunicationRangeRadius);
                double disPij = Math.Exp(-npj);
                double Norangle = Operations.AngleDotProdection(ni.CenterLocation, nj.CenterLocation, packet.Destination.CenterLocation);

                if (Norangle < 0.5 || Double.IsNaN(Norangle))
                {
                    double disAngij;
                    if (Double.IsNaN(Norangle))
                        disAngij = Math.Exp(0);
                    else
                        disAngij = Math.Exp(-Norangle);

                    double aggregatedValue = disPij * disAngij;
                    sum += aggregatedValue;
                    coordinationEntries.Add(new CoordinationEntry() { Priority = aggregatedValue, Sensor = nj }); // candidaite
                }
            }

            Sensor maxSen = null;
            if (coordinationEntries.Count > 0)
            {
                maxSen = coordinationEntries[0].Sensor;

                double max = coordinationEntries[0].Priority;
                foreach (CoordinationEntry en in coordinationEntries)
                {
                    if (en.Priority > max)
                    {
                        max = en.Priority;
                        maxSen = en.Sensor;
                    }
                }
            }
            return maxSen;
        }



    }
}

using MP.Dataplane;
using MP.Dataplane.NOS;
using MP.Dataplane.PacketRouter;
using MP.Intilization;
using MP.MergedPath.computing;
using MP.MergedPath.SinkClustering;
using MP.Properties;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using static MP.MergedPath.computing.Sorters;

namespace MP.MergedPath.Routing
{
    public class SinksAgentsRow
    {
        public Sink Sink { get; set; }
        public Sensor AgentNode { get; set; }
        public Point ClosestPointOnTheDiagonal { get; set; } ////Given a mobile sink m_j located at U(x ̇_j,y ̇_j), the point on diagonal which is the closest to  U(x ̇_j,y ̇_j) has coordinates: V(x ̇_j+y ̇_j/2,x ̇_j+y ̇_j/2), 

    }

    /// <summary>
    /// The sink continuously selects one of its neighbors as an agent to delegate the communication between DVL nodes and other sensor nodes. More details on how the sink selects its agent are explained in [7]. Here in this sub-section we focused on discovering the reporting path to advertise the new sink’s position. Given a mobile sink m_j located at U(x ̇_j,y ̇_j), the point on diagonal which is the closest to  U(x ̇_j,y ̇_j) has coordinates: V(x ̇_j+y ̇_j/2,x ̇_j+y ̇_j/2), and this constructs the shortest distance from the sink to diagonal line. Accordingly, shortest routing path to advertise the sink’s fresh position is computed as follows. The sink’s agent node n_i picks up one its one neighbor n_k, located at (x_k,y_k ), as a relay node or next-hop if n_k   meets two conditions.  First, n_k has the shortest perpendicular distance to (UV) ⃡. The  perpendicular distance from n_k to the line segment (UV) ⃡, denoted by ψ ̂_(i,k), is given by Eq.(14). Second, n_k should be the closest to the point V. The proximity of n_k to the point V, denoted by θ ̂_(i,k) , is expressed by the cosine angle between two Euclidean vectors a ⃗=(x_k-x_i,y_k-y_i) and   c ⃗=((x ̇_j+y ̇_j)⁄2-x_i,(x ̇_j+y ̇_j)⁄2-y_i). These two conditions are aggregated by Eq.(13) such that a higher priority is assigned to the nodes that satisfy the two condition mentioned above. This selection mechanism is repeated until the packet that holds the new position of the mobile sink is received by an DVL node, say n_v which is the closest to the point  V on the diagonal. Then, n_v shares the new position with all DVL nodes via one-hop or multiple hops.
    /// </summary>
    public class ReportSinkPositionMessage
    {
        private NetworkOverheadCounter counter;
        public ReportSinkPositionMessage(SinksAgentsRow reportSinkPosition)
        {
            
            // node should not be a hightier
            if (!reportSinkPosition.AgentNode.IsHightierNode)
            {
                counter = new NetworkOverheadCounter();
                double x = (reportSinkPosition.AgentNode.CenterLocation.X + reportSinkPosition.AgentNode.CenterLocation.Y) / 2;
                reportSinkPosition.ClosestPointOnTheDiagonal = new Point(x, x);
                Packet packet = GeneragtePacket(reportSinkPosition);

                SendPacket(reportSinkPosition.AgentNode, packet);
                // check if the node isself is hightier node. here no need to generate ReportSinkPosition.
            }
            else
            {
                // node just generate sharesinkposition packet.
                // no need to report.
                ShareSinkPositionIntheHighTier xma = new ShareSinkPositionIntheHighTier(reportSinkPosition.AgentNode, reportSinkPosition);
            }

        }

        public ReportSinkPositionMessage()
        {
            counter = new NetworkOverheadCounter();
        }

        /// <summary>
        ///  here the sender should not be the agent.
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="InQuepacket"></param>
        public void HandelInQueuPacket(Sensor currentNode, Packet InQuepacket)
        {
            SendPacket(currentNode, InQuepacket);
        }





        private Packet GeneragtePacket(SinksAgentsRow reportSinkPosition)
        {

            PublicParamerters.NumberofGeneratedPackets += 1;
            Packet pck = new Packet();
            pck.Source = reportSinkPosition.AgentNode;
            pck.Path = "" + reportSinkPosition.AgentNode.ID;
            pck.Destination = null; // has no destination.
            pck.PacketType = PacketType.ReportSinkPosition;
            pck.PID = PublicParamerters.NumberofGeneratedPackets;
            pck.ReportSinkPosition = reportSinkPosition;
            pck.TimeToLive = Convert.ToInt16((Operations.DistanceBetweenTwoPoints(reportSinkPosition.AgentNode.CenterLocation, reportSinkPosition.ClosestPointOnTheDiagonal) / (PublicParamerters.CommunicationRangeRadius / Settings.Default.ComunTTL)));
            counter.IncreasePacketsCounter(reportSinkPosition.AgentNode, PacketType.ReportSinkPosition);
            return pck;
        }

       

        private void SendPacket(Sensor sender, Packet pck)
        {
            if (pck.PacketType == PacketType.ReportSinkPosition)
            {
                sender.SwichToActive();
                // neext hope:
                Sensor Reciver = SelectNextHop(sender, pck);
                if (Reciver != null)
                {
                    counter.ComputeOverhead(pck, EnergyConsumption.Transmit, sender, Reciver);
                    counter.Animate(sender, Reciver, pck);
                    RecivePacket(Reciver, pck);
                }
                else
                {
                    // wait:
                    counter.SaveToQueue(sender, pck);
                }
            }
        }


        private void RecivePacket(Sensor Reciver, Packet packt)
        {
           
            packt.Path += ">" + Reciver.ID;

            if (new LoopMechanizimAvoidance(packt).isLoop)
            {
                // drop the packet:
                counter.DropPacket(packt, Reciver, PacketDropedReasons.Loop);
            }
            else
            {
                packt.ReTransmissionTry = 0;

                if (Reciver.IsHightierNode) // packet is recived.
                {
                    packt.Destination = Reciver;

                    counter.SuccessedDeliverdPacket(packt);
                    counter.ComputeOverhead(packt, EnergyConsumption.Recive, null, Reciver);
                    counter.DisplayRefreshAtReceivingPacket(Reciver);
                    // share:
                    ShareSinkPositionIntheHighTier xma = new ShareSinkPositionIntheHighTier(Reciver, packt.ReportSinkPosition);

                }
                else
                {

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
        }



        private Sensor SelectNextHop(Sensor ni, Packet packet)
        {
            List<CoordinationEntry> coordinationEntries = new List<CoordinationEntry>();
            double sum = 0;
            Sensor sj = null;
            if (ni.VLDNodesNeighbobre.Count > 0)
            {
                foreach (Sensor nj in ni.VLDNodesNeighbobre)
                {
                    double pj = Operations.Perpendiculardistance(nj.CenterLocation, packet.Source.CenterLocation, packet.ReportSinkPosition.ClosestPointOnTheDiagonal);
                    double pi = Operations.Perpendiculardistance(ni.CenterLocation, packet.Source.CenterLocation, packet.ReportSinkPosition.ClosestPointOnTheDiagonal);
                    double npj = pj / (pi + PublicParamerters.CommunicationRangeRadius);
                    double disPij = Math.Exp(-npj);
                    double Edis = 1;
                    if (Settings.Default.ConsiderEnergy)
                    {
                        Edis = MeredPathDistributions.EnergyDistribution(nj.ResidualEnergyPercentage, 100);
                       // Console.WriteLine("Edis :" + Edis);
                    }
                    double Norangle = Operations.AngleDotProdection(ni.CenterLocation, nj.CenterLocation, packet.ReportSinkPosition.ClosestPointOnTheDiagonal);
                    if (Norangle < 0.5 || Double.IsNaN(Norangle))
                    {
                        double disAngij;
                        if (Double.IsNaN(Norangle))
                            disAngij = Math.Exp(-0);
                        else
                            disAngij = Math.Exp(-Norangle);
                        double aggregatedValue = disAngij * (disPij + Edis);
                        sum += aggregatedValue;
                        coordinationEntries.Add(new CoordinationEntry() { Priority = aggregatedValue, Sensor = nj }); // candidaite
                    }    
                }
            }
            else
            {
                foreach (Sensor nj in ni.NeighborsTable)
                {
                    double pj = Operations.Perpendiculardistance(nj.CenterLocation, packet.Source.CenterLocation, packet.ReportSinkPosition.ClosestPointOnTheDiagonal);
                    double pi = Operations.Perpendiculardistance(ni.CenterLocation, packet.Source.CenterLocation, packet.ReportSinkPosition.ClosestPointOnTheDiagonal);
                    double npj = pj / (pi + PublicParamerters.CommunicationRangeRadius);
                    double Edis = 1;
                    if (Settings.Default.ConsiderEnergy)
                    {
                        Edis = MeredPathDistributions.EnergyDistribution(nj.ResidualEnergyPercentage, 100);
                       // Console.WriteLine("Edis :"+Edis);
                    }
                    double disPij = Math.Exp(-npj);

                    double Norangle = Operations.AngleDotProdection(ni.CenterLocation, nj.CenterLocation, packet.ReportSinkPosition.ClosestPointOnTheDiagonal);
                    if (Norangle < 0.5 || Double.IsNaN(Norangle))
                    {
                        double disAngij;
                        if (Double.IsNaN(Norangle))
                            disAngij = Math.Exp(-0);
                        else
                            disAngij = Math.Exp(-Norangle);

                        double aggregatedValue = disAngij * (disPij + Edis);
                        sum += aggregatedValue;
                        coordinationEntries.Add(new CoordinationEntry() { Priority = aggregatedValue, Sensor = nj }); // candidaite
                    }
                }
            }

            if (Settings.Default.CoordinationType == CoordinationType.Random.ToString())
            {
                // random coordinate:
                sj = counter.RandomCoordinate(coordinationEntries, packet, sum);
            }
            else if (Settings.Default.CoordinationType == CoordinationType.Mixed.ToString())
            {
                sj = counter.MaximalCoordinate(coordinationEntries, packet, sum);
            }
            else
            {
                sj = counter.MaximalCoordinate(coordinationEntries, packet, sum);
            }
            return sj;
        }




    }
}

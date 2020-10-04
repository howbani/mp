using MP.Dataplane;
using MP.Dataplane.NOS;
using MP.Dataplane.PacketRouter;
using MP.Intilization;
using MP.MergedPath.computing;
using MP.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MP.MergedPath.Routing
{
    //MP.MergedPath.Routing.CoordinationType
   
    /// <summary>
    /// Requesting Sink Position from the Diagonal Nodes. 
    /// The source nodes (i.e., low-tier nodes) that have available data request the sink position from the DVL nodes. The requesting mechanism is similar to Reporting the Sink Position, explained in the previous sub-section. Given a source node n_s located at S(x_s,y_s), the point on diagonal which is closest to S(x_s,y_s) has coordinates: Z(x_s+y_s/2,x_s+y_s/2), and this constructs the shortest distance from the source node to diagonal line. Therefore, the relay nodes are selected aligned with line segment (SZ) ⃡. This can be implemented as in Eq.(13). Also, the response path from a DVL node to the source node is computed by the same mechanism.
    /// </summary>
    class ObtainSinkFreshPositionMessage
    {
        private NetworkOverheadCounter counter;
        /// <summary>
        /// obtian the position for all sinks.
        /// </summary>
        /// <param name="sensor"></param>
        public ObtainSinkFreshPositionMessage(Sensor sensor)
        {

            counter = new NetworkOverheadCounter();

            // the high tier node has data. 
            if (sensor.IsHightierNode)
            {
                Packet pack = GeneragtePacket(sensor, false);
                pack.SinkIDsNeedsRecovery = null;
                new ResonseSinkPositionMessage(sensor, sensor);
            }
            else
            {
                sensor.Ellipse_nodeTypeIndicator.Fill = Brushes.Yellow;
                Packet ObtainSinkPositionPacket = GeneragtePacket(sensor, true);
                ObtainSinkPositionPacket.SinkIDsNeedsRecovery = null;
                SendPacket(sensor, ObtainSinkPositionPacket);
            }
        }

        /// <summary>
        /// obtian location for the sinks with IDs listed in SinkIDs.
        /// this is used in the recovery mechansim.
        /// when the agent node did not found its sinks.
        /// </summary>
        /// <param name="AgentNode"> the node that make the request.</param>
        /// <param name="SinkIDs"></param>
        public ObtainSinkFreshPositionMessage(Sensor AgentNode, List<int> SinkIDs) 
        {

            counter = new NetworkOverheadCounter();

            // the high tier node has data. 
            if (AgentNode.IsHightierNode)
            {
                Packet pack = GeneragtePacket(AgentNode,false); // fack packet.
                pack.SinkIDsNeedsRecovery = SinkIDs;// not all sinks. this is for data recovery only.
                new ResonseSinkPositionMessage(AgentNode, AgentNode, SinkIDs); // recovery.
            }
            else
            {
                AgentNode.Ellipse_nodeTypeIndicator.Fill = Brushes.Yellow;
                Packet ObtainSinkPositionPacket = GeneragtePacket(AgentNode, true);
                ObtainSinkPositionPacket.SinkIDsNeedsRecovery = SinkIDs; // not all sinks. this is for data recovery only.
                SendPacket(AgentNode, ObtainSinkPositionPacket);
            }
        }

        /// <summary>
        /// just to handel the in queue packets.
        /// </summary>
        public ObtainSinkFreshPositionMessage() 
        {
            counter = new NetworkOverheadCounter();
        }

        public void HandelInQueuPacket(Sensor currentNode, Packet InQuepacket)
        {
            SendPacket(currentNode, InQuepacket);
        }



        /// <summary>
        /// IncreasePid=true--> this is a packet.
        /// IncreasePid=false--> fake packet should not considred as new packet.
        /// </summary>
        /// <param name="sourceNode"></param>
        /// <param name="IncreasePid"></param>
        /// <returns></returns>
        private Packet GeneragtePacket(Sensor sourceNode, bool IncreasePid)
        {
            if (IncreasePid)
            {

                double x = (sourceNode.CenterLocation.X + sourceNode.CenterLocation.Y) / 2;
                PublicParamerters.NumberofGeneratedPackets += 1;
                Packet pck = new Packet();
                pck.Source = sourceNode;
                pck.ClosestPointOnTheDiagonal = new Point(x, x);
                pck.Path = "" + sourceNode.ID;
                pck.Destination = null; // has no destination.
                pck.PacketType = PacketType.ObtainSinkPosition;
                pck.PID = PublicParamerters.NumberofGeneratedPackets;
                pck.TimeToLive = Convert.ToInt16((Operations.DistanceBetweenTwoPoints(sourceNode.CenterLocation, pck.ClosestPointOnTheDiagonal) / (PublicParamerters.CommunicationRangeRadius / Settings.Default.ComunTTL)));
                counter.IncreasePacketsCounter(sourceNode, PacketType.ObtainSinkPosition);
                return pck;
            }
            else
            {
                double x = (sourceNode.CenterLocation.X + sourceNode.CenterLocation.Y) / 2;
               // PublicParamerters.NumberofGeneratedPackets += 1;
                Packet pck = new Packet();
                pck.Source = sourceNode;
                pck.ClosestPointOnTheDiagonal = new Point(x, x);
                pck.Path = "" + sourceNode.ID;
                pck.Destination = null; // has no destination.
                pck.PacketType = PacketType.ObtainSinkPosition;
                pck.PID = PublicParamerters.NumberofGeneratedPackets;
                pck.TimeToLive = Convert.ToInt16((Operations.DistanceBetweenTwoPoints(sourceNode.CenterLocation, pck.ClosestPointOnTheDiagonal) / (PublicParamerters.CommunicationRangeRadius / Settings.Default.ComunTTL)));
                counter.IncreasePacketsCounter(sourceNode, PacketType.ObtainSinkPosition);
                return pck;
            }
        }


        private void SendPacket(Sensor sender, Packet pck)
        {
            if (pck.PacketType == PacketType.ObtainSinkPosition)
            {
                // neext hope:
                sender.SwichToActive();
                Sensor Reciver = SelectNextHop(sender, pck);
                if (Reciver != null)
                {
                    // overhead:
                    counter.ComputeOverhead(pck, EnergyConsumption.Transmit, sender, Reciver);
                    counter.Animate(sender, Reciver, pck);
                    RecivePacket(Reciver, pck);
                }
                else
                {
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

                    // response to the source:
                    if (packt.SinkIDsNeedsRecovery == null) // nomal
                    {
                        new ResonseSinkPositionMessage(Reciver, packt.Source);
                    }
                    else
                    {
                        // recovery packet.
                        // we can also save the record
                        new ResonseSinkPositionMessage(Reciver, packt.Source, packt.SinkIDsNeedsRecovery);
                    }
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
                    double pj = Operations.Perpendiculardistance(nj.CenterLocation, packet.Source.CenterLocation, packet.ClosestPointOnTheDiagonal);
                    double pi = Operations.Perpendiculardistance(ni.CenterLocation, packet.Source.CenterLocation, packet.ClosestPointOnTheDiagonal);
                    double npj = pj / (pi + PublicParamerters.CommunicationRangeRadius);
                    double Edis = 1;
                    if (Settings.Default.ConsiderEnergy)
                    {
                        Edis = MeredPathDistributions.EnergyDistribution(nj.ResidualEnergyPercentage, 100);
                    }
                    double disPij = Math.Exp(-npj);

                    double Norangle = Operations.AngleDotProdection(ni.CenterLocation, nj.CenterLocation, packet.ClosestPointOnTheDiagonal);
                    if (Norangle < 0.5 || Double.IsNaN(Norangle))
                    {
                        double disAngij;
                        if (Double.IsNaN(Norangle))
                         disAngij = Math.Exp(0);
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
                    double pj = Operations.Perpendiculardistance(nj.CenterLocation, packet.Source.CenterLocation, packet.ClosestPointOnTheDiagonal);
                    double pi = Operations.Perpendiculardistance(ni.CenterLocation, packet.Source.CenterLocation, packet.ClosestPointOnTheDiagonal);
                    double npj = pj / (pi + PublicParamerters.CommunicationRangeRadius);
                    double Edis = 1;
                    if (Settings.Default.ConsiderEnergy)
                    {
                        Edis = MeredPathDistributions.EnergyDistribution(nj.ResidualEnergyPercentage, 100);
                    }
                    double disPij =  Math.Exp(-npj);

                    double Norangle = Operations.AngleDotProdection(ni.CenterLocation, nj.CenterLocation, packet.ClosestPointOnTheDiagonal);
                    if (Norangle < 0.5 || Double.IsNaN(Norangle))
                    {
                        double disAngij;
                        if (Double.IsNaN(Norangle))
                            disAngij = Math.Exp(0);
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

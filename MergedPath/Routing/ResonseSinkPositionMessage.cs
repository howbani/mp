using MP.Dataplane;
using MP.Dataplane.NOS;
using MP.Dataplane.PacketRouter;
using MP.Intilization;
using MP.MergedPath.Bifurcation;
using MP.MergedPath.computing;
using MP.MergedPath.SinkClustering;
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
    /// <summary>
    /// from a hightier node to a given source
    /// </summary>
    class ResonseSinkPositionMessage
    {
        private NetworkOverheadCounter counter;

        /// <summary>
        ///  the hightierNode should response to the lowtiernode. 
        ///  here the respnse should contain all sinks.
        /// </summary>
        /// <param name="hightierNode"></param>
        /// <param name="lowtiernode"></param>
        public ResonseSinkPositionMessage(Sensor hightierNode, Sensor lowtiernode)
        {
            counter = new NetworkOverheadCounter();
            // the hightierNode=lowtiernode --> means that the high tier node itself has data to send.
            if (hightierNode.ID == lowtiernode.ID)
            {
                // here high tier has data to send.
                Packet responspacket = GeneragtePacket(hightierNode, hightierNode, null,false); // fack.
                PreparDataTransmission(hightierNode, responspacket);
            }
            else
            {
                Packet responspacket = GeneragtePacket(hightierNode, lowtiernode, null, true);
                SendPacket(hightierNode, responspacket);
            }
        }

        /// <summary>
        /// the recovery process. 
        /// the response from the hightierNode to the lowtiernode should include the SinkIDs only but not all sinks.
        /// </summary>
        /// <param name="hightierNode"></param>
        /// <param name="lowtiernode"></param>
        /// <param name="SinkIDs"></param>
        public ResonseSinkPositionMessage(Sensor hightierNode, Sensor lowtiernode, List<int> SinkIDs)
        {
            counter = new NetworkOverheadCounter();
            // the hightierNode=lowtiernode --> means that the high tier node itself has data to send.
            if (hightierNode.ID == lowtiernode.ID)
            {
                // here high tier has data to send.
                Packet responspacket = GeneragtePacket(hightierNode, hightierNode, SinkIDs,false);
                PreparDataTransmission(hightierNode, responspacket); 
            }
            else
            {
                Packet responspacket = GeneragtePacket(hightierNode, lowtiernode, SinkIDs, true);
                SendPacket(hightierNode, responspacket);
            }
        }

        public Packet GeneragtePacket(Sensor hightierNode, Sensor lowtiernode, List<int> SinkIDs, bool IncreasePid)
        {
            if (IncreasePid)
            {
                if (SinkIDs == null)
                {
                    // normal ResponseSinkPosition:
                    PublicParamerters.NumberofGeneratedPackets += 1;
                    Packet pck = new Packet();
                    pck.SinkIDsNeedsRecovery = null;
                    pck.Source = hightierNode;
                    pck.Path = "" + hightierNode.ID;
                    pck.Destination = lowtiernode; // has no destination.
                    pck.PacketType = PacketType.ResponseSinkPosition;
                    pck.PID = PublicParamerters.NumberofGeneratedPackets;
                    pck.SinksAgentsList = CopyAllSinks(hightierNode.GetSinksAgentsFromHighTierNode); // normal packet. not recovery packet
                    pck.TimeToLive = Convert.ToInt16((Operations.DistanceBetweenTwoPoints(hightierNode.CenterLocation, lowtiernode.CenterLocation) / (PublicParamerters.CommunicationRangeRadius / Settings.Default.ComunTTL)));
                    counter.IncreasePacketsCounter(hightierNode, PacketType.ResponseSinkPosition);
                    return pck;
                }
                else
                {
                    // recovery packet: that is to say , only few sinks are reqiured. not need to respnse by all sinks.
                    PublicParamerters.NumberofGeneratedPackets += 1;
                    Packet pck = new Packet();
                    pck.SinkIDsNeedsRecovery = SinkIDs;
                    pck.Source = hightierNode;
                    pck.Path = "" + hightierNode.ID;
                    pck.Destination = lowtiernode; // has no destination.
                    pck.PacketType = PacketType.ResponseSinkPosition;
                    pck.PID = PublicParamerters.NumberofGeneratedPackets;
                    pck.SinksAgentsList = CopyFewSinks(hightierNode.GetSinksAgentsFromHighTierNode, SinkIDs); // needs recovery
                    pck.TimeToLive = Convert.ToInt16((Operations.DistanceBetweenTwoPoints(hightierNode.CenterLocation, lowtiernode.CenterLocation) / (PublicParamerters.CommunicationRangeRadius / Settings.Default.ComunTTL)));
                    counter.IncreasePacketsCounter(hightierNode, PacketType.ResponseSinkPosition);
                    return pck;
                }
            }
            else
            {
                // fack:
                if (SinkIDs == null)
                {
                    // normal ResponseSinkPosition:
                   // PublicParamerters.NumberofGeneratedPackets += 1;
                    Packet pck = new Packet();
                    pck.SinkIDsNeedsRecovery = null;
                    pck.Source = hightierNode;
                    pck.Path = "" + hightierNode.ID;
                    pck.Destination = lowtiernode; // has no destination.
                    pck.PacketType = PacketType.ResponseSinkPosition;
                    pck.PID = PublicParamerters.NumberofGeneratedPackets;
                    pck.SinksAgentsList = CopyAllSinks(hightierNode.GetSinksAgentsFromHighTierNode); // normal packet. not recovery packet
                    pck.TimeToLive = Convert.ToInt16((Operations.DistanceBetweenTwoPoints(hightierNode.CenterLocation, lowtiernode.CenterLocation) / (PublicParamerters.CommunicationRangeRadius / Settings.Default.ComunTTL)));
                    counter.IncreasePacketsCounter(hightierNode, PacketType.ResponseSinkPosition);
                    return pck;
                }
                else
                {
                    // recovery packet: that is to say , only few sinks are reqiured. not need to respnse by all sinks.
                   // PublicParamerters.NumberofGeneratedPackets += 1;
                    Packet pck = new Packet();
                    pck.SinkIDsNeedsRecovery = SinkIDs;
                    pck.Source = hightierNode;
                    pck.Path = "" + hightierNode.ID;
                    pck.Destination = lowtiernode; // has no destination.
                    pck.PacketType = PacketType.ResponseSinkPosition;
                    pck.PID = PublicParamerters.NumberofGeneratedPackets;
                    pck.SinksAgentsList = CopyFewSinks(hightierNode.GetSinksAgentsFromHighTierNode, SinkIDs); // needs recovery
                    pck.TimeToLive = Convert.ToInt16((Operations.DistanceBetweenTwoPoints(hightierNode.CenterLocation, lowtiernode.CenterLocation) / (PublicParamerters.CommunicationRangeRadius / Settings.Default.ComunTTL)));
                    counter.IncreasePacketsCounter(hightierNode, PacketType.ResponseSinkPosition);
                    return pck;
                }
            }
        }

         
        public List<SinksAgentsRow> CopyFewSinks(List<SinksAgentsRow> list, List<int> SinkIDs)
        {
            List<SinksAgentsRow> re = new List<SinksAgentsRow>(); 
            foreach( int id in SinkIDs)
            {
                foreach (SinksAgentsRow row in list)
                {
                    if (id == row.Sink.ID)
                    {
                        re.Add(new SinksAgentsRow() { AgentNode = row.AgentNode, ClosestPointOnTheDiagonal = row.ClosestPointOnTheDiagonal, Sink = row.Sink });
                    }
                }
            }
            return re;
        }

        public List<SinksAgentsRow> CopyAllSinks(List<SinksAgentsRow> list)
        {
            List<SinksAgentsRow> re = new List<SinksAgentsRow>();
            foreach (SinksAgentsRow row in list)
            {
                re.Add(new SinksAgentsRow() { AgentNode = row.AgentNode, ClosestPointOnTheDiagonal = row.ClosestPointOnTheDiagonal, Sink = row.Sink });
            }

            return re;
        }

        public ResonseSinkPositionMessage()
        {
            counter = new NetworkOverheadCounter();
        }

        public void HandelInQueuPacket(Sensor currentNode, Packet InQuepacket)
        {
            SendPacket(currentNode, InQuepacket);
        }

        public void SendPacket(Sensor sender, Packet pck)
        {
            if (pck.PacketType == PacketType.ResponseSinkPosition)
            {
                sender.SwichToActive();
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
                    counter.SaveToQueue(sender, pck);
                }
            }
        }

        /// <summary>
        /// if source is an agent, we just send the packet directly to the sink and no need for clustring.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="packt"></param>
        private void PreparDataTransmission(Sensor source, Packet packt)
        {
            // when the old agent is hightier node...
            if (packt.isRecovery)
            {
                // durig recovery, the agent should not be a source.
                // source should add a record.
                List<Sensor> NewAents = new List<Sensor>(); // new agent for the recovery.
                foreach (SinksAgentsRow inpacket in packt.SinksAgentsList)
                {
                    if (inpacket.AgentNode.ID != source.ID) 
                    {
                        bool isFound = Operations.FindInAlistbool(inpacket.AgentNode, NewAents);
                        if (!isFound)
                        {
                            NewAents.Add(inpacket.AgentNode);
                        }
                    }
                }


                if (source.RecoveryRow == null)
                {
                    Console.WriteLine("ResonseSinkPositionMessage. New Recovery Record is created at prevAgent ID" + source.ID + " Packet PID " + packt.PID + " Path: " + packt.Path);
                    // keep recored for the recovery of upcoming packets. no need for request and respanse a gain. 
                    source.RecoveryRow = new RecoveryRow()
                    {
                        ObtiantedTime = PublicParamerters.SimulationTime,
                        PrevAgent = source,
                        RecoveryAgentList = packt.SinksAgentsList,
                        ObtainPacketsRetry = 0 
                    };
                }
                else
                {
                    Console.WriteLine("ResonseSinkPositionMessage: PrevAgent ID " + source.ID + " has recovery record Packet PID " + packt.PID + " Path:" + packt.Path);
                    source.RecoveryRow.ObtiantedTime = PublicParamerters.SimulationTime;
                    source.RecoveryRow.PrevAgent = source;
                    source.RecoveryRow.RecoveryAgentList = packt.SinksAgentsList;
                    source.RecoveryRow.ObtainPacketsRetry += 1;
                }

                // the new agent is found:
                if (NewAents.Count > 0)
                {
                    List<Branch> Branches = FindBranches.GetBranches(NewAents, source.CenterLocation, PublicParamerters.MainWindow.Canvas_SensingFeild, Settings.Default.MaxClusteringThreshold);
                    new MergedPathsMessages(source, Branches, packt);
                }
                else
                {

                    if (packt.SinkIDsNeedsRecovery != null)
                    {
                        new RecoveryMessage(source, packt); // obtain
                    }
                    else
                    {
                        Console.WriteLine("ResonseSinkPositionMessage: ******************************* PrevAgent ID " + source.ID + " has recovery record Packet PID " + packt.PID + " Path:" + packt.Path);
                        MessageBox.Show("xxx&&&&&&&&&&&ResonseSinkPositionMessage");
                    }
                }

            }
            else
            {

                // call the merged path:
                List<Sensor> NotMySinks = new List<Sensor>(); // source node (Reciver) is not an agent for these sinks.
                List<Sensor> MySinks = new List<Sensor>(); //  the source node (Reciver) is an agent for these sinks
                foreach (SinksAgentsRow row in packt.SinksAgentsList)
                {
                    if (row.AgentNode.ID == source.ID) // if the agent itslef has data. that is to say no need for clusetrign.
                    {
                        // no need to join merged path and clustring process.
                        MySinks.Add(row.AgentNode);
                    }
                    else
                    {
                        // the sinks are not mine. SOURCE NODE is not an agent for these sinks.
                        bool isFound = Operations.FindInAlistbool(row.AgentNode, NotMySinks);
                        if (!isFound)
                        {
                            NotMySinks.Add(row.AgentNode);
                        }
                    }
                }


                // not my sinks:
                if (NotMySinks.Count > 0)
                {
                    List<Branch> Branches = FindBranches.GetBranches(NotMySinks, source.CenterLocation, PublicParamerters.MainWindow.Canvas_SensingFeild, Settings.Default.MaxClusteringThreshold);
                    new MergedPathsMessages(source, Branches, packt);
                }

                // my sinks:
                if (MySinks.Count > 0)
                {

                   
                    // the source is an agent in this case.
                    new MergedPathsMessages(source, packt);
                }
            }

            // in recovery probelm
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

                if (Reciver.ID == packt.Destination.ID) // packet is recived.
                {
                    counter.SuccessedDeliverdPacket(packt);
                    counter.ComputeOverhead(packt, EnergyConsumption.Recive, null, Reciver);
                    counter.DisplayRefreshAtReceivingPacket(Reciver);
                    Reciver.Ellipse_nodeTypeIndicator.Fill = Brushes.Transparent;


                    PreparDataTransmission(Reciver, packt); // the merge path


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


        public Sensor SelectNextHop(Sensor ni, Packet packet)
        {
            List<CoordinationEntry> coordinationEntries = new List<CoordinationEntry>();
            double sum = 0;
            Sensor sj = null;
            foreach (Sensor nj in ni.NeighborsTable)
            {
                double pj = Operations.Perpendiculardistance(nj.CenterLocation, packet.Source.CenterLocation, packet.Destination.CenterLocation);
                double pi = Operations.Perpendiculardistance(ni.CenterLocation, packet.Source.CenterLocation, packet.Destination.CenterLocation);
                double npj = pj / (pi + PublicParamerters.CommunicationRangeRadius);
                double Edis = 1;
                if (Settings.Default.ConsiderEnergy)
                {
                    Edis = MeredPathDistributions.EnergyDistribution(nj.ResidualEnergyPercentage, 100);
                }
                double disPij = Math.Exp(-npj);
                double Norangle = Operations.AngleDotProdection(ni.CenterLocation, nj.CenterLocation, packet.Destination.CenterLocation);
               

               


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

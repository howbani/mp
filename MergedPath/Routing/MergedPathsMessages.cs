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
using System.Windows;

namespace MP.MergedPath.Routing
{


    class MergedPathsMessages
    {
        private NetworkOverheadCounter counter;
        /// <summary>
        /// currentBifSensor: current node that has the packet.
        /// Branches: the branches 
        /// isSourceAnAgent: the source is an agent for a sink. That is to say no need for clustering the source itslef this time.
        /// </summary>
        /// <param name="currentBifSensor"></param>
        /// <param name="Branches"></param>
        /// <param name="packet"></param>
        /// <param name="isSourceAnAgent"></param>
        public MergedPathsMessages(Sensor currentBifSensor, List<Branch> Branches, Packet packet)
        {
            counter = new NetworkOverheadCounter();

            // the source node creates new
            if (packet.PacketType == PacketType.ResponseSinkPosition) // i d
            {
                // create new:
                foreach (Branch branch in Branches)
                {
                    // Create new packet
                    Packet pck = GeneragtePacket(currentBifSensor, branch); // new.
                    pck.TimeToLive = branch.TimeToliveInBranch;
                    pck.SinksAgentsList = packet.SinksAgentsList;
                    pck.SinkIDsNeedsRecovery = packet.SinkIDsNeedsRecovery; // in case for recovery.
                    SendPacket(currentBifSensor, pck);
                }
            }
            else if (packet.PacketType == PacketType.Data) // duplicate:
            {
                // 
                foreach (Branch branch in Branches)
                {
                    Packet duplicatedPacket = Duplicate(packet, currentBifSensor, false); // reforward recovery packets.                                                             // duplicate the packet:
                    duplicatedPacket.Branch = branch;
                    duplicatedPacket.TimeToLive = duplicatedPacket.TimeToLive + branch.TimeToliveInBranch; // add.
                    duplicatedPacket.SinkIDsNeedsRecovery = packet.SinkIDsNeedsRecovery; // incase for recovery.
                    SendPacket(currentBifSensor, duplicatedPacket);
                }
            }
        }

        /// <summary>
        /// we call this for HandelInQueuPacket
        /// </summary>
        public MergedPathsMessages()
        {
            counter = new NetworkOverheadCounter();
        }

        /// <summary>
        /// The source node is an agent: needs to deliver the packet to the sink.
        /// no need for branch here.
        /// </summary>
        /// <param name="isSourceAnAgent"></param>
        /// <param name="sender"></param>
        /// <param name="packet"></param>
        public MergedPathsMessages(Sensor sender, Packet packet)
        {
            counter = new NetworkOverheadCounter();

            Packet selfPacket = GeneragtePacket(sender, null);
            selfPacket.SinksAgentsList = packet.SinksAgentsList;
            selfPacket.SinkIDsNeedsRecovery = packet.SinkIDsNeedsRecovery; // incase for recovery.
            HandOffToTheSinkOrRecovry(sender, selfPacket);
        }



        /// <summary>
        ///  handel the packets in the queue of the node. 
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="InQuepacket"></param>
        public void HandelInQueuPacket(Sensor currentNode, Packet InQuepacket)
        {
            SendPacket(currentNode, InQuepacket);
        }



        /// <summary>
        /// increase the number of packets by one.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="currentBifSensor"></param>
        /// <returns></returns>
        private Packet Duplicate(Packet packet, Sensor currentBifSensor, bool IncreasePid)
        {
            Packet pck = packet.Clone() as Packet;
            if (IncreasePid)
            {
                PublicParamerters.NumberofGeneratedPackets += 1;
                pck.PID = PublicParamerters.NumberofGeneratedPackets;
                counter.IncreasePacketsCounter(currentBifSensor, PacketType.Data); // 
            }
            return pck;
        }

        

        /// <summary>
        /// due to duplication, we dont count the generated packets here. we count it when it recived.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        private Packet GeneragtePacket(Sensor sender, Branch branch)
        {
            //Settings.Default.MID++;// unique ID.
            Packet pck = new Packet();
            pck.Source = sender;
            pck.Path = "" + sender.ID;
            pck.Destination = null; // has no destination.
            pck.PacketType = PacketType.Data;
            pck.Branch = branch;
            pck.PID = PublicParamerters.NumberofGeneratedPackets;
            //pck.MID = Settings.Default.MID;
            return pck;
        }

        public void SendPacket(Sensor sender, Packet pck)
        {
            if (pck.PacketType == PacketType.Data)
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
                    counter.SaveToQueue(sender, pck); // save in the queue.
                }
            }
        }

        private void RecivePacket(Sensor Reciver, Packet packt)
        {
            // string cluster = packt.Branch.Cluster.MembersString;
            packt.Path += ">" + Reciver.ID;
            
            // if Reciver is an agent, then reciev the packet and remove the Reciver from the cluster
            // reFindBranches if the cluster still have more members.
            if (new LoopMechanizimAvoidance(packt).isLoop)
            {
                // drop the packet:
                packt.IsLooped = true;
                counter.DropPacket(packt, Reciver, PacketDropedReasons.Loop);
            }
            else
            {
            
                // not loop:
                packt.ReTransmissionTry = 0;
                if (Operations.FindInAlist(Reciver, packt.Branch.Cluster.Members) != null)
                {
                    // Reciver is an agent:
                    counter.ComputeOverhead(packt, EnergyConsumption.Recive, null, Reciver);
                    counter.DisplayRefreshAtReceivingPacket(Reciver);
                    // when the packet arrived to Reciver and holds more clustreds. this case reclustring process should be performed.
                    if (packt.Branch.Cluster.Members.Count > 0)
                    {
                        packt.Branch.Cluster.Members.Remove(Reciver);
                        List<Branch> Branches = FindBranches.GetBranches(packt.Branch.Cluster.Members, Reciver.CenterLocation, PublicParamerters.MainWindow.Canvas_SensingFeild, Settings.Default.MaxClusteringThreshold);
                        if (Branches.Count > 0)
                        {
                            MergedPathsMessages merged = new MergedPathsMessages(Reciver, Branches, packt);
                        }
                    }

                    HandOffToTheSinkOrRecovry(Reciver, packt);

                }
                else
                {

                    double dis1 = Operations.DistanceBetweenTwoPoints(Reciver.CenterLocation, packt.Branch.EndPoint);
                    double dis2 = Operations.DistanceBetweenTwoPoints(Reciver.CenterLocation, packt.Branch.Cluster.Centeriod);
                    double disThre = PublicParamerters.CommunicationRangeRadius;
                    if ((dis1 < disThre) && (packt.Branch.Cluster.Members.Count > 1))
                    {
                        double clusteringThreshould = 0;
                        if (dis2 <= disThre)
                        {
                            clusteringThreshould = Settings.Default.MinClusteringThreshold;
                        }
                        else
                        {
                            clusteringThreshould = Settings.Default.MaxClusteringThreshold;
                        }
                        counter.ComputeOverhead(packt, EnergyConsumption.Recive, null, Reciver);
                        List<Branch> Branches = FindBranches.GetBranches(packt.Branch.Cluster.Members, Reciver.CenterLocation, PublicParamerters.MainWindow.Canvas_SensingFeild, clusteringThreshould);
                        if (Branches.Count > 1)
                        {
                            MergedPathsMessages merged = new MergedPathsMessages(Reciver, Branches, packt);
                        }
                        else
                        {
                            counter.ComputeOverhead(packt, EnergyConsumption.Recive, null, Reciver);
                            packt.Branch = Branches[0]; // the branch is not changed. only the end coordinat of the branch is changed.
                            SendPacket(Reciver, packt);
                        }
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
                            // since we did not count PID during the generation, thuse we should add here: 
                            PublicParamerters.NumberofGeneratedPackets += 1;
                            packt.PID = PublicParamerters.NumberofGeneratedPackets;
                            counter.IncreasePacketsCounter(Reciver, PacketType.Data); // 
                        }
                    }
                }
            }
        }


        /// <summary>
        /// find x in inlist
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="inlist"></param>
        /// <returns></returns>
        private bool StillWithinMyRange(SinksAgentsRow x, List<SinksAgentsRow> inlist)
        {
            foreach (SinksAgentsRow rec in inlist)
            {
                if (rec.Sink.ID == x.Sink.ID)
                {
                    return true;
                }
            }

            return false;
        }

        private List<SinksAgentsRow> GetMySinksFromPacket(int AgentID, List<SinksAgentsRow> inpacketSinks)
        {
            List<SinksAgentsRow> re = new List<SinksAgentsRow>();
            foreach(SinksAgentsRow x in inpacketSinks)
            {
                if(x.AgentNode.ID==AgentID)
                {
                    re.Add(x);
                }
            }

            return re;
        }


        /// <summary>
        /// hand the packet to my sink.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="packt"></param>
        public void HandOffToTheSinkOrRecovry(Sensor agent, Packet packt)
        {
            // check how many sinks are there in my record
            if (agent != null)
            {
                if (packt.SinksAgentsList != null)
                {
                    // my sinks recored in the packet:
                    List<SinksAgentsRow> MysinksInPpaket = GetMySinksFromPacket(agent.ID, packt.SinksAgentsList); // my sinks in the packet.
                    List<SinksAgentsRow> MyCurrentSinks = agent.GetSinksAgentsList; //my sinks that currently within my range.
                    List<int> SinksIDsRequiredRecovery = new List<int>(); //  sinks that required recovery. those sinks which are in the packet but not within my range anymore.

                    for (int i = 0; i < MysinksInPpaket.Count; i++)
                    {
                        SinksAgentsRow sinkInPacket = MysinksInPpaket[i];
                        // check if sink still within the range of the agent
                        bool stillWithinMyRange = StillWithinMyRange(sinkInPacket, MyCurrentSinks); // check if sink x  still within my range
                        if (stillWithinMyRange)
                        {

                            // I am an agent for more than one sink
                            // here we should increase the PID, otherwise the number of delivered packets will be more than the generated packets.
                            Packet pck = Duplicate(packt, agent, true); // duplicate and increase the PID
                            Sink sink = sinkInPacket.Sink;
                            pck.Path += "> Sink: " + sink.ID;
                            counter.SuccessedDeliverdPacket(pck);
                            counter.DisplayRefreshAtReceivingPacket(agent);
                        }
                        else
                        {
                            // sinkInPacket.Sink is out of agent range.
                            SinksIDsRequiredRecovery.Add(sinkInPacket.Sink.ID);
                        }
                    }

                    // recovery: SinksIDsRequiredRecovery
                    if (SinksIDsRequiredRecovery.Count > 0)
                    {
                        packt.SinkIDsNeedsRecovery = SinksIDsRequiredRecovery;
                        new RecoveryMessage(agent, packt);
                    }
                }
                else
                {
                    // drop the packet.
                    // i dont know when it should be null.
                    Console.Write(">>>>No agents. MergedPathsMessages->HandOffToTheSinkOrRecovry->packt.SinksAgentsList==null");
                    counter.DropPacket(packt, agent, PacketDropedReasons.Unknow);
                }
            }
            else
            {
                // drop the packet
                Console.Write(">>>>HandOffToTheSinkOrRecovry->agent = null");
                counter.DropPacket(packt, agent, PacketDropedReasons.Unknow);
            }
        }

        /// <summary>
        /// get the max value
        /// </summary>
        /// <param name="ni"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        public Sensor SelectNextHop(Sensor ni, Packet packet)
        {
            if (packet.Branch != null)
            {

                List<CoordinationEntry> coordinationEntries = new List<CoordinationEntry>();
                double sum = 0;
                Sensor sj = null;
                foreach (Sensor nj in ni.NeighborsTable)
                {
                    if (nj.ResidualEnergyPercentage > 0)
                    {
                        if(packet.isRecovery)
                        {

                        }

                        double Norangle;
                        if (packet.Destination != null)
                        {
                            if (nj.ID == packet.Destination.ID) return nj;
                        }
                        else
                        {
                            Norangle = Operations.AngleDotProdection(ni.CenterLocation, nj.CenterLocation, packet.Branch.EndPoint);
                            if (Double.IsNaN(Norangle) || Norangle == 0 || Norangle < 0.0001)
                            {
                                // for none-recovery we had the destination.
                                return nj;
                            }
                            else
                            {
                                if (Norangle < 0.5)
                                {
                                    double ij_ψ = MeredPathDistributions.PerpendicularDistanceDistribution(ni.CenterLocation, nj.CenterLocation, packet.Branch.StartPoint, packet.Branch.EndPoint);
                                    double ij_ω = MeredPathDistributions.ProximityToBranchEndPoint(ni.CenterLocation, nj.CenterLocation, packet.Branch.EndPoint);
                                    double ij_σ = MeredPathDistributions.EnergyDistribution(nj.ResidualEnergyPercentage, 100);
                                    double ij_d = MeredPathDistributions.TransDistDistribution(ni.CenterLocation, nj.CenterLocation);

                                    // double agg1 = Math.Pow(ij_ω, (1 - Norangle)) * (ij_ψ + ij_d + ij_σ); // this needs to be try when lifetime.

                                    double defual = ij_ω * (ij_ψ + ij_d + ij_σ);

                                    double aggregatedValue = defual;
                                    sum += aggregatedValue;
                                    coordinationEntries.Add(new CoordinationEntry() { Priority = aggregatedValue, Sensor = nj });
                                }
                            }
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
                    sj = counter.RandomCoordinate(coordinationEntries, packet, sum);
                }
                else
                {
                    sj = counter.MaximalCoordinate(coordinationEntries, packet, sum);
                }
                return sj;
            }
            else
            {
                return null;
            }

        }


    }
}

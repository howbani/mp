using MP.Dataplane;
using MP.Dataplane.NOS;
using MP.Intilization;
using MP.MergedPath.computing;
using MP.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP.MergedPath.Routing
{
    public enum PacketDirection { OmiDirection, Left, Right }

    /// <summary>
    /// This selection mechanism is repeated until the packet that holds the new position of the mobile sink is received by an DVL node, say n_v which is the closest to the point  V on the diagonal. Then, n_v shares the new position with all DVL nodes via one-hop or multiple hops.
    /// </summary>
    class ShareSinkPositionIntheHighTier
    {
        private NetworkOverheadCounter counter;
        public ShareSinkPositionIntheHighTier(Sensor highTierGateWay, SinksAgentsRow reportSinkPositionRow)
        {
            if (highTierGateWay.IsHightierNode)
            {
                counter = new NetworkOverheadCounter();
                Packet leftpacket = GeneragtePacket(highTierGateWay,reportSinkPositionRow);
                leftpacket.PacketDirection = PacketDirection.Left;
                Packet righttpacket = GeneragtePacket(highTierGateWay,reportSinkPositionRow);
                righttpacket.PacketDirection = PacketDirection.Right;
                SendPacket(highTierGateWay, leftpacket);
                SendPacket(highTierGateWay, righttpacket);


                //: SAVE Sink positions.// this is not agent record. becarful here
                highTierGateWay.AddSinkRecordInHighTierNode(reportSinkPositionRow);

            }
        }

        public ShareSinkPositionIntheHighTier()
        {
            counter = new NetworkOverheadCounter();
        }

        public void HandelInQueuPacket(Sensor currentNode, Packet InQuepacket)
        {
            SendPacket(currentNode, InQuepacket);
        }

        private Packet GeneragtePacket(Sensor highTierGateWay, SinksAgentsRow reportSinkPositionRow)
        {
            PublicParamerters.NumberofGeneratedPackets += 1;
            Packet pck = new Packet();
            pck.Source = highTierGateWay;
            pck.ReportSinkPosition = reportSinkPositionRow;
            pck.Path = "" + highTierGateWay.ID;
            pck.Destination = null; // has no destination.
            pck.PacketType = PacketType.ShareSinkPosition;
            pck.PID = PublicParamerters.NumberofGeneratedPackets;
            counter.IncreasePacketsCounter(highTierGateWay, PacketType.ShareSinkPosition);
            return pck;
        }

        private void SendPacket(Sensor sender, Packet pck)
        {
            if (pck.PacketType == PacketType.ShareSinkPosition)
            {
                sender.SwichToActive();
                // neext hope:
                // note: here the Reciver is null for two reasons: 
                //1- it is in sleep mode.
                //- the current sender has no more right or left diaogonal nodes.
                if (pck.PacketDirection == PacketDirection.Right)
                {
                    if (sender.RightVldNeighbor != null)
                    {
                        if (sender.RightVldNeighbor.CurrentSensorState == SensorState.Active)
                        {
                            counter.ComputeOverhead(pck, EnergyConsumption.Transmit, sender, sender.RightVldNeighbor);


                            counter.Animate(sender, sender.RightVldNeighbor, pck);

                            RecivePacket(sender.RightVldNeighbor, pck);
                        }
                        else
                        {
                            // wait:
                            counter.SaveToQueue(sender, pck); 
                        }
                    }
                    else
                    {
                        pck.Destination = sender; // arrived to dest.
                        RecivePacket(null, pck);
                    }
                }
                else
                {
                    if (sender.LeftVldNeighbor != null)
                    {
                        if (sender.LeftVldNeighbor.CurrentSensorState == SensorState.Active)
                        {
                            counter.ComputeOverhead(pck, EnergyConsumption.Transmit, sender, sender.LeftVldNeighbor);

                            counter.Animate(sender, sender.LeftVldNeighbor, pck);

                            RecivePacket(sender.LeftVldNeighbor, pck);
                        }
                        else
                        {
                            // wait:
                            counter.SaveToQueue(sender, pck);
                        }
                    }
                    else
                    {
                        pck.Destination = sender; // arrived to dest.
                        RecivePacket(null, pck);
                    }
                }
            }
        }


        private void RecivePacket(Sensor Reciver, Packet packt)
        {
            if (Reciver == null) // packet is recived.
            {
                counter.SuccessedDeliverdPacket(packt);
                counter.DisplayRefreshAtReceivingPacket(packt.Source);

            }
            else
            {
                packt.Path += ">" + Reciver.ID;
                packt.ReTransmissionTry = 0;
                counter.ComputeOverhead(packt, EnergyConsumption.Recive, null, Reciver);
                Reciver.AddSinkRecordInHighTierNode(packt.ReportSinkPosition); // keep track of the sink position
                SendPacket(Reciver, packt);
            }
        }
    }
}

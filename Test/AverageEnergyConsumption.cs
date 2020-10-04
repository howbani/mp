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

    /*
     case PacketType.Data: return 1024;
                    case PacketType.DiagonalVirtualLineConstruction: return 256;
                    case PacketType.ShareSinkPosition: return 128;
                    case PacketType.ObtainSinkPosition: return 128;
                    case PacketType.ReportSinkPosition: return 128;
                    case PacketType.ResponseSinkPosition: return 256;
                    default: return 512;
       */

    public class TestRowEnergy
    {
        public double SideLength { get; set; }
        public double ComRange { get; set; }
        public double UsedEnergy_joule { get; set; }
    }

    class AverageEnergyConsumption
    {
        public class TheoriticalEnergy
        {

            private double ConvertToJoule(double UsedEnergy_Nanojoule) //the energy used for current operation
            {
                double _e9 = 1000000000; // 1*e^-9
                double _ONE = 1;
                double oNE_DIVIDE_e9 = _ONE / _e9;
                double re = UsedEnergy_Nanojoule * oNE_DIVIDE_e9;
                return re;
            }

            public double ToDiagonal(double s, double c)
            {
                double E_elec = PublicParamerters.E_elec;
                double Efs = PublicParamerters.Efs;
                double L = 128;
                double Emp = PublicParamerters.Emp;

                double numator = s * L * ((18 * E_elec) + (4 * Efs * Math.Pow(c, 2)));
                double denumator = 18 * Math.Sqrt(2) * c;

                double UsedEnergy_Nanojoule = numator / denumator;
                double UsedEnergy_joule = ConvertToJoule(UsedEnergy_Nanojoule);
                return UsedEnergy_joule;
            }


            public double RandomNodes(double sideLength, double comRang, double SinksNumber)
            {
                double E_elec = PublicParamerters.E_elec;
                double Efs = PublicParamerters.Efs;
                double L = 1024;
                double Emp = PublicParamerters.Emp;

                double x1 = (18 * E_elec) + (4 * Efs * Math.Pow(comRang, 2));
                double x2 = (5 * Math.Log((1 + Math.Sqrt(2)))) + 2 + Math.Sqrt(2);
                double numator = sideLength * SinksNumber * L * (x1) * (x2);
                double denumator = 90 * comRang;

                double UsedEnergy_Nanojoule = numator / denumator;
                double UsedEnergy_joule = ConvertToJoule(UsedEnergy_Nanojoule);
                return UsedEnergy_joule;

            } 
        }


        public class ExpermentalEnergyConsumption 
        {
            public void SendToDiagonal()
            {
                int index = UniformRandomVariable.GetIntValue(0, PublicParamerters.MainWindow.myNetWork.Count - 1);
                Sensor sen = PublicParamerters.MainWindow.myNetWork[index];
                ObtainSinkFreshPositionMessage ob = new ObtainSinkFreshPositionMessage(sen);
            }

            public void SendRandTwoPoints( int sinksCount)
            {
                int indexSource = UniformRandomVariable.GetIntValue(0, PublicParamerters.MainWindow.myNetWork.Count - 1);
                Sensor source = PublicParamerters.MainWindow.myNetWork[indexSource];

                for (int i = 1; i <= sinksCount; i++)
                {
                    int indexDestination = UniformRandomVariable.GetIntValue(0, PublicParamerters.MainWindow.myNetWork.Count - 1);
                    Sensor dest = PublicParamerters.MainWindow.myNetWork[indexDestination];
                    TestSendPacketsBetweenTwoNodesRandomlySelected c = new TestSendPacketsBetweenTwoNodesRandomlySelected(source, dest);
                }

            }
        }
    }
}

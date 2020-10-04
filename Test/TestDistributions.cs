using MP.Computations;
using MP.Dataplane;
using MP.Intilization;
using MP.MergedPath.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MP.Test
{
    public class TestRow
    {
        public int iID
        {
            get; set;
        }

        public int jID
        {
            get; set;
        }

        public double ij { get { return Operations.DistanceBetweenTwoPoints(i, j); } }
        public double TransDis
        {
            get
            {
                return MeredPathDistributions.TransDistDistribution(i, j, 100);
            }
        }
        public double jsd { get { return Operations.Perpendiculardistance(j, s, d); } }

        public double PerDis
        {
            get
            {
                return MeredPathDistributions.PerpendicularDistanceDistribution(i, j, s, d, 100);
            }
        }

        public double Angle 
        {
            get
            {
                return Operations.AngleDotProdection(i, j, d);
            }
        }
        public double DotAngeDistribu
        {
            get
            {
                return MeredPathDistributions.ProximityToBranchEndPoint(i, j, d);
            }
        }

        public double CurentEn { get; set; }
        public double EnDist
        {
            get
            {
                return MeredPathDistributions.EnergyDistribution(CurentEn, 100);
            }
        }



        public Point i { get; set; }
        public Point j { get; set; }
        public Point s { get; set; }
        public Point d { get; set; }



        public double multip1 => DotAngeDistribu * (EnDist * PerDis * TransDis);
        public double multip2 => DotAngeDistribu * PerDis * (EnDist + +TransDis);
        public double multip3 => DotAngeDistribu * (EnDist + PerDis + TransDis);//   ij_ω * (ij_ψ + ij_d + ij_σ);

        public double aggregatedValue1 => Math.Pow(DotAngeDistribu, PerDis) + Math.Pow(EnDist, TransDis);
        public double aggregatedValue2 => Math.Pow(DotAngeDistribu, PerDis) * Math.Pow(EnDist, TransDis);


        public double average => (Math.Pow(DotAngeDistribu, PerDis) * ((EnDist + TransDis) / 2));
        public double Expon1 => Math.Pow(DotAngeDistribu, PerDis + EnDist +TransDis);

        public double Normalized
        {
            get
            {
                double sum = DotAngeDistribu + EnDist + PerDis + TransDis;
                double x1 = DotAngeDistribu / sum;
                double x2 = EnDist / sum;
                double x3 = PerDis / sum;
                double x4 = TransDis / sum;

                return x1 * (x2 + x3 + x4);
            }
        }


       
      
        public double sum => DotAngeDistribu + PerDis + (EnDist + +TransDis);

    }

    class TestDistributions
    {
        public Point s;
        public Point d;
        public TestDistributions(Point _s, Point _d)
        {
            s = _s;
            d = _d;
        }

        public List<TestRow> GetValues(int numberOfNeighbors)
        {
            List<TestRow> re = new List<TestRow>(numberOfNeighbors);
            for (int i = 0; i < numberOfNeighbors; i++)
            {
                TestRow row  = new TestRow();
                row.CurentEn = RandomvariableStream.UniformRandomVariable.GetDoubleValue(1, 100);
                row.i = new Point(RandomvariableStream.UniformRandomVariable.GetDoubleValue(0, 100), RandomvariableStream.UniformRandomVariable.GetDoubleValue(0, 600));
                row.j= new Point(RandomvariableStream.UniformRandomVariable.GetDoubleValue(0, 600), RandomvariableStream.UniformRandomVariable.GetDoubleValue(0, 600));
                row.s = s;
                row.d = d;
                re.Add(row);
            }

            return re;
        }


        public List<TestRow> GetValues(Sensor i)
        {
            List<TestRow> re = new List<TestRow>(i.NeighborsTable.Count);
            foreach (Sensor j in i.NeighborsTable)
            {
                TestRow row = new TestRow();
                row.CurentEn = j.ResidualEnergyPercentage;
                row.i = i.CenterLocation;
                row.j = j.CenterLocation;
                row.s = s;
                row.d = d;
                row.iID = i.ID;
                row.jID = j.ID;
                re.Add(row);
            }

            return re;
        }


    }
}

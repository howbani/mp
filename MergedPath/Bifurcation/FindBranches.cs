using MP.Dataplane;
using MP.Intilization;
using MP.MergedPath.SinkClustering;
using MP.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MP.MergedPath.Bifurcation
{
    public class Branch
    {
        public Cluster Cluster { get; set; }
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public int TimeToliveInBranch
        {
            get
            {
                double x = 1.2;
                double estimate = Operations.DistanceBetweenTwoPoints(StartPoint, EndPoint) / (PublicParamerters.CommunicationRangeRadius / Settings.Default.ComunTTL);
                return Convert.ToInt16(x * estimate);
            }
        }
    }

    public class FindBranches
    {
        public static List<Branch> GetBranches(List<Sensor> bk_sinks, Point b_k, Canvas Mycanvas, double clustringThreshould)
        {
            List<Branch> branches = new List<Branch>();

            // clustering:
            Clustering ck = new Clustering(bk_sinks, b_k, Mycanvas, clustringThreshould);
            List<Cluster> Clusters = ck.GetClusters;
            // find the branses:

            foreach (Cluster cluster in Clusters)
            {
                if (cluster.Members.Count == 1)
                {
                    if (cluster.Members[0] != null)
                    {
                        Point destination = cluster.Members[0].CenterLocation;
                        Branch branch = new Branch();
                        branch.Cluster = cluster;
                        branch.StartPoint = b_k;
                        branch.EndPoint = destination;
                        branches.Add(branch);
                    }
                }
                else
                {
                    
                    double d1 = cluster.DistanceToCenteriod;
                    double d2 = cluster.DistanceToClusterBorder;
                    Point next_Bk = Operations.GetPointMtoNratio(b_k, cluster.Centeriod, d2, d1);

                    Branch branch = new Branch();
                    branch.Cluster = cluster;
                    branch.StartPoint = b_k;
                    branch.EndPoint = next_Bk;
                    branches.Add(branch);
                }
            }
            return branches;
        }

    }
}

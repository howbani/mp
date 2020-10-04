using MP.Dataplane;
using MP.Intilization;
using System;
using Point = System.Windows.Point;

namespace MP.MergedPath.Routing
{
    class MeredPathDistributions
    {
        /// <summary>
        /// The proximity ω ̀_(i,j) to the subordinate point H is interpolated by Logistic distributing that has the mass function expressed in Eq.(19), which assigns higher priority to the nodes which are more closer to the end bifurcation point H (x ̀_h,y ̀_h).
        /// </summary>
        /// <returns></returns>
        public static double PerpendicularDistanceDistribution(Point i, Point j, Point s, Point d)
        {
            double pj = Operations.Perpendiculardistance(j, s, d);
            double pi = Operations.Perpendiculardistance(i, s, d);
            double ψ = pj / (pi + PublicParamerters.CommunicationRangeRadius);

            if (ψ == 0)
            {
                // next hop is the d. thus it retuerns 0.
                return 1;
            }
            else
            {
                double ε = 0.9838, γ = 0.0337, ϑ = 0.60167, λ = 5.05632;
                double re = ε + (γ - ε) * (Math.Pow(ψ, λ) / (Math.Pow(ψ, λ) + Math.Pow(ϑ, λ)));
                return re;
            }
        }

        public static double PerpendicularDistanceDistribution(Point i, Point j, Point s, Point d, double comrange)
        {
            double pj = Operations.Perpendiculardistance(j, s, d);
            double pi = Operations.Perpendiculardistance(i, s, d);
            double ψ = pj / (pi + comrange);

            if (ψ == 0)
            {
                // next hop is the d. thus it retuerns 0.
                return 1;
            }
            else
            {
                double ε = 0.9838, γ = 0.0337, ϑ = 0.60167, λ = 5.05632;
                double re = ε + (γ - ε) * (Math.Pow(ψ, λ) / (Math.Pow(ψ, λ) + Math.Pow(ϑ, λ)));
                return re;
            }
        }

        /// <summary>
        /// The proximity ω ̀_(i,j) to the subordinate point H is interpolated by Logistic distributing that has the mass function expressed in Eq.(19), which assigns higher priority to the nodes which are more closer to the end bifurcation point H (x ̀_h,y ̀_h).
        /// branch (b_k,b_h) 
        /// </summary>
        /// <param name="d"> end point of branch</param>
        /// <param name=""></param>
        public static double ProximityToBranchEndPoint(Point i, Point j, Point d)
        {
            
            double d_jd = Operations.DistanceBetweenTwoPoints(j, d);
            double d_id = Operations.DistanceBetweenTwoPoints(i, d);

            double ω = (d_jd) / (d_id + PublicParamerters.CommunicationRangeRadius);

            double γ = 0.98502, ε = 0.10463, λ = 0.44675, ϑ = 6.9;
            double re = ε + (γ - ε) / (1 + Math.Pow(ω / λ, ϑ));
            return re;
        }

        /// <summary>
        /// Battery status σ ̀_(i,j) of the node is modeled as Sigmoidal Logistic function as in Eq.(18) where σ_j denotes the current battery state of node n_j∈N_i, while σ_* denotes the initial battery energy. It offers a higher priority to the nodes that still have more energy.
        /// </summary>
        /// <param name="CurentEn"></param>
        /// <param name="intialEnergy"></param>
        /// <returns></returns>
        public static double EnergyDistribution(double CurentEn, double intialEnergy)
        {
            if (CurentEn > 0)
            {
                double σ = CurentEn / intialEnergy;
                double γ = 1.0069, ε = 0.70848, ϑ = 17.80843;
                double re = γ / (1 + Math.Exp(-ϑ * (σ - ε)));
                return re;
            }
            else
                return 0;
        }

        /// <summary>
        /// Based on the collected data about the network, these four attributes are modeled curve fitted as follows. The transmission distance d ̀_(i,j) between n_i  and n_j∈N_i is modeled as a random variable with mass function expressed as in Eq.(17), which is a sigmoidal curve interpolated by Boltzmann function. It obviously assigns a higher priority to the node with the shorter transmission distance.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static double TransDistDistribution(Point i, Point j)
        {
            double dis = Operations.DistanceBetweenTwoPoints(i, j);
            if (dis <= PublicParamerters.CommunicationRangeRadius)
            {
                double norDis = dis / PublicParamerters.CommunicationRangeRadius;
                double
                    γ = 0.98649, ε = -0.17869, ϑ = 0.77758, λ = 0.13113;
                double re = ε + ((γ - ε) / (1 + Math.Exp((norDis - ϑ) / (λ))));
                return re;
            }
            else return 0;
        }

        public static double TransDistDistribution(Point i, Point j, double comRange)
        {
            double dis = Operations.DistanceBetweenTwoPoints(i, j);
            if (dis <= comRange)
            {
                double norDis = dis / comRange;
                double
                    γ = 0.98649,
                    ε = -0.17869,
                    ϑ = 0.77758,
                    λ = 0.13113;
                double re = ε + ((γ - ε) / (1 + Math.Exp((norDis - ϑ) / (λ))));
                return re;
            }
            else
            {
                return 0;
            }
        }

        public static double TransDistDistribution(double dis, double comRange)
        {
            if (dis <= comRange)
            {
                double norDis = (dis / comRange);
                double
                    γ = 0.98649,
                    ε = -0.17869,
                    ϑ = 0.77758,
                    λ = 0.13113;
                double re = ε + ((γ - ε) / (1 + Math.Exp((norDis - ϑ) / (λ))));
                return re;
            }
            else
                return 0;
        }

    }
}

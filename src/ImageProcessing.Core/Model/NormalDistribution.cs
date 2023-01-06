using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Core.Model
{
    internal class NormalDistribution
    {
        public double Mean;
        public double Std;
        private Random rand;
        public NormalDistribution(double mean, double std)
        {
            Mean = mean;
            Std = std;
            rand = new Random();
        }

        public double Sample()
        {
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return Mean + Std * randStdNormal; //random normal(mean,stdDev^2)
        }
    }
}

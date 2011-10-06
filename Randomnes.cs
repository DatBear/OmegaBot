using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BattleNet
{
    class Randomnes
    {
        static double GaussianRandom(double mean, double stdDev)
        {
            Random rand = new Random(); //reuse this if you are generating many
            double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
             mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }

        static void sleep(double amount, double stdDev) 
        {
            Thread.Sleep((int)GaussianRandom(amount, stdDev));
        }

    }
}

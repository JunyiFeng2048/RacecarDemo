using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacecarDemo
{
    public class GA
    {
        public List<Car> FitnessList = new List<Car>();

        

        public void SortFitnessList()
        {
            FitnessList.Sort(((x, y) => x.CurrentFitness.CompareTo(y.CurrentFitness)));
        }


        public void Crossover()
        {


            for (int i = 0; i < FitnessList.Count; i += 2)
            {
                
                int CrossValue = FitnessList[i].chromosome.Count < FitnessList[i+1].chromosome.Count ? FitnessList[i].chromosome.Count : FitnessList[i+1].chromosome.Count;
                CrossValue /= 2;
                for (int j = 0; j < CrossValue; j++)
                {
                    //Console.WriteLine(FitnessList[i].chromosome[j]);
                    int temp = FitnessList[i].chromosome[j];
                    FitnessList[i].chromosome[j] = FitnessList[i + 1].chromosome[j];
                    FitnessList[i + 1].chromosome[j] = temp;
                }
                if (FitnessList[i].chromosome.Count < FitnessList[i + 1].chromosome.Count)
                {
                    for(int j = CrossValue; j < FitnessList[i+1].chromosome.Count; j++)
                    {
                        FitnessList[i].chromosome.Add(FitnessList[i+1].chromosome[j]);
                    }
                }
                else
                {
                    for (int j = CrossValue; j < FitnessList[i].chromosome.Count; j++)
                    {
                        FitnessList[i+1].chromosome.Add(FitnessList[i].chromosome[j]);
                    }
                }
            }

        }

        public void Mutation()
        {
            for (int i = 0; i < FitnessList.Count; i++)
            {
                for (int j = 0; j < FitnessList[i].chromosome.Count; j++)
                {
                    int seed = Guid.NewGuid().GetHashCode();
                    Random random = new Random(seed);
                    double a = 0;
                    double b = 1;
                    var next = random.NextDouble() * (b - a);
                    next += a;
                    if (next <= 0.2)
                    {
                        FitnessList[i].chromosome[j] = random.Next(0, 9);
                    }
                }
                

            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace BestChoiceResistor
{
    class Bestchoice
    {
        static void Main(string[] args)
        {
            Console.Write("output voltage (Vo) = ");
            double vo = double.Parse(Console.ReadLine());
            Console.Write("Reference voltage (Vref) = ");
            double vref = double.Parse(Console.ReadLine());

            Choice ru = new Choice(vref, vo);
            ru.calcresult();
            ru.bestr();
            Console.ReadLine();
            
        }
    }


    class Choice
    {
        private static double[] Re96 = {
                                    1.00,1.02,1.05,1.07,1.10,1.13,1.15,1.18,1.21,1.24,1.27,1.30,1.33,1.37,
                                    1.40,1.43,1.47,1.50,1.54,1.58,1.62,1.65,1.69,1.74,1.78,1.82,1.87,1.91,
                                    1.96,2.00,2.05,2.10,2.15,2.21,2.26,2.32,2.37,2.43,2.49,2.55,2.61,2.67,
                                    2.74,2.80,2.87,2.94,3.01,3.09,3.16,3.24,3.32,3.40,3.48,3.57,3.65,3.74,
                                    3.83,3.92,4.02,4.12,4.22,4.32,4.42,4.53,4.64,4.75,4.87,4.99,5.11,5.23,
                                    5.36,5.49,5.62,5.76,5.90,6.04,6.19,6.34,6.49,6.65,6.81,6.98,7.15,7.32,
                                    7.50,7.68,7.87,8.06,8.25,8.45,8.66,8.87,9.09,9.31,9.53,9.76
                                };
        private double vref, vo;

        struct Ru
        {
            public double rdd;
            public double ruu;
            public double runear;
            public double toll;

        }

        private Ru[] r = new Ru[Re96.Length ];
        private int count = 0;

        public Choice(double vref, double vo)
        {
            this.vref = vref;
            this.vo = vo;
            calc();
            //bestr();
        }

        public void testwrite()
        {
            Console.WriteLine("内部呼び出し");
            testwrite("Vref", vref);
            testwrite("Vo", vo);
            Console.WriteLine("Vo = {0}, Vref = {1}\n", vo, vref);
            Console.ReadLine();
        }

        private void testwrite(string st, double val)
        {
            Console.WriteLine("{0} = {1}",st,val);
        }

        double P10(int y)
        {
            return (Math.Pow(10,y));
        }

        private void calc()
        {
            //下側の抵抗を96系列の中から順次選んだ際の上側の抵抗を計算
            foreach (double rd in Re96)
            {
                double ans = (vo / vref - 1) * rd;
                r[count].rdd = rd;
                r[count].ruu = ans;

                /*96系列のテーブルは10^(1)のため、10^(2)や10^(-1)といった計算がそのままでは出来ないため、
                 * 一旦10^(1)に変換してやる
                 */
                int ansdigit = (int) Math.Floor(Math.Log10(ans));
                double ansp = ans / P10(ansdigit);

                //上側の真値に対し、最も近い値を96系列から探る                              
                double tol = Math.Abs((1 - ansp) / ansp);
                double Rnear = 1.0;
                foreach (double Rn in Re96)
                {
                    double tollerance = Math.Abs((Rn - ansp) / Rn);
                    if (tol > tollerance)
                    {
                        tol = tollerance;
                        Rnear = Rn;
                    }
                }
                Rnear = Rnear * P10(ansdigit);
                r[count].runear = Rnear;

                //求められる出力電圧と実際の出力電圧との公差を計算
                //r[count].toll = tol; //真値の抵抗値との公差
                r[count].toll = Math.Abs(vo-(vref*(1+Rnear/rd)))/vo;
                
                //Console.WriteLine("Rd = {0:f2},\t Ru = {1:f} ≒ {2:f},\t tolerance = {3:e2} %",
                    //r[count].rdd,r[count].ruu,r[count].runear,r[count].toll*100);
                count++;

            }

        }

        public void calcresult()
        {
            foreach (Ru rueach in r)
            {
                Console.WriteLine("Rd = {0:f2},\t Ru = {1:f} ≒ {2:f},\t tolerance = {3:e2} %",
                    rueach.rdd,rueach.ruu,rueach.runear,rueach.toll*100);
            }
        }

        public void bestr()
        {
            int same = 1;
            Ru[] btr = new Ru[same];
            double tolmin = 1;

            foreach (Ru rueach in r)
            {
                if ((tolmin > rueach.toll))
                {
                    tolmin = rueach.toll;
                    same = 1;
                    btr = new Ru[same];
                    btr[0].toll = rueach.toll;
                    btr[0].ruu = rueach.ruu;
                    btr[0].rdd = rueach.rdd;
                    btr[0].runear = rueach.runear;

                }
                else if (tolmin == rueach.toll)
                {
                    same = same + 1;
                    btr.CopyTo(btr = new Ru[same], 0);
                    btr[same - 1].toll = rueach.toll;
                    btr[same - 1].ruu = rueach.ruu;
                    btr[same - 1].rdd = rueach.rdd;
                    btr[same - 1].runear = rueach.runear;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Best choice");
            Console.WriteLine("Vo = {0}, Vref = {1}", vo, vref);
            foreach (Ru bestrueach in btr)
            {
                Console.WriteLine("Rd = {0:f2},\t Ru = {1:e3} ≒ {2:f},\t Vo tolerance = {3:e2} %",
                    bestrueach.rdd, bestrueach.ruu, bestrueach.runear, bestrueach.toll*P10(2));
            }
            
        }
    }
}

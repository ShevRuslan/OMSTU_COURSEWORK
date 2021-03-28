using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMSTU_COURSEWORK
{
    class Matrix
    {
        private string[,] data;

        private int m;
        public int M { get => this.m; }

        private int n;
        public int N { get => this.n; }

        public Matrix(int m, int n)
        {
            this.m = m;
            this.n = n;
            this.data = new string[m + 1, n + 1];
        }
        public void AddFirstRow(string[] elements)
        {
            for (int i = 0; i < 1; i++)
            {
                for (int j = 1; j <= this.n; j++)
                {
                    this.data[i, j] = elements[j - 1];
                }
            }
        }
        public void AddFirstCol(string[] elements)
        {
            for (int i = 1; i <= this.m; i++)
            {
                for (int j = 0; j < 1; j++)
                {
                    this.data[i, j] = elements[i - 1];
                }
            }
        }
        public string[,] GetMatrix ()
        {
            return this.data;
        }
     }
}

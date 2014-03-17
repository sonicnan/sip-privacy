using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using Security.Cryptography;


namespace Security.Key
{
    public class OfflineKeyGenerator
    {
        private delegate void CallFunction(int n, int m, string kab, string dk);
        private int m;
        private int n;
        private string kab;
        private string dk;

        public OfflineKeyGenerator()
        {
            //this.LogManager = logmanager;
        }

        public ArrayList GenOfflineKey(int n, int m, string kab, string dk)
        {
            this.n = n;
            this.m = m;
            this.kab = kab;
            this.dk = dk;
            THashAlgorithm.SHATYPE HashAlgorithm = THashAlgorithm.SHATYPE.SHA1;
            ArrayList ki = new ArrayList();
            if (m < 3)
            {
                m = 3;
            }
            string k = "";
            for (int i = 0; i < m; i++)
            {

                if (i == 0)
                {
                    k = THashAlgorithm.ComputeHash(kab + dk, HashAlgorithm);
                }
                else
                {
                    k = THashAlgorithm.ComputeHash(k + dk, HashAlgorithm);

                }
                ki.Add(k);
            }
            int mid1 = 0;
            int mid2 = 0;
            string KeyMid1 = "";
            string KeyMid2 = "";

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (j == 0)
                    {
                        mid1 = FindMid(m);
                        mid2 = FindMid(mid1);
                        KeyMid1 = GetKey(ki, mid1);
                        KeyMid2 = GetKey(ki, mid2);

                        ki.Clear();

                        k = THashAlgorithm.ComputeHash(KeyMid1 + KeyMid2, HashAlgorithm);

                    }
                    else
                    {
                        k = THashAlgorithm.ComputeHash(KeyMid1 + KeyMid2 + k, HashAlgorithm);
                    }
                    ki.Add(k);
                }
            }
            return ki;
        }

        private int FindMid(int m)
        {
            int i = 0;
            i = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(m) / 2));
            return i;
        }

        private string GetKey(ArrayList keyList, int pos)
        {
            string key = "";
            key = keyList[pos - 1].ToString();
            return key;
        }
    }
}

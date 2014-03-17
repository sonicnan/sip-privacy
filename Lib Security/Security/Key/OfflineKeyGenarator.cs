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
        private List<Offlinekey> tmplist;
        private delegate void CallFunction(int n, int m, string kab, string dk);
        private int m;
        private int n;
        private string kab;
        private string dk;

        public OfflineKeyGenerator()
        {
            //this.LogManager = logmanager;
        }

        public List<Offlinekey> GenOfflineKey(int n, int m, string kab, string dk)
        {
            List<Offlinekey> keyList = new List<Offlinekey>();
            this.n = n;
            this.m = m;
            this.kab = kab;
            this.dk = dk;
            THashAlgorithm.SHATYPE HashAlgorithm = THashAlgorithm.SHATYPE.SHA1;
            
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
                keyList.Add(new Offlinekey() { id = k.Substring(0,5), key = k });
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
                        KeyMid1 = GetKey(keyList, mid1);
                        KeyMid2 = GetKey(keyList, mid2);

                        keyList.Clear();

                        k = THashAlgorithm.ComputeHash(KeyMid1 + KeyMid2, HashAlgorithm);

                    }
                    else
                    {
                        k = THashAlgorithm.ComputeHash(KeyMid1 + KeyMid2 + k, HashAlgorithm);
                    }

                    keyList.Add(new Offlinekey() { id = k.Substring(0, 5), key = k });
                }
            }
            return  keyList;
        }

        public List<Offlinekey> updateOfflineKey(int n, int m,List<Offlinekey> oldkey)
        {
            this.tmplist = oldkey.ConvertAll((s => new Offlinekey(s)));
            this.n = n;
            this.m = m;
            int mid1 = 0;
            int mid2 = 0;
            string KeyMid1 = "";
            string KeyMid2 = "";
            string k = "";
            THashAlgorithm.SHATYPE HashAlgorithm = THashAlgorithm.SHATYPE.SHA1;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (j == 0)
                    {
                        mid1 = FindMid(m);
                        mid2 = FindMid(mid1);
                        KeyMid1 = GetKey(tmplist, mid1);
                        KeyMid2 = GetKey(tmplist, mid2);

                        tmplist.Clear();

                        k = THashAlgorithm.ComputeHash(KeyMid1 + KeyMid2, HashAlgorithm);

                    }
                    else
                    {
                        k = THashAlgorithm.ComputeHash(KeyMid1 + KeyMid2 + k, HashAlgorithm);
                    }

                    tmplist.Add(new Offlinekey() { id = k.Substring(0, 5), key = k });
                }
            }
            return tmplist;
        }

        private int FindMid(int m)
        {
            int i = 0;
            i = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(m) / 2));
            return i;
        }

        private string GetKey(List<Offlinekey> keyList, int pos)
        {
            string key = "";
            key = keyList[pos - 1].key;
            return key;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Security.Key
{
    public class OfflineKeyServiceProvider
    {

        int current = 0;

        OfflineKeyGenerator genofflinekey = new OfflineKeyGenerator();
        List<Offlinekey> keyList = new List<Offlinekey>();

        string file = AppDomain.CurrentDomain.BaseDirectory + "\\Offlinekey.xml";

        public void loadOfflinekey(string kab,string dk)
        {

            
            DataSet dsOfflinekey = new DataSet("Offlinekey");
            DataTable dtKeylist = dsOfflinekey.Tables.Add("Keylist");
            dtKeylist.Columns.Add("Id");
            dtKeylist.Columns.Add("Key");

            if (File.Exists(file))
            {
                
                dsOfflinekey.ReadXml(file);

                foreach (DataRow dr in dtKeylist.Rows)
                {
                    keyList.Add(new Offlinekey() { id = GetValueString(dr, "Id"), key = GetValueString(dr, "Key") });
                }
            }
            else 
            {

                keyList = genofflinekey.GenOfflineKey(30, 30, kab, dk);
                foreach (Offlinekey i in keyList)
                {
                    DataRow dr = dtKeylist.NewRow();
                    dr["Id"] = i.id;
                    dr["Key"] = i.key;
                    dtKeylist.Rows.Add(dr);
                }

                dsOfflinekey.WriteXml(file);
            }

        }

        public Offlinekey getOfflinekey()
        {
            current++;
            if (current > 14)
            {

                DataSet dsOfflinekey = new DataSet("Offlinekey");
                DataTable dtKeylist = dsOfflinekey.Tables.Add("Keylist");
                dtKeylist.Columns.Add("Id");
                dtKeylist.Columns.Add("Key");



                keyList.AddRange(genofflinekey.updateOfflineKey(30, 10, keyList));
               
                keyList.RemoveRange(0, 10);

                foreach (Offlinekey i in keyList)
                {
                    DataRow dr = dtKeylist.NewRow();
                    dr["Id"] = i.id;
                    dr["Key"] = i.key;
                    dtKeylist.Rows.Add(dr);
                }

                current = 0;
                dsOfflinekey.WriteXml(file);

            }



            return keyList[current];
        }

        public Offlinekey getOfflinekey(string idkey)
        {
            current++;
            Offlinekey temp = new Offlinekey();
            /*
            for(int i = 0; i<keyList.Count; i++)
            {
                if (keyList[i].id.Equals(idkey))
                {
                    current = i;
                    temp = keyList[i];
                    break;
                }
            }*/

            try
            {
                int index = keyList.IndexOf(keyList.Where<Offlinekey>(x => x.id == idkey).Single<Offlinekey>());
                current = index;
                temp = keyList[index];
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Cannot Found\n");
            }
            
            if (current > 14)
            {

                DataSet dsOfflinekey = new DataSet("Offlinekey");
                DataTable dtKeylist = dsOfflinekey.Tables.Add("Keylist");
                dtKeylist.Columns.Add("Id");
                dtKeylist.Columns.Add("Key");


                keyList.AddRange(genofflinekey.updateOfflineKey(30, 10, keyList));

                keyList.RemoveRange(0, 10);

                foreach (Offlinekey i in keyList)
                {
                    DataRow dr = dtKeylist.NewRow();
                    dr["Id"] = i.id;
                    dr["Key"] = i.key;
                    dtKeylist.Rows.Add(dr);
                }

                current = 0;
                dsOfflinekey.WriteXml(file);

                return keyList[current];
            }
            else
            {
                return temp;
            }


        }

        public string GetValueString(DataRow dr, string fieldName)
        {
            return GetValue(dr, fieldName).ToString();
        }

        public object GetValue(DataRow dr, string fieldName)
        {
            if (dr == null)
            {
                throw new ArgumentNullException("dr");
            }
            if (fieldName == null)
            {
                throw new ArgumentNullException("fieldName");
            }
            if (fieldName == "")
            {
                throw new ArgumentException("Argument 'fieldName' value must be specified.");
            }
            if (!dr.Table.Columns.Contains(fieldName))
            {
                throw new ArgumentException("Specified fieldName '" + fieldName + "' does not exist in table '" + dr.Table.TableName + "'.");
            }

            return dr[fieldName];
        }
    }
}

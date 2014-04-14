using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Security.Key
{
    public class OfflineKeyServiceProvider
    {

        int current = 0;
        string username = null;

        OfflineKeyGenerator genofflinekey = new OfflineKeyGenerator();
        List<Offlinekey> keyList = new List<Offlinekey>();

        string subPath = Application.StartupPath + "\\Settings\\Key\\";
        string file = null;

        public OfflineKeyServiceProvider(string kab, string dk, string username)
        {
            this.username = username;
            file = username+".olk";
            

            if (File.Exists(subPath))
            {
                load();
            }
            else 
            {

                keyList = genofflinekey.GenOfflineKey(30, 30, kab, dk);

                save();
            }

        }

        public Offlinekey getOfflinekey()
        {
            current++;
            if (current > 14)
            {

                keyList.AddRange(genofflinekey.updateOfflineKey(30, 10, keyList));
                keyList.RemoveRange(0, 10);
                current = 0;

                save();
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
                throw new Exception("Cannot get Offlinekey");
            }
            
            if (current > 14)
            {

                keyList.AddRange(genofflinekey.updateOfflineKey(30, 10, keyList));
                keyList.RemoveRange(0, 10);

                save();
                
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

        protected void load()
        {
            DataSet dsOfflinekey = new DataSet("Offlinekey");
            dsOfflinekey.ReadXml(subPath + file);

            if (dsOfflinekey.Tables["Setting"].Rows.Count > 0)
            {
                DataRow dr = dsOfflinekey.Tables["Setting"].Rows[0];
                username = dr["Username"].ToString();
                current = Convert.ToInt16(dr["Current"]);
            }

            foreach (DataRow dr in dsOfflinekey.Tables["Keylist"].Rows)
            {
                keyList.Add(new Offlinekey() { id = GetValueString(dr, "Id"), key = GetValueString(dr, "Key") });
            }
        }

        public void save()
        {
            DataSet dsOfflinekey = new DataSet("Offlinekey");
            
            dsOfflinekey.Tables.Add("Setting");
            dsOfflinekey.Tables["Setting"].Columns.Add("Username");
            dsOfflinekey.Tables["Setting"].Columns.Add("Current");
            dsOfflinekey.Tables.Add("Keylist");
            dsOfflinekey.Tables["Keylist"].Columns.Add("Id");
            dsOfflinekey.Tables["Keylist"].Columns.Add("Key");

            DataRow drSipSettings = dsOfflinekey.Tables["Setting"].Rows.Add();
            drSipSettings["Username"] = username;
            drSipSettings["Current"] = current;

            foreach (Offlinekey i in keyList)
            {
                DataRow dr = dsOfflinekey.Tables["Keylist"].NewRow();
                dr["Id"] = i.id;
                dr["Key"] = i.key;
                dsOfflinekey.Tables["Keylist"].Rows.Add(dr);
            }
            if (!Directory.Exists(subPath))
                Directory.CreateDirectory(subPath);

            dsOfflinekey.WriteXml(subPath + file);
        }

        public string user
        {
            get { return username; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Security;

namespace Nickname
{
    public class Nicknameserviceprovider
    {
        List<Nickname> NameList = new List<Nickname>();

        public string getNickname(string inputname)
        {
            string name = RNGRandom.RandomString(8);
            NameList.Add(new Nickname() { username = inputname, nickname=name });
            return name;
        }

        public string getUsername(string inputname)
        {
            string outputname = "";
            try
            {
                Nickname agedTwenty = NameList.Where<Nickname>(x => x.nickname.ToLower() == inputname.ToLower()).Single<Nickname>();
                int index = NameList.IndexOf(agedTwenty);
                outputname = NameList[index].username;
                NameList.RemoveAt(index);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Cannot Found\n");
            }

            return outputname;
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Mayhaps
{
    public class Folder
    {
        public int? setIndex = null;
        public string fullpath { get; set; }
        public List<string> manifest { get; set; }
        public int? currentIndex = null;

        public Folder(string fullpath, List<string> manifest)
        {
            this.fullpath = fullpath;
            this.manifest = manifest;
            //MessageBox.Show(fullpath);
            //foreach (var item in manifest) { MessageBox.Show(item); }
        }
    }

    static class folderListContainer
    {
        public static List<Folder> folderList = new List<Folder>();
        public static List<Folder> switchFolders = new List<Folder>();
        public static List<Folder> hetFolders = new List<Folder>();
        public static string? bioName;
        public static int darkMode = 0;
        public static string? coatType;
    }

    public static class txtLists
    {
        public static string[] negList { get; set; }
        public static string[] neuList { get; set; }
        public static string[] posList { get; set; }
        public static string[]? preList { get; set; }
        public static string[]? sufList { get; set; }
        public static string[]? MpetList { get; set; }
        public static string[]? FpetList { get; set; }
    }

    public class role
    {
        public string name { get; set; }
        public int[] age { get; set; }
        public string suffix { get; set; }
    }

    public class clan
    {
        public string name { get; set; }
        public List<role> roles { get; set; }
    }
    public class clanList
    {
        public List<clan> clans { get; set; }

        public clanList loadClans()
        {
            string dir = (string)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", null);
            dir += "\\text\\clans.json";
            clanList clanList = JsonConvert.DeserializeObject<clanList>(File.ReadAllText(dir));
            return clanList;
        }
    }
    public class bio
    {
        public string name;
        public int age;
        public string clan;
        public string role;
        public string pers;
        public string gender;
        //public string[] pronounArray = new string[4];

        public void generateBio(clanList clanList1)
        {
            Random rand = new Random();
            //generate clan
            clan[] genClan = clanList1.clans.ToArray();
            clan genClan1 = genClan[rand.Next(0, clanList1.clans.Count)];
            clan = genClan1.name;
            //generate role
            role genRole = genClan1.roles[rand.Next(0, genClan1.roles.Count)];
            role = genRole.name;
            //generate age
            age = rand.Next(genRole.age[0], genRole.age[1]);
            //generate gender
            int genGenderInt = rand.Next(0, 3);
            switch (genGenderInt)
            {
                case 0:
                    gender = "Male";
                    name = txtLists.MpetList[rand.Next(0, txtLists.MpetList.Length)];
                    /*pronounArray[0] = "He";
                    pronounArray[1] = "Him";
                    pronounArray[2] = "His";
                    pronounArray[3] = "he";*/
                    break;
                case 1:
                    gender = "Female";
                    name = txtLists.FpetList[rand.Next(0, txtLists.FpetList.Length)];
                    /*pronounArray[0] = "She";
                    pronounArray[1] = "Her";
                    pronounArray[2] = "Her";
                    pronounArray[3] = "she";*/
                    break;
                case 2:
                    gender = "Non-Binary";
                    /*pronounArray[0] = "They";
                    pronounArray[1] = "Them";
                    pronounArray[2] = "Their";
                    pronounArray[3] = "they";*/
                    int enbySwitch = rand.Next(0, 2);
                    switch (enbySwitch)
                    {
                        case 0:
                            name = txtLists.MpetList[rand.Next(0, txtLists.MpetList.Length)];
                            break;
                        case 1:
                            name = txtLists.FpetList[rand.Next(0, txtLists.FpetList.Length)];
                            break;
                    }
                    break;
            }
            //generate name
            if (role != "Kittypet")
            {
                string prefix = txtLists.preList[rand.Next(0, txtLists.preList.Length)];
                string suffix;
                if (genRole.suffix == null)
                {
                    suffix = txtLists.sufList[rand.Next(0, txtLists.sufList.Length)];
                }
                else
                {
                    suffix = genRole.suffix;
                }
                name = prefix + suffix;
            }
            //generate personality
            string genNeg = txtLists.negList[rand.Next(0, txtLists.negList.Length)];
            string genNeu = txtLists.neuList[rand.Next(0, txtLists.neuList.Length)];
            string genPos = txtLists.posList[rand.Next(0, txtLists.posList.Length)];
            pers = string.Format("{0} is:\n- {1}\n- {2}\n- {3}", name, genPos, genNeu, genNeg);
            return;
        }
    }
}
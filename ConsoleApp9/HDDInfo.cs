using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ConsoleApp9
{
    public class HDDInfo
    {
        DriveInfo[] allDrives = DriveInfo.GetDrives();

        public void PrintInfoDrive()
        {
            foreach (DriveInfo d in allDrives)
            {
                Console.WriteLine("Drive {0}", d.Name);
                
                if (d.IsReady == true && d.TotalSize !=0)
                {
                    Console.WriteLine("Available space to current user:{0, 15} Gb", (d.AvailableFreeSpace)/(1024*1024*1024));
                }
            }
        }


    }
}
  


        
    
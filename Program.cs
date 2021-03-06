﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BMBF_Corrupted_songs_detector
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Please select the BMBF Log.");
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                if (!File.Exists(ofd.FileName))
                {
                    Console.WriteLine("The file doesn't exist.");
                    Console.ReadLine();
                    return;
                }

            }
            Console.WriteLine("found " + ofd.FileName);
            Console.WriteLine("at which line would you want to start?");
            String result = Console.ReadLine();
            if (result == "") result = "0";
            int start = Convert.ToInt32(result);
            int i = 0;
            StreamReader r = new StreamReader(ofd.FileName);
            String line = "";
            List<String> found = new List<string>();

            List<String> cover = new List<string>();
            List<string> failedload = new List<string>();
            List<string> weirderror = new List<string>();

            while((line = r.ReadLine()) != null)
            {
                if (i >= start && line.Length > 28)
                {
                    if (line.Substring(25, 3) == "ERR")
                    {
                        String err;
                        if (line.Contains("custom_level_") && line.Contains("Cover"))
                        {
                            err = "Custom level contains unsupported cover format (" + line.Substring(line.IndexOf("custom_level_"), 53) + ")";
                            if(!cover.Contains(line.Substring(line.IndexOf("custom_level_"), 53))) cover.Add(line.Substring(line.IndexOf("custom_level_"), 53));

                            if (!found.Contains(err))
                            {
                                found.Add(err);
                            }
                        }
                        else if (line.Contains("custom_level_") && line.Contains("failed to load"))
                        {
                            err = "Custom level (" + line.Substring(line.IndexOf("custom_level_"), 53) + ") couldn't load at line " + i;
                            if (!failedload.Contains(line.Substring(line.IndexOf("custom_level_"), 53))) failedload.Add(line.Substring(line.IndexOf("custom_level_"), 53));
                            if (!found.Contains(err))
                            {
                                found.Add(err);
                            }
                        }
                        else if(line.Contains("Exception writing assets file sharedassets0.assets Object reference not set to an instance of an object"))
                        {
                            err = "QuestomAssets Problem at line " + i;
                            if (!found.Contains(err))
                            {
                                found.Add(err);
                            }
                        }
                        else if(line.Contains("System.IO.FileStream"))
                        {
                            err = "FileStream Problem at line " + i;
                            if (!found.Contains(err))
                            {
                                found.Add(err);
                            }
                        }
                        else if(line.Contains("Error loading cover art asset Failed to allocate"))
                        {
                            
                            string tryhash = File.ReadLines(ofd.FileName).Skip(i-1).Take(1).First();
                            string hash = "N/A";
                            try
                            {
                                hash = tryhash.Substring(tryhash.IndexOf("hash") + 5, 40);
                            } catch { }
                            err = "Cover asset art failed to load. Potiential Song hash " + hash + " at line " + i;
                            if (!weirderror.Contains(hash)) weirderror.Add(hash);
                            if (!found.Contains(err))
                            {
                                found.Add(err);
                            }
                        }
                    }
                    else if(line.Substring(25, 3) == "MSG")
                    {
                        String msg;
                        if(line.Contains("custom songs to inject"))
                        {
                            StringReader s = new StringReader(line.Substring(line.IndexOf("Found")));
                            s.ReadWord();
                            msg = s.ReadWord() + " Songs at line " + i;
                            found.Add(msg);
                        }
                        else if (line.Contains("starting up") && line.Contains("BMBF Service"))
                        {
                            StringReader s = new StringReader(line.Substring(line.IndexOf("BMBF Service")));
                            s.ReadWord();
                            s.ReadWord();
                            StringReader time = new StringReader(line);
                            msg = "\nBMBF Service " + s.ReadWord() + " started at " + time.ReadWord() + " " + time.ReadWord() + " (line " + i + ")";
                            found.Add(msg);
                        }
                    }
                }
                i++;
            }
            Console.WriteLine("\ncommon fixes:");
            //Console.WriteLine("- Questom asset problem: tell the person to delete the songs with a unsupported cover format. If that doesn't help tell them to delete songs that failed to load.");
            Console.WriteLine("- FileStream Problem: tell the persong to enable both permssions for BMBF in sidequest");
            Console.WriteLine("\n---Log Start---");
            foreach(String c in found)
            {
                Console.WriteLine(c);
            }
            Console.WriteLine("\n\n---Log End---");
            Console.WriteLine("\n\n---Song Summary---");

            if (cover.Count != 0)
            {
                Console.WriteLine("\nUnsupported Cover formats (delete those songs):");
                foreach (String c in cover)
                {
                    Console.WriteLine("  - " + c);
                }
            }


            if (failedload.Count != 0)
            {
                //Console.WriteLine("\nSongs that failed to load (delete those songs if deleting the ones with unsupported cover format didn't help):");
                foreach (String c in failedload)
                {
                    Console.WriteLine("  - " + c);
                }
            }

            if (weirderror.Count != 0)
            {
                Console.WriteLine("\nCover asset are failed to load (definetly delete those ones; potential song hash):");
                foreach (String c in weirderror)
                {
                    Console.WriteLine("  - " + c);
                }
            }
            if(weirderror.Count != 0 || cover.Count != 0 || failedload.Count != 0)
            {
                Console.WriteLine("\n\nAnd don't forget to hit reload songs folder afterwards");
            } else
            {
                Console.WriteLine("Nothing found");
            }

            Console.ReadLine();
        }
    }
}
public static class StringReaderExtensions
{
    public static string ReadWord(this StringReader reader)
    {
        string result = "";

        // Read characters until we find a space
        while (true)
        {
            char nextChar = (char)reader.Read();
            if (nextChar == ' ') { break; }

            result += nextChar;
        }

        return result; // Return the characters without the space
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PostBuildEvent
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                String workDir = args[0];
                String scriptFilesLua = workDir + "/scriptfiles.lua";
                if (File.Exists(scriptFilesLua))
                    File.Delete(scriptFilesLua);
                FileStream fs = new FileStream(scriptFilesLua, FileMode.Create, FileAccess.Write, FileShare.Write);
                StreamWriter sw = new StreamWriter(fs);
                String[] files = Directory.GetFiles(workDir + "/" + args[1]);
                sw.WriteLine("--This is an auto-generated file please do not modify--");
                sw.Write("WP7Files = \"");
                for(Int32 i = 0; i < files.Length; i++)
                {
                    sw.Write(Path.GetFileName(files[i]));
                    if (i != files.Length - 1)
                        sw.Write(",");
                    else
                        sw.WriteLine("\";");
                }
                sw.Flush();
                sw.Close();
                fs.Close();

                String resFilesLua = workDir + "/resfiles.lua";
                if (File.Exists(resFilesLua))
                    File.Delete(resFilesLua);
                fs = new FileStream(resFilesLua, FileMode.Create, FileAccess.Write, FileShare.Write);
                sw = new StreamWriter(fs);
                String[] directories = Directory.GetDirectories(workDir + "/");

                List<String> drawableDirectoriesList = new List<String>();
                List<String> valueDirectoriesList = new List<String>();

                foreach (String directory in directories)
                {
                    String directoryName = Path.GetFileName(directory);

                    if (directoryName.StartsWith("drawable"))
                        drawableDirectoriesList.Add(directory);
                    else if (directoryName.StartsWith("value"))
                        valueDirectoriesList.Add(directory);
                }

                String[] drawableDirectories = drawableDirectoriesList.ToArray();
                String[] valueDirectories = valueDirectoriesList.ToArray();
                sw.WriteLine("--This is an auto-generated file please do not modify--");

                //Drawable files
                sw.Write("WP7DrawableDirectories = \"");
                for (Int32 i = 0; i < drawableDirectories.Length; i++)
                {
                    sw.Write(Path.GetFileName(drawableDirectories[i]));
                    if (i != drawableDirectories.Length - 1)
                        sw.Write(",");
                }
                sw.Flush();
                sw.WriteLine("\";");

                
                int count = 0;
                sw.Write("WP7DrawableFiles = \"");
                foreach (String directory in drawableDirectories)
                {
                    files = Directory.GetFiles(directory);

                    for (Int32 i = 0; i < files.Length; i++)
                    {
                        if(!(count == 0 && i == 0))
                            sw.Write(",");
                        sw.Write(count);
                        sw.Write("|");
                        sw.Write(Path.GetFileName(files[i]));                            
                    }
                    sw.Flush();
                    count++;
                }
                sw.WriteLine("\";");
                sw.Flush();

                //Value files
                sw.Write("WP7ValueDirectories = \"");
                for (Int32 i = 0; i < valueDirectories.Length; i++)
                {
                    sw.Write(Path.GetFileName(valueDirectories[i]));
                    if (i != valueDirectories.Length - 1)
                        sw.Write(",");
                }
                sw.Flush();
                sw.WriteLine("\";");
                
                count = 0;
                sw.Write("WP7ValueFiles = \"");
                foreach (String directory in valueDirectories)
                {
                    files = Directory.GetFiles(directory);
                    
                    for (Int32 i = 0; i < files.Length; i++)
                    {
                        if (!(count == 0 && i == 0))
                            sw.Write(",");
                        sw.Write(count);
                        sw.Write("|");
                        sw.Write(Path.GetFileName(files[i]));                            
                    }
                    sw.Flush();
                    count++;
                }
                sw.WriteLine("\";");
                sw.Flush();
                sw.Close();
                fs.Close();
            }
        }
    }
}

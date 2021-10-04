using System;
using System.Collections.Generic;
using System.IO;

namespace UnitTests
{
    class TempFileCollection : IDisposable
    {
        List<string> tempFiles = new List<string>();

        public string CreateTempFile(string contents)
        {
            var name = System.IO.Path.GetTempFileName();
            File.WriteAllText(name, contents);
            tempFiles.Add(name);
            return name;
        }

        public void Dispose()
        {
            foreach(var file in tempFiles)
            {
                try
                {
                    if (System.IO.File.Exists(file))
                    {
                        File.Delete(file);
                    }
                } catch { }
            }
        }
    }
}

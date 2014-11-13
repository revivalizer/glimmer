using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace glimmer
{
    class FileViewModel
    {
        static int fileCount = 0;

        string title;

        public FileViewModel()
        {
            fileCount = fileCount + 1;
            title = "New File " + fileCount;
        }
    }
}
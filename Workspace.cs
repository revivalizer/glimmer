using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using Xceed.Wpf.AvalonDock.Layout;

namespace glimmer
{
    class Workspace
    {
        protected Workspace()
        {
            // Test files
            _files.Add(new FileViewModel());
            _files.Add(new FileViewModel());
            _files.Add(new FileViewModel());
        }

        static Workspace _this = new Workspace();

        public static Workspace This
        {
            get { return _this; }
        }

        ObservableCollection<FileViewModel> _files = new ObservableCollection<FileViewModel>();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.ComponentModel;
using Xceed.Wpf.AvalonDock.Layout;
using ICSharpCode.AvalonEdit.Document;
using System.Diagnostics;

namespace glimmer
{
    class Workspace : ViewModelBase
    {
        protected Workspace()
        {
        }

        static Workspace _this = new Workspace();

        public static Workspace This
        {
            get { return _this; }
        }

        ObservableCollection<FileViewModel> _files = new ObservableCollection<FileViewModel>();
        ReadOnlyObservableCollection<FileViewModel> _readonlyFiles = null;
        public ReadOnlyObservableCollection<FileViewModel> Files
        {
            get
            {
                if (_readonlyFiles == null)
                    _readonlyFiles = new ReadOnlyObservableCollection<FileViewModel>(_files);

                return _readonlyFiles;
            }
        }

        #region ActiveDocument
        private FileViewModel _activeDocument = null;
        public FileViewModel ActiveDocument
        {
            get { return _activeDocument; }
            set
            {
                if (_activeDocument != value)
                {
                    _activeDocument = value;
                    RaisePropertyChanged("ActiveDocument");
                    if (ActiveDocumentChanged != null)
                        ActiveDocumentChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler ActiveDocumentChanged;
        #endregion

        #region OpenCommand
        RelayCommand _openCommand = null;
        public ICommand OpenCommand
        {
          get
          {
            if (_openCommand == null)
            {
              _openCommand = new RelayCommand((p) => OnOpen(p), (p) => CanOpen(p));
            }

            return _openCommand;
          }
        }

        private bool CanOpen(object parameter)
        {
          return true;
        }

        private void OnOpen(object parameter)
        {
          var dlg = new OpenFileDialog();
          if (dlg.ShowDialog().GetValueOrDefault())
          {
            var fileViewModel = Open(dlg.FileName);
            ActiveDocument = fileViewModel;
          }
        }

        public FileViewModel Open(string filepath)
        {
          var fileViewModel = _files.FirstOrDefault(fm => fm.FilePath == filepath);
          if (fileViewModel != null)
            return fileViewModel;

          fileViewModel = new FileViewModel(filepath);
          _files.Add(fileViewModel);

          return fileViewModel;
        }

        #endregion

        #region NewCommand
        RelayCommand _newCommand = null;
        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new RelayCommand((p) => OnNew(p), (p) => CanNew(p));
                }

                return _newCommand;
            }
        }

        private bool CanNew(object parameter)
        {
            return true;
        }

        private void OnNew(object parameter)
        {
            _files.Add(new FileViewModel() { Document = new TextDocument() });
            ActiveDocument = _files.Last();
        }
        #endregion 

        internal void Close(FileViewModel fileToClose)
        {
          if (fileToClose.IsDirty)
          {
            var res = MessageBox.Show(string.Format("Save changes for file '{0}'?", fileToClose.FileName), "AvalonDock Test App", MessageBoxButton.YesNoCancel);
            if (res == MessageBoxResult.Cancel)
              return;
            if (res == MessageBoxResult.Yes)
            {
              Save(fileToClose);
            }
          }

          _files.Remove(fileToClose);
        }

        internal void Save(FileViewModel fileToSave, bool saveAsFlag = false)
        {
          if (fileToSave.FilePath == null || saveAsFlag)
          {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog().GetValueOrDefault())
              fileToSave.FilePath = dlg.FileName;
          }

          File.WriteAllText(fileToSave.FilePath, fileToSave.Document.Text);
          ActiveDocument.IsDirty = false;
        }

    }
}
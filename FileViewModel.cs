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
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Utils;
using ICSharpCode.AvalonEdit.Highlighting;

namespace glimmer
{
    class FileViewModel : PaneViewModel
    {
        static int fileCount = 0;

        #region Constructors
        public FileViewModel(string filePath)
        {
          FilePath = filePath;
          Load(filePath);
          base.Title = Title;
        }

        public FileViewModel()
        {
            fileCount = fileCount + 1;
            base.Title = "New File " + fileCount;

            IsDirty = true;
        }

        private void Load(string path)
        {
          if (File.Exists(path))
          {
            this._document = new TextDocument();
            //this.HighlightDef = HighlightingManager.Instance.GetDefinition("XML");
            this._isDirty = false;
            //this.IsReadOnly = false;
            //this.ShowLineNumbers = false;
            //this.WordWrap = false;

            // Check file attributes and set to read-only if file attributes indicate that
            if ((System.IO.File.GetAttributes(path) & FileAttributes.ReadOnly) != 0)
            {
              this.IsReadOnly = true;
              this.IsReadOnlyReason = "This file cannot be edit because another process is currently writting to it.\n" +
                                      "Change the file access permissions or save the file in a different location if you want to edit it.";
            }

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
              using (StreamReader reader = FileReader.OpenStream(fs, Encoding.UTF8))
              {
                this._document = new TextDocument(reader.ReadToEnd());
              }
            }
          }

        }
        #endregion

        #region FilePath
        private string _filePath = null;
        public string FilePath
        {
          get { return _filePath; }
          set
          {
            if (_filePath != value)
            {
              _filePath = value;
              ContentId = _filePath;

              RaisePropertyChanged("FilePath");
              RaisePropertyChanged("FileName");
              RaisePropertyChanged("Title");
            }
          }
        }
        #endregion

        #region FileName
        public string FileName
        {
          get
          {
            if (FilePath == null)
              return base.Title;

            return System.IO.Path.GetFileName(FilePath);
          }
        }
        #endregion FileName

        #region Title
        new public string Title
        {
          get
          {
            return FileName + (this.IsDirty == true ? "*" : string.Empty);
          }

          set
          {
            base.Title = value;
          }
        }
        #endregion

        #region Document
        private TextDocument _document = null;
        public TextDocument Document
        {
            get { return this._document; }
            set
            {
                if (this._document != value)
                {
                    this._document = value;
                    RaisePropertyChanged("Document");
                    IsDirty = true;
                }
            }
        }
        #endregion

        #region IsDirty
        private bool _isDirty = false;
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    RaisePropertyChanged("IsDirty");
                    RaisePropertyChanged("Title");
                }
            }
        }
        #endregion

        #region IsReadOnly
        private bool mIsReadOnly = false;
        public bool IsReadOnly
        {
          get
          {
            return this.mIsReadOnly;
          }

          protected set
          {
            if (this.mIsReadOnly != value)
            {
              this.mIsReadOnly = value;
              this.RaisePropertyChanged("IsReadOnly");
            }
          }
        }

        private string mIsReadOnlyReason = string.Empty;
        public string IsReadOnlyReason
        {
          get
          {
            return this.mIsReadOnlyReason;
          }

          protected set
          {
            if (this.mIsReadOnlyReason != value)
            {
              this.mIsReadOnlyReason = value;
              this.RaisePropertyChanged("IsReadOnlyReason");
            }
          }
        }
        #endregion IsReadOnly

        #region SaveCommand
        RelayCommand _saveCommand = null;
        public ICommand SaveCommand
        {
          get
          {
            if (_saveCommand == null)
            {
              _saveCommand = new RelayCommand((p) => OnSave(p), (p) => CanSave(p));
            }

            return _saveCommand;
          }
        }

        private bool CanSave(object parameter)
        {
          return IsDirty;
        }

        private void OnSave(object parameter)
        {
          Workspace.This.Save(this, false);
        }

        #endregion

        #region SaveAsCommand
        RelayCommand _saveAsCommand = null;
        public ICommand SaveAsCommand
        {
          get
          {
            if (_saveAsCommand == null)
            {
              _saveAsCommand = new RelayCommand((p) => OnSaveAs(p), (p) => CanSaveAs(p));
            }

            return _saveAsCommand;
          }
        }

        private bool CanSaveAs(object parameter)
        {
          return true;
        }

        private void OnSaveAs(object parameter)
        {
          Workspace.This.Save(this, true);
        }
        #endregion

        #region CloseCommand
        RelayCommand _closeCommand = null;
        public ICommand CloseCommand
        {
          get
          {
            if (_closeCommand == null)
            {
              _closeCommand = new RelayCommand((p) => OnClose(), (p) => CanClose());
            }

            return _closeCommand;
          }
        }

        private bool CanClose()
        {
          return true;
        }

        private void OnClose()
        {
          Workspace.This.Close(this);
        }
        #endregion
    }
}
using System;
using System.ComponentModel;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using ICSharpCode.AvalonEdit.Editing;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Drawing;
using System.Windows.Media;

namespace glimmer
{
    public class ExtendedTextEditor : TextEditor, INotifyPropertyChanged
    {
        public ExtendedTextEditor()
        {
            this.Background = new SolidColorBrush(Color.FromArgb(255, 0, 43, 54)); // base 03
            this.Foreground = new SolidColorBrush(Color.FromArgb(255, 131, 148, 150)); // base 0

            this.ShowLineNumbers = true;
            this.Padding = new System.Windows.Thickness(5.0);
            this.LineNumbersForeground = new SolidColorBrush(Color.FromArgb(255, 101, 123, 131)); // base 0
        }

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public event PropertyChangedEventHandler PropertyChanged;

		#region enum Mode
		enum Mode
		{
            None, // No drag
            Drag, // Dragging number
		}

        Mode mode = Mode.None;
		#endregion

        string matchedNumStr = "";
        int matchedNumOffset = -1;
        int matchedNumLength = 0;
        bool matchedNumSign = false;
        bool matchedNumDot = false;
        int matchedNumFracDigits = 0;

        int curLength;

        bool hasMatch = false;

        Point startDragPos;
        double startDragValue;

		object undoGroupDescriptor;

        override protected void OnPreviewMouseMove(MouseEventArgs e)
        {
			if (e.Handled)
				return;

            if (mode == Mode.None)
            {
                TextViewPosition? position = this.TextArea.TextView.GetPositionFloor(e.GetPosition(this.TextArea.TextView) + this.TextArea.TextView.ScrollOffset);

                if (position != null)
                {
                    DocumentLine line = this.TextArea.Document.GetLineByNumber(position.Value.Line); ;

                    int begin = line.Offset;
                    int end = line.EndOffset;
                    int offset = this.TextArea.Document.GetOffset(position.Value.Location);
                    int lineOffset = offset - begin;

                    hasMatch = false;

                    //if (Char.IsDigit(this.TextArea.Document.GetCharAt(offset)))
                    {
                        string l = this.TextArea.Document.GetText(line.Offset, line.Length);
                        //string pat = @"([-+]?)\d+(\.?)(d*)";
                        string pat = @"([-+]?)\d*(?:(\.)(\d*))?";
                        Regex r = new Regex(pat);

                        Match m = r.Match(l);

                        while (m.Success && hasMatch == false)
                        {
                            int start = m.Index;
                            int length = m.Length;

                            if (start <= lineOffset && lineOffset < start + length)
                            {
                                matchedNumStr = m.Value;
                                matchedNumOffset = start + begin;
                                matchedNumLength = length;
                                matchedNumSign = m.Groups[1].Value.Length > 0;
                                matchedNumDot = m.Groups[2].Value.Length > 0;
                                matchedNumFracDigits = m.Groups[3].Value.Length;
                                curLength = matchedNumLength;

                                hasMatch = true;

                                undoGroupDescriptor = new object();
                                this.Document.UndoStack.StartUndoGroup(undoGroupDescriptor);
                                this.Document.UndoStack.EndUndoGroup();
                            }

                            m = m.NextMatch();
                        }
                    }
                }
            }
            else if (mode == Mode.Drag)
            {
                Vector delta = e.GetPosition(this.TextArea) - startDragPos;
                double dist = delta.X - delta.Y;

                double newValue = startDragValue * Math.Exp(dist * 0.001);

                string formatStr = "F" + matchedNumFracDigits.ToString();
                string signStr = (matchedNumSign && newValue >= 0) ? "+" : "";
                string numStr = signStr + newValue.ToString(formatStr, CultureInfo.InvariantCulture);

                this.Document.UndoStack.StartContinuedUndoGroup(undoGroupDescriptor);
                this.Document.Replace(matchedNumOffset, curLength, numStr);
                this.Document.UndoStack.EndUndoGroup();
                curLength = numStr.Length;

                ICommand curSaveCommand = ((FileViewModel)this.TextArea.DataContext).SaveCommand;

                // disabled because IsDirty flag is only set first change
                // this is possible due to the continuing Undo group
                //if (curSaveCommand.CanExecute(null))
                    curSaveCommand.Execute(null);

                e.Handled = true;
            }
        }

		#region MouseLeftButtonDown
		override protected void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
            if (e.Handled==true)
                return;

            FileViewModel curFile = ((FileViewModel)this.TextArea.DataContext);

            if (hasMatch && curFile.IsDirty==false)
            {
                if (this.TextArea.CaptureMouse())
                {
                    mode = Mode.Drag;

                    startDragPos = e.GetPosition(this.TextArea.TextView);
                    startDragValue = Double.Parse(matchedNumStr, CultureInfo.InvariantCulture);
                }

                e.Handled = true;
                return;
            }
		}
		#endregion

		#region MouseLeftButtonUp
		override protected void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
		{
            if (mode == Mode.None || e.Handled == true)
                return;

            if (mode == Mode.Drag)
            {
                this.TextArea.ReleaseMouseCapture();

                mode = Mode.None;

                e.Handled = true;
                return;
            }
		}
		#endregion

    }
}
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

namespace glimmer
{
    sealed class TweakNumberMouseHandler : ITextAreaInputHandler
    {
		#region enum Mode
		enum Mode
		{
            None, // No drag
            Drag, // Dragging number
		}
		#endregion

        readonly TextArea textArea;

        #region Constructor + Attach + Detach
        public TweakNumberMouseHandler(TextArea textArea)
        {
            if (textArea == null)
                throw new ArgumentNullException("textArea");
            this.textArea = textArea;
        }

        public TextArea TextArea
        {
            get { return textArea; }
        }

        public void Attach()
        {
			textArea.MouseLeftButtonDown += textArea_MouseLeftButtonDown;
			textArea.MouseMove += textArea_MouseMove;
			textArea.MouseLeftButtonUp += textArea_MouseLeftButtonUp;
        }

        public void Detach()
        {
			mode = Mode.None;
			textArea.MouseLeftButtonDown -= textArea_MouseLeftButtonDown;
			textArea.MouseMove -= textArea_MouseMove;
			textArea.MouseLeftButtonUp -= textArea_MouseLeftButtonUp;
        }
        #endregion

		/*int GetOffsetFromMousePosition(MouseEventArgs e, out int visualColumn, out bool isAtEndOfLine)
		{
			return GetOffsetFromMousePosition(e.GetPosition(textArea.TextView), out visualColumn, out isAtEndOfLine);
		}
		
		int GetOffsetFromMousePosition(Point positionRelativeToTextView, out int visualColumn, out bool isAtEndOfLine)
		{
			visualColumn = 0;
			TextView textView = textArea.TextView;
			Point pos = positionRelativeToTextView;
			if (pos.Y < 0)
				pos.Y = 0;
			if (pos.Y > textView.ActualHeight)
				pos.Y = textView.ActualHeight;
			pos += textView.ScrollOffset;
			if (pos.Y > textView.DocumentHeight)
				pos.Y = textView.DocumentHeight - ExtensionMethods.Epsilon;
			VisualLine line = textView.GetVisualLineFromVisualTop(pos.Y);
			if (line != null) {
				visualColumn = line.GetVisualColumn(pos, textArea.Selection.EnableVirtualSpace, out isAtEndOfLine);
				return line.GetRelativeOffset(visualColumn) + line.FirstDocumentLine.Offset;
			}
			isAtEndOfLine = false;
			return -1;
		}*/

		#region MouseMove
        string matchedNumStr = "";
        int matchedNumOffset = -1;
        int matchedNumLength = 0;
        bool matchedNumSign = false;
        bool matchedNumDot = false;
        int matchedNumFracDigits = 0;

        bool hasMatch = false;

        Point startDragPos;
        double startDragValue;

        Mode mode = Mode.None;

		void textArea_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Handled)
				return;

            TextViewPosition? position = textArea.TextView.GetPositionFloor(e.GetPosition(textArea.TextView));

            if (position != null)
            {
                //Debug.WriteLine(position);
                DocumentLine line = textArea.Document.GetLineByNumber(position.Value.Line); ;

                int begin = line.Offset;
                int end = line.EndOffset;
                int offset = textArea.Document.GetOffset(position.Value.Location);
                int lineOffset = offset - begin;

                hasMatch = false;

                //if (Char.IsDigit(textArea.Document.GetCharAt(offset)))
                {
                    string l = textArea.Document.GetText(line.Offset, line.Length);
                    //string pat = @"([-+]?)\d+(\.?)(d*)";
                    string pat = @"([-+]?)\d*(?:(\.)(\d*))?";
                    Regex r = new Regex(pat);

                    Match m = r.Match(l);

                    while (m.Success && hasMatch==false)
                    {
                        int start = m.Index;
                        int length = m.Length;

                        if (start <= lineOffset && lineOffset < start + length)
                        {
                            matchedNumStr = m.Value;
                            matchedNumOffset = start;
                            matchedNumLength = length;
                            matchedNumSign = m.Groups[1].Value.Length > 0;
                            matchedNumDot = m.Groups[2].Value.Length > 0;
                            matchedNumFracDigits = m.Groups[3].Value.Length;

                            hasMatch = true;
                        }

                        m = m.NextMatch();
                    }
                }
                // Use Document.GetCharAt

                //textArea.Document.GetLineByNumber(offset.)
            }

            //int offset = this.GetPositionFromPoint();
			/*if (mode == SelectionMode.Normal || mode == SelectionMode.WholeWord || mode == SelectionMode.WholeLine || mode == SelectionMode.Rectangular) {
				e.Handled = true;
				if (textArea.TextView.VisualLinesValid) {
					// If the visual lines are not valid, don't extend the selection.
					// Extending the selection forces a VisualLine refresh, and it is sufficient
					// to do that on MouseUp, we don't have to do it every MouseMove.
					ExtendSelectionToMouse(e);
				}
			} else if (mode == SelectionMode.PossibleDragStart) {
				e.Handled = true;
				Vector mouseMovement = e.GetPosition(textArea) - possibleDragStartMousePos;
				if (Math.Abs(mouseMovement.X) > SystemParameters.MinimumHorizontalDragDistance
				    || Math.Abs(mouseMovement.Y) > SystemParameters.MinimumVerticalDragDistance)
				{
					StartDrag();
				}
			}
            */
		}
		#endregion

		#region MouseLeftButtonDown
		void textArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
            if (e.Handled==true)
                return;

            if (hasMatch)
            {
                if (textArea.CaptureMouse())
                {
                    mode = Mode.Drag;

                    startDragPos = e.GetPosition(textArea.TextView);
                    startDragValue = Double.Parse(matchedNumStr);
                }

                e.Handled = true;
                return;
            }
			/*if (mode == SelectionMode.None || e.Handled)
				return;
			e.Handled = true;
			if (mode == SelectionMode.PossibleDragStart) {
				// -> this was not a drag start (mouse didn't move after mousedown)
				SetCaretOffsetToMousePosition(e);
				textArea.ClearSelection();
			} else if (mode == SelectionMode.Normal || mode == SelectionMode.WholeWord || mode == SelectionMode.WholeLine || mode == SelectionMode.Rectangular) {
				ExtendSelectionToMouse(e);
			}
			mode = SelectionMode.None;
			textArea.ReleaseMouseCapture();
            */
		}
		#endregion

		#region MouseLeftButtonUp
		void textArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
            if (mode == Mode.None || e.Handled == true)
                return;

            if (mode == Mode.Drag)
            {
                textArea.ReleaseMouseCapture();

                e.Handled = true;
                return;
            }
			/*if (mode == SelectionMode.None || e.Handled)
				return;
			e.Handled = true;
			if (mode == SelectionMode.PossibleDragStart) {
				// -> this was not a drag start (mouse didn't move after mousedown)
				SetCaretOffsetToMousePosition(e);
				textArea.ClearSelection();
			} else if (mode == SelectionMode.Normal || mode == SelectionMode.WholeWord || mode == SelectionMode.WholeLine || mode == SelectionMode.Rectangular) {
				ExtendSelectionToMouse(e);
			}
			mode = SelectionMode.None;
			textArea.ReleaseMouseCapture();
            */
		}
		#endregion
    }

    public class ExtendedTextEditor : TextEditor, INotifyPropertyChanged
    {
        public ExtendedTextEditor()
        {
            Debug.WriteLine("cons");
            //this.TextArea.DataContextChanged += TextArea_DataContextChanged;
        }

        //FileViewModel curFile = null;
        //ICommand curSaveCommand = null;

        /*void TextArea_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            curFile = (FileViewModel)this.TextArea.DataContext;
            curSaveCommand = curFile.SaveCommand;
        }*/

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

        Mode mode = Mode.None;

		object undoGroupDescriptor;


        override protected void OnPreviewMouseMove(MouseEventArgs e)
        {
			if (e.Handled)
				return;

            if (mode == Mode.None)
            {
                TextViewPosition? position = this.TextArea.TextView.GetPositionFloor(e.GetPosition(this.TextArea.TextView));

                if (position != null)
                {
                    //Debug.WriteLine(position);
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
                                matchedNumOffset = start;
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

                double newValue = startDragValue + Math.Exp(dist * 0.01);

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
			/*if (mode == SelectionMode.None || e.Handled)
				return;
			e.Handled = true;
			if (mode == SelectionMode.PossibleDragStart) {
				// -> this was not a drag start (mouse didn't move after mousedown)
				SetCaretOffsetToMousePosition(e);
				this.TextArea.ClearSelection();
			} else if (mode == SelectionMode.Normal || mode == SelectionMode.WholeWord || mode == SelectionMode.WholeLine || mode == SelectionMode.Rectangular) {
				ExtendSelectionToMouse(e);
			}
			mode = SelectionMode.None;
			this.TextArea.ReleaseMouseCapture();
            */
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
			/*if (mode == SelectionMode.None || e.Handled)
				return;
			e.Handled = true;
			if (mode == SelectionMode.PossibleDragStart) {
				// -> this was not a drag start (mouse didn't move after mousedown)
				SetCaretOffsetToMousePosition(e);
				textArea.ClearSelection();
			} else if (mode == SelectionMode.Normal || mode == SelectionMode.WholeWord || mode == SelectionMode.WholeLine || mode == SelectionMode.Rectangular) {
				ExtendSelectionToMouse(e);
			}
			mode = SelectionMode.None;
			textArea.ReleaseMouseCapture();
            */
		}
		#endregion

    }
}
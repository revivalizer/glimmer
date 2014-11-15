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


namespace glimmer
{
    sealed class TweakNumberMouseHandler : ITextAreaInputHandler
    {
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
            textArea.MouseMove += textArea_MouseMove;
        }

        public void Detach()
        {
            textArea.MouseMove -= textArea_MouseMove;
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
		void textArea_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Handled)
				return;

            TextViewPosition? position = textArea.TextView.GetPositionFloor(e.GetPosition(textArea.TextView));

            if (position != null)
            {
                Debug.WriteLine(position);
                DocumentLine line = textArea.Document.GetLineByNumber(position.Value.Line); ;

                int begin = line.Offset;
                int end = line.EndOffset;
                int offset = textArea.Document.GetOffset(position.Value.Location);
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
    }

    public class ExtendedTextEditor : TextEditor, INotifyPropertyChanged
    {
        TweakNumberMouseHandler handler = null;

        public ExtendedTextEditor()
        {
            Debug.WriteLine("cons");

            this.TextArea.DefaultInputHandler.NestedInputHandlers.Add(handler = new TweakNumberMouseHandler(this.TextArea));
        }

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
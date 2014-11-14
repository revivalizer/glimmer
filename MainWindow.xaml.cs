using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace glimmer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Data context is singleton workspace class
            this.DataContext = Workspace.This;

            // Load GLSL highlighter
            IHighlightingDefinition glslDef = null;

            using (
                Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("glimmer.glsl.xshd")
            )
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    glslDef = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }

            }

            HighlightingManager.Instance.RegisterHighlighting("GLSL", new[] { "glsl", "frag", "vert", "vsh", "fsh", "gsh" }, glslDef);
        }
    }
}

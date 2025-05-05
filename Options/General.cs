using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PyDjinni
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>
    {
        [Category("General")]
        [DisplayName("Language Server Executable")]
        [Description("Absolute or relative path to the PyDjinni language server executable file.")]
        [Editor(typeof(ExecutablePathEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue("pydjinni-language-server.exe")]
        public string ServerExecutable { get; set; } = "pydjinni-language-server.exe";

        [Category("Debugging")]
        [DisplayName("Enable language server logs")]
        [Description("If enabled, the language server logs will be written to 'pydjinni-language-server.log'")]
        [DefaultValue(false)]
        public bool DebugLogsEnabled { get; set; } = false;
    }

    public class ExecutablePathEditor : System.Drawing.Design.UITypeEditor
    {
        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return System.Drawing.Design.UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }
            }
            return value;
        }
    }
}

using System;
using System.Windows.Forms;
using UuidByString;

namespace StringToUuidGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void CopyUuid() => Clipboard.SetText(tbUuid.Text);

        private void SelectInput()
        {
            tbInput.SelectAll();
            tbInput.Focus();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            string input = tbInput.Text;

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            tbUuid.Text = null;

            var uuid = UuidGenerator.GenerateUuid(input);

            tbUuid.Text = uuid;

            CopyUuid();

            SelectInput();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            CopyUuid();
        }
    }
}

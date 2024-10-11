using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS_ELOAD_Serial
{
    public partial class ProgramForm : Form
    {
        public string ProgramName { get; private set; }

        public ProgramForm()
        {
            InitializeComponent();
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            ProgramName = textBoxProgramName.Text;  // 사용자가 입력한 프로그램 이름을 저장
            DialogResult = DialogResult.OK;  // 다이얼로그 결과를 OK로 설정하여 폼을 닫음
            this.Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;  // 다이얼로그 결과를 취소로 설정하여 폼을 닫음
            this.Close();
        }
    }
}

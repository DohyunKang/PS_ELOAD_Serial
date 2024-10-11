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
    public partial class CCMode : Form
    {
        // 속성 정의
        public string CurrentValue { get; private set; }        // 전류 값
        public string OPPLValue { get; private set; }           // OPPL 값
        public string UVPValue { get; private set; }            // UVP 값
        public string InductanceValue { get; private set; }      // 유도성 값 (Inductance)

        public CCMode()
        {
            InitializeComponent();

            // OK 버튼과 Cancel 버튼의 이벤트 핸들러 설정
            this.buttonOk.Click += new EventHandler(ButtonOk_Click);
            this.buttonCancel.Click += new EventHandler(ButtonCancel_Click);
        }

        // OK 버튼 클릭 시 호출되는 메서드
        private void ButtonOk_Click(object sender, EventArgs e)
        {
            // 각 텍스트박스의 값을 속성에 저장
            CurrentValue = textBoxCurrent.Text;
            OPPLValue = textBoxOPPL.Text;
            UVPValue = textBoxUVP.Text;
            InductanceValue = textBoxCurrent_A_us.Text;

            // 다이얼로그 결과를 OK로 설정하고 창 닫기
            DialogResult = DialogResult.OK;
            this.Close();
        }

        // Cancel 버튼 클릭 시 호출되는 메서드
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            // 다이얼로그 결과를 Cancel로 설정하고 창 닫기
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

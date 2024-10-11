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
    public partial class CRMode : Form
    {
        public string ImpedanceValue { get; private set; }
        public string UVPValue { get; private set; }
        public string OCPLValue { get; private set; }
        public string OPPLValue { get; private set; }

        public CRMode()
        {
            InitializeComponent();

            this.buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);

            // textBoxImpedence의 TextChanged 이벤트 핸들러 추가
            textBoxImpedence.TextChanged += TextBoxImpedence_TextChanged;
        }

        // textBoxImpedence 값 변경 시 호출되는 이벤트 핸들러
        private void TextBoxImpedence_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // textBoxImpedence에 입력된 값을 double로 변환
                double impedanceValue;
                if (double.TryParse(textBoxImpedence.Text, out impedanceValue) && impedanceValue != 0)
                {
                    // 임피던스의 역수를 계산하여 labelResistance에 표시
                    double resistanceValue = 1 / impedanceValue;
                    labelResistance.Text = resistanceValue.ToString("F4"); // 소수점 4자리까지 표시
                }
                else
                {
                    // 입력값이 유효하지 않거나 0일 때는 초기값으로 설정
                    labelResistance.Text = "Invalid";
                }
            }
            catch
            {
                labelResistance.Text = "Error";
            }
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            // 입력값을 변수에 저장
            ImpedanceValue = textBoxImpedence.Text;
            UVPValue = textBoxUVP.Text;
            OCPLValue = textBoxOCPL.Text;
            OPPLValue = textBoxOPPL.Text;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

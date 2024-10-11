using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlServerCe;

namespace PS_ELOAD_Serial
{
    public partial class SettingsForm : Form
    {
        // 데이터베이스 파일 경로
        private string _dbFilePath = @"C:\Users\kangdohyun\Desktop\세미나\4주차\PS_ELOAD_Serial\MyDatabase#1.sdf; Password = a1234!;";

        // 프로그램 ID를 참조하는 변수
        public int ProgramID { get; set; } // 외부에서 프로그램 ID를 설정할 수 있도록 public으로 설정

        public SettingsForm(int programID)
        {
            InitializeComponent();
            ProgramID = programID; // 생성자에서 ProgramID를 설정

            this.buttonOk.Click += new EventHandler(this.ButtonOk_Click);
            this.buttonCancel.Click += new EventHandler(this.ButtonCancel_Click);
        }

        // OK 버튼 클릭 이벤트 핸들러
        private void ButtonOk_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlCeConnection connection = new SqlCeConnection("Data Source=" + _dbFilePath))
                {
                    connection.Open();

                    // 입력한 설정 값을 가져와서 변수에 저장
                    int level = int.Parse(textBoxLevel.Text);
                    float sr = float.Parse(textBoxSR.Text);
                    float dwell = float.Parse(textBoxDwell.Text);
                    string load = textBoxLoad.Text; // 문자열로 Load 값을 저장 (예: "immediate" 또는 "ramp")
                    int walt = int.Parse(textBoxWalt.Text);
                    int post = int.Parse(textBoxPost.Text);
                    string generate = textBoxGenerate.Text; // generate도 문자열로 처리

                    // ProgramSettings 테이블에 데이터 추가
                    string insertQuery = "INSERT INTO ProgramSettings (ProgramID, Level, SR_A_us, Dwell_s, Load_immediate_ramp, Walt_pre, _post_, Generate) " +
                                         "VALUES (@ProgramID, @Level, @SR, @Dwell, @Load, @Walt, @Post, @Generate)";

                    using (SqlCeCommand command = new SqlCeCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ProgramID", ProgramID);
                        command.Parameters.AddWithValue("@Level", level);
                        command.Parameters.AddWithValue("@SR", sr);
                        command.Parameters.AddWithValue("@Dwell", dwell);
                        command.Parameters.AddWithValue("@Load", load); // nvarchar로 load 값을 저장
                        command.Parameters.AddWithValue("@Walt", walt);
                        command.Parameters.AddWithValue("@Post", post);
                        command.Parameters.AddWithValue("@Generate", generate);

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Program settings saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving program settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Close(); // 창 닫기
            }
        }

        // Cancel 버튼 클릭 이벤트 핸들러
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close(); // 창을 닫음
        }
    }
}

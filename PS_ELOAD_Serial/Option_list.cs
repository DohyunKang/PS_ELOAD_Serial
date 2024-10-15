using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlServerCe; // SQL Server Compact Edition 사용

namespace PS_ELOAD_Serial
{
    public partial class Option_list : Form
    {
        private int _programID; // 프로그램 ID를 저장할 필드
        private string _dbFilePath = @"C:\Users\kangdohyun\Desktop\세미나\4주차\PS_ELOAD_Serial\MyDatabase#1.sdf; Password = a1234!;";

        public Option_list(int programID)
        {
            InitializeComponent();
            _programID = programID; // 프로그램 ID를 저장
        }

        private void Option_list_Load(object sender, EventArgs e)
        {
            LoadProgramSettings(); // 폼이 로드될 때 설정 데이터 로드
        }

        private void LoadProgramSettings()
        {
            try
            {
                // 데이터베이스 연결 및 ProgramID를 기준으로 설정 데이터 조회
                using (SqlCeConnection connection = new SqlCeConnection("Data Source=" + _dbFilePath))
                {
                    connection.Open();
                    string query = "SELECT * FROM ProgramSettings WHERE ProgramID = @ProgramID";
                    SqlCeDataAdapter dataAdapter = new SqlCeDataAdapter(query, connection);
                    dataAdapter.SelectCommand.Parameters.AddWithValue("@ProgramID", _programID);
                    DataTable settingsTable = new DataTable();
                    dataAdapter.Fill(settingsTable);

                    // 조회된 데이터를 DataGridView에 바인딩
                    dataGridViewOptions.DataSource = settingsTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading program settings: " + ex.Message);
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {

        }
    }
}

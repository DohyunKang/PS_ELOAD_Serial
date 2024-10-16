using System;
using System.Windows.Forms;
using NationalInstruments.DAQmx;  // DAQmx API 사용을 위한 참조

namespace PS_ELOAD_Serial
{
    public partial class Ps_Eload_Current : Form
    {
        private NationalInstruments.DAQmx.Task voltageTask;  // DAQmx Task 객체
        private AnalogSingleChannelReader reader;  // DAQ에서 데이터를 읽어오기 위한 Reader 객체
        private Timer updateTimer;  // 데이터를 읽어오는 타이머

        private const double supplyVoltage = 5.0; // 공급 전압 (U_c)
        private const double offsetVoltage = 2.5; // 오프셋 전압 (V_0) - 센서의 기본값
        private const double sensitivity = 0.05; // DHAB S/113 채널 1의 감도 (50 mV/A = 0.05 V/A)

        public Ps_Eload_Current()
        {
            InitializeComponent();

            // DAQ로부터 전압을 읽는 Task 설정
            InitializeDAQ();

            // 타이머 초기화
            updateTimer = new Timer();
            updateTimer.Interval = 100; // 0.1초마다 데이터 업데이트
            updateTimer.Tick += (s, ev) => UpdateDAQData();

            // 버튼 클릭 이벤트 핸들러 연결
            this.ReadButton.Click += new System.EventHandler(this.ReadButton_Click);
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
        }

        private void InitializeDAQ()
        {
            try
            {
                // DAQ Task 생성 (ai0 포트에서 전압을 측정)
                voltageTask = new NationalInstruments.DAQmx.Task();
                voltageTask.AIChannels.CreateVoltageChannel("Dev2/ai0", "VoltageChannel", AITerminalConfiguration.Rse, 0.0, 10.0, AIVoltageUnits.Volts);

                // 데이터를 읽기 위한 AnalogSingleChannelReader 초기화
                reader = new AnalogSingleChannelReader(voltageTask.Stream);
            }
            catch (DaqException ex)
            {
                MessageBox.Show("DAQ 초기화 중 오류 발생: " + ex.Message);
            }
        }

        private void UpdateDAQData()
        {
            try
            {
                // 전압값을 DAQ로부터 읽음
                double outputVoltage = reader.ReadSingleSample();

                // lblVoltage_DAQ에 전압값 표시
                lblVoltage_DAQ.Text = outputVoltage.ToString("F2") + " V";

                // 전류값 계산 (공식에 따라 전류 계산)
                double current = ((5 / supplyVoltage) * outputVoltage - offsetVoltage) * (-1 / sensitivity); 
                // -1 / sensitivity에서 1이 아니라 -1한 이유는 측정기를 전선에 반대방향으로 통과시켰기 때문. 

                // lblCurrent_DAQ에 전류값 표시
                lblCurrent_DAQ.Text = current.ToString("F2") + " A";
            }
            catch (DaqException ex)
            {
                MessageBox.Show("DAQ 데이터 업데이트 중 오류 발생: " + ex.Message);
            }
        }

        // ReadButton 클릭 시 타이머 시작
        private void ReadButton_Click(object sender, EventArgs e)
        {
            if (!updateTimer.Enabled)  // 타이머가 이미 동작 중인지 확인
            {
                updateTimer.Start();
                MessageBox.Show("데이터 수집 시작");
            }
            else
            {
                MessageBox.Show("이미 데이터 수집이 진행 중입니다.");
            }
        }

        // StopButton 클릭 시 타이머 중지
        private void StopButton_Click(object sender, EventArgs e)
        {
            if (updateTimer.Enabled)  // 타이머가 동작 중인지 확인
            {
                updateTimer.Stop();
                MessageBox.Show("데이터 수집 중지");
            }
            else
            {
                MessageBox.Show("현재 데이터 수집이 중지된 상태입니다.");
            }
        }
    }
}

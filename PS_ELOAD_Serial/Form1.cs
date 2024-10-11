using System;
using System.IO.Ports;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using Action = System.Action; // System.Action으로 명시

namespace PS_ELOAD_Serial
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort; // ELoad 시리얼 포트 객체
        private TcpClient psClient; // PowerSupply TCP 클라이언트 객체
        private NetworkStream psStream; // PowerSupply 네트워크 스트림 객체
        private StreamReader reader;
        private StreamWriter writer;
        private bool isConnected = false; // ELoad 연결 상태를 확인하기 위한 변수
        private bool psConnected = false; // PowerSupply 연결 상태를 확인하기 위한 변수
        private bool isGraphUpdating = false; // 그래프 업데이트 여부 확인 변수 (추가)
        private readonly object _commandLock = new object(); // 명령어 전송을 위한 Lock 객체

        private System.Windows.Forms.Timer psDataTimer; // Windows Forms Timer
        private double elapsedTime; // 그래프의 시간 흐름을 나타내는 변수
        private WaveformPlot voltagePlot; // 전압 값 플롯
        private WaveformPlot currentPlot; // 전류 값 플롯

        // Sequence 창을 열기 위한 Delegate 정의
        public Action OpenSequenceDelegate;

        public Form1()
        {
            InitializeComponent();

            GetSerialPortList(); // 시작 시 COM 포트 목록을 가져오기 위한 메서드 호출
            switch1.StateChanged += Switch1_StateChanged; // ELoad 스위치 이벤트 핸들러 추가
            switch2.StateChanged += Switch2_StateChanged; // PowerSupply 스위치 이벤트 핸들러 추가

            this.ApplyButton.Click += new System.EventHandler(this.ApplyButton_Click);
            this.ApplyButton2.Click += new System.EventHandler(this.ApplyButton2_Click);
            this.OutPutButton.Click += new System.EventHandler(this.OutPutButton_Click);

            // ELoad 모드 전환 이벤트 핸들러 추가
            CCButton.CheckedChanged += ELoadRadioButton_CheckedChanged;
            CVButton.CheckedChanged += ELoadRadioButton_CheckedChanged;
            CRButton.CheckedChanged += ELoadRadioButton_CheckedChanged;

            psDataTimer = new System.Windows.Forms.Timer();
            psDataTimer.Interval = 100; // 100ms 간격으로 타이머 이벤트 발생

            psDataTimer.Tick += new EventHandler(PsDataTimer_Tick); // 타이머 이벤트 핸들러 등록

            // 그래프 초기화 설정
            InitializeGraph();

            // Delegate를 해당 메서드에 연결
            OpenSequenceDelegate = OpenSequenceWindow;

            ModeButton.Click += ModeButton_Click; // ModeButton의 Click 이벤트 핸들러 설정
        }


        private void InitializeReaderWriter()
        {
            if (psStream != null && psStream.CanWrite && psStream.CanRead)
            {
                reader = new StreamReader(psStream, Encoding.ASCII);
                writer = new StreamWriter(psStream, Encoding.ASCII) { AutoFlush = true };
            }
        }

        // COM 포트 목록을 가져와 comboBox1에 나열하는 메서드
        private void GetSerialPortList()
        {
            comboBox1.Items.Clear(); // 기존 아이템 초기화
            string[] ports = SerialPort.GetPortNames(); // 사용 가능한 모든 COM 포트 가져오기
            comboBox1.Items.AddRange(ports); // COM 포트 목록 추가
        }

        /*private void ModeButton_Click(object sender, EventArgs e)
        {
            try
            {
                Sequence sequenceWindow = new Sequence();
                Console.WriteLine("Sequence 창을 생성했습니다.");
                sequenceWindow.Show(); // Show()를 사용하여 Sequence 창 열기
                Console.WriteLine("Sequence 창을 열었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sequence 창을 열 때 오류가 발생했습니다: " + ex.Message);
            }
        }*/

        // ModeButton 클릭 시 실행되는 이벤트 핸들러
        private void ModeButton_Click(object sender, EventArgs e)
        {
            // Delegate가 null이 아니면 호출하여 창을 엶
            OpenSequenceDelegate.Invoke();
        }

        // Sequence 창을 여는 메서드 (Delegate에 연결)
        private void OpenSequenceWindow()
        {
            Sequence sequenceWindow = new Sequence();
            sequenceWindow.Show(); // 모달리스 창 열기
            MessageBox.Show("Sequence 창이 열렸습니다.");
        }

        // ELoad 스위치 상태가 변경될 때의 이벤트 처리 메서드
        private void Switch1_StateChanged(object sender, NationalInstruments.UI.ActionEventArgs e)
        {
            if (switch1.Value)
            {
                // 스위치가 켜질 때 연결 시도
                ConnectToSelectedPort();
                GetAndShowMeasurements(); // E-load 및 AI 값 측정 및 표시
            }
            else
            {
                // 스위치가 꺼질 때 연결 해제
                DisconnectPort();
            }
        }

        private void Switch2_StateChanged(object sender, NationalInstruments.UI.ActionEventArgs e)
        {
            if (switch2.Value)
            {
                ConnectToPowerSupply();  // PowerSupply 연결 시도
                GetAndShowPSMeasurements();  // PowerSupply 측정 값 가져오기
            }
            else
            {
                DisconnectPowerSupply();  // 연결 해제
            }
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            lock (_commandLock) // 명령어 전송 시 동기화
            {
                if (psConnected && switch2.Value)
                {
                    double voltage = (double)PSVoltage.Value;
                    double current = (double)PSCurrent.Value;

                    // 전압 및 전류 설정 명령어 동기 전송
                    SendCommandToPS("VOLT " + voltage.ToString("F2"));
                    SendCommandToPS("CURR " + current.ToString("F2"));

                    // 하드웨어에서 설정 값 확인 후 업데이트
                    UpdatePSStatus();
                }
                else
                {
                    MessageBox.Show("Power Supply가 연결되지 않았거나 스위치가 꺼져 있습니다.", "설정 오류");
                }
            }
        }

        // ApplyButton2 클릭 이벤트 핸들러 (이 메서드는 Form1.cs에 추가하세요)
        private void ApplyButton2_Click(object sender, EventArgs e)
        {
            lock (_commandLock) // 명령어 전송 시 동기화
            {
                if (psConnected && switch2.Value)
                {
                    // OVP와 OCP 값을 NumericUpDown 컨트롤에서 가져오기
                    double ovpValue = (double)PSOVP.Value;
                    double ocpValue = (double)PSOCP.Value;

                    if (ovpValue >= 10.65 && ovpValue <= 33.6 && ocpValue >= 15 && ocpValue <= 168)
                    {
                        // Power Supply에 OVP와 OCP 설정 명령어 전송
                        SendCommandToPS("VOLT:PROT " + ovpValue.ToString("F2"));
                        SendCommandToPS("CURR:PROT " + ocpValue.ToString("F2"));

                        // 설정이 적용되었는지 확인하기 위해 하드웨어의 실제 값을 읽어옴
                        string currentOVPValue = SendCommandAndReadResponse("VOLT:PROT?");
                        string currentOCPValue = SendCommandAndReadResponse("CURR:PROT?");

                        // 하드웨어에서 읽어온 OVP와 OCP 값을 라벨에 표시
                        lblOVP.Invoke(new System.Action(() => lblOVP.Text = currentOVPValue + " V"));
                        lblOCP.Invoke(new System.Action(() => lblOCP.Text = currentOCPValue + " A"));

                        MessageBox.Show("OVP와 OCP가 성공적으로 설정되었습니다.", "설정 완료");
                    }
                    else
                    {
                        MessageBox.Show("OVP와 OCP 설정 범위를 넘어섰습니다.", "설정 오류");
                    }
                }
                else
                {
                    MessageBox.Show("Power Supply가 연결되지 않았거나 스위치가 꺼져 있습니다.", "설정 오류");
                }
            }
        }

        /*
        private async void ApplyButton_Click(object sender, EventArgs e)
        {
            if (psConnected && switch2.Value)
            {
                double voltage = (double)PSVoltage.Value;
                double current = (double)PSCurrent.Value;

                // SendCommandToPS 메서드를 비동기로 호출하여 UI 스레드가 블로킹되지 않도록 함
                await SendCommandToPSAsync("VOLT " + voltage.ToString("F2"));
                await SendCommandToPSAsync("CURR " + current.ToString("F2"));

                UpdatePSStatus();
            }
            else
            {
                MessageBox.Show("Power Supply가 연결되지 않았거나 스위치가 꺼져 있습니다.", "설정 오류");
            }
        }

        private async void ApplyButton2_Click(object sender, EventArgs e)
        {
            if (psConnected && switch2.Value)
            {
                double ovpValue = (double)PSOVP.Value;
                double ocpValue = (double)PSOCP.Value;

                if (ovpValue >= 10.65 && ovpValue <= 33.6 && ocpValue >= 15 && ocpValue <= 168)
                {
                    await SendCommandToPSAsync("VOLT:PROT " + ovpValue.ToString("F2"));
                    await SendCommandToPSAsync("CURR:PROT " + ocpValue.ToString("F2"));
                }
                else
                {
                    MessageBox.Show("OVP와 OCP 설정 범위를 넘어섰습니다.", "설정 오류");
                }

                string currentOVPValue = await SendCommandAndReadResponseAsync("VOLT:PROT?");
                string currentOCPValue = await SendCommandAndReadResponseAsync("CURR:PROT?");

                lblOVP.Invoke(new Action(() => lblOVP.Text = currentOVPValue + " V"));
                lblOCP.Invoke(new Action(() => lblOCP.Text = currentOCPValue + " A"));

                MessageBox.Show("OVP와 OCP가 성공적으로 설정되었습니다.", "설정 완료");
            }
            else
            {
                MessageBox.Show("Power Supply가 연결되지 않았거나 스위치가 꺼져 있습니다.", "설정 오류");
            }
        }*/



        private void InitializeGraph()
        {
            // Voltage와 Current 플롯 설정
            voltagePlot = new WaveformPlot { LineColor = System.Drawing.Color.Red };
            currentPlot = new WaveformPlot { LineColor = System.Drawing.Color.Blue };

            waveformGraph2.Plots.Add(voltagePlot);
            waveformGraph2.Plots.Add(currentPlot);

            // Y축 설정
            if (waveformGraph2.YAxes.Count < 2)
            {
                waveformGraph2.YAxes.Add(new YAxis()); // 두 번째 Y축을 추가
            }

            // 각 플롯이 사용할 Y축을 설정
            voltagePlot.YAxis = waveformGraph2.YAxes[0]; // 첫 번째 Y축은 전압
            currentPlot.YAxis = waveformGraph2.YAxes[1]; // 두 번째 Y축은 전류

            // Y축 레이블 설정
            waveformGraph2.YAxes[0].Caption = "Voltage (V)";
            waveformGraph2.YAxes[0].CaptionForeColor = voltagePlot.LineColor; // 전압 플롯 색상과 동일하게 설정
            waveformGraph2.YAxes[1].Caption = "Current (A)";
            waveformGraph2.YAxes[1].CaptionForeColor = currentPlot.LineColor; // 전류 플롯 색상과 동일하게 설정

            // X축 레이블 설정
            waveformGraph2.XAxes[0].Caption = "Time (0.1s)";
        }

        // Output 버튼 클릭 이벤트 핸들러
        private void OutPutButton_Click(object sender, EventArgs e)
        {
            lock (_commandLock)
            {
                if (psConnected && switch2.Value)
                {
                    // 현재 Power Supply의 출력 상태 확인
                    string response = SendCommandAndReadResponse("OUTP?");

                    // 출력을 켜고 끌지 결정하기 위한 플래그 변수
                    bool isOutputOn = response.Trim() == "1";

                    if (isOutputOn) // 현재 출력이 켜져 있다면
                    {
                        // 출력 끄기 명령 전송
                        SendCommandToPS("OUTP 0\r");
                        PS_LED.Value = false;
                        psDataTimer.Stop(); // 타이머 중지
                        isGraphUpdating = false;
                    }
                    else // 현재 출력이 꺼져 있다면
                    {
                        // 출력 켜기 명령 전송
                        SendCommandToPS("OUTP 1\r");
                        PS_LED.Value = true;
                        psDataTimer.Start(); // 타이머 시작
                        isGraphUpdating = true;
                    }
                }
                else
                {
                    MessageBox.Show("Power Supply가 연결되지 않았거나 스위치가 꺼져 있습니다.", "설정 오류");
                }
            }
        }

        // 타이머 이벤트 핸들러 (주기적으로 전압 및 전류 값을 그래프에 업데이트)
        private void PsDataTimer_Tick(object sender, EventArgs e)
        {
            if (isGraphUpdating)
            {
                UpdateGraphWithData(); // 그래프 데이터 업데이트 메서드 호출
            }
        }

        // 전압 및 전류 값 읽어서 그래프 업데이트
        private void UpdateGraphWithData()
        {
            // 하드웨어에서 전압 및 전류 값 읽기
            string psVoltage = ReadResponseFromPS("MEAS:VOLT?");
            string psCurrent = ReadResponseFromPS("MEAS:CURR?");

            // 문자열을 double 타입으로 변환하여 값 할당
            double voltageValue;
            double currentValue;

            // 전압 및 전류 값 변환 시도
            double.TryParse(psVoltage, out voltageValue);
            double.TryParse(psCurrent, out currentValue);

            // X축 시간 값 증가
            elapsedTime += psDataTimer.Interval / 1000.0;

            // 라벨에 값 업데이트
            lblPV.Text = String.Format("{0:F2} V", voltageValue);
            lblPC.Text = String.Format("{0:F2} A", currentValue);

            // 그래프에 값 추가
            voltagePlot.PlotYAppend(voltageValue); // 전압 값 추가
            currentPlot.PlotYAppend(currentValue); // 전류 값 추가
        }


        /*
        private async void OutPutButton_Click(object sender, EventArgs e)
        {
            if (psConnected && switch2.Value)
            {
                // PS에 출력 활성화 명령 비동기로 전송
                await SendCommandToPSAsync("OUTP 1\r");
                MessageBox.Show("OUTP 1 명령 전송 완료", "디버깅");

                // 명령 전송 후 상태를 확인하여 출력이 활성화되었는지 체크
                string response = await SendCommandAndReadResponseAsync("OUTP?");
                

                if (response.Trim() == "1")  // 출력이 정상적으로 활성화된 경우
                {
                    MessageBox.Show("Power Supply 출력이 켜졌습니다.", "출력 활성화");
                }
                else  // 출력이 활성화되지 않은 경우
                {
                    MessageBox.Show("Power Supply 출력을 켜는 데 실패했습니다. 설정을 확인하세요.", "출력 오류");
                }
            }
            else
            {
                MessageBox.Show("Power Supply가 연결되지 않았거나 스위치가 꺼져 있습니다.", "설정 오류");
            }
        }*/



        // ELoad COM 포트에 연결하는 메서드 추가
        private void ConnectToSelectedPort()
        {
            if (comboBox1.SelectedItem != null && !isConnected)
            {
                try
                {
                    // ELoad 시리얼 포트 설정
                    serialPort = new SerialPort(comboBox1.SelectedItem.ToString(), 19200, Parity.None, 8, StopBits.One);
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(ELoadDataReceivedHandler); // 데이터 수신 핸들러 연결
                    serialPort.Open();
                    isConnected = true; // 연결 상태 업데이트
                    MessageBox.Show("ELoad 연결 성공: " + comboBox1.SelectedItem.ToString(), "연결 상태");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ELoad 연결 실패: " + ex.Message, "연결 상태");
                }
            }
            else if (isConnected)
            {
                MessageBox.Show("이미 ELoad에 연결된 상태입니다.", "연결 상태");
            }
            else
            {
                MessageBox.Show("COM 포트를 선택하세요.", "연결 상태");
            }
        }

        // ELoad 시리얼 포트 연결 해제 메서드 추가
        private void DisconnectPort()
        {
            if (serialPort != null && isConnected)
            {
                try
                {
                    serialPort.Close(); // 포트 닫기
                    isConnected = false; // 연결 상태 업데이트
                    MessageBox.Show("ELoad 연결이 해제되었습니다.", "연결 상태");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ELoad 연결 해제 실패: " + ex.Message, "연결 상태");
                }
            }
            else
            {
                MessageBox.Show("ELoad가 연결되지 않았습니다.", "연결 상태");
            }
        }

        // E-load 및 AI 전압/전류 측정 및 출력 메서드 추가
        private void GetAndShowMeasurements()
        {
            if (serialPort != null && isConnected)
            {
                // 측정 값 초기화
                string eLoadVoltage = "0";
                string eLoadCurrent = "0";
                string aiCurrent = "0";

                // E-load 전압 요청 및 읽기
                try
                {
                    serialPort.WriteLine("MEAS:VOLT?");
                    eLoadVoltage = serialPort.ReadLine().Trim(); // E-load 전압 읽기
                }
                catch
                {
                    eLoadVoltage = "0"; // 읽기에 실패할 경우 0으로 설정
                }

                // E-load 전류 요청 및 읽기
                try
                {
                    serialPort.WriteLine("MEAS:CURR?");
                    eLoadCurrent = serialPort.ReadLine().Trim(); // E-load 전류 읽기
                }
                catch
                {
                    eLoadCurrent = "0"; // 읽기에 실패할 경우 0으로 설정
                }

                // 메세지 박스로 측정 값 출력
                MessageBox.Show(string.Format("E-load Voltage: {0} V\nE-load Current: {1} A", eLoadVoltage, eLoadCurrent), "Measurement Results");
                // 각 텍스트 박스에 측정 값 표시
                lblVoltage.Invoke(new System.Action(() => lblVoltage.Text = eLoadVoltage + " V"));
                lblCurrent.Invoke(new System.Action(() => lblCurrent.Text = eLoadCurrent + " A"));
                lblAi.Invoke(new System.Action(() => lblAi.Text = aiCurrent + " A"));
            }
            else
            {
                MessageBox.Show("Power Supply가 연결되지 않았습니다.", "측정 오류");
            }
        }

        // ELoad 데이터 수신 이벤트 핸들러 추가
        private void ELoadDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadLine(); // 수신된 데이터를 읽어옴
                this.Invoke(new System.Action(() =>
                {
                    lblAi.Text = data; // 수신된 데이터를 TextBox에 표시 (예시)
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("데이터 수신 실패: " + ex.Message, "데이터 수신 상태");
            }
        }

        // 나머지 PS 관련 메서드는 이전과 동일하게 유지
        private void ELoadRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (serialPort != null && isConnected)
            {
                RadioButton selectedButton = sender as RadioButton;
                if (selectedButton != null && selectedButton.Checked)
                {
                    string command = "";

                    if (selectedButton == CCButton)
                    {
                        command = "MODE CC"; // CC 모드 설정 명령어
                    }
                    else if (selectedButton == CVButton)
                    {
                        command = "MODE CV"; // CV 모드 설정 명령어
                    }
                    else if (selectedButton == CRButton)
                    {
                        command = "MODE CR"; // CR 모드 설정 명령어
                    }

                    try
                    {
                        serialPort.WriteLine(command);
                        MessageBox.Show("명령 전송 성공: " + command, "모드 설정");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("명령 전송 실패: " + ex.Message, "모드 설정 오류");
                    }
                }
            }
            else
            {
                MessageBox.Show("Power Supply가 연결되지 않았습니다.", "모드 설정 오류");
            }
        }

        private void DisconnectPowerSupply()
        {
            if (psClient != null && psConnected)
            {
                try
                {
                    psStream.Close();
                    psClient.Close();
                    psConnected = false;
                    MessageBox.Show("PowerSupply 연결이 해제되었습니다.", "연결 상태");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("PowerSupply 연결 해제 실패: " + ex.Message, "연결 상태");
                }
            }
            else
            {
                MessageBox.Show("PowerSupply가 연결되지 않았습니다.", "연결 상태");
            }
        }

        // ConnectToPowerSupply 메서드에서 StreamReader 및 StreamWriter 초기화
        private void ConnectToPowerSupply()
        {
            if (!psConnected)
            {
                try
                {
                    // TCP 클라이언트 객체 초기화 및 PowerSupply IP와 포트 번호 설정
                    psClient = new TcpClient("169.254.100.192", 5025);
                    psStream = psClient.GetStream(); // 네트워크 스트림 생성
                    psConnected = true; // 연결 상태 업데이트
                    MessageBox.Show("PowerSupply 연결 성공", "연결 상태");

                    // StreamReader 및 StreamWriter 초기화
                    InitializeReaderWriter();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("PowerSupply 연결 실패: " + ex.Message, "연결 상태");
                }
            }
            else
            {
                MessageBox.Show("이미 PowerSupply에 연결된 상태입니다.", "연결 상태");
            }
        }


        private void SendCommandToPS(string command)
        {
            try
            {
                if (writer != null)
                {
                    writer.WriteLine(command);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("PowerSupply 명령어 전송 실패: " + ex.Message, "명령어 오류");
            }
        }

        private string SendCommandAndReadResponse(string command)
        {
            try
            {
                if (writer != null && reader != null)
                {
                    // 명령어 전송
                    writer.WriteLine(command);

                    // 응답 읽기
                    string response = reader.ReadLine();
                    return response.Trim() ?? string.Empty; // Trim() 호출 전 null 체크
                }
                else
                {
                    return string.Empty; // PS가 연결되지 않은 경우
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("명령어 전송 실패: " + ex.Message, "오류");
                return string.Empty;
            }
        }

        /*
        // 비동기 네트워크 통신 메서드 정의
        private async Task SendCommandToPSAsync(string command)
        {
            try
            {
                if (psStream != null && psStream.CanWrite)
                {
                    byte[] commandBytes = Encoding.ASCII.GetBytes(command + "\n");
                    await psStream.WriteAsync(commandBytes, 0, commandBytes.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("PowerSupply 명령어 전송 실패: " + ex.Message, "명령어 오류");
            }
        }

        // 비동기 응답 읽기 메서드 정의
        private async Task<string> SendCommandAndReadResponseAsync(string command)
        {
            try
            {
                if (psStream != null && psStream.CanWrite && psStream.CanRead)
                {
                    byte[] commandBytes = Encoding.ASCII.GetBytes(command + "\r");
                    await psStream.WriteAsync(commandBytes, 0, commandBytes.Length);

                    byte[] buffer = new byte[1024];
                    int bytesRead = await psStream.ReadAsync(buffer, 0, buffer.Length);
                    return Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("명령어 전송 실패: " + ex.Message, "오류");
                return string.Empty;
            }
        }*/


        private void UpdatePSStatus()
        {
            // 현재 전압 읽기
            string voltage = ReadResponseFromPS("MEAS:VOLT?");
            lblPV.Invoke(new System.Action(() => lblPV.Text = voltage + " V"));

            // 현재 전류 읽기
            string current = ReadResponseFromPS("MEAS:CURR?");
            lblPC.Invoke(new System.Action(() => lblPC.Text = current + " A"));
        }

        private void GetAndShowPSMeasurements()
        {
            if (psStream != null && psConnected)
            {
                try
                {
                    // P/S 전압 측정
                    string psVoltage = ReadResponseFromPS("MEAS:VOLT?");
                    lblPV.Text = string.IsNullOrEmpty(psVoltage) ? "0" : psVoltage + " V";

                    // P/S 전류 측정
                    string psCurrent = ReadResponseFromPS("MEAS:CURR?");
                    lblPC.Text = string.IsNullOrEmpty(psCurrent) ? "0" : psCurrent + " A";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("PowerSupply 측정 실패: " + ex.Message, "측정 오류");
                }
            }
            else
            {
                MessageBox.Show("PowerSupply가 연결되지 않았습니다.", "측정 오류");
            }
        }

        private string ReadResponseFromPS(string command)
        {
            try
            {
                if (writer != null && reader != null)
                {
                    writer.WriteLine(command);
                    string response = reader.ReadLine();
                    return response.Trim() ?? "0"; // Trim() 호출 전 null 체크
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("PS 상태 읽기 실패: " + ex.Message, "오류");
            }
            return "0"; // 읽기에 실패한 경우 0 반환
        }

    }
}

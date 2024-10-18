# KIKUSUI 전원공급기 및 ELOAD 제어 시스템

## 개요
이 프로젝트는 **KIKUSUI PWX1500L** 프로그래머블 DC 전원공급기와 **KIKUSUI PLZ1205W** 전자 부하(ELOAD)를 제어하고 측정하는 시스템입니다. 또한 **DHAB S/133** 전류 센서를 NI 하드웨어에 통합하여 AI(Analog Input) 포트를 통해 전류 값을 측정하는 기능이 포함되어 있습니다.

LAN을 통한 SCPI 명령어를 사용하여 전원공급기 및 ELOAD의 정확한 제어와 모니터링을 제공합니다.

## 주요 기능
- **SCPI 기반 제어**: LAN 통신을 통해 **KIKUSUI PWX1500L**과 **PLZ1205W**를 실시간으로 제어할 수 있으며, SCPI 명령어를 사용합니다.
- **DHAB S/133 전류 측정**: **DHAB S/133** 전류 센서를 NI 하드웨어의 AI 포트에 연결하여 정확한 전류 값을 측정할 수 있습니다.
- **그래프 모니터링 및 시각화**: 전압, 전류 등 주요 파라미터를 실시간으로 그래프에 표시하여 쉽게 모니터링 및 분석이 가능합니다.
- **프로그램 및 스텝 제어**: ELOAD에서 프로그램이나 개별 스텝을 선택, 해제, 삭제할 수 있으며, 이를 통해 정밀한 테스트와 자동화를 구현할 수 있습니다.
- **안전 및 리셋 메커니즘**: 안전한 작동을 보장하고, 전원공급기와 ELOAD 설정을 쉽게 초기화할 수 있는 리셋 기능을 포함하고 있습니다.

## 시스템 요구 사항
- **소프트웨어**: 
  - Visual Studio 2012 이상
  - NI LabVIEW 또는 Measurement & Automation Explorer (MAX)
  - KIKUSUI PWX1500L 및 PLZ1205W 드라이버
- **하드웨어**: 
  - KIKUSUI PWX1500L 전원공급기
  - KIKUSUI PLZ1205W 전자 부하(ELOAD)
  - DHAB S/133 전류 센서
  - NI DAQ(데이터 수집) 장치, AI 포트 지원

## 설치 방법
1. 이 프로젝트를 클론합니다:
   ```bash
   git clone https://github.com/your-username/kikusui-control-project.git
   ```

2. Visual Studio에서 프로젝트를 엽니다.
3. KIKUSUI 장비와 NI DAQ 하드웨어 드라이버가 설치되어 있는지 확인합니다.
4. 프로젝트를 빌드하고 실행합니다.

## 사용 방법
1. **전류 및 전압 설정**: 전원공급기와 ELOAD의 출력 전압, 전류 설정 값을 입력합니다.
2. **프로그램 제어**: 프로그램을 선택하고, 스텝을 추가하거나 삭제합니다.
3. **실시간 데이터 측정**: NI 하드웨어 AI 포트로 전류 값을 실시간으로 측정합니다.
4. **그래프 시각화**: 측정된 데이터를 실시간으로 그래프에 표시합니다.
5. **ELOAD 프로그램 삭제**: ELOAD에서 특정 프로그램이나 스텝을 선택하여 삭제할 수 있습니다.

## 주요 코드 설명

### SCPI 명령을 사용한 프로그램 선택

```csharp
string command = string.Format("PROG \"/{0}\"", selectedProgramName);  // 프로그램 선택
serialPort.WriteLine(command);
```

### SCPI 명령을 사용한 프로그램 해제

```csharp
string deselectCommand = "PROG \"\"";  // 프로그램 선택 해제
serialPort.WriteLine(deselectCommand);
```

### ELOAD에서 스텝 삭제 명령어 (예시)

```csharp
string deleteStepCommand = string.Format("PROG:STEP:DEL {0}", stepNumber); // 특정 스텝 삭제
serialPort.WriteLine(deleteStepCommand);
```

## SCPI와 LAN 통신
- **SCPI** (Standard Commands for Programmable Instruments)는 전원공급기나 ELOAD 같은 장비를 제어하기 위한 표준 명령어 세트입니다.
- **LAN 통신**을 사용하여 PC와 KIKUSUI 장비를 연결하고, SCPI 명령어를 통해 제어 및 측정 데이터를 전송합니다.

## 주의 사항
- 시리얼 포트가 올바르게 설정되었는지 확인하고, KIKUSUI 장비와 LAN 연결 상태를 점검하세요.
- 전류 측정을 위한 **DHAB S/133** 센서의 연결 상태와 NI MAX 설정을 확인하세요.

## Reference
- [Week4_DH.pptx](https://github.com/user-attachments/files/17429671/Week4_DH.pptx)

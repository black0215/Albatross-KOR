using System;
using System.Drawing;
using System.Windows.Forms;

namespace Albatross
{
    public class WaitForm : Form
    {
        private Label ProgressLabel;
        private ProgressBar ProgressBar;

        public WaitForm(string message = "저장 중입니다...")
        {
            // === Form 기본 설정 ===
            this.Text = "알림";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false; // X 버튼 제거
            this.ClientSize = new Size(500, 130);
            this.BackColor = Color.White;

            // === 메시지 레이블 ===
            ProgressLabel = new Label();
            ProgressLabel.Text = message;
            ProgressLabel.TextAlign = ContentAlignment.MiddleCenter;
            ProgressLabel.Dock = DockStyle.Fill;
            ProgressLabel.Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);
            ProgressLabel.ForeColor = Color.FromArgb(45, 45, 45);

            // === 프로그레스 바 영역 ===
            var progressPanel = new Panel();
            progressPanel.Dock = DockStyle.Bottom;
            progressPanel.Height = 40;
            progressPanel.Padding = new Padding(20, 5, 20, 15);

            ProgressBar = new ProgressBar();
            ProgressBar.Style = ProgressBarStyle.Continuous; // Blocks for clearer progress
            ProgressBar.Dock = DockStyle.Fill;

            // 파란색 강제 설정을 위한 시도 (Windows 테마에 따라 다를 수 있음)
            // 기본적으로 Windows Vista 이상에서는 초록색이 기본이지만, State 수정 등으로 변경 가능하나 복잡함.
            // 여기서는 기본 스타일을 사용하되 기능 구현에 집중.

            progressPanel.Controls.Add(ProgressBar);

            // === 컨트롤 추가 ===
            this.Controls.Add(ProgressLabel);
            this.Controls.Add(progressPanel);
        }

        // 성능 저하를 막기 위한 업데이트 간격 조절 (마지막 업데이트 시간)
        private long lastUpdateTicks = 0;

        public void SetProgress(int current, int total, string currentFileName)
        {
            // 1. 완료 상태이거나, 마지막 업데이트로부터 일정 시간(예: 30ms)이 지났을 때만 UI 업데이트
            // 이렇게 하지 않으면 파일이 많을 때 UI 갱신 부하로 인해 저장 속도가 2~3배 느려짐
            long now = DateTime.Now.Ticks;
            if (current != total && (now - lastUpdateTicks) < 300000) // 10000 ticks = 1ms, 300000 = 30ms
            {
                return;
            }
            lastUpdateTicks = now;

            // 2. 비동기 호출 (BeginInvoke) 사용하여 작업 스레드가 UI 처리를 기다리지 않게 함
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetProgress(current, total, currentFileName)));
                return;
            }

            try
            {
                ProgressBar.Maximum = total;
                ProgressBar.Value = Math.Min(current, total);

                // 메세지 형식: "변경된 파일 (n/n) name"
                ProgressLabel.Text = $"변경된 파일 ({current}/{total})\n{currentFileName}";
            }
            catch { }
        }
    }
}

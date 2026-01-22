using System;
using System.Drawing;
using System.Windows.Forms;

namespace Albatross
{
    public class AboutForm : Form
    {
        public AboutForm()
        {
            // === Form 기본 설정 ===
            this.Text = "Albatross 정보";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(420, 230);

            // === 텍스트 ===
            var label = new Label();
            label.AutoSize = false;
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Font = new Font("Segoe UI", 9F);
            label.Text =
@"[Albatross v1.2]

원작자 :
Tinifan
https://github.com/Tiniifan

개선 / 확장 :
껌정도깨비

버그 제보 / 개선 문의:
아카라이브 요괴워치 채널
https://arca.live/b/yokaiwatch";

            // === 버튼 ===
            var button = new Button();
            button.Text = "확인";
            button.Width = 80;
            button.Height = 28;
            button.Anchor = AnchorStyles.Bottom;
            button.Location = new Point(
                (this.ClientSize.Width - button.Width) / 2,
                this.ClientSize.Height - 45
            );
            button.Click += (s, e) => this.Close();

            // === 컨트롤 추가 ===
            this.Controls.Add(label);
            this.Controls.Add(button);
        }
    }
}

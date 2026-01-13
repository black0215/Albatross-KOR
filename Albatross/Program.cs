using System;
using System.Windows.Forms;

namespace Albatross
{
    static class Program
    {
        /// <summary>
        /// 애플리케이션 진입점
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 🔹 실행 시 안내 창 표시
            using (var about = new AboutForm())
            {
                about.ShowDialog();
            }

            // 🔹 메인 홈 창 실행
            Application.Run(new Home());
        }
    }
}

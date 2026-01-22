using System;
using System.IO;
using Albatross.Tools;
using System.Windows.Forms;
using Albatross.Yokai_Watch;
using Albatross.Yokai_Watch.Games;
using Albatross.Yokai_Watch.Games.YW2;
using Albatross.Forms.Characters;
using Albatross.Forms.Encounters;
using Albatross.Forms.Shops;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Albatross
{
    public partial class HomeGame : Form
    {
        private IGame Game;

        public HomeGame(string projectName, IGame game)
        {
            InitializeComponent();

            this.Text = projectName;
            Game = game;
        }

        private async void SaveToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            using (WaitForm waitForm = new WaitForm("파일을 저장하고 있습니다..."))
            {
                waitForm.Show();
                waitForm.Refresh();
                this.Enabled = false;

                try
                {
                    await Task.Run(() => Game.Save((current, total, name) =>
                    {
                        waitForm.SetProgress(current, total, name);
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("저장 중 오류가 발생했습니다:\n" + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Enabled = true;
                    waitForm.Close();
                }
            }

            MessageBox.Show("저장되었습니다!\n\n변경사항은 프로그램 종료 시 자동으로 적용됩니다.", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void HomeGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Console.WriteLine("\n=== 프로그램 종료 중 ===");

                // ✅ 1단계: 임시 저장 파일 확인
                YW2 yw2Game = Game as YW2;
                if (yw2Game != null && !string.IsNullOrEmpty(yw2Game.RomfsPath))
                {
                    string tempPath = Path.Combine(Path.GetDirectoryName(yw2Game.RomfsPath), "temp");
                    string infoFile = Path.Combine(tempPath, "_save_info.txt");

                    if (File.Exists(infoFile))
                    {
                        Console.WriteLine("임시 저장 파일 발견 - 원본 업데이트 시작");

                        var info = new Dictionary<string, string>();
                        foreach (var line in File.ReadAllLines(infoFile))
                        {
                            var parts = line.Split('=');
                            if (parts.Length == 2)
                                info[parts[0]] = parts[1];
                        }

                        // ✅ 2단계: 스트림 닫기 (파일 잠금 해제)
                        if (Game.Game != null)
                        {
                            Console.WriteLine("  Game 스트림 닫는 중...");
                            Game.Game.Close();
                        }

                        if (Game.Language != null)
                        {
                            Console.WriteLine("  Language 스트림 닫는 중...");
                            Game.Language.Close();
                        }

                        // 잠시 대기 (파일 잠금 완전 해제)
                        System.Threading.Thread.Sleep(100);

                        // ✅ 3단계: Game 파일 교체
                        if (info.ContainsKey("GameFile") && File.Exists(info["GameFile"]))
                        {
                            string tempGameFile = info["GameFile"];
                            string origGameFile = yw2Game.RomfsPath + @"\yw2_a.fa";

                            Console.WriteLine($"  Game 파일 교체: {Path.GetFileName(origGameFile)}");

                            // 백업 생성
                            if (File.Exists(origGameFile + ".backup"))
                                File.Delete(origGameFile + ".backup");
                            if (File.Exists(origGameFile))
                                File.Move(origGameFile, origGameFile + ".backup");

                            // 임시 파일을 원본으로
                            File.Move(tempGameFile, origGameFile);

                            var origSize = new FileInfo(origGameFile).Length;
                            Console.WriteLine($"  ✅ Game 업데이트 완료: {origSize:N0} bytes ({info["GameFileCount"]} 파일)");

                            // 백업 삭제
                            if (File.Exists(origGameFile + ".backup"))
                                File.Delete(origGameFile + ".backup");
                        }

                        // ✅ 4단계: Language 파일 교체 (있으면)
                        if (info.ContainsKey("LangFile") && info["LangFile"] != "N/A" && File.Exists(info["LangFile"]))
                        {
                            string tempLangFile = info["LangFile"];
                            string langCode = yw2Game.LanguageCode;
                            string origLangFile = yw2Game.RomfsPath + @"\yw2_lg_" + langCode + ".fa";

                            Console.WriteLine($"  Language 파일 교체: {Path.GetFileName(origLangFile)}");

                            // 백업 생성
                            if (File.Exists(origLangFile + ".backup"))
                                File.Delete(origLangFile + ".backup");
                            if (File.Exists(origLangFile))
                                File.Move(origLangFile, origLangFile + ".backup");

                            // 임시 파일을 원본으로
                            File.Move(tempLangFile, origLangFile);

                            var origSize = new FileInfo(origLangFile).Length;
                            Console.WriteLine($"  ✅ Language 업데이트 완료: {origSize:N0} bytes ({info["LangFileCount"]} 파일)");

                            // 백업 삭제
                            if (File.Exists(origLangFile + ".backup"))
                                File.Delete(origLangFile + ".backup");
                        }

                        // ✅ 5단계: 정보 파일 삭제
                        File.Delete(infoFile);
                        Console.WriteLine("=== ✅ 모든 변경사항 저장 완료 ===\n");

                        MessageBox.Show(
                            "모든 변경사항이 성공적으로 저장되었습니다!\n\n" +
                            $"• Game 파일: {info["GameFileCount"]} 파일\n" +
                            $"• Language 파일: {info["LangFileCount"]} 파일\n\n" +
                            "다음 실행 시 업데이트된 데이터가 로드됩니다.",
                            "저장 완료",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                    else
                    {
                        // 저장된 변경사항이 없으면 그냥 닫기
                        if (Game.Game != null) Game.Game.Close();
                        if (Game.Language != null) Game.Language.Close();
                    }
                }
                else
                {
                    // YW2가 아닌 경우 기본 동작
                    if (Game.Game != null) Game.Game.Close();
                    if (Game.Language != null) Game.Language.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 종료 시 오류: {ex.Message}");
                MessageBox.Show(
                    $"종료 시 오류가 발생했습니다:\n\n{ex.Message}\n\n" +
                    "temp 폴더의 파일을 수동으로 복사해주세요.",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void FeatureListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (featureListBox.SelectedIndex == -1) return;

            switch (featureListBox.SelectedItem.ToString())
            {
                case "Charabase":
                    CharabaseButton_Click(sender, e);
                    break;
                case "Charascale":
                    CharascaleButton_Click(sender, e);
                    break;
                case "Charaparam":
                    CharaparamButton_Click(sender, e);
                    break;
                case "Encounters":
                    EncounterWindow encounterWindow = new EncounterWindow(Game);
                    encounterWindow.ShowDialog();
                    break;
                case "Shops":
                    ShopWindow shopWindow = new ShopWindow(Game);
                    shopWindow.ShowDialog();
                    break;
            }
        }

        private void CharabaseButton_Click(object sender, EventArgs e)
        {
            CharabaseWindow charabaseWindow = new CharabaseWindow(Game);
            charabaseWindow.ShowDialog();
        }

        private void CharaparamButton_Click(object sender, EventArgs e)
        {
            CharaparamWindow charaparamWindow = new CharaparamWindow(Game);
            charaparamWindow.ShowDialog();
        }

        private void CharascaleButton_Click(object sender, EventArgs e)
        {
            CharascaleWindow charascaleWindow = new CharascaleWindow(Game);
            charascaleWindow.ShowDialog();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
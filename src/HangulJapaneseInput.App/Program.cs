namespace HangulJapaneseInput.App;

static class Program
{
    [STAThread]
    static void Main()
    {
        using var singleInstance = new Mutex(true, "HangulJapaneseInput.SingleInstance", out var createdNew);
        if (!createdNew)
        {
            MessageBox.Show("한글 발음 일본어 입력기가 이미 실행 중입니다.", "한글 발음 일본어 입력기");
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApplicationContext());
    }
}

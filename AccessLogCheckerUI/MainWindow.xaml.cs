using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AccessLogCheckerUI;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void btnChangeWorkingFolder_Click(object sender, RoutedEventArgs e)
    {
        string workingFolder = txtWorkingDirectory.Text;
        string[] ignoreIpList = txtIgnoreIpList.Text.Split(';').Select(ip => ip).ToArray();
        string keyPhrase = txtKeyPhrase.Text;

        if (Directory.Exists(workingFolder) || string.IsNullOrWhiteSpace(keyPhrase))
        {
            MessageBox.Show("Не заполнена ключевая фраза или не выбрана рабочая дериктория");
            return;
        }


        var files = Directory.GetFiles(workingFolder);

        List<string> resultRows = new List<string>();
        foreach (var file in files)
        {
            var results = await ReadLogFile(file, keyPhrase, ignoreIpList);
            resultRows.AddRange(results);
        }

        var sb = new StringBuilder();
        foreach (var row in resultRows)
        {
            sb.AppendLine(row);
        }

        MessageBox.Show($"Выполнено: найдено {resultRows.Count} строк");

        txtSearchResult.Text = sb.ToString();
    }

    private async Task<string[]> ReadLogFile(string file, string keyPhrase, string[] ignoreIpList)
    {
        List<string> validRows = new List<string>();

        foreach (var row in await File.ReadAllLinesAsync(file))
        {
            if (row.IndexOf(keyPhrase, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (ignoreIpList.Any())
                {
                    bool ignoreRow = false;
                    foreach (var ip in ignoreIpList)
                    {
                        if (row.StartsWith(ip))
                        {
                            ignoreRow = true;
                            break;
                        }
                    }
                    if (ignoreRow)
                    {
                        continue;
                    }
                }

                validRows.Add(row);
            }
        }

        return validRows.ToArray();
    }
}

using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.IO;
using System.Globalization;


namespace Daily_Data_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //Helper to display input box (WPF doesn't have it)
        public static string ShowInputDialog(string message, string title)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var stack = new StackPanel { Margin = new Thickness(10) };
            stack.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap
            });

            var textBox = new TextBox { Margin = new Thickness(0, 10, 0, 10) };
            stack.Children.Add(textBox);

            var okButton = new Button
            {
                Content = "Tamam",
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            okButton.Click += (s, e) => dialog.DialogResult = true;

            stack.Children.Add(okButton);
            dialog.Content = stack;

            return dialog.ShowDialog() == true ? textBox.Text : null;
        }


        public void SelectData_OnClick(object sender, RoutedEventArgs e)
        {
            //Display file dialog to select csv file
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Lutfen CSV dosyasi secin",
                Filter = "CSV Dosyasi (*.csv)|*.csv",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Multiselect = false
            };

            //Check if file was selected
            if(openFileDialog.ShowDialog() == true)
            {
                //File was selected, parse it as CSV
                string filePath = openFileDialog.FileName;

                try
                {
                    //Read all lines of csv
                    var csvLines = File.ReadAllLines(filePath);

                    //Split each line by semicolon
                    List<string[]> csvData = csvLines.Select(eachLine => eachLine.Split(';')).ToList();

                    //Get headers
                    string[] headers = csvData[0];
                    
                    //Find date column indexes                    
                    int startCol = Array.FindIndex(headers, h => h.Equals("Interval Start", StringComparison.OrdinalIgnoreCase));
                    int endCol = Array.FindIndex(headers, h => h.Equals("Interval End", StringComparison.OrdinalIgnoreCase));

                    //If we cannot find date columns, show error and exit
                    if (startCol == -1 || endCol == -1)
                    {
                        MessageBox.Show(
                            "Interval Start veya Interval End sutunlari bulunamadi.","Hata", MessageBoxButton.OK, MessageBoxImage.Error
                        );
                        return;
                    }

                    //Extract hint from interval columns
                    string hintDate = "";
                    if (csvData.Count > 1)
                    {
                        string val = csvData[1][startCol];
                        if (!string.IsNullOrWhiteSpace(val))
                            hintDate = val;
                    }

                    //Get user input for correct date
                    string promptMsg;

                    if (string.IsNullOrEmpty(hintDate))
                        promptMsg = "Lütfen verinin ait olduğu tarihi şu formatta giriniz (gg.aa.yyyy) :";
                    else
                        promptMsg = "(İpucu: Interval Start sütununda " + hintDate + " yazıyor)\n" +
                                    "Lütfen verinin ait olduğu tarihi şu formatta giriniz (gg.aa.yyyy) :";

                    string dateStr = ShowInputDialog(promptMsg, "Tarih Girişi");

                    //If user did not enter a date, return
                    if (string.IsNullOrWhiteSpace(dateStr)) return;


                    //Validate entered date
                    if (!DateTime.TryParseExact(dateStr, new[] { "dd.MM.yyyy", "d.M.yyyy" }, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime inputDate))
                    {
                        MessageBox.Show("Hatali tarih. Lutfen gecerli bir formatta tarih girin (gg.aa.yyyy).", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    //Calculate end date
                    string txtStart = inputDate.ToString("yyyy-MM-dd");
                    string txtEnd = inputDate.AddDays(1).ToString("yyyy-MM-dd");

                    //Fill each row of the data body
                    for (int i = 1; i < csvData.Count; i++)
                    {
                        string[] row = csvData[i];
                        if (row.Length <= Math.Max(startCol, endCol))
                        {
                            //If row has less columns than the header row, resize the row
                            Array.Resize(ref row, Math.Max(startCol, endCol) + 1);
                        }

                        //Fill row's start & end date values
                        row[startCol] = txtStart;
                        row[endCol] = txtEnd;
                        
                        //Replace the row with newly constructed one
                        csvData[i] = row; 
                    }


                    //Save it as new excel file
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Title = "Excel'in nereye kaydedilecegini secin",
                        Filter = "Excel Dosyası (*.xlsx)|*.xlsx",
                        FileName = System.IO.Path.GetFileNameWithoutExtension(filePath) + ".xlsx",
                        InitialDirectory = System.IO.Path.GetDirectoryName(filePath)
                    };

                    //Check if user saved properly
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string savePath = saveFileDialog.FileName;
                        //Catch errors during saving
                        try
                        {
                            //Create excel workbook (with closedxml)
                            using (var workbook = new XLWorkbook())
                            {
                                //Add a new sheet
                                var worksheet = workbook.Worksheets.Add("Data");

                                //Write data from csv into the worksheet
                                for (int j = 0; j < csvData.Count; j++)
                                {
                                    string[] row = csvData[j];
                                    for (int c = 0; c < row.Length; c++)
                                    {
                                        worksheet.Cell(j + 1, c + 1).Value = row[c];
                                    }
                                }

                                //Save the workbook
                                workbook.SaveAs(savePath);
                            }

                            //Success msg
                            MessageBox.Show("Veri duzeltildi ve excel kaydedildi.", "Basarili", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            //If error occurs
                            MessageBox.Show("Excel dosyası kaydedilirken hata olustu. Lutfen BI ekibi ile iletisime gecin. Hata :\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        //If user cancelled
                        MessageBox.Show("Kaydetme iptal edildi.", "Iptal Edildi", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    //If there was an error reading the csv file
                    MessageBox.Show($"CSV okunurken hata olustu. Lutfen BI ekibi ile iletisime gecin.:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Dosya secilmedi.", "Hata",MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

    }

}

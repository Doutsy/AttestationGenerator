using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.Text.RegularExpressions;
using QRCoder;
using iText.IO.Image;
using Image = iText.Layout.Element.Image;
using iText.Layout.Properties;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace AttestationGenerator
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public struct Save
        {
            public string firstName;
            public string lastName;
            public string birthDate;
            public string birthPlace;
            public string adress;
            public string city;
            public string postalCode;
            public string goingOutDate;
            public string goingOutTime;
        };
        Save infos = new Save();

        private static Regex _dateRegex = new Regex(@"^(0[1-9]|[12]\d|3[01])[\/](0[1-9]|1[012])[\/](\d{4})$");
        private static Regex _birthdayRegex = new Regex(@"([0][1-9]|[1-2][0-9]|30|31)\/([0][1-9]|10|11|12)\/(19[0-9][0-9]|20[0-1][0-9]|2020)");
        private static Regex _hourRegex = new Regex(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$");
        private static Regex _postalRegex = new Regex(@"(\d){5}");

        private float[] reasonsHeight = { 572, 527, 471, 429, 390, 352, 289, 249, 205 };
        private string[] reasonsName = { "travail", "achats", "sante", "famille", "handicap", "sport_animaux", "convocation", "missions", "enfants" };

        private bool IsDateCorrect(string _text)
        {
            return _dateRegex.IsMatch(_text);
        }
        private bool IsBirthDayCorrect(string _text)
        {
            return _birthdayRegex.IsMatch(_text);
        }
        private bool IsHourCorrect(string _text)
        {
            return _hourRegex.IsMatch(_text);
        }
        private bool IsPostalCorrect(string _text)
        {
            return _postalRegex.IsMatch(_text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBirthDayCorrect(birthDateTextBox.Text))
            {
                MessageBox.Show("Date de naissance non valide.\nFormat : JJ/MM/AAAA", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!IsDateCorrect(dateTextBox.Text))
            {
                MessageBox.Show("Date de sortie non valide.\nFormat : JJ/MM/AAAA", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!IsHourCorrect(timeTextBox.Text))
            {
                MessageBox.Show("Heure de sortie non valide.\nFormat : HH:MM", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!IsPostalCorrect(postalTextBox.Text))
            {
                MessageBox.Show("Code postal non valide.\nFormat : 01234", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CheckBox[] checkBoxes = { CheckBoxTravail, CheckBoxAchats, CheckBoxSante, CheckBoxFamille, CheckBoxHandicap, CheckBoxSportAnimaux, CheckBoxConvocation, CheckBoxMissions, CheckBoxEnfants };

            #region "save Canvas to Image"
            Rect rect = new Rect(signatureCanvas.RenderSize);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right,
              (int)rect.Bottom, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(signatureCanvas);
            //endcode as PNG
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            //save to memory stream
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            pngEncoder.Save(ms);
            ms.Close();
            #endregion

            string appPath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "PDF file | *.pdf";
            string savePath;
            if (saveFileDialog1.ShowDialog() == true)
            {
                savePath = saveFileDialog1.FileName;
            }
            else
            {
                return;
            }

            PdfDocument attestationPdf = new PdfDocument(new PdfReader(appPath + "/certificate.pdf"), new PdfWriter(new FileStream(savePath, FileMode.Create, FileAccess.Write)));
            string firstName = firstNameTextBox.Text;
            string lastName = lastNameTextBox.Text;
            string fullName = firstName + " " + lastName;
            string birthDate = birthDateTextBox.Text;
            string birthPlace = birthPlaceTextBox.Text;
            string adress = adressTextBox.Text;
            string city = cityTextBox.Text;
            string postalCode = postalTextBox.Text;
            string fullAdress = $"{adress} {postalCode} {city}";
            string goingOutDate = dateTextBox.Text;
            string goingOutTime = timeTextBox.Text;
            Document document = new Document(attestationPdf);
            document.Add(new Paragraph(fullName).SetFixedPosition(119, 690, 600));
            document.Add(new Paragraph(birthDate).SetFixedPosition(119, 668, 100));
            document.Add(new Paragraph(birthPlace).SetFixedPosition(297, 668, 600));
            document.Add(new Paragraph(fullAdress).SetFixedPosition(133, 646, 600));
            document.Add(new Paragraph(city).SetFixedPosition(105, 171, 600));
            document.Add(new Paragraph(goingOutDate).SetFixedPosition(91, 147, 200));
            document.Add(new Paragraph(goingOutTime).SetFixedPosition(264, 147, 200));
            int i = 0;
            List<String> reasonsChecked = new List<string>();
            foreach (var checkbox in checkBoxes)
            {
                if ((bool)checkbox.IsChecked)
                {
                    document.Add(new Paragraph("X").SetFixedPosition(78, reasonsHeight[i], 20));
                    reasonsChecked.Add(reasonsName[i]);
                }
                i++;
            }
            var reasons = String.Join(", ", reasonsChecked);

            string creationDate = DateTime.Now.Date.ToString().Split(' ')[0];
            string creationHour = DateTime.Now.TimeOfDay.ToString().Split('.')[0];
            string qrCodeString = String.Join("; \n", $"Cree le: {creationDate} à {creationHour}", $"Nom: {lastName}", $"Prenom: {firstName}", $"Naissance: {birthDate} à {birthPlace}", $"Adresse: {fullAdress}", $"Sortie: {goingOutDate} à {goingOutTime}", $"Motifs: {reasons}");

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeString, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);
            ImageData imageData = ImageDataFactory.Create(qrCodeImage);
            Image pdfImg = new Image(imageData);
            var smallImg = pdfImg;
            smallImg.SetHeight(100);
            smallImg.SetWidth(100);
            smallImg.SetFixedPosition(document.GetPdfDocument().GetDefaultPageSize().GetWidth() - 156, 94);
            document.Add(smallImg);
            ImageData signatureData = ImageDataFactory.Create(ms.ToArray());
            Image signatureImg = new Image(signatureData);
            signatureImg.SetFixedPosition(119, 94);
            signatureImg.SetHeight(40);
            signatureImg.SetWidth(100);
            document.Add(signatureImg);

            if (mobileCheckbox.IsChecked == true)
            {
                document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                var largeImg = pdfImg;
                largeImg.SetHeight(300);
                largeImg.SetWidth(300);
                largeImg.SetFixedPosition(50f, document.GetPdfDocument().GetDefaultPageSize().GetHeight() - 344);
                document.Add(largeImg);
            }
            document.Close();

            MessageBox.Show("Attestation générée !", "Réussite", MessageBoxButton.OK);
        }
        private Point currentPoint = new Point();
        private void signatureCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                currentPoint = e.GetPosition(signatureCanvas);
        }

        private void signatureCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line line = new Line();

                line.Stroke = SystemColors.WindowFrameBrush;
                line.X1 = currentPoint.X;
                line.Y1 = currentPoint.Y;
                line.X2 = e.GetPosition(signatureCanvas).X;
                line.Y2 = e.GetPosition(signatureCanvas).Y;

                currentPoint = e.GetPosition(signatureCanvas);

                signatureCanvas.Children.Add(line);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            signatureCanvas.Children.Clear();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Window1 window1 = new Window1();
            window1.Show();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Json file|*.json";
            if (openFileDialog1.ShowDialog() == true)
            {
                string infosLoaded = File.ReadAllText(openFileDialog1.FileName);
                infos = JsonConvert.DeserializeObject<Save>(infosLoaded);
                firstNameTextBox.Text = infos.firstName;
                lastNameTextBox.Text = infos.lastName;
                birthDateTextBox.Text = infos.birthDate;
                birthPlaceTextBox.Text = infos.birthPlace;
                adressTextBox.Text = infos.adress;
                cityTextBox.Text = infos.city;
                postalTextBox.Text = infos.postalCode;
                dateTextBox.Text = infos.goingOutDate;
                if (DateTime.Now.Hour < 10)
                {
                    timeTextBox.Text = $"0{DateTime.Now.Hour}:{DateTime.Now.Minute}";
                }
                else
                {
                    timeTextBox.Text = DateTime.Now.Hour + ":" + DateTime.Now.Minute;
                }
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            infos.firstName = firstNameTextBox.Text;
            infos.lastName = lastNameTextBox.Text;
            infos.birthDate = birthDateTextBox.Text;
            infos.birthPlace = birthPlaceTextBox.Text;
            infos.adress = adressTextBox.Text;
            infos.city = cityTextBox.Text;
            infos.postalCode = postalTextBox.Text;
            infos.goingOutDate = dateTextBox.Text;
            infos.goingOutTime = timeTextBox.Text;
            string jsonSave = JsonConvert.SerializeObject(infos);
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Json file|*.json";

            if (saveFileDialog1.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog1.FileName, jsonSave);
            }
        }
    }
}

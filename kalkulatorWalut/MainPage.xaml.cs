using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using System.Xml.Linq;
using System.Globalization;
using Windows.Storage;
using Windows.Security.Cryptography.Core;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace kalkulatorWalut
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string daneNBP = "https://static.nbp.pl/dane/kursy/xml/LastA.xml";
        private string dane_użytkownik;
        List<PozycjaTabeliA> kursyAktualne = new List<PozycjaTabeliA>();
        ContentDialog wiadomosc;
        String dane_backup;
        String dane;
        
        public MainPage()
        {
            this.InitializeComponent();

        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                 await update();
            }
            catch (Exception ex)
            {

            }
            if (dane != null)
            {
                await data_backup(dane);
            }
            else
            {
                await read_backup();
                dane = dane_backup;
            }
            kursyAktualne.Clear();
            XDocument daneKursowe = XDocument.Parse(dane);
            var data = daneKursowe.Descendants("data_publikacji").First();
            data_aktualizacji.Text = "Zaktualizowano: " + data.Value.ToString();
            kursyAktualne = (from item in daneKursowe.Descendants("pozycja")
                             select new PozycjaTabeliA()
                             {
                                 przelicznik = (item.Element("przelicznik").Value),
                                 kod_waluty = (item.Element("kod_waluty").Value),
                                 kurs_sredni = (item.Element("kurs_sredni").Value)
                             }).ToList();
            kursyAktualne.Insert(0, new PozycjaTabeliA() { kurs_sredni = "1,0000", kod_waluty = "PLN", przelicznik = "1" });
            lbxZWaluty.ItemsSource = kursyAktualne;
            lbxNaWalute.ItemsSource = kursyAktualne;
            if ((int)ApplicationData.Current.LocalSettings.Values["lbxZWaluty"] == -1)
            {
                lbxZWaluty.SelectedIndex = 0;
            }
            if ((int)ApplicationData.Current.LocalSettings.Values["lbxNaWalute"] == -1)
            {
                lbxNaWalute.SelectedIndex = 0;
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("lbxZWaluty"))
            {
                lbxZWaluty.SelectedIndex = (int)ApplicationData.Current.LocalSettings.Values["lbxZWaluty"];
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("lbxNaWalute"))
            {
                lbxNaWalute.SelectedIndex = (int)ApplicationData.Current.LocalSettings.Values["lbxNaWalute"];
            }


        }
        private async Task update()
        {
            var serwerNBP = new HttpClient();
            string dane_teraz = "";
            try
            {
                if (dane_użytkownik is string && !string.IsNullOrWhiteSpace(dane_użytkownik) )
                {
                    dane_teraz = await serwerNBP.GetStringAsync(new Uri(dane_użytkownik));
                }
                else
                {
                    dane_teraz = await serwerNBP.GetStringAsync(new Uri(daneNBP));
                }
                dane = dane_teraz;

            }
            catch (Exception ex)
            {
                wiadomosc = new ContentDialog();
                string messageBoxText = "Błąd pobrania danych";
                wiadomosc.Title = messageBoxText;
                wiadomosc.PrimaryButtonText = "ok";
                await wiadomosc.ShowAsync();
                await read_backup();
                dane = dane_backup;
            }
        }
        private async Task  read_backup()
        {
            Windows.Storage.StorageFolder storageFolder =
                Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile =
                await storageFolder.GetFileAsync("dane.xml");
            string text = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);
            dane_backup = text;
        }
        private async Task data_backup(string dane)
        {
            Windows.Storage.StorageFolder storageFolder =
                Windows.Storage.ApplicationData.Current.LocalFolder;
            try
            {
                Windows.Storage.StorageFile writeFile =
                await storageFolder.GetFileAsync("dane.xml");
                await Windows.Storage.FileIO.WriteTextAsync(writeFile, dane);
            }
            catch (Exception e)
            {
                Windows.Storage.StorageFile sampleFile =
                await storageFolder.CreateFileAsync("dane.xml",
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);
                Windows.Storage.StorageFile writeFile =
                await storageFolder.GetFileAsync("dane.xml");
                await Windows.Storage.FileIO.WriteTextAsync(writeFile, dane);
            }
            
            
        }
        private void liczenie()
        {
            var pozZWaluty = lbxZWaluty.SelectedIndex;
            var pozNawalute = lbxNaWalute.SelectedIndex;
            PozycjaTabeliA zWaluty = kursyAktualne[pozZWaluty];
            PozycjaTabeliA naWalute = kursyAktualne[pozNawalute];
            var kursWalutyWyjściowej = zWaluty.kurs_sredni;
            var kursWalutyDocelowej = naWalute.kurs_sredni;
            var znak = Convert.ToChar(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
            kursWalutyWyjściowej = kursWalutyWyjściowej.Replace(',', znak);
            kursWalutyDocelowej = kursWalutyDocelowej.Replace(',', znak);
            znaczekZ.Text = zWaluty.kod_waluty;
            znaczekNa.Text = naWalute.kod_waluty;
            try
            {
                var kwota = txtKwota.Text.Replace(',','.');
                var kwotaPln = Convert.ToDouble(kwota) * Convert.ToDouble(kursWalutyWyjściowej);
                var kwotaDocelowa = kwotaPln / Convert.ToDouble(kursWalutyDocelowej);
                if (pozNawalute == 0)
                {
                    tbPrzeliczona.Text = kwotaPln.ToString(kwotaPln % 1 == 0 ? "F0" : "F2");
                }
                else
                {
                    tbPrzeliczona.Text = kwotaDocelowa.ToString(kwotaDocelowa % 1 == 0 ? "F0" : "F2");
                }
            }
            catch (System.FormatException ex)
            {
                tbPrzeliczona.Text = "";
            }
        }
        private void txtKwota_TextChanged(object sender, TextChangedEventArgs e)
        {

            liczenie();
           
        }

        private void lbxZWaluty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                liczenie();
            }
            catch (Exception ex){
            }
            ApplicationData.Current.LocalSettings.Values["lbxZWaluty"] = lbxZWaluty.SelectedIndex;
            
        }

        private void lbxNaWalute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                liczenie();
            }
            catch (Exception ex)
            {
            }
            ApplicationData.Current.LocalSettings.Values["lbxNaWalute"] = lbxNaWalute.SelectedIndex;
        }

        private void oProgramie_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(OProgramie),daneNBP);
        }

        private void pomoc_Click(object sender, RoutedEventArgs e)
        {
            var pozZWaluty = lbxZWaluty.SelectedIndex;
            var pozNawalute = lbxNaWalute.SelectedIndex;
            PozycjaTabeliA zWaluty = kursyAktualne[pozZWaluty];
            PozycjaTabeliA naWalute = kursyAktualne[pozNawalute];
            Payload payload = new Payload();
            payload.text1 = zWaluty.kod_waluty;
            payload.text2 = naWalute.kod_waluty;
            this.Frame.Navigate(typeof(Pomoc),payload);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await update();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
            {
                dane_użytkownik = (string)e.Parameter;
            }
        }
    }
}

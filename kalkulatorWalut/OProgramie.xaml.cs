
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace kalkulatorWalut
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OProgramie : Page
    {
        string strona;
        public OProgramie()
        {
            this.InitializeComponent();
            
        }

       

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(strona!= "https://static.nbp.pl/dane/kursy/xml/LastA.xml")
            {
                this.Frame.Navigate(typeof(MainPage),strona);
            }
            else
            {
                this.Frame.Navigate(typeof(MainPage));
            }
            
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
           adres.Text="Adres czytania danych kursowych: " +strona;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
            {
                strona= (string)e.Parameter;
            }
        }
        private async Task<string> InputTextDialogAsync(string title)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "Cancel";
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else
                return "";
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            strona = await InputTextDialogAsync("Podaj adres:");
        }

        private void adres_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}

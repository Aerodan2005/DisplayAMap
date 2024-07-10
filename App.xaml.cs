using System.Configuration;
using System.Data;
using System.Windows;

namespace DisplayAMap
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = "AAPTxy8BH1VEsoebNVZXo8HurIaDWYvvUyjPrVsCC9dvPfJZ0Xkkt8eLXgb5ypkvm5Uq6wbaoVW - tX_zxPooZcOcvu0Gvi8iq3ii6uvJYYaW0ez8JTBtBTKQySc_FdV9ISrNENPbYk76sII364NTsU66AmNA - mkjSlhU0loOCvJImkQaiz5KQXXnSZcZWXQxO1 - I - RBSfs5IkOdRX86DWqhhsMKo_kpw2YvPKoy8ua39 - Vs.AT1_rxWg8l2y";
        }
    }


}

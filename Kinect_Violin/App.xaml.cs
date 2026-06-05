using System.Windows;

namespace WpfKinectV2CustomButton
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow ob = new MainWindow();
        }
    }
}

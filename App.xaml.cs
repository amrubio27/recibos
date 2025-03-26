namespace recibos;

public partial class App : Application 
{
    public App() 
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
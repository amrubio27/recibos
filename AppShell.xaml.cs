
using recibos.features.Receipts.Presentation.Pages;

namespace recibos
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas para la navegación
            Routing.RegisterRoute(nameof(features.Receipts.Presentation.Pages.ReceiptDetailPage), 
                typeof(features.Receipts.Presentation.Pages.ReceiptDetailPage));
            
            Routing.RegisterRoute(nameof(features.Receipts.Presentation.Pages.NewReceiptPage), 
                typeof(features.Receipts.Presentation.Pages.NewReceiptPage));
        }
    }
}
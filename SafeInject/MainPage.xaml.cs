using System.Net;
using System.Net.Sockets;
using System.IO;

namespace SafeInject;

public partial class MainPage : ContentPage
{
    
    public MainPage()
    {
        try
        {
            InitializeComponent(); // Initialiserer UI-komponenterne (defineret i XAML)
            StartTcpListener(); // Starter TCP-serveren automatisk
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusLabel.Text = $"Fejl ({DateTime.Now:T}): {ex.Message}";
            });
            Console.WriteLine($"Fejl ({DateTime.Now:T}): {ex}");
            Console.WriteLine($"Fejl: {ex}");
        }
    }


    private async void StartTcpListener()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 5000); //lytter på alle ip-adresser lokalt på port 5000
        listener.Start(); // Starter serveren så den begynder at lytte

        StatusLabel.Text = "Lytter på port 5000..."; //så man ved når serveren er klar

        while (true) //venter bare hele tiden
        {
            try
            {
                TcpClient client = await listener.AcceptTcpClientAsync();// Venter (asynkront) på at en klient opretter forbindelse
                using NetworkStream stream = client.GetStream(); //når en klient forbinder, sendes en stream

                // === MODTAGELSE AF BILLEDE ===

                // Først læses de første 4 bytes, som angiver billedets størrelse i bytes
                byte[] lengthBuffer = new byte[4];
                await stream.ReadAsync(lengthBuffer, 0, 4);
                int imageSize = BitConverter.ToInt32(lengthBuffer, 0); // Konverterer 4 bytes til heltal

                // Allokerer en buffer til selve billedet ud fra størrelsen
                byte[] imageBuffer = new byte[imageSize];
                int totalRead = 0;

                while (totalRead < imageSize) //her læses hele billedet byte for byte(Læser billedet ind i bufferen)
                {
                    // Læser maksimalt så mange bytes som der mangler
                    int read = await stream.ReadAsync(imageBuffer, totalRead, imageSize - totalRead);
                    totalRead += read; // Opdaterer hvor mange bytes der er læst ind
                }
                // Når billedet er læst færdigt, opdateres brugerfladen
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Opdater billedet i UI (hovedtråden) vises i ReceivedImage (Viser billedet i UI ved at lave en stream ud fra byte-arrayet)
                    ReceivedImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBuffer));
                    // Viser status på tidspunkt
                    StatusLabel.Text = $"Billede modtaget kl. {DateTime.Now:T}";

                });
            }
            // Hvis noget går galt i modtagelsen af data
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Viser fejl i UI
                    StatusLabel.Text = $"Fejl ({DateTime.Now:T}): {ex.Message}";
                });
                // Logger fejlen til konsollen (debugging)
                Console.WriteLine($"Fejl ({DateTime.Now:T}): {ex}");
                Console.WriteLine($"Fejl: {ex}");
            }

        }
    }
    
    }


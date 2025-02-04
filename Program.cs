using System;
using Phidget22;
using Phidget22.Events;

class Program
{
    static void Main(string[] args)
    {
        RFID rfid = new RFID();

        //lecture badge
        rfid.Tag += (sender, e) =>
        {
            Console.WriteLine($"Tag détecté : {e.Tag}");
        };

        try
        {
            //ouvrir le leceur
            rfid.Open();
            Console.WriteLine("Lecteur RFID prêt. Placez un badge pour le tester...");
            Console.WriteLine("Appuyez sur Entrée pour quitter.");
            Console.ReadLine();
        }
        catch (PhidgetException ex)
        {
            Console.WriteLine($"Erreur de connexion au lecteur : {ex.Message}");
        }
        finally
        {
            rfid.Close();
            Console.WriteLine("Lecteur RFID déconnecté.");
        }
    }
}

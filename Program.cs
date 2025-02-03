using System;
using Phidget22;
using Phidget22.Events;

class Program
{
    static void Main(string[] args)
    {
        // Initialiser le lecteur RFID
        RFID rfid = new RFID();

        // Événement déclenché lorsque le tag est lu
        rfid.Tag += (sender, e) =>
        {
            Console.WriteLine($"Tag détecté : {e.Tag}");
        };

        // Gérer les erreurs et exceptions
        try
        {
            // Ouvrir la connexion au lecteur
            rfid.Open();
            Console.WriteLine("Lecteur RFID prêt. Placez un badge pour le tester...");
            Console.WriteLine("Appuyez sur Entrée pour quitter.");

            // Attendre que l'utilisateur appuie sur Entrée pour quitter
            Console.ReadLine();
        }
        catch (PhidgetException ex)
        {
            Console.WriteLine($"Erreur de connexion au lecteur : {ex.Message}");
        }
        finally
        {
            // Fermer proprement la connexion lorsque l'application se termine
            rfid.Close();
            Console.WriteLine("Lecteur RFID déconnecté.");
        }
    }
}

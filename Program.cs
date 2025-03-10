using System;
using Phidget22;
using Phidget22.Events;

class Program
{
    static void Main(string[] args)
    {
        RFID rfid = new RFID();
        bool autorize; 

        //lecture badge
        rfid.Tag += (sender, e) =>
        {
            //reconnaitre si un badge est autorisé
            Console.WriteLine($"Badge : {e.Tag}");
            if (e.Tag == "1000e14339" || e.Tag == "3800637172")//tag renseigné dans la bdd
            {
                Console.WriteLine("Bob tonnot");//nom renseigné dans la bdd
                autorize = true;
            }
            else {
                Console.Error.WriteLine("Badge non autorisé");
                autorize = false;
            }

            try
            {

                if (autorize == true)
                {
                    //ouvrir la porte, activer le contacte de la clanche
                    DigitalOutput output = new DigitalOutput();
                    output.Channel = 0;
                    output.Open(1000);
                    output.State = true;
                    Console.WriteLine("porte ouverte");
                    System.Threading.Thread.Sleep(5000);
                    output.State = false;
                    Console.WriteLine("clanche fermée");
                    
                    output.Close();
                }
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine("Erreur de lecture");
            }
        };



        try
        {
            //ouvrir le leceur
            rfid.Open();
            Console.WriteLine("Lecteur RFID prêt. Placez un badge.");
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

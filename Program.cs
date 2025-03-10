using System;
using Phidget22;
using Phidget22.Events;

class Program
{
    static void Main(string[] args)
    {
        RFID rfid = new RFID();
        bool autorize;
        DigitalOutput led = new DigitalOutput();        //on board led
        DigitalOutput clanche = new DigitalOutput();    //impultion clanche

        //lecture badge
        rfid.Tag += (sender, e) =>
        {
            //allumer on board led
            led.Channel = 2;
            led.Open(1000);
            led.State = true;

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

            led.State = false;

            try
            {

                if (autorize == true)
                {
                    //ouvrir la porte, activer le contacte de la clanche
                    
                    clanche.Channel = 1;
                    clanche.Open(1000);
                    clanche.State = true;
                    Console.WriteLine("porte ouverte");
                    System.Threading.Thread.Sleep(2000);
                    clanche.State = false;
                    Console.WriteLine("clanche fermée");
                    
                    clanche.Close();
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

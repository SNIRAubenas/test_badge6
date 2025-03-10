using System;
using System.Threading;
using Phidget22;
using Phidget22.Events;

class Program
{
    static Mutex lcdMutex = new Mutex();
    static Mutex clancheMutex = new Mutex();

    static LCD lcd = new LCD();
    static RFID rfid = new RFID();
    static DigitalOutput led = new DigitalOutput();       //on board led
    static DigitalOutput clanche = new DigitalOutput();   //impulsion clanche

    static void Main(string[] args)
    {
        bool autorize = false;

        //lecture badge
        rfid.Tag += (sender, e) =>
        {
            //allumer on board led
            led.Channel = 2;
            led.Open(1000);
            led.State = true;

            //gestion du lcd
            lcd.Open(1000);
            lcd.Clear();

            led.State = false;

            //reconnaitre si un badge est autorisé
            Console.WriteLine($"Badge : {e.Tag}");
            if (e.Tag == "1000e14339" || e.Tag == "3800637172")//tag renseigné dans la bdd
            {
                Console.WriteLine("Bob tonnot");//nom renseigné dans la bdd

                Thread lcdThread = new Thread(() => AfficherMessageLCD("Bob Tonnot ouverture"));
                lcdThread.Start();

                autorize = true;
            }
            else
            {
                Console.Error.WriteLine("Badge non autorisé");

                Thread lcdThread = new Thread(() => AfficherMessageLCD("Badge non autorise"));
                lcdThread.Start();

                autorize = false;
            }

            try
            {
                if (autorize == true)
                {
                    //ouvrir la porte, activer le contact de la clanche
                    Thread clancheThread = new Thread(OuvrirClanche);
                    clancheThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Erreur de lecture");
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

    static void AfficherMessageLCD(string message)
    {
        lcdMutex.WaitOne();

        lcd.Open(1000);
        lcd.Clear();
        lcd.WriteText(LCDFont.Dimensions_5x8, 0, 0, message);
        lcd.Flush();
        lcd.Backlight = 1.0;
        Thread.Sleep(3000);
        lcd.Clear();
        lcd.Close();

        lcdMutex.ReleaseMutex();
    }

    static void OuvrirClanche()
    {
        clancheMutex.WaitOne();

        clanche.Channel = 1;
        clanche.Open(1000);
        clanche.State = true;
        Console.WriteLine("porte ouverte");
        Thread.Sleep(2000);
        clanche.State = false;
        Console.WriteLine("clanche fermée");
        clanche.Close();

        clancheMutex.ReleaseMutex();
    }
}

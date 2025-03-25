using System;

using System.IO;
using System.Threading;
using Phidget22;
using Phidget22.Events;
using Serilog;

class Program
{
    static LCD lcd = new LCD();
    static RFID rfid = new RFID();
    static DigitalOutput led = new DigitalOutput();       // on-board led
    static DigitalOutput gache = new DigitalOutput();    // impulsion gâche
    static DigitalInput porte = new DigitalInput();      // aimant porte
    static DigitalOutput lumiere = new DigitalOutput(); //ampoule
    static VoltageInput proximitySensor = new VoltageInput();  // capteur de présence pour la lumière

    static void Main(string[] args)
    {
        Thread logPorteThread = new Thread(logPorte);
        logPorteThread.Start();

        Thread proximityThread = new Thread(DetectMotion); // Thread pour la détection de mouvement
        proximityThread.Start();

        bool autorize = false; //autorisation badge


        RFID rfid = new RFID();
        DigitalOutput clanche = new DigitalOutput();    // impulsion clanche
        var CheminLog = Path.Combine(AppContext.BaseDirectory, "../../logs.txt");
        var CheminLogs = Environment.ExpandEnvironmentVariables(CheminLog);

        // lecture badge
        rfid.Tag += (sender, e) =>
        {
            // allumer on-board led
            led.Channel = 2;
            led.Open(1000);
            led.State = true;

            // gestion du lcd
            lcd.Open(1000);
            lcd.Clear();

            led.State = false;

            // reconnaitre si un badge est autorisé
            Console.WriteLine($"Badge : {e.Tag}");
            if (e.Tag == "1000e14339" || e.Tag == "3800637172") // tag renseigné dans la bdd
            {
                Serilog.Log.Logger = new LoggerConfiguration()
                  .MinimumLevel.Debug()
                  .WriteTo.File(CheminLogs, rollingInterval: RollingInterval.Infinite)
                  .CreateLogger();
                Serilog.Log.Error("Bob tonnot");
                Console.WriteLine("Bob tonnot"); // nom renseigné dans la bdd

                Thread lcdThread = new Thread(() => AfficherMessageLCD("Bob Tonnot ouverture"));
                lcdThread.Start();

                autorize = true;

            }
            else
            {
                Serilog.Log.Logger = new LoggerConfiguration()
                  .MinimumLevel.Debug()
                  .WriteTo.File(CheminLogs, rollingInterval: RollingInterval.Infinite)
                  .CreateLogger();
                Serilog.Log.Error("Badge not authorized");
                Console.Error.WriteLine("Badge not authorized");

                Thread lcdThread = new Thread(() => AfficherMessageLCD("Badge non autorisé"));
                lcdThread.Start();

                autorize = false;
            }

            try
            {
                if (autorize == true)
                {
                    // ouvrir la porte, activer le contact de la gâche
                    Thread clancheThread = new Thread(OuvrirClanche);
                    clancheThread.Start();
                    Serilog.Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .WriteTo.File(CheminLogs, rollingInterval: RollingInterval.Infinite)
                   .CreateLogger();
                    clanche.Channel = 0;
                    clanche.Open(1000);
                    clanche.State = true;
                    Serilog.Log.Information("Gache ouverte");
                    System.Threading.Thread.Sleep(2000);
                    clanche.State = false;
                    Serilog.Log.Information("Gache Fermee");

                    Serilog.Log.CloseAndFlush();
                    clanche.Close();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erreur de lecture: {ex.Message}");
            }
        };

        try
        {
            // ouvrir le lecteur
            rfid.Open();
            Console.WriteLine("Lecteur RFID prêt. Placez un badge.");
            Console.WriteLine("Appuyez sur Entrée pour quitter.");
            Console.ReadLine();

            // lecture badge
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(CheminLogs, rollingInterval: RollingInterval.Infinite)
                .CreateLogger();
            Serilog.Log.Information("test");
            Serilog.Log.CloseAndFlush();
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
        lcd.Open(1000);
        lcd.Clear();
        lcd.WriteText(LCDFont.Dimensions_5x8, 0, 0, message);
        lcd.Flush();
        lcd.Backlight = 1.0;
        Thread.Sleep(1000);
        lcd.Close();
    }

    static void OuvrirClanche()
    {
        gache.Channel = 7;
        gache.Open(1000);
        gache.State = true;
        Console.WriteLine("Gache ouverte");
        Thread.Sleep(2000);
        gache.State = false;
        Console.WriteLine("Gache fermée");
        gache.Close();
    }

    static void logPorte()
    {
        porte.Channel = 3;
        porte.Open(0);

        try
        {
            if (!porte.Attached)
            {
                Console.WriteLine("Erreur : L'aimant n'est pas détecté.");
                return;
            }
            porte.StateChange += (sender, e) =>
            {
                Console.WriteLine(e.State ? "Porte fermée" : "Porte ouverte");
                Thread lcdThread = new Thread(() => AfficherMessageLCD(e.State ? "porte fermee" : "porte ouverte"));
                lcdThread.Start();
            };
        }
        catch (PhidgetException ex)
        {
            Console.Error.WriteLine($"Erreur aimant : {ex.Message}");
        }
    }


    //capture de mouvement pour la lumière
    static void DetectMotion()
    {
        proximitySensor.Channel = 0;
        proximitySensor.Open(1000);

        while (true)
        {
            double voltage = proximitySensor.Voltage;


            if (voltage > 3)
            {
                lumiere.Channel = 6;
                lumiere.Open(1000);
                lumiere.State = true;
                Thread.Sleep(60000);
                lumiere.State = false;
            }
        }
    }
}
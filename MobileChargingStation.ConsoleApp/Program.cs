using MobileChargingStation.Lib.Boundary;
using MobileChargingStation.Lib.Control;

namespace MobileChargingStation.ConsoleApp;

class Program
{
    static void Main(string[] args)
    {
        // Assemble your system here from all the classes
        var door = new Door();
        var rfidReader = new RfidReader();
        var usbCharger = new UsbChargerSimulator();
        var display = new Display();
        var log = new LogFile();

        var chargeControl = new ChargeControl(usbCharger, display);
        _ = new StationControl(door, rfidReader, chargeControl, display, log);

        bool overload = false;
        bool finish = false;
        do
        {
            Console.WriteLine(
                "Indtast E (afslut), O (åbn dør), C (luk dør), R (RFID), "
                    + "T (tilslut/frakobl telefon), K (kortslutning til/fra): "
            );
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
                continue;

            switch (char.ToUpper(input[0]))
            {
                case 'E':
                    finish = true;
                    break;

                case 'O':
                    door.OnDoorOpen();
                    break;

                case 'C':
                    door.OnDoorClose();
                    break;

                case 'R':
                    Console.WriteLine("Indtast RFID id: ");
                    string? idString = Console.ReadLine();

                    if (int.TryParse(idString, out int id))
                        rfidReader.OnRfidRead(id);
                    else
                        Console.WriteLine("Ugyldigt RFID id - skal være et heltal");
                    break;

                case 'T':
                    usbCharger.SimulateConnected(!usbCharger.Connected);
                    Console.WriteLine(
                        usbCharger.Connected ? "Telefon tilsluttet" : "Telefon frakoblet"
                    );
                    break;

                case 'K':
                    overload = !overload;
                    usbCharger.SimulateOverload(overload);
                    Console.WriteLine(overload ? "Kortslutning simuleret" : "Kortslutning fjernet");
                    break;

                default:
                    break;
            }
        } while (!finish);
    }
}

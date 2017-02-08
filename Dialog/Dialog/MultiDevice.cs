using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Speech.Synthesis;

namespace Dialog
{
    class MultiDevice : Dialog
    {
        String TurnGram;
        String ActionGram;
        SpeechSynthesizer SS = new SpeechSynthesizer();  //tworzenie obiektu do syntezy mowy

        public MultiDevice()
        {
            TurnGram = System.IO.File.ReadAllText("turn.txt"); //gramatyka do wlaczania urzadzenia
            ActionGram = System.IO.File.ReadAllText("action.txt"); //gramatyka do obslugi urzadzenia
        }
        public override string ToString()
        {
            return "Urządzenie wielofunkcyjne";
        }

        public override void Run(Action<string> print)
        {
            // json dynamic object example  :
            //dynamic stuff = JsonConvert.DeserializeObject("{ 'Name': 'Jon Smith', 'Address': { 'City': 'New York', 'State': 'NY' }, 'Age': 42 }");
            //
            //string name = stuff.Name;
            //string address = stuff.Address.City;

            print("URZĄDZENIE WIELOFUNKCYJNE");
            print("Urządzenie wielofunkcyjne jest wyłączone.");
            SS.SpeakAsync("The multifunctional device is turned off. Waiting for commands."); //synteza mowy
            print("PODAJ KOMENDĘ, ABY WŁĄCZYĆ.");

            while (true)
            {
                activeSession = new SarmataSession(host, new Dictionary<string, string>{
                        { "grammar", TurnGram },
                        { "complete-timeout", "1000" },
                        { "incomplete-timeout", "3000" },
                        { "no-input-timeout", "10000" },
                        { "no-rec-timeout", "10000" },
                    });

                var response = activeSession.WaitForResponse();
                print(status2string(response));
                response = activeSession.WaitForResponse();
                //print(status2string(response));

                dynamic command = JsonConvert.DeserializeObject(
                   response.Results[0].SemanticInterpretation);

                int state = 0;
                if (command.turnon == "włącz")
                {
                    state = 1;
                }

                while (state == 1)
                {
                    print("Urządzenie wielofunkcyjne jest włączone.");
                    SS.SpeakAsync("The multifunctional device is turned on. Waiting for commands.");

                    activeSession = new SarmataSession(host, new Dictionary<string, string>{
                        { "grammar", ActionGram },
                        { "complete-timeout", "1000" },
                        { "incomplete-timeout", "3000" },
                        { "no-input-timeout", "10000" },
                        { "no-rec-timeout", "10000" },
                    });

                    var response2 = activeSession.WaitForResponse();
                    print(status2string(response2));
                    response2 = activeSession.WaitForResponse();
                    //print(status2string(response2));

                    dynamic command2 = JsonConvert.DeserializeObject(
                       response2.Results[0].SemanticInterpretation);

                    if (command2.action == "drukuj")
                    {
                        print("Proszę czekać. Drukuję " + command2.howmany + " " + command2.page + ".");
                        SS.SpeakAsync("Printing. Please wait.");
                        Thread.Sleep(3000); //wstrzymanie wątku na 3s celem symulacji wykonywania akcji
                        print("Zakończono drukowanie. Co chcesz zrobić dalej?");
                        SS.SpeakAsync("Finished printing.");
                    }
                    if (command2.action == "skanuj")
                    {
                        print("Proszę czekać. Skanuję " + command2.howmany + " " + command2.page + ".");
                        SS.SpeakAsync("Scanning. Please wait.");
                        Thread.Sleep(3000);
                        print("Zakończono skanowanie. Co chcesz zrobić dalej?");
                        SS.SpeakAsync("Finished scanning.");
                    }
                    if (command2.action == "kseruj")
                    {
                        print("Proszę czekać. Kseruję " + command2.howmany + " " + command2.page + ".");
                        SS.SpeakAsync("Copying. Please wait.");
                        Thread.Sleep(3000);
                        print("Zakończono kserowanie. Co chcesz zrobić dalej?");
                        SS.SpeakAsync("Finished copying.");
                    }
                    if (command2.turnoff == "wyłącz")
                    {
                        print("Wyłączam urządzenie wielofunkcyjne.");
                        SS.SpeakAsync("The multifunctional device is being turned off. Please wait.");
                        Thread.Sleep(5000);
                        state = 0;
                    }
                }
                print("URZĄDZENIE WIELOFUNKCYJNE");
                print("Urządzenie wielofunkcyjne jest wyłączone.");
                SS.SpeakAsync("The multifunctional device is turned off. Waiting for commands.");
                print("PODAJ KOMENDĘ, ABY WŁĄCZYĆ.");
            }
        }
    }
}

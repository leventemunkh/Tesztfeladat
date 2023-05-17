using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Tesztfeladat
{
    public struct Kiadas
    {
        public DateTime Datum;
        public int Osszeg;
        public string Megjegyzes;
    }

     public class KiadasManager
    {
        private List<Kiadas> _kiadasok;
        private const string FileName = "kiadasok.txt";

        public KiadasManager()
        {
            _kiadasok = new List<Kiadas>();
            LoadData();
        }

        public void UjKiadas(Kiadas kiadas) 
        { 
            _kiadasok.Add(kiadas);
            SaveData();

        }

        public List<Kiadas> UtolsoTiz()
        {
            return _kiadasok.OrderByDescending(x=> x.Datum).Take(10).ToList();
        }
        public void KiadasTorles(int index) 
        {
            _kiadasok.RemoveAt(index);
            SaveData();
        }

        public Dictionary<string, int> HaviStatisztika()
        {
            return _kiadasok.GroupBy(x=> x.Datum.ToString("yyyy.MM"))
                .ToDictionary(g=>g.Key, g=>g.Sum(x=>x.Osszeg));
        }

        public void LoadData()
        {
            if(File.Exists(FileName)) 
            {
                _kiadasok = File.ReadAllLines(FileName)
                    .Select(line =>
                    {
                        var parts = line.Split(';');
                        return new Kiadas
                        {
                            Datum = DateTime.ParseExact(parts[0], "yyyy.MM.dd", CultureInfo.InvariantCulture),
                            Osszeg = int.Parse(parts[1]),
                            Megjegyzes = parts[2]
                        };
                    }).ToList();

                    
            }
        }

        public void SaveData() 
        {
            File.WriteAllLines(FileName, _kiadasok.Select(x => $"{x.Datum:yyyy.MM.dd};{x.Osszeg};{x.Megjegyzes}"));
        }
    }

    class Program
    {
        static void Main(string[]args)
        {
            KiadasManager kiadasManager = new KiadasManager();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. Új kiadás felvétele ");
                Console.WriteLine("2. Utolsó 10 kiadás ");
                Console.WriteLine("3. Havi statisztika ");
                Console.WriteLine("4. Állapot mentése ");
                Console.WriteLine("5. Kilépés");

                int kivalaszt=int.Parse(Console.ReadLine());

                switch (kivalaszt)
                {
                    case 1:
                       UjKiadas(kiadasManager); break;

                    case 2:
                        Kilistazas(kiadasManager); break;

                    case 3:
                        HaviStatisztika(kiadasManager); break;

                    case 4:
                        kiadasManager.SaveData();
                        Console.WriteLine("Állapot mentve!");
                        Console.ReadKey();
                        break;

                    case 5:
                        return;

                    default:
                        Console.WriteLine("Érvénytelen választás. Próbáld újra");
                        Console.ReadKey();
                        break;                  
                }
            }
        }

        private static void UjKiadas(KiadasManager kiadasManager)
        {
            Console.Clear();
            Console.Write("Dátum (YYYY.MM.DD formátumban): ");
            DateTime date;
            while (!DateTime.TryParseExact(Console.ReadLine(), "yyyy.MM.dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                Console.Write("Érvénytelen dátum. Próbáld újra: ");
            }

            Console.Write("Összeg: ");
            int amount;
            while (!int.TryParse(Console.ReadLine(), out amount) || amount <= 0)
            {
                Console.Write("Érvénytelen összeg. Próbáld újra: ");
            }

            Console.Write("Megjegyzés: ");
            string note = Console.ReadLine();

            kiadasManager.UjKiadas(new Kiadas { Datum = date, Osszeg = amount, Megjegyzes = note });

            Console.WriteLine("Kiadás hozzáadva.");
            Console.ReadKey();
        }


        private static void Kilistazas(KiadasManager kiadasManager)
        {
            
            Console.Clear();
            var expenses = kiadasManager.UtolsoTiz();
            for (int i = 0; i < expenses.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {expenses[i].Datum:yyyy.MM.dd} - {expenses[i].Osszeg} - {expenses[i].Megjegyzes}");
            }

            Console.Write("Törlendő kiadás sorszáma (0 - kilépés): ");
            int index;
            if (int.TryParse(Console.ReadLine(), out index) && index > 0 && index <= expenses.Count)
            {
                kiadasManager.KiadasTorles(index - 1);
                Console.WriteLine("Kiadás törölve.");
            }

            Console.ReadKey();
        }

        private static void HaviStatisztika(KiadasManager kiadasManager)
        {
           
            Console.Clear();
            var stats = kiadasManager.HaviStatisztika();
            foreach (var stat in stats)
            {
                Console.WriteLine($"{stat.Key}: {stat.Value}");
            }

            if (stats.Any())
            {
                var minMonth = stats.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
                var maxMonth = stats.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                Console.WriteLine($"Legkisebb kiadású hónap: {minMonth}");
                Console.WriteLine($"Legnagyobb kiadású hónap: {maxMonth}");
            }
            else
            {
                Console.WriteLine("Nincs adat a statisztikához.");
            }

            Console.ReadKey();
        }

    }
}

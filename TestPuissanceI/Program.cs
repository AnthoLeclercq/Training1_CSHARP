using System.Data.SQLite;
using System.Text;

namespace ConsoleExercises
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== MENU PRINCIPAL ===");
                Console.WriteLine("1. Exercice 1 - SQL en mémoire");
                Console.WriteLine("2. Exercice 2 - Premier trou dans une suite");
                Console.WriteLine("3. Exercice 3 - Fusion de plages horaires");
                Console.WriteLine("4. Exercice 4 - Chiffrement César (+3)");
                Console.WriteLine("0. Quitter");
                Console.Write("\nVotre choix : ");

                string choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1":
                        Exercise1();
                        break;
                    case "2":
                        Exercise2();
                        break;
                    case "3":
                        Exercise3();
                        break;
                    case "4":
                        Exercise4();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Choix invalide !");
                        break;
                }

                Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
                Console.ReadKey();
            }
        }

        // -----------------------
        // Exercice 1 : SQL en mémoire
        // -----------------------
        static void Exercise1()
        {
            using var conn = new SQLiteConnection("Data Source=:memory:");
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                CREATE TABLE Clients (
                    Code INTEGER PRIMARY KEY,
                    Nom TEXT,
                    CodePostal TEXT,
                    Ville TEXT
                );
                CREATE TABLE Historique (
                    DateLiv TEXT,
                    CodeClient INTEGER,
                    NbColis INTEGER,
                    Temperature REAL,
                    Precipitations REAL
                );";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
                INSERT INTO Clients VALUES
                (10162, 'CIT HENNEBONT - CC DE KERLIVIO', '56700', 'HENNEBONT'),
                (14792, 'AUTRE CLIENT', '56000', 'VANNES'),
                (10345, 'CLIENT TEST', '56100', 'LORIENT'),
                (10456, 'CLIENT FOURNI', '56200', 'AURAY');

                INSERT INTO Historique VALUES
                ('150316', 14792, 59, 7.0, 0.125),
                ('160316', 14792, 20, 4.0, 0.0),
                ('150316', 10162, 10, 6.5, 1.2),
                ('170316', 10162, 15, 8.0, 0.0),
                ('180316', 10345, 5, 5.0, 0.5),
                ('190316', 10345, 12, 3.0, 0.0),
                ('010116', 10456, 25, 10.0, 0.3),
                ('020116', 10456, 30, 12.0, 0.0),
                ('150416', 10162, 7, 9.0, 0.2),
                ('160416', 14792, 13, 5.5, 0.1),
                ('170416', 10345, 20, 6.0, 0.0),
                ('180416', 10456, 18, 8.0, 0.6);";
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("\n--- Nombre de colis par client par année ---");
            using (var cmd1 = conn.CreateCommand())
            {
                cmd1.CommandText = @"
                SELECT c.Nom AS NomClient,
                    '20' || SUBSTR(h.DateLiv,1,2) AS Annee,
                    SUM(h.NbColis) AS NbColisTotal
                FROM Historique h
                JOIN Clients c ON c.Code = h.CodeClient
                GROUP BY c.Nom, Annee
                ORDER BY c.Nom, Annee;";

                using var reader1 = cmd1.ExecuteReader();
                while (reader1.Read())
                {
                    Console.WriteLine($"{reader1["NomClient"]} | {reader1["Annee"]} | {reader1["NbColisTotal"]}");
                }
            }

            Console.WriteLine("\n--- Moyennes température / précipitations (Temp>=5, Prec>0) ---");
            using (var cmd2 = conn.CreateCommand())
            {
                cmd2.CommandText = @"
                SELECT '20' || SUBSTR(DateLiv,1,2) AS Annee,
                    SUBSTR(DateLiv,3,2) AS Mois,
                    SUBSTR(DateLiv,5,2) AS Jour,
                    AVG(Temperature) AS MoyenneTemperature,
                    AVG(Precipitations) AS MoyennePrecipitations
                FROM Historique
                WHERE Temperature >= 5 AND Precipitations > 0
                GROUP BY Annee, Mois, Jour
                ORDER BY Annee, Mois, Jour;";

                using var reader2 = cmd2.ExecuteReader();
                while (reader2.Read())
                {
                    Console.WriteLine($"{reader2["Annee"]}-{reader2["Mois"]}-{reader2["Jour"]} | Temp: {reader2["MoyenneTemperature"]:0.##} | Prec: {reader2["MoyennePrecipitations"]:0.###}");
                }
            }
        }

        // -----------------------
        // Exercice 2 : Premier trou
        // -----------------------
        static void Exercise2()
        {
            Console.WriteLine("=== Exercice 2 : Premier trou dans une suite ===\n");
            Console.Write("Entrez une liste de nombres séparés par des virgules : ");
            string input = Console.ReadLine();
            var numbers = input.Split(',')
                               .Select(s => int.TryParse(s.Trim(), out int n) ? n : 0)
                               .ToList();
            Console.WriteLine($"Premier trou strictement supérieur à 0 : {FindFirstMissingPositive(numbers)}");
        }

        static int FindFirstMissingPositive(List<int> numbers)
        {
            var set = new HashSet<int>(numbers.Where(n => n > 0));
            int i = 1;
            while (true)
            {
                if (!set.Contains(i)) return i;
                i++;
            }
        }

        // -----------------------
        // Exercice 3 : Fusion de plages horaires
        // -----------------------
        static void Exercise3()
        {
            Console.WriteLine("=== Exercice 3 : Fusion de plages horaires ===\n");
            Console.WriteLine("Saisissez des plages au format : Jour HH:mm-HH:mm");
            Console.WriteLine("Exemple : Lundi 08:00-09:30");
            Console.WriteLine("Tapez 'fin' pour terminer.\n");

            var timeSlots = new List<TimeSlot>();
            var validDays = new HashSet<string> { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" };

            while (true)
            {
                Console.Write("Plage : ");
                string input = Console.ReadLine();
                if (input.Trim().ToLower() == "fin")
                    break;

                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    Console.WriteLine("Format invalide !");
                    continue;
                }

                string day = parts[0];
                if (!validDays.Contains(day))
                {
                    Console.WriteLine("Jour invalide !");
                    continue;
                }

                string[] hours = parts[1].Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (hours.Length != 2)
                {
                    Console.WriteLine("Format invalide !");
                    continue;
                }

                if (!TimeSpan.TryParse(hours[0], out TimeSpan start) || !TimeSpan.TryParse(hours[1], out TimeSpan end))
                {
                    Console.WriteLine("Heure invalide !");
                    continue;
                }

                if (start > end)
                {
                    Console.WriteLine("Plage invalide : début après fin.");
                    continue;
                }

                timeSlots.Add(new TimeSlot(day, start, end));
            }

            if (timeSlots.Count == 0)
            {
                Console.WriteLine("\nAucune plage saisie !");
                return;
            }

            var mergedByDay = MergeTimeSlotsByDay(timeSlots);
            Console.WriteLine("\n=== Résultat ===\n");
            foreach (var day in mergedByDay.Keys)
            {
                Console.WriteLine($"{day} :");
                int index = 1;
                foreach (var slot in mergedByDay[day])
                {
                    Console.WriteLine($"   Plage {index} : {slot.Start:hh\\:mm} - {slot.End:hh\\:mm}");
                    index++;
                }
                Console.WriteLine();
            }
        }

        static Dictionary<string, List<TimeSlot>> MergeTimeSlotsByDay(List<TimeSlot> slots) =>
            slots.GroupBy(s => s.Day)
                 .ToDictionary(g => g.Key, g => MergeTimeSlots(g.OrderBy(s => s.Start).ToList()));

        static List<TimeSlot> MergeTimeSlots(List<TimeSlot> slots)
        {
            var merged = new List<TimeSlot>();
            TimeSlot current = slots[0];

            for (int i = 1; i < slots.Count; i++)
            {
                var next = slots[i];
                if (next.Start <= current.End)
                    current.End = next.End > current.End ? next.End : current.End;
                else
                {
                    merged.Add(current);
                    current = next;
                }
            }

            merged.Add(current);
            return merged;
        }

        class TimeSlot
        {
            public string Day { get; set; }
            public TimeSpan Start { get; set; }
            public TimeSpan End { get; set; }

            public TimeSlot(string day, TimeSpan start, TimeSpan end)
            {
                Day = day;
                Start = start;
                End = end;
            }
        }

        // -----------------------
        // Exercice 4 : Chiffrement César
        // -----------------------
        static void Exercise4()
        {
            Console.WriteLine("=== Exercice 4 : Chiffrement César (+3) ===\n");
            Console.Write("Entrez un texte : ");
            string text = Console.ReadLine();
            const int shift = 3;
            string encrypted = CaesarEncrypt(text, shift);
            string decrypted = CaesarDecrypt(encrypted, shift);

            Console.WriteLine($"\nTexte original : {text}");
            Console.WriteLine($"Crypté (+{shift}) : {encrypted}");
            Console.WriteLine($"Décrypté : {decrypted}");
        }

        static string CaesarEncrypt(string text, int shift)
        {
            var sb = new StringBuilder(text.Length);
            foreach (char c in text)
                sb.Append(ShiftChar(c, shift));
            return sb.ToString();
        }

        static string CaesarDecrypt(string text, int shift)
        {
            var sb = new StringBuilder(text.Length);
            foreach (char c in text)
                sb.Append(ShiftChar(c, -shift));
            return sb.ToString();
        }

        static char ShiftChar(char c, int shift)
        {
            if (!char.IsLetter(c))
                return c;
            char baseChar = char.IsUpper(c) ? 'A' : 'a';
            return (char)(((c - baseChar + shift + 26) % 26) + baseChar);
        }
    }
}

using Microsoft.Data.Sqlite;

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
                Console.WriteLine("4. Exercice 4 - Code César (+3)");
                Console.WriteLine("0. Quitter");
                Console.Write("\nVotre choix : ");

                string choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1":
                        Exercise1_SQL();
                        break;
                    case "2":
                        Exercise2();
                        break;
                    case "3":
                        Exercise3();
                        break;
                    case "4":
                        Exercise4_Caesar();
                        break;
                    case "0":
                        Console.WriteLine("Au revoir !");
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
        static void Exercise1_SQL()
        {
            Console.WriteLine("=== Exercice 1 : SQL en mémoire ===");

            using var conn = new SqliteConnection("Data Source=:memory:");
            conn.Open();
            using var cmd = conn.CreateCommand();

            // Création des tables
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

            // Insertion des données
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

            // Requête 1 : Nombre de colis par client par année
            Console.WriteLine("\n--- Nombre de colis par client par année ---");
            cmd.CommandText = @"
            SELECT c.Nom AS NomClient,
                '20' || SUBSTR(h.DateLiv,1,2) AS Annee,
                SUM(h.NbColis) AS NbColisTotal
            FROM Historique h
            JOIN Clients c ON c.Code = h.CodeClient
            GROUP BY c.Nom, Annee
            ORDER BY c.Nom, Annee;";

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["NomClient"]} | {reader["Annee"]} | {reader["NbColisTotal"]}");
                }
            } // <- reader fermé automatiquement ici

            // Requête 2 : Moyennes température / précipitations
            Console.WriteLine("\n--- Moyennes température / précipitations (Temp>=5, Prec>0) ---");
            cmd.CommandText = @"
            SELECT '20' || SUBSTR(DateLiv,1,2) AS Annee,
                SUBSTR(DateLiv,3,2) AS Mois,
                AVG(Temperature) AS MoyenneTemperature,
                AVG(Precipitations) AS MoyennePrecipitations
            FROM Historique
            WHERE Temperature >= 5
            AND Precipitations > 0
            GROUP BY Annee, Mois
            ORDER BY Annee, Mois;";

            using (var reader2 = cmd.ExecuteReader())
            {
                while (reader2.Read())
                {
                    Console.WriteLine($"{reader2["Annee"]}-{reader2["Mois"]} | Temp: {reader2["MoyenneTemperature"]:0.##} | Prec: {reader2["MoyennePrecipitations"]:0.###}");
                }
            }
        }

        // -----------------------
        // Exercice 2 : Premier trou dans une suite
        // -----------------------
        static void Exercise2()
        {
            Console.WriteLine("=== Exercice 2 : Premier trou dans une suite ===\n");

            Console.Write("Entrez une liste de nombres séparés par des virgules : ");
            string input = Console.ReadLine();

            var numbers = input.Split(',')
                               .Select(s => int.TryParse(s.Trim(), out int n) ? n : 0)
                               .ToList();

            int result = FindFirstMissingPositive(numbers);
            Console.WriteLine($"Premier trou strictement supérieur à 0 : {result}");
        }

        static int FindFirstMissingPositive(List<int> numbers)
        {
            var positives = numbers.Where(n => n > 0).Distinct().OrderBy(n => n).ToList();
            int expected = 1;

            foreach (int n in positives)
            {
                if (n != expected)
                    return expected;
                expected++;
            }
            return expected;
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

            while (true)
            {
                Console.Write("Plage : ");
                string input = Console.ReadLine();
                if (input.Trim().ToLower() == "fin") break;

                try
                {
                    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) throw new Exception();

                    string day = parts[0];
                    var hours = parts[1].Split('-', StringSplitOptions.RemoveEmptyEntries);

                    if (hours.Length != 2) throw new Exception();

                    string start = hours[0].Contains(":") ? hours[0] : hours[0].Insert(2, ":");
                    string end = hours[1].Contains(":") ? hours[1] : hours[1].Insert(2, ":");

                    timeSlots.Add(new TimeSlot(day, start, end));
                }
                catch
                {
                    Console.WriteLine("Format invalide ! Exemple attendu : Mardi 08:00-09:00");
                }
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
                var mergedList = mergedByDay[day];
                int index = 1;

                foreach (var slot in mergedList)
                {
                    Console.WriteLine($"   Plage {index} : {slot.Start:hh\\:mm} - {slot.End:hh\\:mm}");
                    index++;
                }

                Console.WriteLine();
            }
        }

        static Dictionary<string, List<TimeSlot>> MergeTimeSlotsByDay(List<TimeSlot> slots)
        {
            var result = new Dictionary<string, List<TimeSlot>>();

            foreach (var dayGroup in slots.GroupBy(s => s.Day))
            {
                var merged = MergeTimeSlots(dayGroup.ToList());
                result[dayGroup.Key] = merged;
            }

            return result;
        }

        static List<TimeSlot> MergeTimeSlots(List<TimeSlot> slots)
        {
            var sorted = slots.OrderBy(s => s.Start).ToList();
            var merged = new List<TimeSlot>();

            TimeSlot current = sorted[0];

            for (int i = 1; i < sorted.Count; i++)
            {
                var next = sorted[i];

                if (next.Start <= current.End)
                {
                    current.End = (next.End > current.End) ? next.End : current.End;
                }
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

            public TimeSlot(string day, string start, string end)
            {
                Day = day;
                Start = TimeSpan.Parse(start);
                End = TimeSpan.Parse(end);
            }
        }

        // -----------------------
        // Exercice 4 : Code César (+3)
        // -----------------------
        static void Exercise4_Caesar()
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
            return new string(text.Select(c => ShiftCharacter(c, shift)).ToArray());
        }

        static string CaesarDecrypt(string text, int shift)
        {
            return new string(text.Select(c => ShiftCharacter(c, -shift)).ToArray());
        }

        static char ShiftCharacter(char c, int shift)
        {
            if (!char.IsLetter(c))
                return c;

            char baseChar = char.IsUpper(c) ? 'A' : 'a';
            return (char)((((c - baseChar) + shift + 26) % 26) + baseChar);
        }
    }
}

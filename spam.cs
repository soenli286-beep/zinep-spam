using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics; // Уақытты өлшеу үшін

class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== AI СПАМ-Детектор + Аналитика ===");
        Console.ResetColor();

        Console.Write("\nМәтінді енгізіңіз: ");
        string userText = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userText)) return;

        string apiKey = ""; 

        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        // ЖИ-ге тапсырманы күшейту (енді тоналдықты да сұраймыз)
        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { 
                    role = "user", 
                    content = "Сен спам сарапшысысың. Мәтінді талдап, жауапты мына үлгіде бер:\n" +
                              "Нәтиже: (СПАМ немесе СПАМ ЕМЕС)\n" +
                              "Тоналдық: (Жағымды, Жағымсыз немесе Бейтарап)\n" +
                              "Себеп: (қысқаша)" +
                              "\n\nМәтін: " + userText 
                }
            },
            temperature = 0.7 // Жауаптың шығармашылық деңгейі
        };

        Stopwatch timer = Stopwatch.StartNew(); // Таймерді қосу
        
        try 
        {
            string json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            string responseText = await response.Content.ReadAsStringAsync();

            timer.Stop(); // Таймерді тоқтату

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("API қатесі: " + responseText);
                return;
            }

            using JsonDocument doc = JsonDocument.Parse(responseText);
            string answer = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            // Нәтижені безендіріп шығару
            Console.WriteLine("\n----------------------------");
            
            // Егер жауапта СПАМ сөзі болса, қызылмен жазамыз
            if (answer.Contains("СПАМ") && !answer.Contains("СПАМ ЕМЕС"))
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine(answer);
            Console.ResetColor();
            
            Console.WriteLine($"\nТалдау уақыты: {timer.ElapsedMilliseconds} мс");
            Console.WriteLine("----------------------------");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Қате: {ex.Message}");
        }
    }
}
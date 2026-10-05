# FinoBank - Secure Web Banking Application

## Panel Użytkownika
* **Rachunki:** Przegląd salda, historii transakcji i obsługa wielu walut (PLN, USD, EUR, GBP).
* **Przelewy:** Wykonywanie przelewów wewnętrznych i zewnętrznych z walidacją danych.
* **Książka Odbiorców:** Zapisywanie danych zaufanych odbiorców i szybkie wybieranie ich podczas wykonywania przelewu.
* **Kantor Walutowy:** Wymiana walut po aktualnych kursach pobieranych z API NBP (Narodowy Bank Polski).
* **Karty Płatnicze:** Zamawianie wirtualnych kart, blokowanie/odblokowywanie, zmiana limitów.
* **Analizy:** Wizualizacja wydatków za pomocą wykresów (Recharts).
* **Potwierdzenia PDF:** Generowanie potwierdzeń transakcji w czasie rzeczywistym.

## Panel Administratora
* **Zarządzanie użytkownikami:** Blokowanie/odblokowywanie kont.
* **Podgląd logów audytowych (Security Logs):** Śledzenie podejrzanych aktywności.
* **Symulacja:** Generowanie danych testowych (Seeder transakcji, obsługa JSON) i symulacja wpłatomatu (obsługa JSON).

## Zastosowane Środki Bezpieczeństwa
Projekt realizuje wybrane wymogi standardów bankowych (w tym elementy PSD2 i OWASP ASVS):

1. **Szyfrowanie Danych (Encryption at Rest):** Dane wrażliwe (PESEL, Adres) są szyfrowane w bazie algorytmem AES-256 (CBC Mode).
2. **Uwierzytelnianie 2FA:** Logowanie dwuskładnikowe przy użyciu hasła oraz kodów TOTP (Google Authenticator).
3. **Bezpieczna Sesja:** Tokeny JWT (Refresh Tokens) przechowywane w ciasteczkach HttpOnly, Secure, SameSite, co chroni przed atakami XSS i kradzieżą sesji.
4. **Transakcyjność (ACID):** Wykorzystanie BeginTransactionAsync i ExecutionStrategy w EF Core gwarantuje atomowość i spójność operacji finansowych.
5. **Ochrona przed atakami:**
    * **Rate Limiting:** Ochrona przed Brute Force i DDoS na endpointach logowania i przelewów.
    * **HSTS & CSP:** Nagłówki wymuszające bezpieczne połączenie i ograniczające źródła skryptów.
    * **Optymistyczna kontrola współbieżności:** Zapobieganie nadpisaniu salda przy równoległych żądaniach (RowVersion).
6. **Zabezpieczenie haseł:** Haszowanie algorytmem BCrypt (z solą).
7. **Logi Audytowe (Audit Trails):** Rejestrowanie zdarzeń krytycznych (logowanie, zmiany danych) w celu zapewnienia rozliczalności (Accountability).
8. **Walidacja Danych:** Ścisła weryfikacja danych wejściowych po stronie serwera (RegEx, Data Annotations) chroniąca przed błędami logicznymi i wstrzykiwaniem kodu.
9. **Ochrona przed wyciekiem informacji:** Globalny Middleware ukrywający szczegóły techniczne błędów przed użytkownikiem końcowym.
10. **Bezpieczeństwo Transportu:** Wymuszenie protokołu TLS 1.3 na poziomie konfiguracji serwera aplikacji.

## Stack Technologiczny

### Backend (.NET 8 Web API)
* **Język:** C#
* **Baza danych:** MySQL (Entity Framework Core 8)
* **Autoryzacja:** JWT (Access Token + Refresh Token w Cookie)
* **Biblioteki:** QuestPDF (generowanie PDF), OtpNet (2FA), BCrypt.Net

### Frontend (React.js)
* **Framework:** React 18 (Vite)
* **Styling:** Tailwind CSS
* **Komunikacja:** Axios (z interceptorami do Silent Refresh)
* **Wykresy:** Recharts
* **Routing:** React Router DOM (z chronionymi trasami)

## Instalacja i Uruchomienie

### Wymagania wstępne
* .NET SDK 8.0
* Node.js (v18+)
* MySQL Server

### 1. Konfiguracja Backend

* **Przejdź do katalogu API:**
cd backend/FinoBankApi

* **Skonfiguruj User Secrets (aby nie trzymać kluczy w kodzie).** 
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "<your-secret-key-min-32-chars>"
dotnet user-secrets set "EncryptionSettings:Key" "<your-encryption-key-32-chars>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=finobank;User=root;Password=<your-password>;"

* **Zaaplikuj migracje bazy danych:**
dotnet ef database update

* **Uruchom API:**
dotnet run

### 2. Konfiguracja Frontend
* **Przejdź do katalogu klienta:**
cd frontend

* **Zainstaluj zależności:**
npm install

* **Uruchom aplikację:**
npm run dev
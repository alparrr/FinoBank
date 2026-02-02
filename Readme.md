
## Funkcjonalności
1. Uwierzytelnianie (login/register) z 2FA
2. Zarządzanie rachunkami bankowymi (multi-currency)
3. Przelewy międzybankowe z audytem
4. Kantor walutowy (kursy NBP)
5. Karty płatnicze (limity, blokada)
6. Statystyki wydatków (wykresy)
7. Panel administracyjny

## Bezpieczeństwo (wątek badawczy)
- Strong Customer Authentication (PSD2)
- JWT tokens + auto-refresh
- 2FA (TOTP) - Google Authenticator
- Rate limiting (login, transfers)
- HSTS, CSP, X-Frame-Options
- Encryption (AES-256 dla PESEL, adresów)
- BCrypt dla haseł (cost factor 10)
- HTTPS/TLS 1.3

## Instalacja

### Backend:
```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend:
```bash
cd frontend
npm install
npm run dev
```

## Zmienne środowiskowe
(lista secretów w .env)

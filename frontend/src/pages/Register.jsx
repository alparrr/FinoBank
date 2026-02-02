import { useState } from "react";
import useAuth from "../context/AuthContext";
import axiosClient from "../api/axios";
import { useNavigate, Link } from "react-router-dom";
import toast from 'react-hot-toast';

const Register = () => {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [smsSent, setSmsSent] = useState(false);

  const [formData, setFormData] = useState({
    firstName: "", lastName: "", email: "", password: "",
    pesel: "", phoneNumber: "", smsCode: "",
    street: "", city: "", zipCode: ""
  });

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const validateForm = () => {
    if (!/^\d{11}$/.test(formData.pesel)) {
      return "PESEL musi składać się z dokładnie 11 cyfr";
    }

    if (!/^\d{9}$/.test(formData.phoneNumber)) {
      return "Numer telefonu musi składać się z dokładnie 9 cyfr";
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      return "Nieprawidłowy format adresu email";
    }

    if (!/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/.test(formData.password)) {
      return "Hasło musi zawierać minimum 8 znaków, w tym dużą literę, małą literę, cyfrę i znak specjalny (@$!%*?&)";
    }

    if (!/^\d{2}-\d{3}$/.test(formData.zipCode)) {
      return "Kod pocztowy musi być w formacie XX-XXX (np. 00-950)";
    }

    if (!/^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ \-]+$/.test(formData.firstName)) {
      return "Imię może zawierać tylko litery";
    }

    if (!/^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ \-]+$/.test(formData.lastName)) {
      return "Nazwisko może zawierać tylko litery";
    }

    if (!/^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ \-]+$/.test(formData.city)) {
      return "Miasto może zawierać tylko litery";
    }

    return null;
  };

  const sendSmsCode = async () => {
    if (!/^\d{9}$/.test(formData.phoneNumber)) {
      toast.error("Podaj prawidłowy numer telefonu (9 cyfr)");
      return;
    }

  const loadingToast = toast.loading('Wysyłanie kodu SMS...');
    try {
      await axiosClient.post("/auth/send-code-register", { phoneNumber: formData.phoneNumber });
      toast.dismiss(loadingToast);
      setSmsSent(true);
      toast.success(`Kod SMS został wysłany na numer ${formData.phoneNumber}`);
    } catch (err) {
      toast.dismiss(loadingToast);
      const errorMsg = err.response?.data?.message || "Błąd wysyłania SMS";
      toast.error(errorMsg);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const validationError = validateForm();
    if (validationError) {
      toast.error(validationError);
      return;
    }

    if (!smsSent) {
      toast.error("Najpierw wyślij i wprowadź kod SMS");
      return;
    }

    if (!formData.smsCode || formData.smsCode.length !== 6) {
      toast.error("Kod SMS musi składać się z 6 cyfr");
      return;
    }

    setLoading(true);
    const loadingToast = toast.loading('Rejestracja w toku...');

    try {
      await register(formData);
      toast.dismiss(loadingToast);
      toast.success('Konto utworzone pomyślnie! Witamy w FinoBank.');
      navigate("/");
    } catch (err) {
      toast.dismiss(loadingToast);
      console.error("Pełny błąd:", err.response);

      let errorMsg = "Wystąpił nieznany błąd rejestracji.";

      if (err.response?.data?.errors) {
        const errorKeys = Object.keys(err.response.data.errors);
        if (errorKeys.length > 0) {
           const firstKey = errorKeys[0];
           const firstMessage = err.response.data.errors[firstKey][0];
           errorMsg = `Błąd w polu ${firstKey}: ${firstMessage}`;
        }
      } else if (err.response?.data?.error || err.response?.data?.message) {
        errorMsg = err.response.data.error || err.response.data.message;
      }
          
      toast.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

return (
    <div className="min-h-screen bg-gray-100 py-10 px-4">
      <div className="mx-auto max-w-2xl rounded-xl bg-white p-8 shadow-lg">
        <h2 className="mb-6 text-2xl font-bold text-bank-blue">Otwórz konto w FinoBank</h2>
        
        <form onSubmit={handleSubmit} className="grid gap-4 md:grid-cols-2">
          <div className="col-span-2 text-lg font-semibold text-gray-700 border-b pb-2">Dane osobowe</div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Imię *</label>
            <input 
              name="firstName" 
              placeholder="Jan" 
              onChange={handleChange}
              value={formData.firstName}
              required 
              className="w-full border p-2 rounded focus:ring-2 focus:ring-bank-blue focus:border-transparent" 
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Nazwisko *</label>
            <input 
              name="lastName" 
              placeholder="Kowalski" 
              onChange={handleChange}
              value={formData.lastName}
              required 
              className="w-full border p-2 rounded focus:ring-2 focus:ring-bank-blue focus:border-transparent" 
            />
          </div>

          <div className="col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">PESEL (11 cyfr) *</label>
            <input 
              name="pesel" 
              placeholder="00000000000" 
              onChange={handleChange}
              value={formData.pesel}
              maxLength={11}
              pattern="\d{11}"
              required 
              className="w-full border p-2 rounded font-mono tracking-widest focus:ring-2 focus:ring-bank-blue focus:border-transparent" 
            />
          </div>
          
          <div className="col-span-2 text-lg font-semibold text-gray-700 border-b pb-2 mt-4">Adres zamieszkania</div>
          
          <div className="col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">Ulica i numer *</label>
            <input 
              name="street" 
              placeholder="Złota 44" 
              onChange={handleChange}
              value={formData.street}
              required 
              className="w-full border p-2 rounded focus:ring-2 focus:ring-bank-blue focus:border-transparent" 
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Kod pocztowy (XX-XXX) *</label>
            <input 
              name="zipCode" 
              placeholder="00-000" 
              onChange={handleChange}
              value={formData.zipCode}
              pattern="\d{2}-\d{3}"
              maxLength={6}
              required 
              className="w-full border p-2 rounded font-mono focus:ring-2 focus:ring-bank-blue focus:border-transparent" 
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Miasto *</label>
            <input 
              name="city" 
              placeholder="Warszawa" 
              onChange={handleChange}
              value={formData.city}
              required 
              className="w-full border p-2 rounded focus:ring-2 focus:ring-bank-blue focus:border-transparent" 
            />
          </div>

          <div className="col-span-2 text-lg font-semibold text-gray-700 border-b pb-2 mt-4">Bezpieczeństwo i Kontakt</div>
          
          <div className="col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">Email *</label>
            <input 
              name="email" 
              type="email" 
              placeholder="jan.kowalski@example.com" 
              onChange={handleChange}
              value={formData.email}
              required 
              className="w-full border p-2 rounded focus:ring-2 focus:ring-bank-blue focus:border-transparent" 
            />
          </div>

          <div className="col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Hasło * <span className="text-xs text-gray-500">(min. 8 znaków, duża, mała, cyfra, znak specjalny)</span>
            </label>
            <input 
              name="password" 
              type="password" 
              placeholder="••••••••" 
              onChange={handleChange}
              value={formData.password}
              required 
              className="w-full border p-2 rounded focus:ring-2 focus:ring-bank-blue focus:border-transparent" 
            />
          </div>

          <div className="col-span-2 bg-blue-50 p-4 rounded-md border border-blue-200">
             <label className="block text-sm font-medium mb-1">Weryfikacja numeru telefonu *</label>
             <div className="flex gap-2">
                <input 
                  name="phoneNumber" 
                  placeholder="000000000" 
                  onChange={handleChange}
                  value={formData.phoneNumber}
                  maxLength={9}
                  pattern="\d{9}"
                  className="border p-2 rounded flex-1 font-mono tracking-widest focus:ring-2 focus:ring-bank-blue focus:border-transparent"
                />
                <button 
                  type="button" 
                  onClick={sendSmsCode}
                  disabled={loading || !formData.phoneNumber || formData.phoneNumber.length !== 9}
                  className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 text-sm whitespace-nowrap disabled:bg-gray-400 disabled:cursor-not-allowed transition"
                >
                  {smsSent ? "Wyślij ponownie" : "Wyślij kod SMS"}
                </button>
             </div>
             {smsSent && (
                <div className="mt-3">
                  <label className="block text-sm font-medium mb-1">Kod SMS (6 cyfr) *</label>
                  <input 
                    name="smsCode" 
                    placeholder="000000" 
                    onChange={handleChange}
                    value={formData.smsCode}
                    maxLength={6}
                    pattern="\d{6}"
                    required 
                    className="w-full border p-2 rounded border-green-500 font-mono text-center text-lg tracking-[0.5em] focus:ring-2 focus:ring-green-500"
                  />
                  <p className="text-xs text-gray-600 mt-1">Sprawdź logi backendu, aby odczytać kod</p>
                </div>
             )}
          </div>

          <button 
            type="submit" 
            disabled={loading || !smsSent}
            className="col-span-2 mt-4 rounded-md bg-bank-blue py-3 text-white font-bold hover:bg-bank-dark disabled:bg-gray-400 disabled:cursor-not-allowed transition flex items-center justify-center gap-2"
          >
            {loading ? (
              <>
                Rejestracja...
              </>
            ) : (
              'Zarejestruj się'
            )}
          </button>
        </form>
        
        <div className="mt-4 text-center text-sm">
          Masz już konto? <Link to="/login" className="text-bank-blue hover:underline font-semibold">Zaloguj się</Link>
        </div>
      </div>
    </div>
  );
};

export default Register;
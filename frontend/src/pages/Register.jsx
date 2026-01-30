import { useState } from "react";
import useAuth from "../context/AuthContext";
import axiosClient from "../api/axios";
import { useNavigate, Link } from "react-router-dom";

const Register = () => {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState(null);
  const [smsSent, setSmsSent] = useState(false);

  const [formData, setFormData] = useState({
    firstName: "", lastName: "", email: "", password: "",
    pesel: "", phoneNumber: "", smsCode: "",
    street: "", city: "", zipCode: ""
  });

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const sendSmsCode = async () => {
    if (formData.phoneNumber.length !== 9) {
      setError("Podaj prawidłowy numer telefonu (9 cyfr).");
      return;
    }
    try {
      await axiosClient.post("/auth/send-code-register", { phoneNumber: formData.phoneNumber });
      setSmsSent(true);
      setError(null);
      alert("Kod SMS został wysłany! Sprawdź logi backendu (konsolę API).");
    } catch (err) {
      setError(err.response?.data?.message || "Błąd wysyłania SMS");
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    try {
      await register(formData);
      navigate("/");
    } catch (err) {
      console.error("Pełny błąd:", err.response);

      if (err.response?.data?.errors) {
        const errorKeys = Object.keys(err.response.data.errors);
        if (errorKeys.length > 0) {
           const firstKey = errorKeys[0];
           const firstMessage = err.response.data.errors[firstKey][0];
           setError(`Błąd w polu ${firstKey}: ${firstMessage}`);
           return;
        }
      }

      const errorMsg = 
          err.response?.data?.error || 
          err.response?.data?.message || 
          "Wystąpił nieznany błąd rejestracji.";
          
      setError(errorMsg);
    }
  };

  return (
    <div className="min-h-screen bg-gray-100 py-10 px-4">
      <div className="mx-auto max-w-2xl rounded-xl bg-white p-8 shadow-lg">
        <h2 className="mb-6 text-2xl font-bold text-bank-blue">Otwórz konto w FinoBank</h2>
        
        {error && <div className="mb-4 rounded bg-red-100 p-3 text-red-700 text-sm">{error}</div>}

        <form onSubmit={handleSubmit} className="grid gap-4 md:grid-cols-2">
          <div className="col-span-2 text-lg font-semibold text-gray-700 border-b pb-2">Dane osobowe</div>
          
          <input name="firstName" placeholder="Imię" onChange={handleChange} required className="border p-2 rounded" />
          <input name="lastName" placeholder="Nazwisko" onChange={handleChange} required className="border p-2 rounded" />
          <input name="pesel" placeholder="PESEL (11 cyfr)" onChange={handleChange} required className="border p-2 rounded" maxLength={11} />
          
          <div className="col-span-2 text-lg font-semibold text-gray-700 border-b pb-2 mt-4">Adres zamieszkania</div>
          <input name="street" placeholder="Ulica i numer" onChange={handleChange} required className="border p-2 rounded col-span-2" />
          <input name="zipCode" placeholder="Kod pocztowy (XX-XXX)" onChange={handleChange} required className="border p-2 rounded" />
          <input name="city" placeholder="Miasto" onChange={handleChange} required className="border p-2 rounded" />

          <div className="col-span-2 text-lg font-semibold text-gray-700 border-b pb-2 mt-4">Bezpieczeństwo i Kontakt</div>
          <input name="email" type="email" placeholder="Email" onChange={handleChange} required className="border p-2 rounded col-span-2" />
          <input name="password" type="password" placeholder="Hasło (min. 8 znaków, Duża, mała, znak, cyfra)" onChange={handleChange} required className="border p-2 rounded col-span-2" />

          <div className="col-span-2 bg-blue-50 p-4 rounded-md border border-blue-200">
             <label className="block text-sm font-medium mb-1">Weryfikacja numeru telefonu</label>
             <div className="flex gap-2">
                <input 
                  name="phoneNumber" 
                  placeholder="Numer telefonu (9 cyfr)" 
                  onChange={handleChange} 
                  maxLength={9}
                  className="border p-2 rounded flex-1"
                />
                <button 
                  type="button" 
                  onClick={sendSmsCode}
                  className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 text-sm whitespace-nowrap"
                >
                  {smsSent ? "Wyślij ponownie" : "Wyślij kod SMS"}
                </button>
             </div>
             {smsSent && (
                <input 
                  name="smsCode" 
                  placeholder="Wpisz kod SMS (z logów backendu)" 
                  onChange={handleChange} 
                  required 
                  className="mt-2 w-full border p-2 rounded border-green-500"
                />
             )}
          </div>

          <button type="submit" className="col-span-2 mt-4 rounded-md bg-bank-blue py-3 text-white font-bold hover:bg-bank-dark">
            Zarejestruj się
          </button>
        </form>
        
        <div className="mt-4 text-center text-sm">
          Masz już konto? <Link to="/login" className="text-bank-blue hover:underline">Zaloguj się</Link>
        </div>
      </div>
    </div>
  );
};

export default Register;
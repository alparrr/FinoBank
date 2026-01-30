import { useState } from "react";
import useAuth from "../context/AuthContext";
import { useNavigate, Link } from "react-router-dom";

const Login = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [twoFactorCode, setTwoFactorCode] = useState("");
  const [show2FA, setShow2FA] = useState(false);
  const [error, setError] = useState(null);
  
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    try {
      await login(email, password, twoFactorCode);
      navigate("/");
    } catch (err) {
      if (err.message === "2FA_REQUIRED") {
        setShow2FA(true);
      } else {
        const errorMsg = err.response?.data?.error || err.response?.data?.message || "Błąd logowania";
        setError(errorMsg);
      }
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-100 p-4">
      <div className="w-full max-w-md rounded-xl bg-white p-8 shadow-lg">
        <h2 className="mb-6 text-center text-2xl font-bold text-bank-blue">Logowanie do FinoBank</h2>
        
        {error && <div className="mb-4 rounded bg-red-100 p-3 text-red-700 text-sm">{error}</div>}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">Email</label>
            <input
              type="email"
              className="mt-1 block w-full rounded-md border border-gray-300 p-2 focus:border-bank-blue focus:ring-bank-blue"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Hasło</label>
            <input
              type="password"
              className="mt-1 block w-full rounded-md border border-gray-300 p-2 focus:border-bank-blue focus:ring-bank-blue"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          {show2FA && (
            <div className="animate-fade-in">
              <label className="block text-sm font-medium text-gray-700">Kod 2FA (Authenticator)</label>
              <input
                type="text"
                className="mt-1 block w-full rounded-md border border-blue-300 bg-blue-50 p-2 text-center text-lg tracking-widest focus:border-bank-blue focus:ring-bank-blue"
                value={twoFactorCode}
                onChange={(e) => setTwoFactorCode(e.target.value)}
                placeholder="000000"
              />
            </div>
          )}

          <button
            type="submit"
            className="w-full rounded-md bg-bank-blue px-4 py-2 text-white hover:bg-bank-dark transition"
          >
            {show2FA ? "Potwierdź logowanie" : "Zaloguj się"}
          </button>
        </form>

        <div className="mt-4 text-center text-sm">
          Nie masz konta?{" "}
          <Link to="/register" className="text-bank-blue hover:underline">
            Zarejestruj się
          </Link>
        </div>
      </div>
    </div>
  );
};

export default Login;
import { useEffect, useState } from "react";
import axiosClient from "../api/axios";
import Navbar from "../components/Navbar";

const Exchange = () => {
  const [rates, setRates] = useState([]);
  const [accounts, setAccounts] = useState([]);
  const [loading, setLoading] = useState(true);
  
  const [fromAccountId, setFromAccountId] = useState("");
  const [toAccountId, setToAccountId] = useState("");
  const [amount, setAmount] = useState("");
  const [result, setResult] = useState(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [ratesRes, accountsRes] = await Promise.all([
            axiosClient.get("/exchange/rates"),
            axiosClient.get("/accounts")
        ]);
        setRates(ratesRes.data);
        setAccounts(accountsRes.data);
        
        if(accountsRes.data.length >= 2) {
            setFromAccountId(accountsRes.data[0].id);
            setToAccountId(accountsRes.data[1].id);
        }
      } catch (error) {
        console.error("Błąd pobierania danych kantoru", error);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  const handleExchange = async (e) => {
    e.preventDefault();
    try {
        await axiosClient.post("/exchange/execute", {
            fromAccountId: Number(fromAccountId),
            toAccountId: Number(toAccountId),
            amount: Number(amount)
        });
        alert("Wymiana zakończona sukcesem!");
        setAmount("");
    } catch (err) {
        alert("Błąd wymiany: " + (err.response?.data?.message || err.message));
    }
  };

  if (loading) return <div className="p-10 text-center">Ładowanie kursów NBP...</div>;

  return (
    <div className="min-h-screen bg-gray-100">
      <Navbar />
      <div className="container mx-auto p-6">
        <h2 className="mb-6 text-2xl font-bold text-bank-blue">Kantor Walutowy</h2>

        <div className="grid gap-6 md:grid-cols-2">
            
            {/* Tabela Kursów */}
            <div className="bg-white p-6 rounded-xl shadow-md">
                <h3 className="text-lg font-bold mb-4 text-gray-700">Aktualne kursy (NBP)</h3>
                <table className="w-full text-left">
                    <thead>
                        <tr className="border-b text-gray-500 text-sm">
                            <th className="py-2">Waluta</th>
                            <th className="py-2 text-right">Kurs średni</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rates.map((r) => (
                            <tr key={r.currencyCode} className="border-b last:border-0">
                                <td className="py-2 font-bold text-bank-blue">{r.currencyCode}</td>
                                <td className="py-2 text-right">{r.rate.toFixed(4)} PLN</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                <p className="mt-4 text-xs text-gray-400">Kursy pobierane automatycznie z API NBP.</p>
            </div>

            {/* Formularz Wymiany */}
            <div className="bg-white p-6 rounded-xl shadow-md">
                <h3 className="text-lg font-bold mb-4 text-gray-700">Wymień walutę</h3>
                
                {accounts.length < 2 ? (
                    <div className="text-red-600 bg-red-50 p-4 rounded">
                        Musisz posiadać co najmniej dwa konta w różnych walutach, aby dokonać wymiany.
                        <br/>
                        <div className="mt-2 underline font-bold">Otwórz konto walutowe</div>
                    </div>
                ) : (
                    <form onSubmit={handleExchange} className="space-y-4">
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Z konta (Sprzedaję)</label>
                            <select 
                                className="w-full border p-2 rounded"
                                value={fromAccountId}
                                onChange={(e) => setFromAccountId(e.target.value)}
                            >
                                {accounts.map(acc => (
                                    <option key={acc.id} value={acc.id}>{acc.currency} - {acc.balance} (Nr: ...{acc.accountNumber.slice(-4)})</option>
                                ))}
                            </select>
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-700">Na konto (Kupuję)</label>
                            <select 
                                className="w-full border p-2 rounded"
                                value={toAccountId}
                                onChange={(e) => setToAccountId(e.target.value)}
                            >
                                {accounts.map(acc => (
                                    <option key={acc.id} value={acc.id} disabled={acc.id == fromAccountId}>{acc.currency} - {acc.balance}</option>
                                ))}
                            </select>
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-700">Kwota (z konta źródłowego)</label>
                            <input 
                                type="number" 
                                step="0.01"
                                className="w-full border p-2 rounded font-bold"
                                value={amount}
                                onChange={(e) => setAmount(e.target.value)}
                                placeholder="0.00"
                            />
                        </div>

                        <button type="submit" className="w-full bg-blue-600 text-white py-3 rounded font-bold hover:bg-blue-700 transition">
                            Przelicz i Wymień
                        </button>
                    </form>
                )}
            </div>
        </div>
      </div>
    </div>
  );
};

export default Exchange;
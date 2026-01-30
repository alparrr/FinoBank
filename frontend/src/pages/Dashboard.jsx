import { useEffect, useState } from "react";
import axiosClient from "../api/axios";
import Navbar from "../components/Navbar";
import { useNavigate } from "react-router-dom";

const Dashboard = () => {
  const [accounts, setAccounts] = useState([]);
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newAccountForm, setNewAccountForm] = useState({
      currency: "PLN",
      accountType: "Personal" 
  });

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      const { data: accountsData } = await axiosClient.get("/accounts");
      setAccounts(accountsData);

      if (accountsData.length > 0) {
        const mainAccountId = accountsData[0].id;
        const { data: transData } = await axiosClient.get(`/transactions/account/${mainAccountId}`);
        setTransactions(transData);
      }
    } catch (error) {
      console.error("Błąd pobierania danych:", error);
    } finally {
      setLoading(false);
    }
  };

  const downloadPdf = async (transactionId) => {
    try {
      const response = await axiosClient.get(`/transactions/${transactionId}/pdf`, {
        responseType: 'blob',
      });
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `potwierdzenie_${transactionId}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.parentNode.removeChild(link);
    } catch (error) {
      alert("Nie udało się pobrać potwierdzenia.");
    }
  };

  const handleOpenAccountSubmit = async (e) => {
    e.preventDefault();
    try {
        await axiosClient.post("/accounts", {
            accountType: newAccountForm.accountType,
            currency: newAccountForm.currency
        });
        alert(`Sukces! Utworzono konto ${newAccountForm.accountType} w ${newAccountForm.currency}.`);
        setIsModalOpen(false); 
        fetchData(); 
    } catch (err) {
        alert("Błąd: " + (err.response?.data?.message || err.message));
    }
  };

  if (loading) return <div className="p-10 text-center">Ładowanie finansów...</div>;

  return (
    <div className="min-h-screen bg-gray-100 relative">
      <Navbar />

      <main className="container mx-auto p-6">
        
        <div className="mb-8">
          <div className="flex justify-between items-center mb-4">
             <h2 className="text-xl font-bold text-gray-700">Twoje Rachunki</h2>
             
             <button 
                onClick={() => setIsModalOpen(true)}
                className="text-sm bg-blue-100 text-blue-700 px-3 py-1 rounded hover:bg-blue-200 border border-blue-300 font-bold transition shadow-sm"
             >
                + Otwórz nowe konto
             </button>
          </div>

          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {accounts.map((acc) => (
              <div key={acc.id} className="rounded-xl bg-gradient-to-r from-bank-blue to-blue-600 p-6 text-white shadow-lg transform transition hover:scale-105">
                <div className="mb-2 text-sm opacity-80 uppercase tracking-wider">{acc.accountType}</div>
                <div className="mb-4 text-2xl font-bold tracking-widest">
                  {acc.balance.toFixed(2)} {acc.currency}
                </div>
                <div className="text-sm opacity-70">Numer konta:</div>
                <div className="font-mono tracking-wider text-xs md:text-sm">{acc.accountNumber}</div>
              </div>
            ))}
          </div>
        </div>

        <div className="mb-8 flex gap-4">
            <button 
                onClick={() => navigate("/transfer")}
                className="rounded-lg bg-green-600 px-6 py-3 font-semibold text-white shadow hover:bg-green-700 transition flex items-center gap-2"
            >
                <span>💸</span> Nowy Przelew
            </button>
        </div>

        <div className="rounded-xl bg-white p-6 shadow-md">
          <h3 className="mb-4 text-lg font-bold text-gray-800">Ostatnie transakcje (Konto Główne)</h3>
          
          {transactions.length === 0 ? (
            <p className="text-gray-500 italic">Brak historii transakcji.</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="border-b text-sm text-gray-500">
                    <th className="py-3 px-2">Data</th>
                    <th className="py-3 px-2">Tytuł</th>
                    <th className="py-3 px-2">Strona transakcji</th>
                    <th className="py-3 px-2 text-right">Kwota</th>
                    <th className="py-3 px-2 text-center">Akcje</th>
                  </tr>
                </thead>
                <tbody>
                  {transactions.map((t) => (
                    <tr key={t.id} className="border-b hover:bg-gray-50 transition">
                      <td className="py-3 px-2 text-sm text-gray-600">
                        {new Date(t.createdAt).toLocaleDateString()}
                      </td>
                      <td className="py-3 px-2 font-medium">{t.title}</td>
                      
                      <td className="py-3 px-2 text-sm text-gray-500">
                        {t.transactionType === "Transfer" && t.toAccountId === 999 ? (
                            <span className="flex items-center gap-2 text-gray-700 font-medium">
                                💳 Płatność Kartą
                            </span>
                        ) : t.transactionType === "Transfer" && t.toAccountId !== 0 ? (
                            `Do: ${t.toAccountNumber}`
                        ) : (
                            `Od: ${t.fromAccountNumber || "Przelew zewnętrzny"}`
                        )}
                      </td>

                      <td className={`py-3 px-2 text-right font-bold ${
                          t.amount > 0 && t.toAccountNumber !== accounts[0]?.accountNumber
                          ? "text-red-600" 
                          : "text-green-600"
                      }`}>
                        {t.amount > 0 && t.toAccountNumber !== accounts[0]?.accountNumber ? "-" : "+"}
                        {t.amount.toFixed(2)} {t.currency}
                      </td>
                      <td className="py-3 px-2 text-center">
                        <button 
                          onClick={() => downloadPdf(t.id)}
                          className="rounded bg-gray-200 px-3 py-1 text-xs font-semibold text-gray-700 hover:bg-gray-300 transition"
                        >
                          📄 PDF
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>`
              </table>
            </div>
          )}
        </div>

      </main>

      {isModalOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-xl shadow-2xl max-w-md w-full p-6">
                <h3 className="text-xl font-bold mb-4 text-gray-800">Otwórz nowe konto</h3>
                <form onSubmit={handleOpenAccountSubmit} className="space-y-4">
                    <div>
                        <label className="block text-sm font-medium text-gray-700">Waluta</label>
                        <select 
                            className="w-full border p-2 rounded mt-1"
                            value={newAccountForm.currency}
                            onChange={e => setNewAccountForm({...newAccountForm, currency: e.target.value})}
                        >
                            <option value="PLN">PLN (Polski Złoty)</option>
                            <option value="USD">USD (Dolar Amerykański)</option>
                            <option value="EUR">EUR (Euro)</option>
                            <option value="GBP">GBP (Funt Szterling)</option>
                        </select>
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-gray-700">Kategoria Konta</label>
                        <select 
                            className="w-full border p-2 rounded mt-1"
                            value={newAccountForm.accountType}
                            onChange={e => setNewAccountForm({...newAccountForm, accountType: e.target.value})}
                        >
                            <option value="Personal">Konto Osobiste (ROR)</option>
                            <option value="Savings">Konto Oszczędnościowe</option>
                            <option value="Business">Konto Firmowe</option>
                            <option value="Currency">Konto Walutowe</option>
                        </select>
                    </div>
                    
                    <div className="flex justify-end gap-2 mt-6">
                        <button 
                            type="button"
                            onClick={() => setIsModalOpen(false)}
                            className="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded"
                        >
                            Anuluj
                        </button>
                        <button 
                            type="submit"
                            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                        >
                            Utwórz Konto
                        </button>
                    </div>
                </form>
            </div>
        </div>
      )}

    </div>
  );
};

export default Dashboard;
import { useState, useEffect } from "react";
import axiosClient from "../api/axios";
import { useNavigate } from "react-router-dom";
import Navbar from "../components/Navbar";

const Transfer = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [accounts, setAccounts] = useState([]);
  const [error, setError] = useState(null);

  const [contacts, setContacts] = useState([]);
  const [newContactName, setNewContactName] = useState("");
  const [newContactAccount, setNewContactAccount] = useState("");

  const [formData, setFormData] = useState({
    fromAccountId: "",
    toAccountNumber: "",
    amount: "",
    title: "",
    description: "",
  });

  useEffect(() => {
    const initData = async () => {
      try {
        const [accountsRes, contactsRes] = await Promise.all([
            axiosClient.get("/accounts"),
            axiosClient.get("/contacts")
        ]);

        setAccounts(accountsRes.data);
        setContacts(contactsRes.data);

        if (accountsRes.data.length > 0) {
          setFormData((prev) => ({ ...prev, fromAccountId: accountsRes.data[0].id }));
        }
      } catch (err) {
        setError("Nie udało się pobrać danych.");
      } finally {
        setLoading(false);
      }
    };
    initData();
  }, []);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);

    if (formData.amount <= 0) {
      setError("Kwota musi być większa od zera.");
      return;
    }

    try {
      await axiosClient.post("/transactions", {
        fromAccountId: Number(formData.fromAccountId),
        toAccountNumber: formData.toAccountNumber,
        amount: Number(formData.amount),
        title: formData.title,
        description: formData.description,
      });

      alert("Przelew wysłany pomyślnie!");
      navigate("/"); 
    } catch (err) {
      setError(err.response?.data?.message || err.response?.data?.error || "Błąd wysyłania przelewu");
    }
  };

  // --- OBSŁUGA KONTAKTÓW ---
  const handleAddContact = async (e) => {
      e.preventDefault();
      try {
          const { data } = await axiosClient.post("/contacts", {
              name: newContactName,
              accountNumber: newContactAccount
          });
          setContacts([...contacts, data]); 
          setNewContactName("");
          setNewContactAccount("");
          alert("Dodano nowego odbiorcę!");
      } catch(err) {
          alert("Błąd dodawania kontaktu: " + (err.response?.data?.message || err.message));
      }
  };

  const handleSelectContact = (contact) => {
      setFormData(prev => ({
          ...prev,
          toAccountNumber: contact.accountNumber,
          title: `Przelew dla ${contact.name}` 
      }));
  };

  if (loading) return <div className="text-center p-10">Ładowanie...</div>;

  return (
    <div className="min-h-screen bg-gray-100">
      <Navbar />
      <div className="container mx-auto p-6">
        
        <div className="flex flex-col lg:flex-row gap-8">
            
            <div className="lg:w-2/3">
                <div className="rounded-xl bg-white p-8 shadow-lg">
                <h2 className="mb-6 text-2xl font-bold text-bank-blue">Wykonaj przelew</h2>

                {error && <div className="mb-4 rounded bg-red-100 p-3 text-red-700">{error}</div>}

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                    <label className="block text-sm font-medium text-gray-700">Z konta</label>
                    <select
                        name="fromAccountId"
                        value={formData.fromAccountId}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border p-3 bg-gray-50"
                    >
                        {accounts.map((acc) => (
                        <option key={acc.id} value={acc.id}>
                            {acc.accountNumber} ({acc.balance.toFixed(2)} {acc.currency})
                        </option>
                        ))}
                    </select>
                    </div>

                    <div>
                    <label className="block text-sm font-medium text-gray-700">Numer konta odbiorcy</label>
                    <input
                        name="toAccountNumber"
                        placeholder="Wpisz numer lub wybierz z listy kontaktów ->"
                        value={formData.toAccountNumber}
                        onChange={handleChange}
                        required
                        className="mt-1 block w-full rounded-md border p-3 focus:border-bank-blue focus:ring-bank-blue font-mono"
                    />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Kwota</label>
                            <input
                                name="amount"
                                type="number"
                                step="0.01"
                                min="0.01"
                                placeholder="0.00"
                                value={formData.amount}
                                onChange={handleChange}
                                required
                                className="mt-1 block w-full rounded-md border p-3 text-lg font-bold text-bank-blue"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Tytuł przelewu</label>
                            <input
                                name="title"
                                placeholder="np. Opłata za..."
                                value={formData.title}
                                onChange={handleChange}
                                required
                                className="mt-1 block w-full rounded-md border p-3"
                            />
                        </div>
                    </div>

                    <div>
                    <label className="block text-sm font-medium text-gray-700">Opis (opcjonalny)</label>
                    <textarea
                        name="description"
                        rows="2"
                        value={formData.description}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border p-3"
                    />
                    </div>

                    <div className="flex gap-4 pt-4">
                    <button
                        type="button"
                        onClick={() => navigate("/")}
                        className="w-1/3 rounded-md border border-gray-300 py-3 text-gray-700 hover:bg-gray-50"
                    >
                        Anuluj
                    </button>
                    <button
                        type="submit"
                        className="w-2/3 rounded-md bg-green-600 py-3 font-bold text-white hover:bg-green-700 shadow-md transition"
                    >
                        Wyślij przelew
                    </button>
                    </div>
                </form>
                </div>
            </div>

            <div className="lg:w-1/3 space-y-6">
                
                <div className="bg-white p-6 rounded-xl shadow-md">
                    <h3 className="font-bold text-gray-700 mb-4 border-b pb-2">Zapisani Odbiorcy</h3>
                    {contacts.length === 0 ? (
                        <p className="text-sm text-gray-500 italic">Brak zapisanych odbiorców.</p>
                    ) : (
                        <ul className="space-y-2 max-h-[300px] overflow-y-auto">
                            {contacts.map(c => (
                                <li 
                                    key={c.id} 
                                    onClick={() => handleSelectContact(c)}
                                    className="p-3 border rounded hover:bg-blue-50 cursor-pointer transition group"
                                    title="Kliknij, aby użyć danych do przelewu"
                                >
                                    <div className="font-bold text-sm text-gray-800 group-hover:text-blue-700">{c.name}</div>
                                    <div className="text-xs text-gray-500 font-mono truncate">{c.accountNumber}</div>
                                </li>
                            ))}
                        </ul>
                    )}
                </div>

                <div className="bg-white p-6 rounded-xl shadow-md">
                    <h3 className="font-bold text-gray-700 mb-4 text-sm">Dodaj nowego odbiorcę</h3>
                    <form onSubmit={handleAddContact} className="space-y-3">
                        <input 
                            placeholder="Nazwa (np. Mama, Czynsz)" 
                            className="w-full border p-2 rounded text-sm"
                            value={newContactName}
                            onChange={e => setNewContactName(e.target.value)}
                            required
                        />
                        <input 
                            placeholder="Numer konta" 
                            className="w-full border p-2 rounded text-sm font-mono"
                            value={newContactAccount}
                            onChange={e => setNewContactAccount(e.target.value)}
                            required
                        />
                        <button className="w-full bg-blue-100 text-blue-700 font-bold py-2 rounded text-sm hover:bg-blue-200 transition">
                            + Zapisz Odbiorcę
                        </button>
                    </form>
                </div>

            </div>

        </div>
      </div>
    </div>
  );
};

export default Transfer;
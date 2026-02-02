import { useEffect, useState } from "react";
import axiosClient from "../api/axios";
import Navbar from "../components/Navbar";
import toast from "react-hot-toast";

const Cards = () => {
  const [cards, setCards] = useState([]);
  const [accounts, setAccounts] = useState([]); 
  const [loading, setLoading] = useState(true);
  const [revealedCardId, setRevealedCardId] = useState(null);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newCardForm, setNewCardForm] = useState({
      accountId: "",
      pin: ""
  });

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      const [cardsRes, accountsRes] = await Promise.all([
          axiosClient.get("/cards"),
          axiosClient.get("/accounts")
      ]);
      setCards(cardsRes.data);
      setAccounts(accountsRes.data);
      
      if (accountsRes.data.length > 0) {
          setNewCardForm(prev => ({ ...prev, accountId: accountsRes.data[0].id }));
      }
    } catch (error) {
      console.error("Błąd pobierania danych", error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateCardSubmit = async (e) => {
    e.preventDefault();
    
    if (!/^\d{4}$/.test(newCardForm.pin)) {
        toast.error("PIN musi składać się dokładnie z 4 cyfr!");
        return;
    }

    try {
        await axiosClient.post(`/cards/create/${newCardForm.accountId}`, { pin: newCardForm.pin });
        toast.success("Karta zamówiona pomyślnie!");
        setIsModalOpen(false);
        setNewCardForm({ ...newCardForm, pin: "" }); 
        fetchData();
    } catch(err) {
        toast.error("Błąd zamawiania karty: " + (err.response?.data?.message || err.message));
    }
  };

  const handleBlockCard = async (cardId) => {
    if (!window.confirm("Czy na pewno chcesz zablokować tę kartę?")) return;
    try {
      await axiosClient.post(`/cards/${cardId}/block`);
      toast.success("Karta zablokowana.");
      fetchData();
    } catch (error) {
      toast.error("Błąd blokowania.");
    }
  };

  const handleChangeLimits = async (cardId, currentDaily, currentMonthly) => {
    const newDaily = prompt("Podaj nowy limit dzienny:", currentDaily);
    if (newDaily === null) return;
    const newMonthly = prompt("Podaj nowy limit miesięczny:", currentMonthly);
    if (newMonthly === null) return;

    try {
      await axiosClient.post(`/cards/${cardId}/limits`, {
        dailyLimit: Number(newDaily),
        monthlyLimit: Number(newMonthly)
      });
      toast.success("Limity zaktualizowane.");
      fetchData();
    } catch (error) {
      toast.error("Błąd aktualizacji.");
    }
  };

  const formatCardNumber = (number, isRevealed) => {
    if (isRevealed) {
        return number.match(/.{1,4}/g).join(" ");
    }
    return "**** **** **** " + number.slice(-4);
  };

  if (loading) return <div className="p-10 text-center">Ładowanie kart...</div>;

  return (
    <div className="min-h-screen bg-gray-100 relative">
      <Navbar />
      <div className="container mx-auto p-6">
        <div className="flex justify-between items-center mb-6">
            <h2 className="text-2xl font-bold text-bank-blue">Twoje Karty</h2>
            <button 
                onClick={() => setIsModalOpen(true)}
                className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 transition shadow"
            >
                + Zamów nową kartę
            </button>
        </div>

        {cards.length === 0 ? (
          <div className="text-center py-10 bg-white rounded-xl shadow">
            <p className="text-gray-500 mb-4">Nie posiadasz jeszcze żadnej karty.</p>
            <button onClick={() => setIsModalOpen(true)} className="text-blue-600 underline">Zamów pierwszą kartę</button>
          </div>
        ) : (
          <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-2">
            {cards.map((card) => {
              const isRevealed = revealedCardId === card.id;
              return (
                <div key={card.id} className="bg-white rounded-xl shadow-lg overflow-hidden flex flex-col md:flex-row border border-gray-200">
                  
                  <div className={`relative p-6 w-full md:w-1/2 text-white flex flex-col justify-between h-60 transition-colors duration-500 ${card.isBlocked ? 'bg-gray-600 grayscale' : 'bg-gradient-to-br from-gray-900 via-slate-800 to-black'}`}>
                    <div className="flex justify-between items-start z-10">
                      <span className="font-bold tracking-widest text-lg italic">FinoBank</span>
                      <span className="text-[10px] border border-gray-400 px-1 rounded opacity-80">DEBIT</span>
                    </div>
                    <div className="space-y-1 z-10">
                        <div className="flex gap-2 items-center">
                            <div className="w-10 h-8 bg-yellow-500 rounded-md opacity-90 shadow-inner overflow-hidden relative">
                                <div className="absolute border border-yellow-700 w-full h-1 top-2"></div>
                                <div className="absolute border border-yellow-700 w-full h-1 bottom-2"></div>
                                <div className="absolute border border-yellow-700 h-full w-1 left-3"></div>
                           </div>
                           <span className="text-xl opacity-60">)))</span>
                        </div>
                        <div className="font-mono text-xl tracking-widest drop-shadow-md mt-2 h-8 flex items-center">
                          {formatCardNumber(card.cardNumber, isRevealed)}
                        </div>
                    </div>
                    <div className="flex justify-between text-xs opacity-80 uppercase z-10">
                      <div>
                          <div className="text-[9px] text-gray-400">Wygasa</div>
                          <div className="font-mono">{card.expiryDate}</div>
                      </div>
                      <div>
                          <div className="text-[9px] text-gray-400">Waluta</div>
                          <div className="font-mono">{card.accountCurrency}</div>
                      </div>
                    </div>
                    <div className="absolute right-[-20px] bottom-[-20px] text-9xl opacity-5 font-bold select-none pointer-events-none">BANK</div>
                    {card.isBlocked && (
                      <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-60 z-20 backdrop-blur-[2px]">
                          <div className="border-4 border-red-500 text-red-500 font-bold text-2xl px-4 py-2 rounded-lg -rotate-12">ZABLOKOWANA</div>
                      </div>
                    )}
                  </div>

                  <div className="p-6 w-full md:w-1/2 flex flex-col justify-between">
                      <div className="space-y-4">
                          <div className="flex justify-between items-center border-b pb-2">
                              <span className="text-gray-500 text-sm">Pokaż dane</span>
                              <button 
                                onClick={() => setRevealedCardId(isRevealed ? null : card.id)}
                                className="text-blue-600 hover:bg-blue-50 p-2 rounded-full transition"
                              >
                                {isRevealed ? "Ukryj" : "Pokaż"}
                              </button>
                          </div>
                          <div className="grid grid-cols-2 gap-4">
                              <div><div className="text-xs text-gray-400">Limit dzienny</div><div className="font-bold text-gray-800">{card.dailyLimit}</div></div>
                              <div><div className="text-xs text-gray-400">Limit miesięczny</div><div className="font-bold text-gray-800">{card.monthlyLimit}</div></div>
                          </div>
                      </div>
                      <div className="space-y-2 mt-4">
                          <button onClick={() => handleChangeLimits(card.id, card.dailyLimit, card.monthlyLimit)} disabled={card.isBlocked} className="w-full text-sm rounded border border-blue-600 text-blue-600 py-2 hover:bg-blue-50 disabled:opacity-50">Zmień limity</button>
                          {!card.isBlocked && <button onClick={() => handleBlockCard(card.id)} className="w-full text-sm rounded bg-red-50 text-red-600 border border-red-200 py-2 hover:bg-red-100">Zablokuj kartę</button>}
                      </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {isModalOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-xl shadow-2xl max-w-md w-full p-6">
                <h3 className="text-xl font-bold mb-4 text-gray-800">Zamów nową kartę</h3>
                <form onSubmit={handleCreateCardSubmit} className="space-y-4">
                    
                    <div>
                        <label className="block text-sm font-medium text-gray-700">Wybierz konto</label>
                        <select 
                            className="w-full border p-2 rounded mt-1"
                            value={newCardForm.accountId}
                            onChange={e => setNewCardForm({...newCardForm, accountId: e.target.value})}
                            required
                        >
                            {accounts.map(acc => (
                                <option key={acc.id} value={acc.id}>
                                    {acc.accountType} ({acc.currency}) - {acc.accountNumber.slice(-4)}...
                                </option>
                            ))}
                        </select>
                        <p className="text-xs text-gray-500 mt-1">Karta zostanie przypisana do wybranego rachunku.</p>
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-gray-700">Ustal PIN (4 cyfry)</label>
                        <input 
                            type="password"
                            maxLength={4}
                            pattern="\d{4}"
                            className="w-full border p-2 rounded mt-1 text-center tracking-widest text-lg"
                            value={newCardForm.pin}
                            onChange={e => {
                                const value = e.target.value;
                                if (/^\d*$/.test(value)) {
                                    setNewCardForm({...newCardForm, pin: value})
                                }
                            }}
                            placeholder="****"
                            required
                        />
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
                            className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
                        >
                            Zamów kartę
                        </button>
                    </div>
                </form>
            </div>
        </div>
      )}

    </div>
  );
};

export default Cards;
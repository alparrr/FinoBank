import { useState } from "react";
import axiosClient from "../api/axios";
import { Link } from "react-router-dom";

const Atm = () => {
  const [accountNumber, setAccountNumber] = useState("");
  const [amount, setAmount] = useState("");
  const [message, setMessage] = useState(null);

  const handleDeposit = async (e) => {
    e.preventDefault();
    try {
        await axiosClient.post("/atm/deposit", {
            accountNumber: accountNumber,
            amount: Number(amount)
        });
        setMessage(`Sukces! Wpłacono ${amount} PLN na konto ${accountNumber}`);
        setAmount("");
    } catch (err) {
        setMessage("Błąd wpłaty: " + (err.response?.data || "Nieznany błąd"));
    }
  };

  return (
    <div className="min-h-screen bg-gray-800 flex items-center justify-center p-4">
        <div className="bg-gray-200 p-8 rounded-xl shadow-2xl max-w-md w-full border-4 border-gray-400">
            <div className="flex justify-between items-center mb-6">
                <h1 className="text-3xl font-bold text-gray-700 tracking-tighter">ATM 24/7</h1>
                <div className="w-12 h-12 bg-blue-600 rounded-full flex items-center justify-center text-white font-bold">F</div>
            </div>

            <div className="bg-blue-900 text-green-400 p-4 rounded mb-6 font-mono text-sm min-h-[60px] flex items-center">
                {message || "WITAJ. WPROWADŹ DANE ABY DOKONAĆ WPŁATY."}
            </div>

            <form onSubmit={handleDeposit} className="space-y-4">
                <div>
                    <label className="font-bold text-gray-600 block mb-1">NUMER RACHUNKU</label>
                    <input 
                        className="w-full p-3 font-mono text-xl border-2 border-gray-400 rounded focus:border-blue-500 outline-none"
                        value={accountNumber}
                        onChange={e => setAccountNumber(e.target.value)}
                        placeholder="PL..."
                    />
                </div>
                <div>
                    <label className="font-bold text-gray-600 block mb-1">KWOTA (PLN)</label>
                    <input 
                        type="number"
                        className="w-full p-3 font-mono text-xl border-2 border-gray-400 rounded focus:border-blue-500 outline-none"
                        value={amount}
                        onChange={e => setAmount(e.target.value)}
                        placeholder="1000"
                    />
                </div>

                <button className="w-full bg-green-600 text-white font-bold py-4 rounded text-xl hover:bg-green-500 shadow-lg active:translate-y-1 transition">
                    POTWIERDŹ WPŁATĘ
                </button>
            </form>

            <div className="mt-8 text-center">
                <Link to="/login" className="text-blue-600 underline text-sm">Powrót do logowania banku</Link>
            </div>
        </div>
    </div>
  );
};

export default Atm;
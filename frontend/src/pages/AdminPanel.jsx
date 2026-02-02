import { useEffect, useState } from "react";
import axiosClient from "../api/axios";
import Navbar from "../components/Navbar";
import toast from "react-hot-toast";

const AdminPanel = () => {
  const [users, setUsers] = useState([]);
  const [logs, setLogs] = useState([]);
  const [activeTab, setActiveTab] = useState("users");
  const [loading, setLoading] = useState(true);

  const [selectedUser, setSelectedUser] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      const usersRes = await axiosClient.get("/admin/users");
      const logsRes = await axiosClient.get("/admin/logs");
      setUsers(usersRes.data);
      setLogs(logsRes.data);
    } catch (err) {
      toast.error("Brak dostępu. Tylko dla Administratora.");
    } finally {
      setLoading(false);
    }
  };

  const generateData = async (userId) => {
    if(!confirm("Czy wygenerować 20 losowych transakcji dla tego usera?")) return;
    try {
        await axiosClient.post(`/admin/seed-transactions/${userId}`);
        toast.success("Dane wygenerowane! Sprawdź wykresy na koncie użytkownika.");
    } catch(err) {
        toast.error("Błąd generowania: " + (err.response?.data?.message || err.message));
    }
};

  const toggleBlockUser = async (userId, isBlocked) => {
    const endpoint = isBlocked ? "unblock" : "block";
    try {
        await axiosClient.post(`/admin/users/${userId}/${endpoint}`);
        toast.success(`Status użytkownika zmieniony.`);
        fetchData();
    } catch(err) {
        toast.error("Błąd zmiany statusu.");
    }
  };

  const viewUserDetails = async (userId) => {
      try {
          const { data } = await axiosClient.get(`/admin/users/${userId}`);
          setSelectedUser(data);
          setIsModalOpen(true);
      } catch (err) {
          toast.error("Nie udało się pobrać szczegółów.");
      }
  };

  if (loading) return <div className="p-10 text-center">Ładowanie panelu admina...</div>;

  return (
    <div className="min-h-screen bg-gray-100 relative">
      <Navbar />
      <div className="container mx-auto p-6">
        <h2 className="mb-6 text-2xl font-bold text-red-800 flex items-center gap-2">
            Panel Administratora
        </h2>

        <div className="flex gap-4 mb-6">
            <button 
                onClick={() => setActiveTab("users")}
                className={`px-4 py-2 rounded font-bold ${activeTab === "users" ? "bg-red-700 text-white" : "bg-white text-gray-700"}`}
            >
                Użytkownicy
            </button>
            <button 
                onClick={() => setActiveTab("logs")}
                className={`px-4 py-2 rounded font-bold ${activeTab === "logs" ? "bg-red-700 text-white" : "bg-white text-gray-700"}`}
            >
                Logi Audytowe
            </button>
        </div>

        {activeTab === "users" && (
            <div className="bg-white rounded-xl shadow p-6 overflow-x-auto">
                <table className="w-full text-left">
                    <thead>
                        <tr className="border-b bg-gray-50">
                            <th className="p-3">ID</th>
                            <th className="p-3">Email</th>
                            <th className="p-3">Rola</th>
                            <th className="p-3">Status</th>
                            <th className="p-3">Konta</th>
                            <th className="p-3">Akcje</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map(u => (
                            <tr key={u.id} className="border-b hover:bg-gray-50">
                                <td className="p-3">{u.id}</td>
                                <td className="p-3 font-medium">{u.email}</td>
                                <td className="p-3"><span className="bg-blue-100 text-blue-800 text-xs px-2 py-1 rounded">{u.role}</span></td>
                                <td className="p-3">
                                    {u.isBlocked 
                                    ? <span className="text-red-600 font-bold">ZABLOKOWANY</span> 
                                    : <span className="text-green-600">Aktywny</span>}
                                </td>
                                <td className="p-3">{u.accountCount}</td>
                                <td className="p-3 flex gap-2">
                                    <button 
                                        onClick={() => viewUserDetails(u.id)}
                                        className="text-xs px-3 py-1 rounded bg-blue-600 text-white hover:bg-blue-700"
                                    >
                                        SZCZEGÓŁY
                                    </button>
                                    {u.role !== "Admin" && (
                                        <button 
                                            onClick={() => toggleBlockUser(u.id, u.isBlocked)}
                                            className={`text-xs px-3 py-1 rounded text-white font-bold ${u.isBlocked ? "bg-green-600 hover:bg-green-700" : "bg-red-600 hover:bg-red-700"}`}
                                        >
                                            {u.isBlocked ? "ODBLOKUJ" : "ZABLOKUJ"}
                                        </button>
                                    
                                    )}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        )}

        {activeTab === "logs" && (
            <div className="bg-white rounded-xl shadow p-6 overflow-x-auto">
                <table className="w-full text-left text-sm">
                    <thead>
                        <tr className="border-b bg-gray-50">
                            <th className="p-3">Czas</th>
                            <th className="p-3">Akcja</th>
                            <th className="p-3">Opis</th>
                            <th className="p-3">IP</th>
                            <th className="p-3">User ID</th>
                        </tr>
                    </thead>
                    <tbody className="font-mono">
                        {logs.map(log => (
                            <tr key={log.id} className="border-b hover:bg-gray-50">
                                <td className="p-3">{new Date(log.timestamp).toLocaleString()}</td>
                                <td className="p-3 font-bold text-blue-600">{log.action}</td>
                                <td className="p-3">{log.description}</td>
                                <td className="p-3">{log.ipAddress}</td>
                                <td className="p-3">{log.userId}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        )}
      </div>

      {isModalOpen && selectedUser && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
              <div className="bg-white rounded-xl shadow-2xl max-w-2xl w-full p-6 max-h-[90vh] overflow-y-auto">
                  <div className="flex justify-between items-center mb-4 border-b pb-2">
                      <h3 className="text-xl font-bold">Szczegóły użytkownika: {selectedUser.email}</h3>
                      <button onClick={() => setIsModalOpen(false)} className="text-gray-500 hover:text-black font-bold text-xl">&times;</button>
                  </div>
                  
                  <div className="grid grid-cols-2 gap-4 mb-6">
                      <div><strong>Imię:</strong> {selectedUser.firstName}</div>
                      <div><strong>Nazwisko:</strong> {selectedUser.lastName}</div>
                      <div><strong>2FA:</strong> {selectedUser.twoFactorSecret ? "Aktywne" : "Nieaktywne"}</div>
                  </div>

                  <h4 className="font-bold text-lg mb-2 bg-gray-100 p-2 rounded">Konta Bankowe</h4>
                  {selectedUser.accounts && selectedUser.accounts.length > 0 ? (
                      <div className="space-y-4">
                          {selectedUser.accounts.map(acc => (
                              <div key={acc.id} className="border p-3 rounded bg-gray-50">
                                  <div className="flex justify-between font-mono font-bold">
                                      <span>{acc.accountNumber}</span>
                                      <span>{acc.balance} {acc.currency}</span>
                                  </div>
                                  
                                  <div className="mt-2 text-sm ml-4">
                                      <div className="font-semibold text-gray-500">Karty przypisane:</div>
                                      {acc.cards && acc.cards.length > 0 ? (
                                          <ul className="list-disc pl-5">
                                              {acc.cards.map((c, idx) => (
                                                  <li key={idx}>
                                                      {c.cardNumber} ({c.isActive ? "Aktywna" : "Nieaktywna"})
                                                  </li>
                                              ))}
                                          </ul>
                                      ) : (
                                          <div className="text-gray-400 italic">Brak kart</div>
                                      )}
                                  </div>
                              </div>
                          ))}
                      </div>
                  ) : (
                      <p>Brak kont.</p>
                  )}

                  <div className="mt-6 text-right">
                      <button onClick={() => setIsModalOpen(false)} className="bg-gray-600 text-white px-4 py-2 rounded hover:bg-gray-700">Zamknij</button>
                  </div>
              </div>
          </div>
      )}

    </div>
  );
};

export default AdminPanel;
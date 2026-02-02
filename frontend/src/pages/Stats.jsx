import { useEffect, useState, useMemo } from "react";
import axiosClient from "../api/axios";
import Navbar from "../components/Navbar";
import { PieChart, Pie, Cell, Tooltip, ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid } from 'recharts';
import toast from "react-hot-toast";

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#AF19FF', '#FF4560', '#775DD0'];

const Stats = () => {
  const [transactions, setTransactions] = useState([]);
  const [accountId, setAccountId] = useState(null);
  const [loading, setLoading] = useState(true);

  const [startDate, setStartDate] = useState(new Date(new Date().setDate(new Date().getDate() - 30)).toISOString().split('T')[0]);
  const [endDate, setEndDate] = useState(new Date().toISOString().split('T')[0]);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      const { data: accounts } = await axiosClient.get("/accounts");
      if (accounts.length > 0) {
        const id = accounts[0].id;
        setAccountId(id);
        
        const { data: trans } = await axiosClient.get(`/transactions/account/${id}`);
        setTransactions(trans);
      }
    } catch (error) {
      console.error(error);
      toast.error("Nie udało się pobrać statystyk.");
    } finally {
      setLoading(false);
    }
  };

  const chartData = useMemo(() => {
    if (!accountId) return [];

    const filtered = transactions.filter(t => {
      const tDate = t.createdAt.split('T')[0];
      return tDate >= startDate && tDate <= endDate;
    });

    const categoryMap = {};

    filtered.forEach(t => {
      if (t.fromAccountId === accountId) {
        const category = t.title; 
        if (!categoryMap[category]) {
          categoryMap[category] = 0;
        }
        categoryMap[category] += t.amount;
      }
    });

    return Object.keys(categoryMap).map(key => ({
      name: key,
      value: categoryMap[key]
    })).sort((a, b) => b.value - a.value); 

  }, [transactions, startDate, endDate, accountId]);

  const totalExpenses = chartData.reduce((acc, curr) => acc + curr.value, 0);

  if (loading) return <div className="p-10 text-center">Analizowanie danych...</div>;

  return (
    <div className="min-h-screen bg-gray-100">
      <Navbar />
      <div className="container mx-auto p-6">
        <h2 className="mb-6 text-2xl font-bold text-bank-blue flex items-center gap-2">
            Analiza Wydatków
        </h2>

        <div className="bg-white p-4 rounded-xl shadow-md mb-6 flex flex-wrap gap-4 items-end">
            <div>
                <label className="block text-xs text-gray-500 mb-1">Data od:</label>
                <input 
                    type="date" 
                    value={startDate}
                    onChange={e => setStartDate(e.target.value)}
                    className="border p-2 rounded"
                />
            </div>
            <div>
                <label className="block text-xs text-gray-500 mb-1">Data do:</label>
                <input 
                    type="date" 
                    value={endDate}
                    onChange={e => setEndDate(e.target.value)}
                    className="border p-2 rounded"
                />
            </div>
            <div className="ml-auto text-right">
                <div className="text-xs text-gray-500">Suma wydatków w okresie:</div>
                <div className="text-xl font-bold text-red-600">{totalExpenses.toFixed(2)} PLN</div>
            </div>
        </div>

        {chartData.length === 0 ? (
            <div className="text-center py-10 text-gray-500">Brak wydatków w wybranym okresie.</div>
        ) : (
            <div className="grid md:grid-cols-2 gap-6">
                
                <div className="bg-white p-6 rounded-xl shadow-md flex flex-col items-center">
                    <h3 className="text-lg font-bold text-gray-700 mb-4">Struktura Wydatków</h3>
                    <div className="w-full h-[300px]">
                        <ResponsiveContainer width="100%" height="100%">
                            <PieChart>
                                <Pie
                                    data={chartData}
                                    cx="50%"
                                    cy="50%"
                                    labelLine={false}
                                    label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                                    outerRadius={100}
                                    fill="#8884d8"
                                    dataKey="value"
                                >
                                    {chartData.map((entry, index) => (
                                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                                    ))}
                                </Pie>
                                <Tooltip formatter={(value) => `${value.toFixed(2)} PLN`} />
                            </PieChart>
                        </ResponsiveContainer>
                    </div>
                </div>

                <div className="bg-white p-6 rounded-xl shadow-md">
                    <h3 className="text-lg font-bold text-gray-700 mb-4">Wydatki wg Kategorii (PLN)</h3>
                    <div className="w-full h-[300px]">
                        <ResponsiveContainer width="100%" height="100%">
                            <BarChart data={chartData} layout="vertical" margin={{ left: 20 }}>
                                <CartesianGrid strokeDasharray="3 3" />
                                <XAxis type="number" />
                                <YAxis dataKey="name" type="category" width={80} />
                                <Tooltip formatter={(value) => `${value.toFixed(2)} PLN`} />
                                <Bar dataKey="value" fill="#0047AB" radius={[0, 4, 4, 0]} />
                            </BarChart>
                        </ResponsiveContainer>
                    </div>
                </div>

            </div>
        )}
      </div>
    </div>
  );
};

export default Stats;
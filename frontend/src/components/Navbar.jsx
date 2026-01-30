import { Link } from "react-router-dom";
import useAuth from "../context/AuthContext";

const Navbar = () => {
  const { user, logout } = useAuth();

  return (
    <nav className="bg-bank-dark text-white shadow-md">
      <div className="container mx-auto flex flex-wrap items-center justify-between p-4 gap-4">
        {/* Logo */}
        <Link to="/" className="text-2xl font-bold tracking-wider text-blue-400 hover:text-blue-300 transition">
          FinoBank
        </Link>

        {/* MENU GŁÓWNE */}
        <div className="flex flex-wrap gap-4 md:gap-8 text-sm font-medium items-center justify-center w-full md:w-auto order-3 md:order-2">
          <Link to="/" className="hover:text-blue-300 transition border-b-2 border-transparent hover:border-blue-400 pb-1">Pulpit</Link>
          <Link to="/transfer" className="hover:text-blue-300 transition border-b-2 border-transparent hover:border-blue-400 pb-1">Przelewy</Link>
          <Link to="/cards" className="hover:text-blue-300 transition border-b-2 border-transparent hover:border-blue-400 pb-1">Karty</Link>
          <Link to="/exchange" className="hover:text-blue-300 transition border-b-2 border-transparent hover:border-blue-400 pb-1">Kantor</Link>
          <Link to="/stats" className="hover:text-blue-300 transition border-b-2 border-transparent hover:border-blue-400 pb-1">Analizy</Link>
          
          {/* Link widoczny TYLKO jeśli user ma rolę Admin */}
          {user?.role === "Admin" && (
              <Link to="/admin" className="text-red-400 hover:text-red-300 transition font-bold border border-red-500 rounded px-2 py-1">
                  PANEL ADMIN
              </Link>
          )}
        </div>

        {/* MENU UŻYTKOWNIKA */}
        <div className="flex items-center gap-4 ml-auto order-2 md:order-3">
          <Link to="/profile" className="text-right hover:opacity-80 cursor-pointer group">
            <div className="text-[10px] text-gray-400 group-hover:text-blue-200">Zalogowany jako</div>
            <div className="font-semibold text-sm">
              {user ? `${user.firstName} ${user.lastName}` : "..."}
            </div>
          </Link>

          <button
            onClick={logout}
            className="rounded border border-red-500 px-3 py-1 text-xs text-red-400 hover:bg-red-600 hover:text-white transition"
          >
            Wyloguj
          </button>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
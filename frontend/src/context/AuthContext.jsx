import { createContext, useState, useEffect, useContext } from "react";
import axiosClient from "../api/axios";

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  const getUser = async () => {
    try {
      const { data } = await axiosClient.get("/auth/profile");
      setUser(data);
    } catch (error) {
      setUser(null);
    } finally {
      setLoading(false);
    }
  };

  const login = async (email, password, twoFactorCode) => {
    try {
      await axiosClient.post("/auth/login", { email, password, twoFactorCode });
      await getUser();
      return true;
    } catch (error) {
        if (error.response && error.response.status === 403 && error.response.data.requires2FA) {
            throw new Error("2FA_REQUIRED");
        }
        throw error;
    }
  };

  const register = async (formData) => {
    await axiosClient.post("/auth/register", formData);
    await getUser();
  };

  const logout = async () => {
    await axiosClient.post("/auth/logout");
    setUser(null);
  };

  useEffect(() => {
    getUser();
  }, []);

  return (
    <AuthContext.Provider value={{ user, login, register, logout, loading, getUser }}>
      {children}
    </AuthContext.Provider>
  );
};

export default function useAuth() {
  return useContext(AuthContext);
}
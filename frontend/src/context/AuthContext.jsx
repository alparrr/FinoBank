import { createContext, useState, useEffect, useContext, useRef } from "react";
import axiosClient from "../api/axios";

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const refreshIntervalRef = useRef(null);
  
  const stopTokenRefresh = () => {
    if (refreshIntervalRef.current) {
      clearInterval(refreshIntervalRef.current);
      refreshIntervalRef.current = null;
      console.log("❌ Auto-refresh token stopped");
    }
  };

  const getUser = async () => {
    try {
      const { data } = await axiosClient.get("/auth/profile");
      setUser(data);
      return true; 
    } catch (error) {
      console.log("User not logged in or session expired");
      setUser(null);
      stopTokenRefresh();
      return false; 
    } finally {
      setLoading(false);
    }
  };

  const startTokenRefresh = () => {
    stopTokenRefresh(); 

    console.log("✅ Auto-refresh token started");
    refreshIntervalRef.current = setInterval(async () => {
      try {
        await axiosClient.post("/auth/refresh");
        console.log("✅ Token auto-refreshed");
      } catch (error) {
        console.error("❌ Token refresh failed");
        stopTokenRefresh();
        setUser(null);
      }
    }, 45 * 60 * 1000);
  };

  const login = async (email, password, twoFactorCode) => {
    try {
      await axiosClient.post("/auth/login", { email, password, twoFactorCode });
      const success = await getUser();
      if (success) {
          startTokenRefresh(); 
      }
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
    const success = await getUser();
    if (success) {
        startTokenRefresh();
    }
  };

  const logout = async () => {
    try {
      await axiosClient.post("/auth/logout");
    } catch (error) {
      console.error("Logout error:", error);
    } finally {
      setUser(null);
      stopTokenRefresh();
    }
  };

  useEffect(() => {
    const initAuth = async () => {
      const isLoggedIn = await getUser();
      if (isLoggedIn) {
        startTokenRefresh();
      }
    };
    
    initAuth();

    return () => stopTokenRefresh();
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
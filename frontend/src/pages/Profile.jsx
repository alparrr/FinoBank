import { useEffect, useState } from "react";
import axiosClient from "../api/axios";
import Navbar from "../components/Navbar";
import toast from 'react-hot-toast';

const Profile = () => {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  
  const [isEditing, setIsEditing] = useState(false);
  const [editForm, setEditForm] = useState({ city: "", street: "", zipCode: "" });

  const [isPhoneModalOpen, setIsPhoneModalOpen] = useState(false);
  const [phoneStep, setPhoneStep] = useState(1); 
  const [newPhoneNumber, setNewPhoneNumber] = useState("");
  const [smsCode, setSmsCode] = useState("");

  const [setupData, setSetupData] = useState(null);
  const [code, setCode] = useState("");
  
  const [showPesel, setShowPesel] = useState(false);

  useEffect(() => {
    fetchProfile();
  }, []);

  const fetchProfile = async () => {
    try {
      const { data } = await axiosClient.get("/auth/profile");
      setProfile(data);
      setEditForm({
          city: data.city,
          street: data.street,
          zipCode: data.zipCode
      });
    } catch (err) {
      toast.error("Nie udało się pobrać profilu.");
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateProfile = async (e) => {
      e.preventDefault();
      const loadingToast = toast.loading("Aktualizacja danych...");
      
      try {
          await axiosClient.post("/auth/update-profile", editForm);
          toast.dismiss(loadingToast);
          toast.success("Dane adresowe zostały zaktualizowane.");
          setIsEditing(false);
          fetchProfile();
      } catch (err) {
          toast.dismiss(loadingToast);
          toast.error("Błąd aktualizacji: " + (err.response?.data?.message || err.message));
      }
  };

  const handleRequestPhoneChange = async (e) => {
      e.preventDefault();
      
      if (!/^\d{9}$/.test(newPhoneNumber)) {
          toast.error("Numer telefonu musi składać się z 9 cyfr.");
          return;
      }

      const loadingToast = toast.loading("Wysyłanie kodu SMS...");
      try {
          await axiosClient.post("/auth/change-phone/request", {
              newPhoneNumber: newPhoneNumber,
              code: "000000" 
          });
          toast.dismiss(loadingToast);
          toast.success(`Kod SMS wysłany na numer ${newPhoneNumber}.`);
          setPhoneStep(2);
      } catch(err) {
          toast.dismiss(loadingToast);
          
          const serverMessage = err.response?.data?.message;

          if (serverMessage === "This phone number is already in use.") {
              toast.error("Ten numer telefonu jest już zajęty.");
          } else {
              toast.error(serverMessage || "Wystąpił błąd. Sprawdź poprawność numeru.");
          }
      }
  };

  const handleConfirmPhoneChange = async (e) => {
      e.preventDefault();
      const loadingToast = toast.loading("Weryfikacja kodu...");
      
      try {
          await axiosClient.post("/auth/change-phone/confirm", {
              newPhoneNumber: newPhoneNumber,
              code: smsCode
          });
          toast.dismiss(loadingToast);
          toast.success("Numer telefonu został zmieniony pomyślnie!");
          
          setIsPhoneModalOpen(false);
          setPhoneStep(1);
          setNewPhoneNumber("");
          setSmsCode("");
          fetchProfile(); 
      } catch(err) {
          toast.dismiss(loadingToast);
          toast.error("Błąd: " + (err.response?.data?.message || "Nieprawidłowy kod SMS"));
      }
  };

  const start2FASetup = async () => {
    const loadingToast = toast.loading("Generowanie klucza...");
    try {
      const { data } = await axiosClient.get("/2fa/setup");
      setSetupData(data); 
      toast.dismiss(loadingToast);
    } catch (err) {
      toast.dismiss(loadingToast);
      toast.error("Nie udało się rozpocząć konfiguracji 2FA.");
    }
  };

  const enable2FA = async () => {
    if (!code || code.length !== 6) {
        toast.error("Kod musi mieć 6 cyfr.");
        return;
    }

    const loadingToast = toast.loading("Weryfikacja...");
    try {
      await axiosClient.post("/2fa/enable", {
        secretKey: setupData.secretKey,
        code: code
      });
      toast.dismiss(loadingToast);
      toast.success("Uwierzytelnianie dwuetapowe zostało włączone!");
      setSetupData(null);
      setCode("");
      fetchProfile();
    } catch (err) {
      toast.dismiss(loadingToast);
      toast.error("Nieprawidłowy kod. Spróbuj ponownie.");
    }
  };

  if (loading) return <div className="p-10 text-center">Ładowanie profilu...</div>;

  return (
    <div className="min-h-screen bg-gray-100 relative">
      <Navbar />

      <main className="container mx-auto p-6">
        <div className="mx-auto max-w-4xl space-y-6">
          
          <div className="rounded-xl bg-white p-6 shadow-md">
            <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl font-bold text-bank-blue flex items-center gap-2">
                Twoje Dane Osobowe
                </h2>
                <button 
                    onClick={() => setIsEditing(!isEditing)}
                    className="text-sm text-blue-600 hover:underline"
                >
                    {isEditing ? "Anuluj edycję" : "Edytuj adres"}
                </button>
            </div>

            {isEditing ? (
                <form onSubmit={handleUpdateProfile} className="space-y-4 bg-blue-50 p-4 rounded-lg">
                    <div className="grid md:grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium">Ulica i numer</label>
                            <input className="w-full border p-2 rounded" value={editForm.street} onChange={e => setEditForm({...editForm, street: e.target.value})} required />
                        </div>
                        <div>
                            <label className="block text-sm font-medium">Kod pocztowy</label>
                            <input className="w-full border p-2 rounded" value={editForm.zipCode} onChange={e => setEditForm({...editForm, zipCode: e.target.value})} required />
                        </div>
                        <div>
                            <label className="block text-sm font-medium">Miasto</label>
                            <input className="w-full border p-2 rounded" value={editForm.city} onChange={e => setEditForm({...editForm, city: e.target.value})} required />
                        </div>
                    </div>
                    <button className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700">Zapisz zmiany</button>
                </form>
            ) : (
                <div className="grid gap-4 md:grid-cols-2">
                    <div>
                        <label className="text-sm text-gray-500">Imię i Nazwisko</label>
                        <div className="font-medium">{profile?.firstName} {profile?.lastName}</div>
                    </div>
                    <div>
                        <label className="text-sm text-gray-500">Email</label>
                        <div className="font-medium">{profile?.email}</div>
                    </div>
                    <div>
                        <label className="text-sm text-gray-500">PESEL</label>
                        <div className="flex items-center gap-2">
                            <div className="font-mono bg-yellow-50 p-1 inline-block rounded border border-yellow-200 text-yellow-800">
                                {showPesel ? profile?.pesel : "***********"}
                            </div>
                            <button 
                                onClick={() => setShowPesel(!showPesel)}
                                className="text-xs text-blue-600"
                            >
                                {showPesel ? "Ukryj" : "Pokaż"}
                            </button>
                        </div>
                    </div>
                    
                    <div className="flex justify-between items-start md:block">
                        <div>
                            <label className="text-sm text-gray-500">Telefon</label>
                            <div className="font-medium">{profile?.phoneNumber}</div>
                        </div>
                        <button 
                            onClick={() => setIsPhoneModalOpen(true)}
                            className="text-xs bg-gray-200 hover:bg-gray-300 px-2 py-1 rounded mt-1 transition"
                        >
                            Zmień numer
                        </button>
                    </div>

                    <div className="md:col-span-2 border-t pt-2 mt-2">
                        <label className="text-sm text-gray-500">Adres Zamieszkania</label>
                        <div className="font-medium text-lg text-gray-800">{profile?.street}, {profile?.zipCode} {profile?.city}</div>
                    </div>
                </div>
            )}
          </div>

          <div className="rounded-xl bg-white p-6 shadow-md">
            <h2 className="mb-4 text-xl font-bold text-bank-blue flex items-center gap-2">Bezpieczeństwo</h2>
            
            {profile?.is2faEnabled ? (
              <div className="flex items-center gap-3 rounded-lg border border-green-200 bg-green-50 p-4 text-green-800">
                <div><strong>2FA jest aktywne</strong><p className="text-sm">Konto zabezpieczone.</p></div>
              </div>
            ) : (
              <div>
                <div className="mb-4 rounded-lg border border-orange-200 bg-orange-50 p-4 text-orange-800"><strong>2FA nie jest aktywne.</strong></div>
                {!setupData ? (
                  <button onClick={start2FASetup} className="rounded bg-bank-blue px-4 py-2 text-white hover:bg-bank-dark transition">Konfiguruj 2FA</button>
                ) : (
                  <div className="mt-4 border-t pt-4 animate-fade-in">
                    <h3 className="mb-2 font-bold">Zeskanuj kod w Google Authenticator:</h3>
                    <div className="mb-4 flex justify-center bg-white p-2"><img src={setupData.qrCodeSetupImageUrl} alt="QR" className="border" /></div>
                    <p className="mb-2 text-sm">Klucz: <span className="font-mono font-bold">{setupData.manualEntryKey}</span></p>
                    <div className="flex gap-2 max-w-xs mx-auto">
                      <input type="text" placeholder="Kod (6 cyfr)" value={code} onChange={(e) => setCode(e.target.value)} className="w-full rounded border p-2 text-center" maxLength={6}/>
                      <button onClick={enable2FA} className="rounded bg-green-600 px-4 py-2 text-white hover:bg-green-700">Włącz</button>
                    </div>
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      </main>

      {isPhoneModalOpen && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
              <div className="bg-white rounded-xl shadow-2xl max-w-md w-full p-6">
                  <h3 className="text-xl font-bold mb-4">Zmiana numeru telefonu</h3>
                  
                  {phoneStep === 1 ? (
                      <form onSubmit={handleRequestPhoneChange} className="space-y-4">
                          <p className="text-sm text-gray-600">Wprowadź nowy numer telefonu. Wyślemy na niego kod weryfikacyjny SMS.</p>
                          <div>
                              <label className="block text-sm font-bold">Nowy numer (9 cyfr)</label>
                              <input 
                                  type="text" 
                                  className="w-full border p-3 rounded text-lg tracking-widest"
                                  placeholder="000000000"
                                  maxLength={9}
                                  value={newPhoneNumber}
                                  onChange={e => setNewPhoneNumber(e.target.value)}
                                  required
                              />
                          </div>
                          <div className="flex justify-end gap-2 pt-2">
                              <button type="button" onClick={() => setIsPhoneModalOpen(false)} className="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded">Anuluj</button>
                              <button type="submit" className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">Dalej</button>
                          </div>
                      </form>
                  ) : (
                      <form onSubmit={handleConfirmPhoneChange} className="space-y-4">
                          <p className="text-sm text-gray-600">Kod SMS został wysłany na numer <strong>{newPhoneNumber}</strong>.</p>
                          <div className="bg-yellow-50 border border-yellow-200 p-2 text-xs text-yellow-800 rounded">
                              Sprawdź logi w konsoli backendu, aby odczytać kod.
                          </div>
                          <div>
                              <label className="block text-sm font-bold">Kod SMS</label>
                              <input 
                                  type="text" 
                                  className="w-full border p-3 rounded text-center text-2xl tracking-[0.5em] font-mono"
                                  placeholder="000000"
                                  maxLength={6}
                                  value={smsCode}
                                  onChange={e => setSmsCode(e.target.value)}
                                  required
                              />
                          </div>
                          <div className="flex justify-end gap-2 pt-2">
                              <button type="button" onClick={() => setPhoneStep(1)} className="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded">Wróć</button>
                              <button type="submit" className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700">Zatwierdź</button>
                          </div>
                      </form>
                  )}
              </div>
          </div>
      )}
    </div>
  );
};

export default Profile;
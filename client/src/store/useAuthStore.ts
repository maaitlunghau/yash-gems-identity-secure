import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface User {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  kycStatus: string;
}

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  setAuth: (user: User) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      isAuthenticated: false,
      setAuth: (user) => set({ user, isAuthenticated: true }),
      logout: () => {
         // Thực hiện các logic logout ở đây
         set({ user: null, isAuthenticated: false });
      },
    }),
    {
      name: 'auth-storage', // Lưu vào localStorage để không bị mất khi F5
    }
  )
);

'use client';
import Link from 'next/link';
import { useAuthStore } from '@/store/useAuthStore';
import { useRouter } from 'next/navigation';
import { Diamond, LogOut, User as UserIcon, ShieldCheck } from 'lucide-react';
import Cookies from 'js-cookie';

export default function Header() {
  const { user, isAuthenticated, logout } = useAuthStore();
  const router = useRouter();

  const handleLogout = () => {
    logout();
    Cookies.remove('access_token');
    router.push('/login');
  };

  return (
    <header className="bg-slate-900 border-b border-slate-800 text-white shadow-md sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          <Link href="/" className="flex items-center gap-2 group">
            <div className="p-2 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-lg group-hover:scale-105 transition-transform">
              <Diamond className="w-5 h-5 text-white" />
            </div>
            <span className="font-bold text-xl tracking-tight bg-clip-text text-transparent bg-gradient-to-r from-indigo-400 to-purple-400">
              Yash Gems
            </span>
          </Link>

          <nav className="hidden md:flex gap-6">
            <Link href="/" className="text-slate-300 hover:text-white transition-colors">Home</Link>
            <Link href="/products" className="text-slate-300 hover:text-white transition-colors">Jewelry</Link>
            {isAuthenticated && (
              <Link href="/kyc" className="text-slate-300 hover:text-white transition-colors flex items-center gap-1">
                <ShieldCheck className="w-4 h-4 text-green-400" />
                eKYC
              </Link>
            )}
          </nav>

          <div className="flex items-center gap-4">
            {isAuthenticated ? (
              <div className="flex items-center gap-4">
                <Link 
                  href="/profile"
                  className="flex items-center gap-2 text-sm text-slate-300 hover:text-white transition-all group"
                >
                  <div className="w-8 h-8 rounded-full bg-slate-700 flex items-center justify-center border border-slate-600 group-hover:border-indigo-500 group-hover:bg-indigo-500/10 transition-all overflow-hidden">
                    <UserIcon className="w-4 h-4 group-hover:text-indigo-400" />
                  </div>
                  <div className="flex flex-col">
                    <span className="hidden sm:inline font-medium text-white">{user?.fullName || 'User'}</span>
                    <span className="hidden sm:inline text-[10px] text-slate-400 leading-none">View Profile</span>
                  </div>
                </Link>
                <button
                  onClick={handleLogout}
                  className="flex items-center gap-2 px-3 py-1.5 text-sm text-red-400 hover:text-red-300 hover:bg-red-400/10 rounded-md transition-colors border border-transparent hover:border-red-400/20"
                >
                  <LogOut className="w-4 h-4" />
                  <span className="hidden sm:inline">Logout</span>
                </button>
              </div>
            ) : (
              <div className="flex gap-3">
                <Link
                  href="/login"
                  className="px-4 py-2 text-sm font-medium text-slate-300 hover:text-white transition-colors"
                >
                  Sign in
                </Link>
                <Link
                  href="/register"
                  className="px-4 py-2 text-sm font-medium bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg transition-colors shadow-lg shadow-indigo-500/20"
                >
                  Create Account
                </Link>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}

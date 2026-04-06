'use client';
import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { authService } from '@/api/authService';
import { useAuthStore } from '@/store/useAuthStore';
import { useRouter } from 'next/navigation';
import Cookies from 'js-cookie';
import Link from 'next/link';
import { Mail, Lock, Loader2, ArrowRight } from 'lucide-react';
import { GoogleLogin } from '@react-oauth/google';
import FacebookLogin from 'react-facebook-login/dist/facebook-login-render-props';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const setAuth = useAuthStore((state) => state.setAuth);
  const router = useRouter();

  const loginMutation = useMutation({
    mutationFn: authService.login,
    onSuccess: (data: any) => {
      // Assuming data returns access token and user info
      Cookies.set('access_token', data.accessToken, { expires: 1 }); // 1 day
      setAuth({
        email: email,
        id: 'mock-user-id', // Adjust according to real API response
        fullName: 'User Name',
        kycStatus: 'Pending'
      });
      router.push('/');
    },
    onError: (err: any) => {
      setError(err.response?.data || 'Invalid email or password');
    }
  });

  const googleLoginMutation = useMutation({
    mutationFn: authService.googleLogin,
    onSuccess: (data: any) => {
      Cookies.set('access_token', data.accessToken, { expires: 1 });
      setAuth({
        email: data.email,
        id: 'google-user',
        fullName: data.fullName,
        kycStatus: 'None' // Default for new social users
      });
      router.push('/');
    },
    onError: (err: any) => {
      setError(err.response?.data || 'Google login failed');
    }
  });

  const facebookLoginMutation = useMutation({
    mutationFn: authService.facebookLogin,
    onSuccess: (data: any) => {
      Cookies.set('access_token', data.accessToken, { expires: 1 });
      setAuth({
        email: data.email,
        id: 'facebook-user',
        fullName: data.fullName,
        kycStatus: 'None'
      });
      router.push('/');
    },
    onError: (err: any) => {
      setError(err.response?.data || 'Facebook login failed');
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    loginMutation.mutate({ email, password });
  };

  return (
    <div className="min-h-[calc(100vh-4rem)] flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8 bg-gradient-to-b from-slate-50 to-slate-100">
      <div className="max-w-md w-full space-y-8 bg-white p-10 rounded-2xl shadow-xl border border-slate-100">
        <div>
          <h2 className="text-center text-3xl font-extrabold text-slate-900 tracking-tight">
            Welcome back
          </h2>
          <p className="mt-2 text-center text-sm text-slate-600">
            Sign in to access your Yash Gems account
          </p>
        </div>
        <form className="mt-8 space-y-6" onSubmit={handleSubmit}>
          {error && (
            <div className="bg-red-50 text-red-500 p-3 rounded-lg text-sm text-center border border-red-100">
              {error}
            </div>
          )}
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Email address</label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <Mail className="h-5 w-5 text-slate-400" />
                </div>
                <input
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="pl-10 block w-full border-slate-300 rounded-lg shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm py-2.5 border"
                  placeholder="you@example.com"
                />
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Password</label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <Lock className="h-5 w-5 text-slate-400" />
                </div>
                <input
                  type="password"
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="pl-10 block w-full border-slate-300 rounded-lg shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm py-2.5 border"
                  placeholder="••••••••"
                />
              </div>
            </div>
          </div>

          <div className="flex items-center justify-between">
            <div className="flex items-center">
              <input id="remember-me" name="remember-me" type="checkbox" className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-slate-300 rounded" />
              <label htmlFor="remember-me" className="ml-2 block text-sm text-slate-900">
                Remember me
              </label>
            </div>

            <div className="text-sm">
              <a href="#" className="font-medium text-indigo-600 hover:text-indigo-500">
                Forgot your password?
              </a>
            </div>
          </div>

          <button
            type="submit"
            disabled={loginMutation.isPending}
            className="group relative w-full flex justify-center py-2.5 px-4 border border-transparent text-sm font-medium rounded-lg text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-70 disabled:cursor-not-allowed shadow-lg shadow-indigo-500/30 transition-all"
          >
            {loginMutation.isPending ? (
              <Loader2 className="w-5 h-5 animate-spin" />
            ) : (
              <span className="flex items-center gap-2">
                Sign in
                <ArrowRight className="w-4 h-4 group-hover:translate-x-1 transition-transform" />
              </span>
            )}
          </button>
        </form>

        <div className="text-center text-sm">
          <span className="text-slate-600">Don't have an account? </span>
          <Link href="/register" className="font-medium text-indigo-600 hover:text-indigo-500">
            Sign up now
          </Link>
        </div>

        <div className="relative my-6">
          <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t border-slate-200"></div>
          </div>
          <div className="relative flex justify-center text-sm">
            <span className="px-2 bg-white text-slate-500 uppercase tracking-wider">Or continue with</span>
          </div>
        </div>

        <div className="flex flex-col gap-3">
          <GoogleLogin
            onSuccess={credentialResponse => {
              if (credentialResponse.credential) {
                googleLoginMutation.mutate(credentialResponse.credential);
              }
            }}
            onError={() => {
              setError('Google login failed');
            }}
            useOneTap={true}
            theme="outline"
            shape="pill"
          />

          <FacebookLogin
            appId="YOUR_FACEBOOK_APP_ID"
            callback={(response: any) => {
              if (response.accessToken) {
                facebookLoginMutation.mutate(response.accessToken);
              }
            }}
            render={(renderProps: any) => (
              <button
                onClick={renderProps.onClick}
                className="flex items-center justify-center gap-2 w-full py-2.5 px-4 border border-slate-200 rounded-full bg-[#1877F2] text-white font-medium hover:bg-[#166fe5] transition-colors shadow-sm"
              >
                <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
                </svg>
                <span>Continue with Facebook</span>
              </button>
            )}
          />
        </div>
      </div>
    </div>
  );
}

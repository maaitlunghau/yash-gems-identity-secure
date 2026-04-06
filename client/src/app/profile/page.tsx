'use client';
import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { authService } from '@/api/authService';
import { useAuthStore } from '@/store/useAuthStore';
import { User, Phone, Mail, ShieldCheck, Loader2, Save, Camera } from 'lucide-react';
import Image from 'next/image';

export default function ProfilePage() {
  const queryClient = useQueryClient();
  const { user, setAuth } = useAuthStore();
  const [fullName, setFullName] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [message, setMessage] = useState({ type: '', text: '' });

  const { data: profile, isLoading } = useQuery({
    queryKey: ['profile'],
    queryFn: authService.getProfile,
  });

  useEffect(() => {
    if (profile) {
      setFullName(profile.fullName);
      setPhoneNumber(profile.phoneNumber || '');
    }
  }, [profile]);

  const updateMutation = useMutation({
    mutationFn: authService.updateProfile,
    onSuccess: () => {
      setMessage({ type: 'success', text: 'Profile updated successfully!' });
      queryClient.invalidateQueries({ queryKey: ['profile'] });
      // Update global store if needed
      if (user) {
        setAuth({ ...user, fullName, phoneNumber });
      }
      setTimeout(() => setMessage({ type: '', text: '' }), 3000);
    },
    onError: () => {
      setMessage({ type: 'error', text: 'Failed to update profile.' });
      setTimeout(() => setMessage({ type: '', text: '' }), 3000);
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    updateMutation.mutate({ fullName, phoneNumber });
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Loader2 className="w-8 h-8 animate-spin text-indigo-600" />
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto py-12 px-4 sm:px-6 lg:px-8">
      <div className="bg-white shadow-xl rounded-2xl overflow-hidden border border-slate-100">
        <div className="bg-indigo-600 h-32 relative">
          <div className="absolute -bottom-16 left-8 flex items-end space-x-6">
            <div className="relative w-32 h-32 rounded-2xl border-4 border-white bg-white shadow-lg overflow-hidden">
               {profile?.facePhotoUrl ? (
                 <Image 
                    src={profile.facePhotoUrl} 
                    alt="Profile" 
                    fill 
                    className="object-cover"
                 />
               ) : (
                 <div className="w-full h-full bg-slate-100 flex items-center justify-center">
                    <User className="w-12 h-12 text-slate-400" />
                 </div>
               )}
            </div>
            <div className="pb-4">
              <h1 className="text-2xl font-bold text-white mb-1 shadow-sm">{profile?.fullName}</h1>
              <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                profile?.kycStatus === 'Verified' ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'
              }`}>
                {profile?.kycStatus}
              </span>
            </div>
          </div>
        </div>

        <div className="pt-24 pb-12 px-8">
          <form onSubmit={handleSubmit} className="space-y-8">
             {message.text && (
               <div className={`p-4 rounded-xl text-sm font-medium ${
                 message.type === 'success' ? 'bg-green-50 text-green-700 border border-green-100' : 'bg-red-50 text-red-700 border border-red-100'
               }`}>
                 {message.text}
               </div>
             )}

             <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
               <div className="space-y-6">
                 <h2 className="text-lg font-semibold text-slate-900 border-b pb-2">Personal Information</h2>
                 
                 <div>
                   <label className="block text-sm font-medium text-slate-700 mb-1.5">Full Name</label>
                   <div className="relative">
                     <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                       <User className="h-4 h-4 text-slate-400" />
                     </div>
                     <input
                       type="text"
                       value={fullName}
                       onChange={(e) => setFullName(e.target.value)}
                       className="pl-10 block w-full border-slate-200 rounded-xl shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm py-2.5 bg-slate-50 transition-all border"
                       placeholder="Họ và tên"
                     />
                   </div>
                 </div>

                 <div>
                   <label className="block text-sm font-medium text-slate-700 mb-1.5">Phone Number</label>
                   <div className="relative">
                     <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                       <Phone className="h-4 h-4 text-slate-400" />
                     </div>
                     <input
                       type="tel"
                       value={phoneNumber}
                       onChange={(e) => setPhoneNumber(e.target.value)}
                       className="pl-10 block w-full border-slate-200 rounded-xl shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm py-2.5 bg-slate-50 transition-all border"
                       placeholder="Số điện thoại"
                     />
                   </div>
                 </div>

                 <div>
                   <label className="block text-sm font-medium text-slate-700 mb-1.5">Email Address</label>
                   <div className="relative">
                     <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                       <Mail className="h-4 h-4 text-slate-400" />
                     </div>
                     <input
                       type="email"
                       disabled
                       value={profile?.email}
                       className="pl-10 block w-full border-slate-200 rounded-xl shadow-sm bg-slate-100 text-slate-500 sm:text-sm py-2.5 cursor-not-allowed border"
                     />
                   </div>
                   <p className="mt-1.5 text-xs text-slate-400">Email cannot be changed after registration.</p>
                 </div>
               </div>

               <div className="space-y-6">
                 <h2 className="text-lg font-semibold text-slate-900 border-b pb-2">Verification & Security</h2>
                 
                 <div className="bg-slate-50 p-6 rounded-2xl border border-slate-100">
                    <div className="flex items-start">
                       <div className={`p-2 rounded-lg ${profile?.kycStatus === 'Verified' ? 'bg-green-100 text-green-600' : 'bg-yellow-100 text-yellow-600'}`}>
                          <ShieldCheck className="w-6 h-6" />
                       </div>
                       <div className="ml-4">
                          <h3 className="text-sm font-bold text-slate-900 uppercase">KYC Status</h3>
                          <p className="text-sm text-slate-600 mt-1">
                             Current Status: <span className="font-semibold text-slate-900">{profile?.kycStatus}</span>
                          </p>
                          {profile?.kycSimilarityScore && (
                            <p className="text-xs text-slate-500 mt-2">
                               Identity Trust Score: <span className="text-indigo-600 font-bold">{profile.kycSimilarityScore.toFixed(1)}%</span>
                            </p>
                          )}
                          {profile?.kycStatus !== 'Verified' && (
                            <button 
                               type="button"
                               className="mt-4 text-sm font-semibold text-indigo-600 hover:text-indigo-500"
                            >
                               Complete Verification &rarr;
                            </button>
                          )}
                       </div>
                    </div>
                 </div>

                 <div className="bg-slate-50 p-6 rounded-2xl border border-slate-100">
                    <h3 className="text-sm font-bold text-slate-900 uppercase mb-3">Security</h3>
                    <p className="text-sm text-slate-600">
                       Protect your account with advanced security features.
                    </p>
                    <button type="button" className="mt-4 w-full py-2 px-4 border border-slate-200 rounded-lg text-sm font-medium text-slate-700 hover:bg-white transition-all bg-transparent">
                       Change Password
                    </button>
                 </div>
               </div>
             </div>

             <div className="flex justify-end pt-6 border-t border-slate-100">
                <button
                  type="submit"
                  disabled={updateMutation.isPending}
                  className="flex items-center gap-2 px-8 py-3 bg-indigo-600 text-white rounded-xl font-bold hover:bg-indigo-700 shadow-lg shadow-indigo-200 disabled:opacity-50 transition-all active:scale-95"
                >
                  {updateMutation.isPending ? (
                    <Loader2 className="w-5 h-5 animate-spin" />
                  ) : (
                    <>
                      <Save className="w-5 h-5" />
                      Save Profile Changes
                    </>
                  )}
                </button>
             </div>
          </form>
        </div>
      </div>
    </div>
  );
}

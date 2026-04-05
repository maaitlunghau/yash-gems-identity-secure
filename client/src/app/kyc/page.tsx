'use client';
import { useState, useRef } from 'react';
import { useMutation } from '@tanstack/react-query';
import { authService } from '@/api/authService';
import WebcamCapture from '@/components/kyc/WebcamCapture';
import { UploadCloud, FileImage, ShieldAlert, Loader2, CheckCircle } from 'lucide-react';
import { useAuthStore } from '@/store/useAuthStore';
import { useRouter } from 'next/navigation';

export default function KycPage() {
  const [idCardFront, setIdCardFront] = useState<File | null>(null);
  const [idCardBack, setIdCardBack] = useState<File | null>(null);
  const [facePhoto, setFacePhoto] = useState<File | null>(null);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const { isAuthenticated } = useAuthStore();
  const router = useRouter();

  // If not authenticated, we could protect this route. 
  // For demo, we'll assume the user is or they'll be redirected by layout.
  
  const uploadMutation = useMutation({
    mutationFn: authService.uploadKyc,
    onSuccess: (data: any) => {
      setSuccess('KYC documents submitted securely! Our system is processing your verification.');
    },
    onError: (err: any) => {
      setError(err.response?.data || 'An error occurred while uploading. Please try again.');
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    
    if (!idCardFront || !idCardBack || !facePhoto) {
      setError('Please provide all three required images.');
      return;
    }

    const formData = new FormData();
    formData.append('IdCardFront', idCardFront);
    formData.append('IdCardBack', idCardBack);
    formData.append('FacePhoto', facePhoto);

    uploadMutation.mutate(formData);
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>, setter: React.Dispatch<React.SetStateAction<File | null>>) => {
    if (e.target.files && e.target.files[0]) {
      setter(e.target.files[0]);
    }
  };

  return (
    <div className="py-12 px-4 sm:px-6 lg:px-8 max-w-5xl mx-auto">
      <div className="text-center mb-12">
        <h1 className="text-4xl font-extrabold text-slate-900 tracking-tight">Identity Verification</h1>
        <p className="mt-4 text-lg text-slate-600 max-w-2xl mx-auto">
          Complete your eKYC process to unlock full access. We use AI integration for fast and secure verification.
        </p>
      </div>

      {success ? (
         <div className="bg-emerald-50 text-emerald-700 p-8 rounded-2xl border border-emerald-100 flex flex-col items-center justify-center text-center shadow-sm">
             <div className="w-16 h-16 bg-emerald-100 rounded-full flex items-center justify-center mb-4">
                 <CheckCircle className="w-8 h-8 text-emerald-600" />
             </div>
             <h3 className="text-2xl font-bold mb-2">Verification In Progress</h3>
             <p className="text-emerald-600/80 max-w-md">{success}</p>
         </div>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-10 bg-white p-8 md:p-12 rounded-3xl shadow-[0_8px_30px_rgb(0,0,0,0.04)] border border-slate-100">
          
          {error && (
            <div className="bg-red-50 text-red-600 p-4 rounded-xl flex items-start gap-3 border border-red-100">
                <ShieldAlert className="w-5 h-5 mt-0.5 shrink-0" />
                <p>{error}</p>
            </div>
          )}

          <div className="grid md:grid-cols-2 gap-8">
            {/* Step 1: ID Cards */}
            <div className="space-y-6">
              <div>
                <h3 className="text-xl font-bold text-slate-900 flex items-center gap-2">
                  <span className="flex items-center justify-center w-6 h-6 rounded-full bg-indigo-100 text-indigo-600 text-sm">1</span>
                  Upload ID Card
                </h3>
                <p className="text-sm text-slate-500 mt-1 mb-4">Please upload clear photos of your ID card.</p>
              </div>

              <div className="space-y-4">
                <FileUploadBox 
                  label="Front of ID Card" 
                  file={idCardFront} 
                  onChange={(e) => handleFileChange(e, setIdCardFront)} 
                />
                <FileUploadBox 
                  label="Back of ID Card" 
                  file={idCardBack} 
                  onChange={(e) => handleFileChange(e, setIdCardBack)} 
                />
              </div>
            </div>

            {/* Step 2: Liveness Face Capture */}
            <div className="space-y-6">
              <div>
                <h3 className="text-xl font-bold text-slate-900 flex items-center gap-2">
                  <span className="flex items-center justify-center w-6 h-6 rounded-full bg-indigo-100 text-indigo-600 text-sm">2</span>
                  Liveness Check
                </h3>
                <p className="text-sm text-slate-500 mt-1 mb-4">Follow the on-screen instructions to verify your face.</p>
              </div>
              
              <div className="bg-slate-50 p-4 rounded-2xl border border-slate-200 flex justify-center">
                 <WebcamCapture onCapture={(file) => setFacePhoto(file)} />
              </div>
            </div>
          </div>

          <div className="pt-8 border-t border-slate-100 flex justify-end">
             <button
                type="submit"
                disabled={uploadMutation.isPending || !idCardFront || !idCardBack || !facePhoto}
                className="px-8 py-3.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl font-semibold transition-all shadow-lg shadow-indigo-500/30 flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed disabled:shadow-none"
             >
                {uploadMutation.isPending ? (
                  <>
                     <Loader2 className="w-5 h-5 animate-spin" />
                     Uploading Data...
                  </>
                ) : (
                  <>
                    <ShieldAlert className="w-5 h-5" />
                    Submit for AI Verification
                  </>
                )}
             </button>
          </div>
        </form>
      )}
    </div>
  );
}

function FileUploadBox({ label, file, onChange }: { label: string, file: File | null, onChange: (e: React.ChangeEvent<HTMLInputElement>) => void }) {
    const fileId = "file-" + label.replace(/\s+/g, '-').toLowerCase();
    
    return (
        <div className="relative">
            <label className="block text-sm font-medium text-slate-700 mb-2">{label}</label>
            <div className="relative">
                <input 
                    type="file" 
                    id={fileId}
                    accept="image/*" 
                    onChange={onChange}
                    className="absolute inset-0 w-full h-full opacity-0 cursor-pointer z-10" 
                />
                <div className={`flex items-center gap-4 p-4 rounded-xl border-2 border-dashed transition-colors ${
                    file ? 'border-emerald-300 bg-emerald-50' : 'border-slate-300 bg-slate-50 hover:bg-slate-100 hover:border-indigo-300'
                }`}>
                    <div className={`p-3 rounded-lg ${file ? 'bg-emerald-100' : 'bg-white shadow-sm border border-slate-200'}`}>
                        {file ? <FileImage className="w-6 h-6 text-emerald-600" /> : <UploadCloud className="w-6 h-6 text-indigo-500" />}
                    </div>
                    <div>
                        <p className="font-medium text-slate-900 truncate max-w-[200px]">
                           {file ? file.name : "Click to browse"}
                        </p>
                        <p className="text-xs text-slate-500">
                           {file ? (file.size / 1024 / 1024).toFixed(2) + ' MB' : 'PNG, JPG up to 10MB'}
                        </p>
                    </div>
                    {file && <CheckCircle className="w-5 h-5 text-emerald-500 ml-auto" />}
                </div>
            </div>
        </div>
    )
}

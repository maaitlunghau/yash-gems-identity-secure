import Link from "next/link";
import { ArrowRight, ShieldCheck, Gem, UserCheck, Smartphone } from "lucide-react";

export default function Home() {
  return (
    <div className="flex flex-col flex-grow">
      {/* Hero Section */}
      <section className="relative overflow-hidden bg-gradient-to-b from-slate-900 to-slate-800 text-white flex-grow flex items-center py-20">
        <div className="absolute inset-0 bg-[url('/noise.png')] opacity-5 mix-blend-overlay"></div>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10 w-full">
          <div className="grid md:grid-cols-2 gap-12 items-center">
            <div className="space-y-8">
              <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-indigo-500/10 border border-indigo-500/20 text-indigo-300 text-sm font-medium">
                <span className="relative flex h-2 w-2">
                  <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-indigo-400 opacity-75"></span>
                  <span className="relative inline-flex rounded-full h-2 w-2 bg-indigo-500"></span>
                </span>
                Identity Server is Live
              </div>
              <h1 className="text-5xl md:text-6xl font-extrabold tracking-tight leading-tight">
                Trust meets <span className="text-transparent bg-clip-text bg-gradient-to-r from-indigo-400 to-purple-400">Luxury</span>.
              </h1>
              <p className="text-lg text-slate-300 max-w-xl leading-relaxed">
                Experience seamless and secure onboarding. Yash Gems utilizes advanced AI face matching technology to verify your identity in seconds, ensuring a safe platform for premium jewelry shopping.
              </p>
              
              <div className="flex flex-col sm:flex-row gap-4">
                <Link href="/register" className="px-8 py-3.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg font-semibold transition-all shadow-lg shadow-indigo-500/30 flex items-center justify-center gap-2 group">
                  Get Started
                  <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
                </Link>
                <Link href="/login" className="px-8 py-3.5 bg-slate-800 hover:bg-slate-700 text-white rounded-lg font-semibold transition-all border border-slate-700 hover:border-slate-600 flex items-center justify-center">
                  Sign In
                </Link>
              </div>
            </div>
            
            <div className="relative hidden md:block">
              {/* Abstract decorative elements */}
              <div className="absolute -inset-4 bg-gradient-to-r from-indigo-500 to-purple-500 rounded-2xl blur-2xl opacity-20 animate-pulse"></div>
              <div className="bg-slate-800/80 backdrop-blur-xl border border-slate-700 rounded-2xl p-8 relative shadow-2xl">
                <div className="flex items-center justify-between mb-8">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-full bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center">
                      <ShieldCheck className="w-5 h-5 text-white" />
                    </div>
                    <div>
                      <h3 className="font-semibold">Security Status</h3>
                      <p className="text-xs text-slate-400">System running optimally</p>
                    </div>
                  </div>
                  <span className="px-2.5 py-1 text-xs font-semibold bg-emerald-500/10 text-emerald-400 rounded-full border border-emerald-500/20">
                    Protected
                  </span>
                </div>
                
                <div className="space-y-4">
                  {[ 
                    { icon: Gem, title: "Premium Accounts", desc: "For verified buyers only" },
                    { icon: UserCheck, title: "AI eKYC", desc: "Comparing selfie & ID via FPT AI" },
                    { icon: Smartphone, title: "OTP Verification", desc: "Multi-channel delivery" }
                  ].map((feature, i) => (
                    <div key={i} className="flex items-center gap-4 bg-slate-900/50 p-4 rounded-xl border border-slate-700/50 hover:bg-slate-700/50 transition-colors">
                      <div className="p-2 bg-slate-800 rounded-lg">
                        <feature.icon className="w-5 h-5 text-indigo-400" />
                      </div>
                      <div>
                        <h4 className="font-medium text-sm">{feature.title}</h4>
                        <p className="text-xs text-slate-400">{feature.desc}</p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}

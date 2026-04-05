export default function Footer() {
  return (
    <footer className="bg-slate-900 border-t border-slate-800 text-slate-400 py-12 mt-auto">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          <div className="col-span-1 md:col-span-2">
            <h3 className="text-xl font-bold text-white mb-4">Yash Gems Identity Server</h3>
            <p className="text-sm max-w-sm mb-4">
              Secure identity and KYC verification platform. Offering state-of-the-art AI face matching technology to ensure seamless and secure user onboarding.
            </p>
          </div>
          <div>
            <h4 className="text-white font-semibold mb-4">Quick Links</h4>
            <ul className="space-y-2 text-sm">
              <li><a href="/" className="hover:text-indigo-400 transition-colors">Home</a></li>
              <li><a href="/login" className="hover:text-indigo-400 transition-colors">Login</a></li>
              <li><a href="/kyc" className="hover:text-indigo-400 transition-colors">Verify eKYC</a></li>
            </ul>
          </div>
          <div>
            <h4 className="text-white font-semibold mb-4">Legal</h4>
            <ul className="space-y-2 text-sm">
              <li><a href="#" className="hover:text-indigo-400 transition-colors">Privacy Policy</a></li>
              <li><a href="#" className="hover:text-indigo-400 transition-colors">Terms of Service</a></li>
            </ul>
          </div>
        </div>
        <div className="mt-12 pt-8 border-t border-slate-800 text-sm text-center">
          &copy; {new Date().getFullYear()} Yash Gems. All rights reserved.
        </div>
      </div>
    </footer>
  );
}

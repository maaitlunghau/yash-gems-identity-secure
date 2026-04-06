'use client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { useState } from 'react';
import { GoogleOAuthProvider } from '@react-oauth/google';

export default function QueryProvider({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(() => new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 5 * 60 * 1000, 
        retry: 1, 
        refetchOnWindowFocus: false, 
      },
    },
  }));

  return (
    <GoogleOAuthProvider clientId="1028248624558-u01tj4cems88afovrqp7nqr195c4tflf.apps.googleusercontent.com">
      <QueryClientProvider client={queryClient}>
        {children}
        <ReactQueryDevtools initialIsOpen={false} />
      </QueryClientProvider>
    </GoogleOAuthProvider>
  );
}

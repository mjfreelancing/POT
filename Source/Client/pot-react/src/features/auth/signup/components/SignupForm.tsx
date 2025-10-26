import { useState } from 'react';

import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

type SignupFormProps = {
  onSubmit: (username: string, email: string) => Promise<void>;
  isLoading?: boolean;
  errorMessage?: string;
};

function SignupForm({
  onSubmit,
  isLoading = false,
  errorMessage,
}: SignupFormProps) {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    await onSubmit(username, email);
  };

  const isFormValid = username !== '' && email !== '';

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="space-y-3">
        <Label htmlFor="signup-username">Username</Label>
        <Input
          id="signup-username"
          name="username"
          type="text"
          placeholder="Enter your username"
          value={username}
          onChange={e => setUsername(e.target.value.trim())}
          required
          disabled={isLoading}
        />
      </div>

      <div className="space-y-3">
        <Label htmlFor="signup-email">Email</Label>
        <Input
          id="signup-email"
          name="email"
          type="email"
          placeholder="Enter your email"
          value={email}
          onChange={e => setEmail(e.target.value.trim())}
          required
          disabled={isLoading}
        />
      </div>

      {errorMessage && (
        <div className="bg-red-500/15 border border-red-400/30 rounded-md p-4 text-center">
          <p className="text-sm font-semibold text-red-400">{errorMessage}</p>
        </div>
      )}

      <Button
        type="submit"
        className="w-full"
        disabled={isLoading || !isFormValid}
      >
        {isLoading ? 'Sending...' : 'Send Verification Code'}
      </Button>
    </form>
  );
}

export default SignupForm;

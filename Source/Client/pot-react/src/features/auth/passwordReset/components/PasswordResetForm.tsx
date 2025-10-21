import { useState } from 'react';

import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

type PasswordResetFormProps = {
  onSubmit: (username: string) => Promise<void>;
  isLoading?: boolean;
};

function PasswordResetForm({
  onSubmit,
  isLoading = false,
}: PasswordResetFormProps) {
  const [username, setUsername] = useState('');

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    await onSubmit(username.trim());
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="space-y-3">
        <Label htmlFor="reset-username">Username</Label>
        <Input
          id="reset-username"
          name="username"
          type="text"
          placeholder="Enter your username"
          value={username}
          onChange={e => setUsername(e.target.value.trim())}
          required
          disabled={isLoading}
        />
      </div>

      <Button
        type="submit"
        className="w-full"
        disabled={isLoading || username.trim() === ''}
      >
        {isLoading ? 'Sending...' : 'Send Reset Code'}
      </Button>
    </form>
  );
}

export default PasswordResetForm;
